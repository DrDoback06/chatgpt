using System;
using System.Collections.Generic;
using UnityEngine; // Needed for Debug, JsonUtility, PlayerPrefs, Mathf

// NOTE: Requires supporting classes: CharacterAttributes, SkillTree, Skill, Item, Inventory, ItemAffix to be defined and [System.Serializable] where needed.

[System.Serializable]
public class Character
{
    // --- Basic Info ---
    public string characterName;
    public string mainClass;
    public string subClass;
    public string sprite;

    // --- Core Data Containers ---
    public CharacterAttributes attributes; // Base attributes assigned/increased by player
    public SkillTree mainClassSkillTree;
    public SkillTree subClassSkillTree;

    // --- Inventory ---
    // Ensure this is initialized in constructor and LoadCharacter, and owner set
    public Inventory inventory { get; private set; } // Public getter, private setter is implicit via constructor

    // --- Player Progression ---
    [SerializeField] private int _level = 1;
    [SerializeField] private int _experiencePoints = 0;
    [SerializeField] private int _requiredExperienceForNextLevel = 100;
    [SerializeField] private int _skillPoints = 0;
    [SerializeField] private int _statPoints = 5;

    // --- Combat Stats ---
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _maxHealth; // Dynamically calculated
    [SerializeField] private float _currentMana;
    [SerializeField] private float _maxMana; // Dynamically calculated

    // --- Faction & Pet ---
    private string faction;
    private string pet;

    // --- Runtime State ---
    private bool isDead = false;

    // --- Combat Calculation Constants ---
    // These determine how stats affect derived values. Adjust for balance.
    private const float BASE_HP = 50f;
    private const float HP_PER_VIT = 10f;
    private const float HP_PER_LEVEL = 5f;
    private const float BASE_MANA = 20f;
    private const float MANA_PER_INT = 5f;
    private const float MANA_PER_LEVEL = 2f;
    private const float BASE_ARMOR = 0f;
    private const float ARMOR_PER_AGI = 0.5f;
    private const float ARMOR_CALC_CONSTANT = 50f; // For mitigation formula


    // --- Events ---
    public event Action OnAttributesChanged;
    public event Action OnExperienceChanged;
    public event Action OnLevelUp;
    public event Action OnStatsChanged; // Covers HP, MP, Points, Level, MaxHP, MaxMP changes
    public event Action OnDeath;

    // --- Public Properties (Read-Only Accessors) ---
    public int Level => _level;
    public int ExperiencePoints => _experiencePoints;
    public int RequiredExperienceForNextLevel => _requiredExperienceForNextLevel;
    public int SkillPoints => _skillPoints;
    public int StatPoints => _statPoints;

    // Properties with private setters for controlled modification via methods
    public float CurrentHealth { get => _currentHealth; private set { /* ... setter logic ... */ } }
    public float MaxHealth { get => _maxHealth; private set { /* ... setter logic ... */ } }
    public float CurrentMana { get => _currentMana; private set { /* ... setter logic ... */ } }
    public float MaxMana { get => _maxMana; private set { /* ... setter logic ... */ } }
    // Property Setters Implementation (Included Below)
    // ...

