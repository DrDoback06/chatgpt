using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

// Generates item instances, handling unique items, random base stats, rarity, affixes, and naming.
public static class ItemGenerator
{
    private static ItemDatabase itemDatabase;
    private static AffixManager affixManager; // Central place to hold affix definitions and name data
    private static bool isInitialized = false;

    // Call this once at game start (e.g., from a GameManager or Initialization scene)
    public static void Initialize(ItemDatabase itemDb, AffixManager affixMgr) {
        if (isInitialized) return;
        itemDatabase = itemDb;
        affixManager = affixMgr; // Store reference to affix manager
        if (itemDatabase == null) Debug.LogError("ItemGenerator init failed: Null Item DB!");
        if (affixManager == null) Debug.LogError("ItemGenerator init failed: Null Affix Manager!");
        isInitialized = (itemDatabase != null && affixManager != null);
        if(isInitialized) Debug.Log("ItemGenerator Initialized.");
    }

    /// <summary>
    /// Primary method to generate an item instance.
    /// Can generate specific uniques or random magic/rare/etc. items based on criteria.
    /// </summary>
    public static Item GenerateItem(int contextLevel, float magicFind = 0f, string specificBaseItemID = null, string uniqueItemID = null)
    {
        if (!isInitialized) { Debug.LogError("ItemGenerator not initialized!"); return null; }

        // --- I. Handle Unique Generation ---
        if (!string.IsNullOrEmpty(uniqueItemID)) {
            return GenerateUniqueItem(uniqueItemID);
        }

        // --- II. Generate Magic/Rare/etc. ---
        // 1. Select Base Template
        Item baseItemTemplate = SelectBaseTemplate(specificBaseItemID);
        if (baseItemTemplate == null) { Debug.LogWarning("GenerateItem: No suitable base template found."); return null; }

        // 2. Create Instance copy
        Item newItem = itemDatabase.CreateItemInstance(baseItemTemplate.itemID);
        if (newItem == null) return null;

        // 3. Roll Rarity (Magic or higher for generated equipment)
        newItem.generatedRarity = RollRarity(magicFind, true); // Roll actual rarity

        // 4. Roll Base Stats (within template range)
        newItem.rolledBaseArmor = UnityEngine.Random.Range(newItem.minBaseArmor, newItem.maxBaseArmor + 1);
        newItem.rolledBaseDamage = UnityEngine.Random.Range(newItem.minBaseDamage, newItem.maxBaseDamage + 1);

        // 5. Roll and Add Affixes based on Rarity/Pool
        RollAndAddAffixes(newItem, contextLevel, magicFind);

        // 6. Generate Dynamic Name
        newItem.generatedName = GenerateDynamicName(newItem);

        // 7. Set Final Visuals (Icon/World Sprite) based on INSTANCE rarity
        newItem.icon = GetSpriteForItem(newItem); // Use helper

        return newItem;
    }

    /// <summary>
    /// Generates a specific unique item instance from its definition.
    /// </summary>
    private static Item GenerateUniqueItem(string uniqueItemID) {
         Item uniqueTemplate = itemDatabase.GetItemByID(uniqueItemID); // Get definition
         if (uniqueTemplate == null || !uniqueTemplate.isUnique) {
             Debug.LogError($"GenerateItem Error: Unique ID '{uniqueItemID}' invalid or item not marked unique.");
             return null;
         }
         Item uniqueInstance = itemDatabase.CreateItemInstance(uniqueItemID); // Create runtime copy
         if (uniqueInstance != null) {
             uniqueInstance.generatedRarity = uniqueInstance.rarity; // Use defined Unique rarity
             uniqueInstance.rolledBaseArmor = uniqueInstance.minBaseArmor; // Use defined base stats
             uniqueInstance.rolledBaseDamage = uniqueInstance.minBaseDamage;
             uniqueInstance.generatedName = uniqueInstance.itemName; // Use predefined name
             uniqueInstance.icon = uniqueInstance.GetCurrentIcon(); // Get unique icon if defined
             // Note: predefinedAffixes are copied by CreateItemInstance via JSON
         }
         return uniqueInstance;
    }

