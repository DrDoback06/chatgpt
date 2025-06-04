using UnityEngine;

[System.Serializable]
public class SlashSkill : Skill // Inherit from Skill
{
    // Skill-Specific Parameters
    [Header("Slash Settings")]
    public float baseDamage = 15f;
    public float damagePerLevel = 3f;
    public float rangeMultiplier = 1.2f;
    public float widthMultiplier = 1.5f;

    // Constructor
    public SlashSkill() {
        skillID = "blademaster_slash";
        skillName = "Slash";
        description = "A quick sword slash hitting enemies directly in front.";
        skillType = SkillType.Active;
        manaCost = 5f;
        cooldown = 1.0f;
        requiredLevel = 1;
        maxLevel = 5;
        requiredSkillPoints = 1;
        // Assign icon in BlademasterClass factory: this.icon = BlademasterClass.slashIcon;
    }

    // Implement Execute Method
    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("SlashEffect"); // Access animator via caster if needed

        SpriteRenderer spriteRenderer = caster.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return false;

        // Perform Hit Detection
        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackOrigin = (Vector2)caster.transform.position + (attackDirection * caster.attackOffset);
        float slashRange = caster.attackRange * rangeMultiplier;
        float slashWidth = caster.attackRange * widthMultiplier;
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackOrigin + attackDirection * (slashRange / 2f), new Vector2(slashRange, slashWidth), 0f, caster.enemyLayer);
        // Debug.DrawLine(attackOrigin, attackOrigin + attackDirection * slashRange, Color.cyan, 0.6f);

        // Calculate Damage (using base helper method)
        float totalDamage = GetCurrentDamage(casterCharacter, baseDamage);
        totalDamage = Mathf.Max(1f, totalDamage);

        // Apply Effects to Targets
        int enemiesHit = 0;
        foreach (Collider2D hitCollider in hits) {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDefeated()) {
                enemiesHit++;
                caster.ApplyDamageToEnemy(enemy, totalDamage); // Use caster's helper for crit etc.
            }
        }

        // Play Effects
        if (enemiesHit > 0) { /* Play hit sound via AudioManager.Instance */ } else { /* Play miss sound */ }
        return true; // Execution successful
    }

     // Override damage calculation if needed (example shows more per level scaling)
     public override float GetCurrentDamage(Character characterData, float baseSkillDamage) {
          float levelBonus = (currentLevel - 1) * damagePerLevel; // Use skill-specific scaling
          float statBonus = characterData.attributes.Strength * 0.6f; // Slash might scale slightly better with Str?
          float weaponDamage = characterData.GetWeaponDamage(); // Use full weapon damage

          return baseSkillDamage + levelBonus + statBonus + weaponDamage;
     }

     // Override GetStatsText to add specific info
     public override string GetStatsText() {
          string baseText = base.GetStatsText();
          float currentBaseDmg = baseDamage + ((currentLevel-1) * damagePerLevel);
          baseText += $"Base Damage: {currentBaseDmg:F0} (+ Scaling)\n";
          return baseText;
     }
}