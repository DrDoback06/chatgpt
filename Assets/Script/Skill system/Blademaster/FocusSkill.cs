using UnityEngine;

[System.Serializable]
public class FocusSkill : Skill
{
    [Header("Focus Settings")]
    public float baseDuration = 6f;
    public float durationPerLevel = 1.5f;
    public int baseAgilityBonus = 10;
    public int agilityBonusPerLevel = 3; // Increase bonus more per level
    public string hasteStatusEffectID = "Haste"; // Ensure this matches StatusEffect logic

    public FocusSkill() {
        skillID = "blademaster_focus";
        skillName = "Focus";
        description = "Temporarily increases Agility, boosting attack speed and other related stats.";
        // Originally Passive, but applying temporary buff fits Active better
        skillType = SkillType.Active;
        manaCost = 15f;
        cooldown = 30f; // Longer cooldown for a buff
        requiredLevel = 10;
        maxLevel = 5;
        requiredSkillPoints = 2;
        // this.icon = BlademasterClass.focusIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("FocusBuff");

        if (statusController != null)
        {
            float currentDuration = baseDuration + ((currentLevel - 1) * durationPerLevel);
            int currentAgilityBonus = baseAgilityBonus + ((currentLevel - 1) * agilityBonusPerLevel);

            // Create effect definition with value
            StatusEffect hasteEffect = new StatusEffect(
                hasteStatusEffectID, "Focus", $"Increases Agility by {currentAgilityBonus}.",
                currentDuration, true, false, icon, currentAgilityBonus // Pass calculated bonus
            );

            // Apply effect TO SELF
            statusController.ApplyStatusEffect(hasteEffect);
             // Play buff sound effect via AudioManager
            return true;
        } else { return false; } // Failed if no status controller
    }

    public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentDuration = baseDuration + ((currentLevel-1) * durationPerLevel);
         int currentBonus = baseAgilityBonus + ((currentLevel - 1) * agilityBonusPerLevel);
         baseText += $"Agility Bonus: +{currentBonus}\n";
         baseText += $"Duration: {currentDuration:F1}s\n";
         return baseText;
    }
}