    // --- Helper Methods ---

    private static Item SelectBaseTemplate(string specificBaseItemID) {
        IEnumerable<Item> query;
        // List<Item.ItemType> allowedTypes = new List<Item.ItemType> { Item.ItemType.Weapon, Item.ItemType.Armor, Item.ItemType.Charm }; // Example

        if (!string.IsNullOrEmpty(specificBaseItemID)) {
            Item forcedBase = itemDatabase.GetItemByID(specificBaseItemID);
            // Ensure forced base is not unique and is equippable (has a slot)
            if (forcedBase != null && !forcedBase.isUnique && forcedBase.equipSlot != Item.EquipmentSlot.None) query = new List<Item>{ forcedBase };
            else { Debug.LogError($"Invalid forced base ID '{specificBaseItemID}' or item is not equippable/is unique."); return null; }
        } else {
            // Filter for non-unique items that can be equipped (have an EquipSlot != None)
            query = itemDatabase.allItems?.Where(i => i != null && !i.isUnique && i.equipSlot != Item.EquipmentSlot.None);
        }
        List<Item> validBases = query?.ToList();
        if (validBases == null || validBases.Count == 0) return null;
        return validBases[UnityEngine.Random.Range(0, validBases.Count)];
    }

    // Apply Magic Find to shift roll ranges for higher rarity
    private static Item.ItemRarity RollRarity(float magicFind, bool forceMagicOrHigher) {
        // Example Magic Find implementation - Needs balancing!
        // Scale MF effect non-linearly (diminishing returns)
        float mfFactor = magicFind / (magicFind + 150f); // Example: 150 MF = 50% effective boost towards threshold shift
        float commonChance = forceMagicOrHigher ? 0f : 0.60f * (1f - mfFactor * 0.8f); // MF heavily reduces common chance
        float magicChance = 0.25f + (0.60f * mfFactor * 0.5f); // Increase magic chance based on reduced common
        float rareChance = 0.12f + (0.60f * mfFactor * 0.3f); // Increase rare chance
        float epicChance = 0.02f + (0.60f * mfFactor * 0.15f);
        float legendaryChance = 0.01f + (0.60f * mfFactor * 0.05f);

        // Normalize chances (approximate) - ensure they roughly add up
        float totalChance = commonChance + magicChance + rareChance + epicChance + legendaryChance;
        if (totalChance <= 0) totalChance = 1f; // Avoid division by zero

        float commonMax = commonChance / totalChance;
        float magicMax = commonMax + (magicChance / totalChance);
        float rareMax = magicMax + (rareChance / totalChance);
        float epicMax = rareMax + (epicChance / totalChance);
        // Legendary is anything above epicMax

        float roll = UnityEngine.Random.value;

        if (roll < commonMax) return Item.ItemRarity.Common;
        if (roll < magicMax) return Item.ItemRarity.Magic;
        if (roll < rareMax) return Item.ItemRarity.Rare;
        // Skip Set/Unique rolling here
        if (roll < epicMax) return Item.ItemRarity.Epic;
        // Check Legendary before defaulting
        if (roll < 1.0f) return Item.ItemRarity.Legendary; // Simplified: Any remaining roll is legendary

        return forceMagicOrHigher ? Item.ItemRarity.Magic : Item.ItemRarity.Common; // Fallback
    }


    private static int GetNumberOfAffixes(Item.ItemRarity rarity) {
         // Diablo 2 style: Magic=1-2 (1 prefix, 1 suffix), Rare=3-6 total
        switch (rarity) {
            case Item.ItemRarity.Common: return 0;
            case Item.ItemRarity.Magic: return 2; // Aim for 1 prefix, 1 suffix (generator logic handles selection)
            case Item.ItemRarity.Rare: return UnityEngine.Random.Range(3, 7); // 3 to 6
            case Item.ItemRarity.Epic: return UnityEngine.Random.Range(4, 7); // 4 to 6
            case Item.ItemRarity.Legendary: return UnityEngine.Random.Range(5, 8); // 5 to 7
            default: return 0;
        }
    }

