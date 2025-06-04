using UnityEngine;

[System.Serializable]
public class ResilienceSkill : Skill
{
    [Header("Resilience Settings")]
    public float baseHPBonus = 20f;
    public float hpBonusPerLevel = 10f;

    public ResilienceSkill() {
        skillID = "defender_resilience";
        skillName = "Resilience";
        description = "Passively increases your maximum Health.";
        skillType = SkillType.Passive;
        manaCost = 0f;
        cooldown = 0f;
        requiredLevel = 15;
        maxLevel = 5;
        requiredSkillPoints = 2;
        // this.icon = DefenderClass.resilienceIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController) {
        // Passive - does nothing on execution
        return true;
    }

    public float GetCurrentHPBonus() {
        return baseHPBonus + ((currentLevel - 1) * hpBonusPerLevel);
    }

    public override string GetStatsText() {
        string baseText = base.GetStatsText();
        float currentBonus = GetCurrentHPBonus();
        baseText += $"Max Health Bonus: +{currentBonus:F0}\n";
        return baseText;
    }
}