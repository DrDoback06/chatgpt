using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text; // For StringBuilder
using System.Linq; // For Linq Sum etc.

// Final Enhanced Item definition
// Defines the *template* for an item. Generated instances hold runtime data.
[System.Serializable]
public class Item
{
    [Header("Core Information")]
    [Tooltip("Unique identifier for this item type (e.g., 'unique_shako', 'base_leather_helm', 'potion_health_small'). MUST BE UNIQUE.")]
    public string itemID;
    [Tooltip("Base item name used for generation (e.g., 'Shako', 'Short Sword', 'Health Potion').")]
    public string itemName; // Base name, generated items get prefixes/suffixes
    [TextArea] [Tooltip("Flavor text or basic description.")]
    public string description;
    [Tooltip("Inventory icon sprite (base look).")]
    public Sprite icon;
    [Tooltip("General category of the item.")]
    public ItemType itemType = ItemType.Miscellaneous;

    [Header("Item Quality & Uniqueness")]
    [Tooltip("Base rarity. Generated items roll higher rarities.")]
    public ItemRarity rarity = ItemRarity.Common; // Rarity defined here is often just Common for base templates
    [Tooltip("Is this a specifically defined Unique/Set item with fixed properties?")]
    public bool isUnique = false; // Set to true for hand-crafted Uniques/Sets

    [Header("Requirements")]
    [Min(1)] public int requiredLevel = 1;
    [Min(0)] public int requiredStrength = 0;
    [Min(0)] public int requiredAgility = 0;
    [Min(0)] public int requiredIntelligence = 0;
    [Min(0)] public int requiredVitality = 0;
    [Tooltip("Leave empty if usable by all classes.")]
    public List<string> requiredClasses = new List<string>();

    [Header("Stacking & Value")]
    public bool isStackable = false;
    [Min(1)] public int maxStackSize = 1;
    [Min(0)] public int goldValue = 0;

    [Header("Equipment Properties (Base Template Definition)")]
    [Tooltip("Which slot(s) this item occupies.")]
    public EquipmentSlot equipSlot = EquipmentSlot.None;
    [Tooltip("Does this weapon use both Weapon and OffHand slots?")]
    public bool isTwoHanded = false;
    [Tooltip("Minimum base armor. Generator rolls between Min/Max for non-uniques.")]
    [Min(0)] public int minBaseArmor = 0;
    [Tooltip("Maximum base armor. Set Max > Min to allow random roll.")]
    [Min(0)] public int maxBaseArmor = 0;
    [Tooltip("Minimum base damage.")]
    [Min(0)] public int minBaseDamage = 0;
    [Tooltip("Maximum base damage. Set Max > Min to allow random roll.")]
    [Min(0)] public int maxBaseDamage = 0;

    [Header("Inherent Stats (Always present on this Item Type)")]
    [Tooltip("Stats hardcoded onto this item base type (e.g., all 'Tower Shields' might give +Block Chance). Separate from random affixes.")]
    public int inherentStrength = 0;
    public int inherentAgility = 0;
    public int inherentIntelligence = 0;
    public int inherentVitality = 0;
    // Add other inherent base stats if needed

    [Header("Affixes")]
    [Tooltip("Reference to the Affix Pool used for generating random affixes on Magic/Rare/etc versions of this item base. Leave empty for items that don't get random stats (like potions, quest items).")]
    public AffixPool potentialAffixPool;
    [Tooltip("For Unique/Set items ONLY: Define their specific, fixed affixes here using AffixDefinition assets and exact values.")]
    public List<ItemAffixEntry> predefinedAffixes; // Use helper struct for Inspector setup

    // --- Runtime Data (Populated on Generated Instances) ---
    [Header("Generated/Instance Data (Not set on templates)")]
    [Tooltip("Actual rolled rarity for this instance.")]
    public ItemRarity generatedRarity = ItemRarity.Common; // Store the actual rolled rarity
    [Tooltip("Holds the randomly rolled affixes for Magic/Rare/etc items.")]
    public List<ItemAffix> generatedAffixes = new List<ItemAffix>();
    [Tooltip("Holds the rolled base armor value for generated items.")]
    public int rolledBaseArmor; // Stores result of Random.Range(minBaseArmor, maxBaseArmor)
    [Tooltip("Holds the rolled base damage value for generated items.")]
    public int rolledBaseDamage;
    [Tooltip("Holds the generated name (e.g., 'Cruel Sword of Strength').")]
    public string generatedName;

