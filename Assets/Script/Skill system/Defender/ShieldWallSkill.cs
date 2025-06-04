using UnityEngine;

[System.Serializable]
public class ShieldWallSkill : Skill
{
    [Header("Shield Wall Settings")]
    public float baseDuration = 4f;
    public float durationPerLevel = 0.5f;
    // Example: Reduces damage taken by a percentage
    public float damageReductionPercent = 50f; // 50% reduction
    public float reductionPerLevel = 5f; // +5% per level
    public string shieldWallStatusEffectID = "ShieldWallActive";

    public ShieldWallSkill() {
        skillID = "defender_shieldwall";
        skillName = "Shield Wall";
        description = "Raise your shield, significantly reducing incoming damage for a short time.";
        skillType = SkillType.Active;
        manaCost = 25f;
        cooldown = 45f;
        requiredLevel = 10;
        maxLevel = 5;
        requiredSkillPoints = 2;
        // this.icon = DefenderClass.shieldWallIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("ShieldWall");

        if (statusController != null) {
            float currentDuration = baseDuration + ((currentLevel - 1) * durationPerLevel);
            float currentReduction = damageReductionPercent + ((currentLevel - 1) * reductionPerLevel);

            // Apply a status effect. The TakeDamage logic will check for this effect.
            StatusEffect wallEffect = new StatusEffect(
                shieldWallStatusEffectID, "Shield Wall", $"Reducing incoming damage by {currentReduction}%.",
                currentDuration, true, false, icon, currentReduction // Store reduction % in value
            );
            statusController.ApplyStatusEffect(wallEffect);
            // Play sound/VFX
            return true;
        }
        return false;
    }

     public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentDuration = baseDuration + ((currentLevel - 1) * durationPerLevel);
         float currentReduction = damageReductionPercent + ((currentLevel - 1) * reductionPerLevel);
         baseText += $"Damage Reduction: {currentReduction:F0}%\n";
         baseText += $"Duration: {currentDuration:F1}s\n";
         return baseText;
     }
}