using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for Linq methods

// Manages access to Affix Definitions and Naming data.
// Attach to a persistent GameObject (e.g., under --- MANAGERS ---).
public class AffixManager : MonoBehaviour
{
    public static AffixManager Instance { get; private set; }

    [Header("Affix Definitions")]
    [Tooltip("Assign ALL AffixDefinition ScriptableObject assets here. Load order doesn't matter.")]
    public List<AffixDefinition> allAffixDefinitions;

    [Header("Naming Data (Rare Name Pools)")]
    // Define pools for random rare names per item type
    public List<string> rareWeaponNames = new List<string> { "Doom Bringer", "Soul Carver", "Wraith Spike", "Eagle Talon", "Ghost Reaver", "Bone Splitter" };
    public List<string> rareHelmNames = new List<string> { "Demon Visage", "Spirit Mask", "Chaos Crest", "Griffon's Gaze" };
    public List<string> rareArmorNames = new List<string> { "Serpent Skin", "Kraken Shell", "Wyrmhide", "Archon Plate" };
    public List<string> rareShieldNames = new List<string> { "Aegis", "Defender", "Bulwark", "Spike Shield" };
    public List<string> rareGloveNames = new List<string> { "Iron Grip", "Shadow Fist", "Blood Touch", "Viper Clasp" };
    public List<string> rareBootNames = new List<string> { "Storm Treads", "Bone Spurs", "Wyvern Flights", "Shadow Striders" };
    public List<string> rareBeltNames = new List<string> { "Demon Sash", "Spider Cord", "Guardian Coil", "Thunder Vise" };
    public List<string> rareAmuletNames = new List<string> { "Wraith Coil", "Serpent Scarab", "Doom Torc", "Eagle Pendant" };
    public List<string> rareRingNames = new List<string> { "Chaos Loop", "Bone Signet", "Storm Circle", "Wraith Band" };
    // Add pools for other item types (Charms, etc.) if needed

    // --- Runtime Lookups (Built in InitializeData) ---
    private Dictionary<ItemAffix.AffixType, AffixDefinition> definitionsByType = new Dictionary<ItemAffix.AffixType, AffixDefinition>();
    private Dictionary<ItemAffix.AffixType, List<AffixNameTier>> prefixNames = new Dictionary<ItemAffix.AffixType, List<AffixNameTier>>();
    private Dictionary<ItemAffix.AffixType, List<AffixNameTier>> suffixNames = new Dictionary<ItemAffix.AffixType, List<AffixNameTier>>();
    private Dictionary<Item.ItemType, List<string>> rareNamePoolsByType = new Dictionary<Item.ItemType, List<string>>();

    void Awake()
    {
        // Singleton Setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeData(); // Build lookups
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Processes assigned definitions and builds lookup dictionaries for faster access.
    /// </summary>
    private void InitializeData()
    {
        definitionsByType.Clear();
        prefixNames.Clear();
        suffixNames.Clear();
        rareNamePoolsByType.Clear();

        if (allAffixDefinitions == null || allAffixDefinitions.Count == 0) {
            Debug.LogError("AffixManager: No Affix Definitions assigned in the Inspector!");
            return;
        }

        // Build definition lookup and name lookups
        foreach (AffixDefinition def in allAffixDefinitions) {
            if (def == null) continue;
            if (!definitionsByType.ContainsKey(def.affixType)) {
                definitionsByType.Add(def.affixType, def);
                // Sort name tiers by value threshold ascending for correct lookup later
                if (def.prefixes != null && def.prefixes.Count > 0) {
                    prefixNames.Add(def.affixType, def.prefixes.OrderBy(n => n.minValueThreshold).ToList());
                }
                if (def.suffixes != null && def.suffixes.Count > 0) {
                    suffixNames.Add(def.affixType, def.suffixes.OrderBy(n => n.minValueThreshold).ToList());
                }
            } else {
                Debug.LogWarning($"AffixManager: Duplicate AffixDefinition for type {def.affixType}. Ignoring '{def.name}'.");
            }
        }

        // Build rare name lookup
        rareNamePoolsByType[Item.ItemType.Weapon] = rareWeaponNames;
        rareNamePoolsByType[Item.ItemType.Armor] = rareArmorNames; // Use generic armor pool for now
        // Assign specific pools if lists exist
        if (rareHelmNames.Count > 0) rareNamePoolsByType[Item.ItemType.Armor] = rareHelmNames; // Example override - needs better logic if ItemType isn't granular enough
        if (rareShieldNames.Count > 0) rareNamePoolsByType[Item.ItemType.Armor] = rareShieldNames; // Override based on EquipSlot would be better
        // TODO: Refine rare name pool assignment based on ItemType or EquipSlot

         Debug.Log($"AffixManager Initialized: {definitionsByType.Count} definitions, {prefixNames.Count} prefix types, {suffixNames.Count} suffix types.");
    }

    /// <summary>
    /// Gets the AffixDefinition for a specific AffixType using the lookup dictionary.
    /// </summary>
    public AffixDefinition GetAffixDefinitionByType(ItemAffix.AffixType type) {
        definitionsByType.TryGetValue(type, out AffixDefinition def);
        if (def == null) Debug.LogWarning($"AffixDefinition not found for type: {type}");
        return def;
    }

    /// <summary>
    /// Gets a suitable prefix or suffix name word for an affix type and its rolled value from lookup dictionaries.
    /// </summary>
    public string GetNameWordForAffix(ItemAffix.AffixType type, float value, bool isPrefix) {
        Dictionary<ItemAffix.AffixType, List<AffixNameTier>> source = isPrefix ? prefixNames : suffixNames;
        if (source.TryGetValue(type, out List<AffixNameTier> nameTiers)) {
            // Find best fit (highest threshold <= value) in the sorted list
            string nameWord = "";
            foreach(var tier in nameTiers) { // Iterate sorted list (ascending threshold)
                 if(value >= tier.minValueThreshold) {
                    nameWord = tier.nameWord; // Keep track of the best fit so far
                 } else {
                      break; // Since list is sorted, no higher tier will match
                 }
            }
            return nameWord;
        }
        return "";
    }

    /// <summary>
    /// Gets a random name from the appropriate rare name pool based on item type.
    /// </summary>
    public string GetRandomRareName(Item.ItemType itemType) {
        // TODO: Improve this lookup, maybe based on EquipSlot for more specificity?
        List<string> pool;
        if (!rareNamePoolsByType.TryGetValue(itemType, out pool)) {
             // Fallback to a default pool if type not found
             if (!rareNamePoolsByType.TryGetValue(Item.ItemType.Armor, out pool)) { // Example fallback
                return "Rare Item"; // Ultimate fallback
             }
        }

        if (pool != null && pool.Count > 0) {
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }
        return "Rare Item"; // Fallback if pool empty
    }
}