    [Header("Regenerative / DoT / Proc Effects")]
    [Tooltip("StatusEffect ID applied on Use/Hit (e.g., 'Heal_Small', 'Poison_Weak'). Needs StatusEffect system.")]
    public string statusEffectIDOnUseOrHit;
    public float statusEffectDuration;
    public float statusEffectValue;
    [Range(0f, 100f)] public float statusEffectChance = 100f;

    [Header("Visual Variations (Optional - Define on Base Template)")]
    public Sprite iconCommon; // Use base 'icon' for this?
    public Sprite iconUncommon;
    public Sprite iconMagic;
    public Sprite iconRare;
    public Sprite iconEpic;
    public Sprite iconSet;
    public Sprite iconUnique;
    public Sprite iconLegendary;
    public Sprite worldSpriteCommon;
    public Sprite worldSpriteUncommon;
    public Sprite worldSpriteMagic;
    public Sprite worldSpriteRare;
    public Sprite worldSpriteEpic;
    public Sprite worldSpriteSet;
    public Sprite worldSpriteUnique;
    public Sprite worldSpriteLegendary;


    // --- Enums ---
    public enum ItemRarity { Common, Uncommon, Magic, Rare, Epic, Set, Unique, Legendary } // YOUR Definition
    public enum ItemType { Weapon, Armor, Consumable, Material, QuestItem, Miscellaneous, Charm } // YOUR Definition
    public enum EquipmentSlot { None, Head, Chest, Boots, Gloves, Belt, Weapon, OffHand, Amulet, Ring1, Ring2 } // YOUR Definition


    // --- Constructor ---
    public Item() { generatedAffixes = new List<ItemAffix>(); predefinedAffixes = new List<ItemAffixEntry>(); }

    // --- Methods ---

    // Use the ROLLED base value for generated items, or the predefined Min value for templates/uniques
    public int GetEffectiveBaseArmor() => (isUnique || generatedRarity == ItemRarity.Common) ? minBaseArmor : rolledBaseArmor;
    public int GetEffectiveBaseDamage() => (isUnique || generatedRarity == ItemRarity.Common) ? minBaseDamage : rolledBaseDamage;

    /// <summary>Calculates TOTAL value for a stat (Inherent + Predefined Affix + Generated Affix).</summary>
    public float GetTotalStatModifier(ItemAffix.AffixType type) {
        float total = 0;
        // Add inherent base stats from the template
        switch (type) {
            case ItemAffix.AffixType.Strength: total += inherentStrength; break;
            case ItemAffix.AffixType.Agility: total += inherentAgility; break;
            case ItemAffix.AffixType.Intelligence: total += inherentIntelligence; break;
            case ItemAffix.AffixType.Vitality: total += inherentVitality; break;
        }
        // Add predefined fixed affixes (for Uniques/Sets)
        if ((isUnique || generatedRarity == ItemRarity.Set) && predefinedAffixes != null) {
             foreach (var entry in predefinedAffixes) if (entry.affixDefinition?.affixType == type) total += entry.value;
        }
        // Add randomly generated affixes (for Magic/Rare etc.)
        if (!isUnique && generatedRarity > ItemRarity.Common && generatedAffixes != null) {
             foreach (var affix in generatedAffixes) if (affix.type == type) total += affix.value;
        }
        return total;
    }

