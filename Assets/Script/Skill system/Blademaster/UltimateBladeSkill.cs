using UnityEngine;

[System.Serializable]
public class UltimateBladeSkill : Skill
{
    [Header("Ultimate Settings")]
    public float baseDamage = 100f; // High base damage
    public float damagePerLevel = 50f;
    public float range = 3f; // Longer range line attack?
    public float width = 1.5f;

    public UltimateBladeSkill() {
        skillID = "blademaster_ultimate";
        skillName = "Ultimate Blade";
        description = "Unleash a devastating sword attack in front of you.";
        skillType = SkillType.Ultimate; // Mark as Ultimate
        manaCost = 50f; // High cost
        cooldown = 60f; // Long cooldown
        requiredLevel = 20;
        maxLevel = 3;
        requiredSkillPoints = 5;
        // this.icon = BlademasterClass.ultimateBladeIcon;
    }

    public override bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController)
    {
        // Debug.Log($"Executing ULTIMATE: {skillName} (Lvl {currentLevel})");
        // animator?.SetTrigger("UltimateAttack");
        // Play charging/ultimate sound/VFX

        SpriteRenderer spriteRenderer = caster.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return false;

        // Hit Detection (e.g., larger Box or multiple slashes)
        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackOrigin = (Vector2)caster.transform.position + (attackDirection * caster.attackOffset);
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackOrigin + attackDirection * (range / 2f), new Vector2(range, width), 0f, caster.enemyLayer);
        // Debug.DrawLine(attackOrigin, attackOrigin + attackDirection * range, Color.magenta, 1.0f);

        // Calculate Damage (High scaling)
        float totalDamage = GetCurrentDamage(casterCharacter, baseDamage);
        totalDamage = Mathf.Max(10f, totalDamage); // Ensure high minimum damage

        // Apply Effects
        int enemiesHit = 0;
        foreach (Collider2D hitCollider in hits) {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDefeated()) {
                enemiesHit++;
                caster.ApplyDamageToEnemy(enemy, totalDamage); // Apply damage (crit possible)
                // Add knockback? Status Effect?
            }
        }

        // Play impact sounds/VFX

        return true;
    }

    public override float GetCurrentDamage(Character characterData, float baseSkillDamage) {
        float levelBonus = (currentLevel - 1) * damagePerLevel;
        float statBonus = characterData.attributes.Strength * 1.0f; // Ultimate scales strongly with Str?
        float weaponDamage = characterData.GetWeaponDamage() * 1.5f; // Uses weapon damage heavily?

        return baseSkillDamage + levelBonus + statBonus + weaponDamage;
    }

     public override string GetStatsText() {
         string baseText = base.GetStatsText();
         float currentBaseDmg = baseDamage + ((currentLevel-1) * damagePerLevel);
         baseText += $"Base Damage: {currentBaseDmg:F0} (+ Scaling)\n";
         return baseText;
     }
}