    // --- Constructor ---
    public Character(string characterName, string mainClass, string sprite, SkillTree mainClassSkillTree, CharacterAttributes attributes, int initialInventorySize = 20)
    {
        this.characterName = characterName;
        this.mainClass = mainClass;
        this.sprite = sprite;
        this.mainClassSkillTree = mainClassSkillTree ?? new SkillTree();
        this.subClassSkillTree = new SkillTree(); // Initialize empty subclass tree
        this.attributes = attributes ?? new CharacterAttributes(5, 5, 5, 5);

        // Initialize progression
        this._level = 1;
        this._experiencePoints = 0;
        this._skillPoints = 0;
        this._statPoints = 5;
        this._requiredExperienceForNextLevel = CalculateRequiredExperience(this._level);
        this.isDead = false;

        // --- Initialize Inventory ---
        this.inventory = new Inventory(initialInventorySize);
        this.inventory.SetOwner(this); // <<< Crucial link

        // Subscribe to events AFTER objects are created
        this.attributes.OnAttributesChanged += HandleAttributeChange;
        this.inventory.OnEquipmentChanged += HandleEquipmentChange;

        // Initial calculation and setting of stats
        RecalculateCombatStats(); // Calculate MaxHP/MaxMP first
        this._currentHealth = this.MaxHealth; // Set current to initial max
        this._currentMana = this.MaxMana; // Set current to initial max

        Debug.Log($"Character '{characterName}' created. Lvl:{Level}, HP:{CurrentHealth:F0}/{MaxHealth:F0}, MP:{CurrentMana:F0}/{MaxMana:F0}, SP:{StatPoints}");
    }

    // --- Property Setter Implementations (Moved here for organization) ---
    // Allows private setting but includes clamping and event firing logic

    private float currentHealth // Backing field for CurrentHealth property
    {
        get => _currentHealth;
        set
        {
            float previousValue = _currentHealth;
            _currentHealth = Mathf.Clamp(value, 0, MaxHealth); // Use MaxHealth property
            if (!Mathf.Approximately(previousValue, _currentHealth)) // Use tolerance for float comparison
            {
                // Debug.Log($"CurrentHealth changed: {previousValue} -> {_currentHealth}"); // Optional Log
                OnStatsChanged?.Invoke();
            }
        }
    }

     private float maxHealth // Backing field for MaxHealth property
    {
        get => _maxHealth;
        set
        {
             float previousValue = _maxHealth;
             _maxHealth = Mathf.Max(1f, value); // Ensure at least 1 max health
             // Clamp current health if max health decreases below current
             CurrentHealth = Mathf.Min(CurrentHealth, _maxHealth); // Use property access for CurrentHealth setter logic
             if (!Mathf.Approximately(previousValue, _maxHealth))
             {
                 // Debug.Log($"MaxHealth changed: {previousValue} -> {_maxHealth}"); // Optional Log
                 OnStatsChanged?.Invoke();
             }
        }
    }

    private float currentMana // Backing field for CurrentMana property
    {
        get => _currentMana;
        set
        {
            float previousValue = _currentMana;
            _currentMana = Mathf.Clamp(value, 0, MaxMana); // Use MaxMana property
             if (!Mathf.Approximately(previousValue, _currentMana))
             {
                 // Debug.Log($"CurrentMana changed: {previousValue} -> {_currentMana}"); // Optional Log
                 OnStatsChanged?.Invoke();
             }
        }
    }
     private float maxMana // Backing field for MaxMana property
    {
        get => _maxMana;
        set
        {
             float previousValue = _maxMana;
             _maxMana = Mathf.Max(0f, value); // Can have 0 max mana
             // Clamp current mana if max decreases below current
             CurrentMana = Mathf.Min(CurrentMana, _maxMana); // Use property access for CurrentMana setter logic
              if (!Mathf.Approximately(previousValue, _maxMana))
              {
                  // Debug.Log($"MaxMana changed: {previousValue} -> {_maxMana}"); // Optional Log
                  OnStatsChanged?.Invoke();
              }
        }
    }

    // --- Event Handlers ---
    private void HandleAttributeChange()
    {
        RecalculateCombatStats();
        OnAttributesChanged?.Invoke(); // Notify systems specifically about base attribute change
        // OnStatsChanged is invoked internally by MaxHealth/MaxMana setters if they change
    }

    private void HandleEquipmentChange(Item.EquipmentSlot slotChanged)
    {
        // When equipment changes, derived stats need recalculation
        RecalculateCombatStats();
        // OnStatsChanged is invoked internally by MaxHealth/MaxMana setters if they change
    }

    // --- Core Stat Calculation Methods ---