    // Calculate total Armor/Damage using the effective base + relevant affixes (handling % increases)
    public int GetTotalArmor() {
         int flatBase = GetEffectiveBaseArmor();
         float flatAffix = GetTotalStatModifier(ItemAffix.AffixType.Armor_Flat);
         float percentAffix = GetTotalStatModifier(ItemAffix.AffixType.Armor_Percent); // Assumes value is like '15' for 15%
         return Mathf.Max(0, Mathf.RoundToInt((flatBase + flatAffix) * (1f + percentAffix / 100f))); // Apply % increase to base+flat
    }
    public int GetTotalDamage() {
         int flatBase = GetEffectiveBaseDamage();
         float flatAffix = GetTotalStatModifier(ItemAffix.AffixType.Damage_Flat);
         float percentAffix = GetTotalStatModifier(ItemAffix.AffixType.Damage_Percent);
         return Mathf.Max(0, Mathf.RoundToInt((flatBase + flatAffix) * (1f + percentAffix / 100f)));
    }


    public bool MeetsRequirements(Character character) {
        if (character == null) return false;
        if (character.Level < requiredLevel) return false;
        if (character.attributes.Strength < requiredStrength) return false;
        if (character.attributes.Agility < requiredAgility) return false;
        if (character.attributes.Intelligence < requiredIntelligence) return false;
        if (character.attributes.Vitality < requiredVitality) return false;
        if (requiredClasses != null && requiredClasses.Count > 0 && !requiredClasses.Contains(character.mainClass)) return false;
        return true;
    }

    public virtual string GetTooltipInfo() {
        StringBuilder sb = new StringBuilder();
        // Use generated name if available, otherwise base template name
        string finalName = string.IsNullOrEmpty(generatedName) ? itemName : generatedName;
        string nameColor = GetRarityColorHex(generatedRarity); // Use generatedRarity for color

        sb.AppendLine($"<b><color=#{nameColor}>{finalName}</color></b>");
        if (generatedRarity != ItemRarity.Common && !isUnique && !string.IsNullOrEmpty(generatedName) && generatedName != itemName) {
            sb.AppendLine($"<i>({itemName})</i>"); // Show base type name if generated name differs
        }
        if (isUnique) sb.AppendLine("<color=#C7B377><i>Unique Item</i></color>");
        else if (generatedRarity != ItemRarity.Common) sb.AppendLine($"({generatedRarity})");

        if (isUnique && !string.IsNullOrEmpty(description)) sb.AppendLine($"\n{description}"); // Show description for uniques

        // --- Equipment Section ---
        if (itemType == ItemType.Weapon || itemType == ItemType.Armor) {
            if (equipSlot != EquipmentSlot.None) sb.AppendLine($"Slot: {equipSlot}{(isTwoHanded ? " (2-Handed)" : "")}");
            int totalDamage = GetTotalDamage(); int totalArmor = GetTotalArmor();
            if (totalDamage > 0) sb.AppendLine($"Damage: {totalDamage}");
            if (totalArmor > 0) sb.AppendLine($"Armor: {totalArmor}");
            sb.AppendLine();
        }

        // --- Requirements ---
        bool hasReq = requiredLevel > 1 || requiredStrength > 0 || requiredAgility > 0 || requiredIntelligence > 0 || requiredVitality > 0 || (requiredClasses != null && requiredClasses.Count > 0);
        if (hasReq) {
            sb.AppendLine("--- Requirements ---");
            if (requiredLevel > 1) sb.AppendLine($"Level: {requiredLevel}");
            // ... Add other requirement lines ...
            sb.AppendLine();
        }

        // --- Inherent Stats ---
        bool hasInherent = inherentStrength!=0 || inherentAgility!=0 || inherentIntelligence!=0 || inherentVitality!=0;
        if (hasInherent) {
             sb.AppendLine("--- Item Stats ---");
             if (inherentStrength != 0) sb.AppendLine($"Strength: {inherentStrength}");
             // ... Add other inherent stat lines ...
             sb.AppendLine();
        }

        // --- Affixes (Predefined or Generated) ---
        // Use the correct affix list based on whether it's unique or generated
        List<ItemAffix> affixesToDisplay = (isUnique) ? GetPredefinedAffixesAsRuntime() : generatedAffixes;
        if (affixesToDisplay != null && affixesToDisplay.Count > 0) {
            sb.AppendLine("--- Magic Properties ---");
            // Sort affixes for consistent display? Optional.
            // affixesToDisplay = affixesToDisplay.OrderBy(a => a.type.ToString()).ToList();
            foreach (ItemAffix affix in affixesToDisplay) sb.AppendLine($"{affix.GetAffixDescription()}");
            sb.AppendLine();
        }

        // --- Effects ---
        if (!string.IsNullOrEmpty(statusEffectIDOnUseOrHit)) { /* Add effect line */ }

        // --- Other Info ---
        if (isStackable) sb.AppendLine($"Max Stack: {maxStackSize}");
        if (goldValue > 0) sb.AppendLine($"Value: {goldValue} Gold");

        return sb.ToString().TrimEnd();
    }