    private static void RollAndAddAffixes(Item item, int itemLevel, float magicFind) {
        if (item.potentialAffixPool == null || item.rarity <= Item.ItemRarity.Common) return;

        int numAffixesToRoll = GetNumberOfAffixes(item.rarity);
        item.generatedAffixes = new List<ItemAffix>();
        if (numAffixesToRoll <= 0) return;

        // Get valid affix *definitions* from the pool based on item level
        List<AffixDefinition> possibleAffixDefs = GetValidAffixesFromPool(item.potentialAffixPool, itemLevel);
        if (possibleAffixDefs == null || possibleAffixDefs.Count == 0) { Debug.LogWarning($"No valid affixes found for item {item.itemID} (Lvl {itemLevel}) in pool {item.potentialAffixPool.poolID}"); return; }

        List<AffixDefinition> addedDefinitions = new List<AffixDefinition>(); // Track added *types*
        int prefixesAdded = 0;
        int suffixesAdded = 0;
        int maxPrefixes = (item.rarity == Item.ItemRarity.Magic) ? 1 : 3; // Magic max 1 prefix, Rare max 3
        int maxSuffixes = (item.rarity == Item.ItemRarity.Magic) ? 1 : 3; // Magic max 1 suffix, Rare max 3

        // Create a temporary weighted list for selection that we can modify
        List<AffixDefinition> currentPool = new List<AffixDefinition>(possibleAffixDefs);

        for (int i = 0; i < numAffixesToRoll && currentPool.Count > 0; ) {
            AffixDefinition chosenAffixDef = GetRandomAffixFromListWeighted(currentPool, item.potentialAffixPool);
            if (chosenAffixDef == null) { currentPool.Clear(); break; } // Exhausted pool or error

            // Check Prefix/Suffix limits for Magic/Rare items
            bool isPrefix = chosenAffixDef.IsPrefix;
            if (item.rarity <= Item.ItemRarity.Rare) {
                if (isPrefix && prefixesAdded >= maxPrefixes) { currentPool.Remove(chosenAffixDef); continue; } // Skip if prefix limit reached
                if (!isPrefix && suffixesAdded >= maxSuffixes) { currentPool.Remove(chosenAffixDef); continue; } // Skip if suffix limit reached
            }
            // Prevent adding the exact same definition (e.g., two identical +Str rolls)
            if (addedDefinitions.Contains(chosenAffixDef)) { currentPool.Remove(chosenAffixDef); continue; }

            // Roll the value using the definition's tiers
            float value = chosenAffixDef.RollValue(itemLevel);
            // TODO: Apply Magic Find bonus to the *value roll*? (e.g., higher chance to roll near max tier value)

            if (!Mathf.Approximately(value, 0f)) {
                item.generatedAffixes.Add(new ItemAffix(chosenAffixDef.affixType, value));
                addedDefinitions.Add(chosenAffixDef); // Track that this definition was used
                if (isPrefix) prefixesAdded++; else suffixesAdded++;
                i++; // Count successful add
            }

            // Always remove the chosen definition from the temporary pool for this item instance
            currentPool.Remove(chosenAffixDef);
        }
    }

    // --- Keep helper methods from before ---
    private static List<AffixDefinition> GetValidAffixesFromPool(AffixPool pool, int itemLevel) { /* ... unchanged ... */ return null; }
    private static AffixDefinition GetRandomAffixFromListWeighted(List<AffixDefinition> availableDefs, AffixPool sourcePool) { /* ... unchanged ... */ return null; }

