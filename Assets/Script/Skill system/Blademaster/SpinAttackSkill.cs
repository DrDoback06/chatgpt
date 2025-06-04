using UnityEngine;

[System.Serializable]
public class SpinAttackSkill : Skill
{
    [Header("Spin Attack Settings")]
    public float baseDamage = 10f;
    public float damagePerLevel = 2f;
    public float radius = 2.0f; // Radius of the spin
    public float radiusPerLevel = 0.1f; // Radius increases slightly

    public SpinAttackSkill() {
        skillID = "blademaster_spin";
        skillName = "Spin Attack";
        description = "Spin around, damaging all nearby enemies.";
        skillType = SkillType.Active;
        manaCost = 20f;
        cooldown = 8.0f;
        requiredLevel = 5;
        maxLevel = 5;
        requiredSkillPoints = 2;
        // this.icon = BlademasterClass.spinAttackIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("SpinAttack");

        // 1. Calculate Current Radius
        float currentRadius = radius + ((currentLevel - 1) * radiusPerLevel);

        // 2. Hit Detection (Circle around caster)
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, currentRadius, caster.enemyLayer); // Use caster's position/layer

        // 3. Calculate Damage
        float totalDamage = GetCurrentDamage(casterCharacter, baseDamage); // Uses base helper method
        totalDamage = Mathf.Max(1f, totalDamage);

        // 4. Apply Effects
        int enemiesHit = 0;
        foreach (Collider2D hitCollider in hits) {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDefeated()) {
                enemiesHit++;
                caster.ApplyDamageToEnemy(enemy, totalDamage);
            }
        }

        // 5. Play Effects
        // Play Spin SFX/VFX

        return true;
    }

     public override float GetCurrentDamage(Character characterData, float baseSkillDamage) {
          float levelBonus = (currentLevel - 1) * damagePerLevel;
          float statBonus = characterData.attributes.Strength * 0.4f; // Spin might scale slightly less with Str
          float weaponDamage = characterData.GetWeaponDamage() * 0.7f; // Spin might use less weapon damage per hit

          return baseSkillDamage + levelBonus + statBonus + weaponDamage;
     }

     public override string GetStatsText() {
          string baseText = base.GetStatsText();
          float currentBaseDmg = baseDamage + ((currentLevel-1) * damagePerLevel);
          float currentRadius = radius + ((currentLevel - 1) * radiusPerLevel);
          baseText += $"Base Damage: {currentBaseDmg:F0} (+ Scaling)\n";
          baseText += $"Radius: {currentRadius:F1}\n";
          return baseText;
     }
}