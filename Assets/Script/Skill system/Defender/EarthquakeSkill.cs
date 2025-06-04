using UnityEngine;

[System.Serializable]
public class EarthquakeSkill : Skill
{
    [Header("Earthquake Settings")]
    public float baseDamage = 75f;
    public float damagePerLevel = 30f;
    public float radius = 4.0f;
    public float knockDownDuration = 2.0f; // Use Stun effect
    public string stunStatusEffectID = "Stun"; // Reuse Stun ID

    public EarthquakeSkill() {
        skillID = "defender_earthquake";
        skillName = "Earthquake";
        description = "Slam the ground, creating a shockwave that damages and knocks down nearby enemies.";
        skillType = SkillType.Ultimate;
        manaCost = 60f;
        cooldown = 120f; // Long ultimate cooldown
        requiredLevel = 20;
        maxLevel = 3;
        requiredSkillPoints = 5;
        // this.icon = DefenderClass.earthquakeIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing ULTIMATE: {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("Earthquake");
        // Play heavy slam sound/VFX

        // 1. Hit Detection (Large circle)
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius, caster.enemyLayer);

        // 2. Calculate Damage
        float totalDamage = GetCurrentDamage(casterCharacter, baseDamage);
        totalDamage = Mathf.Max(10f, totalDamage); // Ensure decent damage

        // 3. Apply Effects
        int enemiesHit = 0;
        foreach (Collider2D hitCollider in hits) {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDefeated()) {
                enemiesHit++;
                caster.ApplyDamageToEnemy(enemy, totalDamage); // Apply damage

                // Apply Knockdown/Stun
                StatusEffectController enemyStatus = enemy.GetComponent<StatusEffectController>();
                if (enemyStatus != null) {
                     // Knockdown might be same as Stun or a unique effect
                     StatusEffect knockDownEffect = new StatusEffect(stunStatusEffectID, "Knocked Down", "Cannot act.", knockDownDuration, false, false, null, 0);
                     enemyStatus.ApplyStatusEffect(knockDownEffect);
                }
            }
        }

        // Play ground shake effect?

        return true;
    }

     public override float GetCurrentDamage(Character characterData, float baseSkillDamage) {
         // Ultimates might scale strongly
         float levelBonus = (currentLevel - 1) * damagePerLevel;
         float statBonus = characterData.attributes.Strength * 1.0f; // Scale with Strength
         float weaponDamage = 0; // Maybe doesn't use weapon?
         return baseSkillDamage + levelBonus + statBonus + weaponDamage;
     }

     public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentBaseDmg = baseDamage + ((currentLevel-1) * damagePerLevel);
         baseText += $"Damage: {currentBaseDmg:F0} (+ Scaling)\n";
         baseText += $"Radius: {radius:F1}\n";
         baseText += $"Knockdown: {knockDownDuration:F1}s\n";
         return baseText;
     }
}