    /// <summary>
    /// Recalculates derived stats like Max Health, Max Mana based on base attributes, level, and gear.
    /// </summary>
    private void RecalculateCombatStats()
    {
        if (attributes == null) return; // Cannot calculate without base attributes

        // Get total contributing stats (Base Attribute + Gear Affixes)
        int totalVitality = attributes.Vitality + (int)(inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.Vitality) ?? 0);
        int totalIntelligence = attributes.Intelligence + (int)(inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.Intelligence) ?? 0);
        // Add similar calculations for Str, Agi if they influence other derived stats

        // Calculate MaxHP/MaxMP using constants and total contributing stats + gear affixes
        float baseHpFromStatsAndLevel = BASE_HP + (totalVitality * HP_PER_VIT) + (_level * HP_PER_LEVEL);
        float flatHpBonus = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.MaxHealth_Flat) ?? 0f;
        float percentHpBonus = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.MaxHealth_Percent) ?? 0f;
        MaxHealth = (baseHpFromStatsAndLevel + flatHpBonus) * (1f + percentHpBonus / 100f);

        float baseMpFromStatsAndLevel = BASE_MANA + (totalIntelligence * MANA_PER_INT) + (_level * MANA_PER_LEVEL);
        float flatMpBonus = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.MaxMana_Flat) ?? 0f;
        float percentMpBonus = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.MaxMana_Percent) ?? 0f;
        MaxMana = (baseMpFromStatsAndLevel + flatMpBonus) * (1f + percentMpBonus / 100f);

        // Recalculate other derived stats here if needed (e.g., Crit Chance, Attack Speed based on Agility/Gear)
        // ...

        // Debug log moved from setters to here for consolidation
        // Debug.Log($"Stats recalculated -> HP:{CurrentHealth:F0}/{MaxHealth:F0}, MP:{CurrentMana:F0}/{MaxMana:F0}");
    }

    /// <summary>
    /// Calculates the total armor value from stats and gear.
    /// </summary>
    public float GetArmorValue()
    {
        float statArmor = attributes?.Agility * ARMOR_PER_AGI ?? 0f;
        float gearAffixArmor = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.Armor_Flat) ?? 0f; // Get total Armor affix FLAT
        float gearAffixArmorPercent = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.Armor_Percent) ?? 0f; // Get total Armor affix PERCENT
        float baseGearArmor = 0f;

         if(inventory != null) {
             foreach(var slot in inventory.GetEquippedItems().Values) {
                 if(slot != null && !slot.IsEmpty()) {
                    // Use the Item's method to get its effective base armor
                    baseGearArmor += slot.Item.GetEffectiveBaseArmor();
                 }
             }
         }

        // Apply percentage bonus to the sum of base armor from gear and flat affix armor
        return BASE_ARMOR + statArmor + ((baseGearArmor + gearAffixArmor) * (1f + gearAffixArmorPercent / 100f));
    }


    /// <summary>
    /// Calculates the damage contribution from equipped weapon(s).
    /// </summary>
    public float GetWeaponDamage()
    {
        if (inventory == null) return 0f;
        float totalWeaponDamage = 0f;

        // Check Main Weapon Slot
        Inventory.InventorySlot weaponSlot = inventory.GetEquippedSlot(Item.EquipmentSlot.Weapon);
        if (weaponSlot != null && !weaponSlot.IsEmpty())
        {
            totalWeaponDamage += weaponSlot.Item.GetTotalDamage(); // Use Item's method including base+affix
        }

        // Optional: Add logic for OffHand (dual wield damage, stats from shield etc.)
        // Inventory.InventorySlot offHandSlot = inventory.GetEquippedSlot(Item.EquipmentSlot.OffHand);
        // if (offHandSlot != null && !offHandSlot.IsEmpty()) {
        //      if(offHandSlot.Item.itemType == Item.ItemType.Weapon) {
        //          // totalWeaponDamage += offHandSlot.Item.GetTotalDamage() * 0.5f; // Example DW penalty
        //      }
        // }

        return totalWeaponDamage;
    }

     /// <summary>
    /// Gets the character's total Magic Find value from gear and potentially buffs.
    /// Placeholder - Requires MagicFind affix type and potentially buff system.
    /// </summary>
    public float GetMagicFindValue() {
        float baseMF = 0f; // Character base MF
        // Add MF from gear affixes (Needs ItemAffix.AffixType.MagicFind)
        // float gearMF = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.MagicFind) ?? 0f;
        // Add MF from status effects (Needs StatusEffect system)
        // float buffMF = 0f; // statusEffectController?.GetValue(StatusEffectType.MagicFind) ?? 0f;

        // Debug.Log("Placeholder GetMagicFindValue returning 0"); // Remove in final
        return baseMF; // + gearMF + buffMF;
    }

    // --- Progression Methods ---

    private int CalculateRequiredExperience(int currentLevel)
    {
        return Mathf.RoundToInt(100 * Mathf.Pow(currentLevel, 1.5f)); // Example formula
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0 || isDead) return;
        _experiencePoints += amount;
        OnExperienceChanged?.Invoke();

        bool leveledUp = false;
        while (_experiencePoints >= _requiredExperienceForNextLevel)
        {
            _experiencePoints -= _requiredExperienceForNextLevel;
            _level++;
            _skillPoints += 1;
            _statPoints += 5;
            _requiredExperienceForNextLevel = CalculateRequiredExperience(_level);

            RecalculateCombatStats();
            Heal(MaxHealth); // Full restore on level up
            RestoreMana(MaxMana);

            Debug.Log($"LEVEL UP! Reached Level {Level}. Points-> Stat:{StatPoints}, Skill:{SkillPoints}. Next XP:{RequiredExperienceForNextLevel}");
            OnLevelUp?.Invoke();
            leveledUp = true;
        }
        if (leveledUp) OnStatsChanged?.Invoke();
    }

    public bool IncreaseAttribute(string attributeName, int amount = 1)
    {
        if (_statPoints < amount || amount <= 0) return false;
        if (attributes == null) return false;

        bool success = false;
        switch (attributeName.ToLower()) // Use ToLower for case-insensitivity
        {
            case "strength": attributes.Strength += amount; success = true; break;
            case "agility": attributes.Agility += amount; success = true; break;
            case "intelligence": attributes.Intelligence += amount; success = true; break;
            case "vitality": attributes.Vitality += amount; success = true; break;
            default: Debug.LogWarning("Invalid attribute name: " + attributeName); return false;
        }

        if (success)
        {
            _statPoints -= amount;
            // HandleAttributeChange (subscribed to attributes.OnAttributesChanged) will trigger RecalculateCombatStats
            // Need to manually trigger OnStatsChanged for the point change itself for UI.
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool SpendSkillPoints(Skill skillToUpgrade)
    {
        if (skillToUpgrade == null || _level < skillToUpgrade.requiredLevel) return false;
        if (_skillPoints >= skillToUpgrade.requiredSkillPoints && skillToUpgrade.CanLevelUp())
        {
            _skillPoints -= skillToUpgrade.requiredSkillPoints;
            OnStatsChanged?.Invoke(); // Update UI for skill points
            return true;
        }
        return false;
    }


    // --- Combat Methods ---

    public void TakeDamage(float damageAmount)
    {
        if (isDead || CurrentHealth <= 0 || damageAmount <= 0) return;

        // Mitigation
        float armor = GetArmorValue();
        float damageReduction = (ARMOR_CALC_CONSTANT > 0) ? armor / (armor + ARMOR_CALC_CONSTANT) : 0f;
        float mitigatedDamage = damageAmount * (1f - damageReduction);
        float finalDamage = Mathf.Max(0, mitigatedDamage);

        this.currentHealth -= finalDamage; // Use the private field directly or property setter 'CurrentHealth = ...'

        // Debug.Log($"{characterName} took {finalDamage} damage ({damageAmount} raw). HP: {CurrentHealth}/{MaxHealth}");

        if (this.currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead || healAmount <= 0) return;
        this.currentHealth += healAmount; // Use private field or property 'CurrentHealth = ...'
        // Debug.Log($"{characterName} healed {healAmount}. HP: {CurrentHealth}/{MaxHealth}");
    }

    public bool SpendMana(float amount)
    {
        if (isDead || amount <= 0) return false;
        if (CurrentMana >= amount)
        {
            this.currentMana -= amount; // Use private field or property 'CurrentMana = ...'
            return true;
        }
        return false;
    }

    public void RestoreMana(float amount)
    {
        if (isDead || amount <= 0) return;
        this.currentMana += amount; // Use private field or property 'CurrentMana = ...'
    }


    // --- Death Handling ---
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        this.currentHealth = 0; // Ensure health is zero via property setter
        Debug.Log($"{characterName} has died!");
        OnDeath?.Invoke();
    }
    public bool IsDead() => isDead;


    // --- Misc Methods ---
    public void AssignFaction(string factionName) { faction = factionName; }
    public void AssignPet(string petName) { pet = petName; }
    public string GetFaction() => faction;
    public string GetPet() => pet;

    // Renamed from BoostAttribute to avoid confusion with spending points
    public void ApplyTemporaryAttributeBoost(string attributeName, int amount)
    {
        if (attributes == null || amount == 0) return;
        // This should likely be handled by the StatusEffect system which would
        // temporarily modify attributes and handle reverting them.
        // Avoid direct, permanent changes here unless intended.
        Debug.LogWarning($"ApplyTemporaryAttributeBoost called - Consider using Status Effects instead.");
        // switch (attributeName.ToLower()) { ... apply change ... }
        // Remember to handle removal/duration if doing it this way!
    }


    // --- Saving & Loading ---
    // Needs Inventory and related classes to be [System.Serializable]
    [System.Serializable]
    private class CharacterSaveData
    {
        public int level;
        public int experiencePoints;
        public int requiredExperienceForNextLevel;
        public int skillPoints;
        public int statPoints;
        public float currentHealth; // Save current health state
        public float currentMana;   // Save current mana state
        public bool isDead;
        public CharacterAttributes attributes;
        public SkillTree mainClassSkillTree;
        public SkillTree subClassSkillTree;
        public string faction;
        public string pet;
        public string inventoryJson; // Store serialized inventory data
    }

    public void SaveCharacter()
    {
        try
        {
            CharacterSaveData data = new CharacterSaveData
            {
                level = this._level,
                experiencePoints = this._experiencePoints,
                requiredExperienceForNextLevel = this._requiredExperienceForNextLevel,
                skillPoints = this._skillPoints,
                statPoints = this._statPoints,
                currentHealth = this._currentHealth, // Save actual current HP
                currentMana = this._currentMana,   // Save actual current MP
                isDead = this.isDead,
                attributes = this.attributes,
                mainClassSkillTree = this.mainClassSkillTree,
                subClassSkillTree = this.subClassSkillTree,
                faction = this.faction,
                pet = this.pet,
                inventoryJson = this.inventory?.GetSaveDataJson() ?? "" // Serialize inventory
            };

            string json = JsonUtility.ToJson(data, true); // Pretty print for debug
            PlayerPrefs.SetString($"CharacterData_{characterName}", json); // Using PlayerPrefs - replace with file save later
            PlayerPrefs.Save();
            Debug.Log($"Character '{characterName}' data saved.");
        }
        catch (Exception e) { Debug.LogError($"Failed to save character {characterName}: {e}"); }
    }

    public bool LoadCharacter()
    {
        string key = $"CharacterData_{characterName}";
        if (!PlayerPrefs.HasKey(key)) { Debug.Log($"No save data for {characterName}."); return false; }

        try
        {
            string json = PlayerPrefs.GetString(key);
            CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

            // --- Load Core Data ---
            this._level = data.level;
            this._experiencePoints = data.experiencePoints;
            this._requiredExperienceForNextLevel = data.requiredExperienceForNextLevel;
            this._skillPoints = data.skillPoints;
            this._statPoints = data.statPoints;
            this.isDead = data.isDead;
            this.attributes = data.attributes ?? new CharacterAttributes(5, 5, 5, 5);
            this.mainClassSkillTree = data.mainClassSkillTree ?? new SkillTree();
            this.subClassSkillTree = data.subClassSkillTree ?? new SkillTree();
            this.faction = data.faction;
            this.pet = data.pet;

            // --- Ensure Inventory Exists & Load ---
            if (this.inventory == null) { this.inventory = new Inventory(); }
            this.inventory.SetOwner(this); // Re-establish owner link
            this.inventory.LoadFromSaveDataJson(data.inventoryJson);

            // --- Re-subscribe Events ---
            this.attributes.OnAttributesChanged -= HandleAttributeChange; // Unsub first
            this.attributes.OnAttributesChanged += HandleAttributeChange;
            this.inventory.OnEquipmentChanged -= HandleEquipmentChange; // Unsub first
            this.inventory.OnEquipmentChanged += HandleEquipmentChange;

            // --- Recalculate Derived Stats (Based on loaded attributes/gear) ---
            RecalculateCombatStats(); // This calculates MaxHP/MaxMP

            // --- Load Current HP/MP (AFTER Max values are set) ---
            this.currentHealth = Mathf.Clamp(data.currentHealth, 0, MaxHealth);
            this.currentMana = Mathf.Clamp(data.currentMana, 0, MaxMana);

            // --- Trigger Events to Update UI ---
            // Invoke AFTER all loading and recalculation is done
            OnExperienceChanged?.Invoke();
            OnAttributesChanged?.Invoke();
            OnStatsChanged?.Invoke(); // This single event signals UI to update everything (HP, MP, Points etc)
            if (isDead) OnDeath?.Invoke(); // Trigger death state logic if loaded dead

            Debug.Log($"Character '{characterName}' loaded successfully. HP:{CurrentHealth:F0}/{MaxHealth:F0}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load character {characterName}: {e}. Resetting?");
            // Optionally reset character state here
            return false;
        }
    }

    // --- Cleanup ---
    public void Cleanup() // Call if character object is disposed or no longer needed
    {
        if (this.attributes != null) this.attributes.OnAttributesChanged -= HandleAttributeChange;
        if (this.inventory != null) this.inventory.OnEquipmentChanged -= HandleEquipmentChange;
        // Clear delegates to prevent memory leaks if objects hold references
        OnAttributesChanged = null; OnExperienceChanged = null; OnLevelUp = null;
        OnStatsChanged = null; OnDeath = null;
    }

    private float GetTotalEquippedStatModifier(ItemAffix.AffixType type) {
        float total = 0f;
        if (inventory == null) return 0f;

        foreach (var slot in inventory.GetEquippedItems().Values) { // Iterate through InventorySlots
            if (slot != null && slot.Item != null) { // Check if slot and Item within slot exist
                // Access generatedAffixes from the Item object inside the slot
                foreach (var affix in slot.Item.generatedAffixes) {
                    if (affix.type == type) {
                        total += affix.value;
                    }
                }
            }
        }
        return total;
    }

    // Placeholder for Status Effect System Integration
    public void ApplyStatusEffect(string effectID, float duration, float value) {
        Debug.LogWarning($"ApplyStatusEffect called (ID: {effectID}, Duration: {duration}, Value: {value}) - Placeholder. Implement with actual Status Effect system.");
        // TODO: Integrate with a proper StatusEffectController
    }

} // End of Character Class