    public virtual bool Use(Character character) {
        if (character == null) return false;
        
        // Check if item is consumable
        if (itemType != ItemType.Consumable) return false;
        
        // Apply status effect if defined
        if (!string.IsNullOrEmpty(statusEffectIDOnUseOrHit)) {
            if (UnityEngine.Random.value <= statusEffectChance / 100f) {
                // Apply the status effect to the character
                // This assumes you have a method to apply status effects
                character.ApplyStatusEffect(statusEffectIDOnUseOrHit, statusEffectDuration, statusEffectValue);
            }
        }
        
        return true;
    }

    // Gets appropriate INVENTORY icon based on generated/unique rarity
    public Sprite GetCurrentIcon() {
        Sprite sprite = null;
        if (isUnique) sprite = iconUnique;
        if (sprite == null) {
            switch(this.generatedRarity) { // Use generatedRarity here
                case ItemRarity.Uncommon: sprite = iconUncommon; break;
                case ItemRarity.Magic: sprite = iconMagic; break;
                case ItemRarity.Rare: sprite = iconRare; break;
                case ItemRarity.Epic: sprite = iconEpic; break;
                case ItemRarity.Set: sprite = iconSet; break;
                case ItemRarity.Legendary: sprite = iconLegendary; break;
            }
        }
        return sprite ?? iconCommon ?? icon; // Fallback chain
    }

    // Gets appropriate WORLD sprite based on generated/unique rarity
    public Sprite GetWorldSpriteForCurrentRarity() {
        Sprite sprite = null;
        if (isUnique) sprite = worldSpriteUnique;
        if (sprite == null) {
            switch(this.generatedRarity) {
                case ItemRarity.Uncommon: sprite = worldSpriteUncommon; break;
                case ItemRarity.Magic: sprite = worldSpriteMagic; break;
                case ItemRarity.Rare: sprite = worldSpriteRare; break;
                case ItemRarity.Epic: sprite = worldSpriteEpic; break;
                case ItemRarity.Set: sprite = worldSpriteSet; break;
                case ItemRarity.Legendary: sprite = worldSpriteLegendary; break;
            }
        }
        return sprite ?? worldSpriteCommon ?? icon; // Fallback chain
    }


    // --- Private Helpers ---
    private List<ItemAffix> GetPredefinedAffixesAsRuntime() {
         if(predefinedAffixes == null) return new List<ItemAffix>();
         // Convert List<ItemAffixEntry> to List<ItemAffix>
         return predefinedAffixes.Where(e => e.affixDefinition != null)
                                 .Select(e => new ItemAffix(e.affixDefinition.affixType, e.value))
                                 .ToList();
    }
    private string GetRarityColorHex(ItemRarity r) {
        switch(r) {
            case ItemRarity.Common: return "FFFFFF"; case ItemRarity.Magic: return "6969FF";
            case ItemRarity.Rare: return "FFFF00"; case ItemRarity.Set: return "00FF00";
            case ItemRarity.Unique: return "C7B377"; case ItemRarity.Legendary: return "FF8000";
            // Epic color? Add if needed.
            default: return "FFFFFF";
        }
    }

} // End Item Class


// Helper Struct for defining unique item affixes in the Inspector
[System.Serializable]
public struct ItemAffixEntry {
    [Tooltip("Reference to the Affix Definition asset.")]
    public AffixDefinition affixDefinition; // <<< Assign Affix Definition SO here
    [Tooltip("The specific, fixed value for this affix on the unique item.")]
    public float value; // Use float to match ItemAffix
}