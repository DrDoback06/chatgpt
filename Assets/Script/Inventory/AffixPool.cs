using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for Linq Sum/Where

// Defines a weighted pool of possible affixes for a category of items.
[CreateAssetMenu(fileName = "AffixPool_", menuName = "Items/Affix Pool", order = 12)]
public class AffixPool : ScriptableObject
{
    [Tooltip("Identifier for this pool (e.g., 'Armor_Pool', 'Weapon_Pool').")]
    public string poolID;

    [Tooltip("List of possible affixes and their relative chance (weight) to be chosen.")]
    public List<WeightedAffixEntry> possibleAffixes;

    /// <summary>
    /// Selects a random AffixDefinition from this pool based on weights and item level.
    /// Returns null if no suitable affix can be selected.
    /// </summary>
    public AffixDefinition GetRandomAffixDefinition(int itemLevel)
    {
        if (possibleAffixes == null || possibleAffixes.Count == 0) return null;

        // Filter affixes based on itemLevel (check if affix has *any* tier applicable for this level)
        var eligibleAffixes = possibleAffixes.Where(entry =>
            entry.affixDefinition != null &&
            entry.affixDefinition.valueTiers != null &&
            entry.affixDefinition.valueTiers.Any(tier => itemLevel >= tier.minItemLevel)
        ).ToList();

        if (eligibleAffixes.Count == 0) return null; // No affixes possible at this level

        // Calculate total weight of eligible affixes
        float totalWeight = eligibleAffixes.Sum(entry => entry.weight);
        if (totalWeight <= 0) return null; // No valid weights

        // Roll weighted random
        float randomRoll = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        foreach (var entry in eligibleAffixes)
        {
            currentWeightSum += entry.weight;
            if (randomRoll <= currentWeightSum)
            {
                return entry.affixDefinition; // Found our affix
            }
        }

        // Fallback (shouldn't happen if totalWeight > 0)
        return eligibleAffixes.LastOrDefault()?.affixDefinition;
    }
}

// Helper class for assigning weights to affix definitions within a pool
[System.Serializable]
public class WeightedAffixEntry {
    [Tooltip("Reference to the Affix Definition ScriptableObject.")]
    public AffixDefinition affixDefinition;
    [Tooltip("Relative chance/weight. Higher values are more likely.")]
    [Min(0.1f)] public float weight = 1.0f;
}