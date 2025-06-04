using UnityEngine;

[System.Serializable]
public class ToughSkinSkill : Skill
{
    [Header("Tough Skin Settings")]
    public float baseArmorBonus = 5f;
    public float armorBonusPerLevel = 2.5f;

    public ToughSkinSkill() {
        skillID = "defender_toughskin";
        skillName = "Tough Skin";
        description = "Passively increases your Armor based on skill level.";
        skillType = SkillType.Passive; // << Mark as Passive
        manaCost = 0f; // No cost
        cooldown = 0f; // No cooldown
        requiredLevel = 5;
        maxLevel = 5;
        requiredSkillPoints = 1;
        // this.icon = DefenderClass.toughSkinIcon;
    }

    // Execute method for Passive skills:
    // Option A (Simple): Does nothing actively. Effect is checked by Character.GetArmorValue().
    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Passive skills usually don't have an 'Execute' action triggered by the player.
        // Their effect is constantly applied or checked by other systems.
        // Debug.Log($"{skillName} is passive, no execution action.");
        return true; // Return true indicating it's valid, but does nothing on demand.
    }

    /// <summary>
    /// Helper method to calculate the current bonus provided by this skill level.
    /// This would be called by Character.GetArmorValue().
    /// </summary>
    public float GetCurrentArmorBonus() {
         return baseArmorBonus + ((currentLevel - 1) * armorBonusPerLevel);
    }

     public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentBonus = GetCurrentArmorBonus();
         baseText += $"Armor Bonus: +{currentBonus:F0}\n"; // Show the passive bonus
         return baseText;
     }

     // No GetCurrentDamage override needed
}