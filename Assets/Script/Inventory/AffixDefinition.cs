using UnityEngine;
using System.Collections.Generic; // For List

// Defines a single type of affix (e.g., "+Strength") and how its values roll.
[CreateAssetMenu(fileName = "Affix_", menuName = "Items/Affix Definition", order = 11)]
public class AffixDefinition : ScriptableObject
{
    [Tooltip("Unique identifier for this affix type (e.g., 'str_mod', 'ias_percent').")]
    public string affixID;

    [Tooltip("The type of stat this affix modifies.")]
    public ItemAffix.AffixType affixType = ItemAffix.AffixType.Strength; // Default

    [Header("Naming")]
    [Tooltip("Potential prefixes associated with this affix (e.g., 'Strong', 'Mighty').")]
    public List<AffixNameTier> prefixes;
    [Tooltip("Potential suffixes associated with this affix (e.g., 'of Strength', 'of Power').")]
    public List<AffixNameTier> suffixes;
    public bool IsPrefix => prefixes != null && prefixes.Count > 0; // Convenience check

    [Header("Value Rolling")]
    [Tooltip("Value ranges based on Item Level (or Affix Tier). Order by Min Level ascending.")]
    public List<AffixValueTier> valueTiers;

    /// <summary>
    /// Rolls a value for this affix based on the item's level.
    /// </summary>
    /// <param name="itemLevel">The level of the item this affix is rolling on.</param>
    /// <returns>The rolled affix value, or 0 if no suitable tier found.</returns>
    public float RollValue(int itemLevel)
    {
        AffixValueTier suitableTier = null;
        if (valueTiers != null) {
            // Find the highest tier the item level qualifies for
            for (int i = valueTiers.Count - 1; i >= 0; i--) {
                if (itemLevel >= valueTiers[i].minItemLevel) {
                    suitableTier = valueTiers[i];
                    break;
                }
            }
        }

        if (suitableTier != null) {
            // Roll within the tier's range
            return Random.Range(suitableTier.minValue, suitableTier.maxValue);
        } else {
            // Debug.LogWarning($"Affix '{affixID}' has no suitable value tier for item level {itemLevel}.");
            return 0; // Default to 0 if no tier matches
        }
    }

    /// <summary>
    /// Gets an appropriate prefix or suffix word based on the rolled value.
    /// </summary>
    public string GetNameWord(float rolledValue, bool getPrefix) {
        List<AffixNameTier> nameList = getPrefix ? prefixes : suffixes;
        if (nameList == null || nameList.Count == 0) return "";

        // Find the best matching name tier for the rolled value (highest tier <= value)
        string nameWord = ""; // Default empty
         for (int i = nameList.Count - 1; i >= 0; i--) {
             if (rolledValue >= nameList[i].minValueThreshold) {
                 nameWord = nameList[i].nameWord;
                 break;
             }
         }
         return nameWord;
    }
}

// Helper class for defining value ranges per item level bracket
[System.Serializable]
public class AffixValueTier {
    [Tooltip("Minimum item level required for this tier to be possible.")]
    public int minItemLevel = 1;
    [Tooltip("Minimum value rolled within this tier.")]
    public float minValue = 1;
    [Tooltip("Maximum value rolled within this tier.")]
    public float maxValue = 2;
}

// Helper class for associating names with value thresholds
[System.Serializable]
public class AffixNameTier {
     [Tooltip("The prefix or suffix word (e.g., 'Strong', 'of Power').")]
     public string nameWord;
     [Tooltip("Minimum rolled value required to use this name word.")]
     public float minValueThreshold = 1;
}