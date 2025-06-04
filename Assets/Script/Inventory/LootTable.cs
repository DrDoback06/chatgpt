using UnityEngine;
using System.Collections.Generic;

// Defines potential loot drops from an enemy or source.
// Can include specific items and chances for randomly generated items.

// Represents a specific predefined item that might drop.
[System.Serializable]
public class SpecificLootDrop
{
    [Tooltip("Item ID to drop (from ItemDatabase).")]
    public string itemID;
    [Tooltip("Chance of this specific item dropping (0-100).")]
    [Range(0f, 100f)] public float dropChancePercentage = 10f;
    [Tooltip("Minimum amount to drop if chance succeeds.")]
    [Min(1)] public int minAmount = 1;
    [Tooltip("Maximum amount to drop if chance succeeds.")]
    [Min(1)] public int maxAmount = 1;
}

// Parameters for randomly generated item drops.
[System.Serializable]
public class RandomDropSettings
{
    [Tooltip("Overall chance (0-100) to drop at least one randomly generated item based on these settings.")]
    [Range(0f, 100f)] public float generationChancePercentage = 25f;
    [Tooltip("Minimum number of random items to generate if chance succeeds.")]
    [Min(0)] public int minGeneratedItems = 0; // Can be 0 if generationChance applies per potential item
    [Tooltip("Maximum number of random items to generate if chance succeeds.")]
    [Min(1)] public int maxGeneratedItems = 1;

    // --- Optional Filters for Generated Items ---
    [Tooltip("Restrict generated item rarity? (Requires ItemGenerator support).")]
    public bool filterRarity = false;
    public Item.ItemRarity minRarity = Item.ItemRarity.Common;
    public Item.ItemRarity maxRarity = Item.ItemRarity.Legendary;

    [Tooltip("Restrict generated item type? (Requires ItemGenerator support).")]
    public bool filterItemType = false;
    public Item.ItemType requiredItemType = Item.ItemType.Miscellaneous; // e.g., only generate Weapons
}

// ScriptableObject Asset for Loot Tables
[CreateAssetMenu(fileName = "NewLootTable", menuName = "ScriptableObjects/Loot Table", order = 3)]
public class LootTable : ScriptableObject
{
    [Header("Specific Item Drops")]
    [Tooltip("List of specific predefined items and their individual drop chances.")]
    public List<SpecificLootDrop> specificDrops;

    [Header("Randomly Generated Item Drops")]
    [Tooltip("Settings governing the chance and type of randomly generated items.")]
    public RandomDropSettings randomDropSettings; // Use the nested settings class

    /// <summary>
    /// Evaluates the loot table based on drop chances to determine what should drop.
    /// Does NOT yet account for Magic Find.
    /// </summary>
    /// <param name="contextLevel">The level context (e.g., enemy level) for generation.</param>
    /// <param name="magicFind">Player's Magic Find value (e.g., 0-100+). Placeholder for future use.</param>
    /// <returns>A list of ItemDropResult detailing specific and generated item requests.</returns>
    public List<ItemDropResult> EvaluateDrops(int contextLevel, float magicFind = 0f)
    {
        List<ItemDropResult> results = new List<ItemDropResult>();

        // --- 1. Evaluate Specific Drops ---
        if (specificDrops != null)
        {
            foreach (SpecificLootDrop dropInfo in specificDrops)
            {
                // TODO: Modify chance based on Magic Find?
                // float chance = dropInfo.dropChancePercentage * CalculateMagicFindMultiplier(magicFind, "specific");
                float chance = dropInfo.dropChancePercentage;

                if (Random.Range(0f, 100f) <= chance)
                {
                    // TODO: Modify amount based on Magic Find? (Less common)
                    int amount = Random.Range(dropInfo.minAmount, Mathf.Max(dropInfo.minAmount, dropInfo.maxAmount) + 1); // Ensure max >= min
                    if (amount > 0)
                    {
                        results.Add(new ItemDropResult { isGenerated = false, itemID = dropInfo.itemID, amount = amount });
                    }
                }
            }
        }

        // --- 2. Evaluate Randomly Generated Drops ---
        if (randomDropSettings != null)
        {
            // TODO: Modify overall generation chance based on Magic Find?
            // float generationChance = randomDropSettings.generationChancePercentage * CalculateMagicFindMultiplier(magicFind, "generation");
             float generationChance = randomDropSettings.generationChancePercentage;

            if (Random.Range(0f, 100f) <= generationChance)
            {
                int count = Random.Range(randomDropSettings.minGeneratedItems, randomDropSettings.maxGeneratedItems + 1);
                for (int i = 0; i < count; i++)
                {
                    // Pass generation parameters (level, potentially MF-influenced rarity filter)
                    results.Add(new ItemDropResult
                    {
                        isGenerated = true,
                        generationLevel = contextLevel,
                        amount = 1, // Generated items typically drop one at a time
                        // Pass filter info if ItemGenerator uses it
                        useRarityFilter = randomDropSettings.filterRarity,
                        minGenRarity = randomDropSettings.minRarity,
                        maxGenRarity = randomDropSettings.maxRarity,
                        useItemTypeFilter = randomDropSettings.filterItemType,
                        requiredGenType = randomDropSettings.requiredItemType,
                        magicFindInfluence = magicFind // Pass MF for potential use in generation process
                    });
                }
            }
        }

        return results;
    }

     // --- Placeholder for Magic Find Calculation ---
     // private float CalculateMagicFindMultiplier(float magicFind, string type) {
     //    // Example: Diminishing returns. Higher MF gives smaller % increases.
     //    // return 1.0f + (magicFind / (magicFind + 100f)); // Adjust formula!
     //    return 1.0f; // No MF applied yet
     // }

} // End of LootTable class


// Helper struct for drop results, expanded for generated item parameters
public struct ItemDropResult
{
    public bool isGenerated;
    public string itemID; // Only if !isGenerated
    public int amount;
    public int generationLevel; // Only if isGenerated

    // --- Parameters for ItemGenerator (passed if isGenerated=true) ---
    public float magicFindInfluence; // Player's MF value
    public bool useRarityFilter;
    public Item.ItemRarity minGenRarity;
    public Item.ItemRarity maxGenRarity;
    public bool useItemTypeFilter;
    public Item.ItemType requiredGenType;
}