    // --- Dynamic Name Generation ---
    private static string GenerateDynamicName(Item item) {
        if (item == null || item.isUnique || item.rarity <= Item.ItemRarity.Common) return item.itemName; // Use predefined name

        string prefix = ""; string suffix = "";
        AffixDefinition prefixAffixDef = null; float prefixVal = 0;
        AffixDefinition suffixAffixDef = null; float suffixVal = 0;

        // Find best prefix/suffix based on generated affixes
        foreach (var affixInstance in item.generatedAffixes) {
            if (affixManager == null) break; // Need manager for lookup
            AffixDefinition def = affixManager.GetAffixDefinitionByType(affixInstance.type);
            if (def == null) continue;
            // Simple logic: track highest value prefix and highest value suffix separately
            if (def.IsPrefix && Mathf.Abs(affixInstance.value) >= Mathf.Abs(prefixVal)) { prefixAffixDef = def; prefixVal = affixInstance.value; }
            else if (!def.IsPrefix && Mathf.Abs(affixInstance.value) >= Mathf.Abs(suffixVal)) { suffixAffixDef = def; suffixVal = affixInstance.value; }
        }

        // Get name words using AffixManager
        if (prefixAffixDef != null) prefix = affixManager.GetNameWordForAffix(prefixAffixDef.affixType, prefixVal, true);
        if (suffixAffixDef != null) suffix = affixManager.GetNameWordForAffix(suffixAffixDef.affixType, suffixVal, false);

        // Rares get a random cool name from the pool
        if (item.rarity == Item.ItemRarity.Rare && affixManager != null) {
            return affixManager.GetRandomRareName(item.itemType);
        }

        // Magic/Epic/Legendary: Combine Prefix + BaseName + Suffix
        // Get base name from template in database
        string baseName = itemDatabase?.GetItemByID(item.itemID)?.itemName ?? item.itemName;

        // Build the name carefully to avoid extra spaces
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix)) sb.Append(prefix).Append(" ");
        sb.Append(baseName);
        if (!string.IsNullOrEmpty(suffix)) sb.Append(" ").Append(suffix);

        string finalName = sb.ToString();

        // If somehow only base name resulted (e.g., 1 affix with no name word), try using that affix's name word
        if (finalName == baseName && item.generatedAffixes.Count > 0) {
            AffixDefinition singleAffixDef = affixManager?.GetAffixDefinitionByType(item.generatedAffixes[0].type);
            if (singleAffixDef != null) {
                 string word = singleAffixDef.GetNameWord(item.generatedAffixes[0].value, singleAffixDef.IsPrefix);
                 if (!string.IsNullOrEmpty(word)) {
                      if (singleAffixDef.IsPrefix) return $"{word} {baseName}";
                      else return $"{baseName} {word}";
                 }
            }
        }
        return finalName;
    }

    // --- Sprite Selection Helper ---
    private static Sprite GetSpriteForItem(Item item) {
        if (item == null) return null;
        Item baseTemplate = itemDatabase?.GetItemByID(item.itemID); // Need template for visual fields
        if (baseTemplate == null) return item.icon;

        Sprite chosenIcon = null;
        if (item.isUnique) chosenIcon = baseTemplate.iconUnique; // Check unique first
        if (chosenIcon == null) {
             switch(item.generatedRarity) { // Check instance rarity
                case Item.ItemRarity.Magic: chosenIcon = baseTemplate.iconMagic; break;
                case Item.ItemRarity.Rare: chosenIcon = baseTemplate.iconRare; break;
                case Item.ItemRarity.Set: chosenIcon = baseTemplate.iconSet; break;
                case Item.ItemRarity.Legendary: chosenIcon = baseTemplate.iconLegendary; break;
                case Item.ItemRarity.Epic: chosenIcon = baseTemplate.iconEpic; break;
             }
        }
        // Fallback chain: SpecificRarityIcon -> CommonIcon -> BaseIcon defined on template
        return chosenIcon ?? baseTemplate.iconCommon ?? baseTemplate.icon;
    }

} // End ItemGenerator Class