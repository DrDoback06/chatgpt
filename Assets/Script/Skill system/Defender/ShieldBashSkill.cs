using UnityEngine;

[System.Serializable]
public class ShieldBashSkill : Skill
{
    [Header("Shield Bash Settings")]
    public float baseDamage = 8f;
    public float damagePerLevel = 2f;
    public float stunDuration = 1.5f;
    public float stunDurationPerLevel = 0.2f;
    public float range = 1.0f; // Short range
    public float width = 1.2f;
    public string stunStatusEffectID = "Stun";

    public ShieldBashSkill() {
        skillID = "defender_shieldbash";
        skillName = "Shield Bash";
        description = "Slam your shield into a foe, dealing damage and stunning them briefly.";
        skillType = SkillType.Active;
        manaCost = 12f;
        cooldown = 6f;
        requiredLevel = 1;
        maxLevel = 5;
        requiredSkillPoints = 1;
        // this.icon = DefenderClass.shieldBashIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("ShieldBash");

        SpriteRenderer spriteRenderer = caster.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return false;

        // Hit Detection (Short, wide box)
        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackOrigin = (Vector2)caster.transform.position + (attackDirection * caster.attackOffset * 0.5f); // Closer origin?
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackOrigin + attackDirection * (range / 2f), new Vector2(range, width), 0f, caster.enemyLayer);

        // Calculate Damage (Scales with Str maybe?)
        float totalDamage = GetCurrentDamage(casterCharacter, baseDamage);
        totalDamage = Mathf.Max(1f, totalDamage);

        // Apply Effects
        int enemiesHit = 0;
        foreach (Collider2D hitCollider in hits) {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDefeated()) {
                enemiesHit++;
                caster.ApplyDamageToEnemy(enemy, totalDamage);

                // Apply Stun Status Effect
                StatusEffectController enemyStatus = enemy.GetComponent<StatusEffectController>();
                if (enemyStatus != null) {
                     float currentStunDuration = stunDuration + ((currentLevel - 1) * stunDurationPerLevel);
                     StatusEffect stunEffect = new StatusEffect(stunStatusEffectID, "Stunned", "Cannot act.", currentStunDuration, false, false, null, 0);
                     enemyStatus.ApplyStatusEffect(stunEffect);
                }
                 // Only hit the first enemy? Optional design choice.
                 // break;
            }
        }
        // Play bash sound

        return true;
    }

     public override float GetCurrentDamage(Character characterData, float baseSkillDamage) {
          float levelBonus = (currentLevel - 1) * damagePerLevel;
          float statBonus = characterData.attributes.Strength * 0.7f; // Good Str scaling for a bash
          float weaponDamage = 0f; // Bash likely doesn't use weapon damage directly
          return baseSkillDamage + levelBonus + statBonus + weaponDamage;
     }

     public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentBaseDmg = baseDamage + ((currentLevel-1) * damagePerLevel);
         float currentStun = stunDuration + ((currentLevel - 1) * stunDurationPerLevel);
         baseText += $"Base Damage: {currentBaseDmg:F0} (+ Scaling)\n";
         baseText += $"Stun Duration: {currentStun:F1}s\n";
         return baseText;
     }
}