using UnityEngine;

[System.Serializable]
public class ParrySkill : Skill
{
    [Header("Parry Settings")]
    public float parryWindowDuration = 0.75f; // How long the parry state lasts
    public string parryStatusEffectID = "Parrying"; // Status effect ID

    public ParrySkill() {
        skillID = "blademaster_parry";
        skillName = "Parry";
        description = "Enter a defensive stance. If hit during this stance, block the attack and quickly counter-attack.";
        skillType = SkillType.Active; // It's an active stance
        manaCost = 10f;
        cooldown = 12f;
        requiredLevel = 15;
        maxLevel = 3; // Max level might increase duration or counter-attack damage
        requiredSkillPoints = 3;
        // this.icon = BlademasterClass.parryIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("ParryStance");

        if (statusController != null)
        {
            // Calculate duration based on level?
             float currentDuration = parryWindowDuration; // + (currentLevel - 1) * 0.1f; // Example scaling

            // Define the "Parrying" status effect
             // This effect itself might not modify stats directly, but signals the Character/PlayerController
             // to react differently if damage is taken while it's active.
            StatusEffect parryEffect = new StatusEffect(
                parryStatusEffectID, "Parrying", "Ready to parry incoming attack.",
                currentDuration, true, false, icon, 0 // Value might indicate counter-attack damage %?
            );

            // Apply effect TO SELF
            statusController.ApplyStatusEffect(parryEffect);
            Debug.Log($"Entered Parry stance for {currentDuration}s.");
            // Play stance sound/visual effect

            // --- IMPORTANT ---
            // The actual *blocking* and *counter-attacking* logic needs to be implemented elsewhere:
            // 1. In `Character.TakeDamage()`: Check `if (statusController.HasEffect("Parrying"))`. If true, negate damage and potentially trigger counter-attack via `caster.TriggerParryCounterAttack(damageSource)`.
            // 2. Add `TriggerParryCounterAttack` method to `PlayerController`.
            // 3. The `StatusEffect.Apply/Remove` for "Parrying" might not need to modify stats.

            return true;
        } else { return false; }
    }

     public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentDuration = parryWindowDuration; // + scaling
         baseText += $"Parry Window: {currentDuration:F2}s\n";
         // Add Counter-Attack damage info?
         return baseText;
     }
}