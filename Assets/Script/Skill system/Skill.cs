using UnityEngine;
using System;
using System.Collections.Generic; // If Execute needs parameters

[System.Serializable]
public abstract class Skill // Changed to abstract class
{
    [Header("Core Info")]
    public string skillID; // Use a unique ID instead of relying on Name for logic
    public string skillName;
    public string name => skillName; // Add this line for backward compatibility
    [TextArea] public string description;
    public SkillType skillType;
    public Sprite icon;

    [Header("Progression")]
    public int currentLevel = 1;
    public int? maxLevel; // Nullable for no max
    public int requiredSkillPoints = 1;
    public int requiredLevel = 1;

    [Header("Gameplay")]
    public float cooldown = 0f;
    public float manaCost = 5f; // Base mana cost (can scale)

    // Public properties (read-only access)
    public bool IsUpgradable => CanLevelUp();

    // --- Abstract Execution Method ---
    /// <summary>
    /// Executes the primary effect of this skill.
    /// Must be implemented by derived skill classes.
    /// </summary>
    /// <param name="caster">The PlayerController performing the skill.</param>
    /// <param name="casterCharacter">The Character data of the caster.</param>
    /// <param name="statusController">The StatusEffectController of the caster.</param>
    /// <returns>True if execution was successful (passed checks), false otherwise.</returns>
    public abstract bool Execute(PlayerController caster, Character casterCharacter, StatusEffectController statusController);

    // --- Common Helper Methods ---
    public bool CanLevelUp() => !maxLevel.HasValue || currentLevel < maxLevel.Value;

    public bool Upgrade()
    {
        if (CanLevelUp()) { currentLevel++; return true; }
        return false;
    }

    // Optional: Method to calculate scaled cost/damage based on level
    public virtual float GetCurrentManaCost() => manaCost + ((currentLevel - 1) * 1.5f); // Example scaling
    public virtual float GetCurrentDamage(Character characterData, float baseSkillDamage) {
         // Example: Base + Level + Stat Scaling + Weapon
         float levelBonus = (currentLevel - 1) * (baseSkillDamage * 0.2f); // +20% base damage per level past 1
         float statBonus = characterData.attributes.Strength * 0.5f; // Example scaling
         float weaponDamage = characterData.GetWeaponDamage() * 0.8f; // Skill might use weapon differently
         return baseSkillDamage + levelBonus + statBonus + weaponDamage;
    }
     // Add GetCurrentDuration, GetCurrentBuffValue etc. as needed

    public virtual string GetStatsText()
    {
        string txt = $"Level: {currentLevel} / {(maxLevel.HasValue ? maxLevel.Value.ToString() : "∞")}\n";
        txt += $"Type: {skillType}\n";
        txt += $"Req. Lvl: {requiredLevel}\n";
        txt += $"Cost: {GetCurrentManaCost():F0} MP\n"; // Show current cost
        if (cooldown > 0) txt += $"Cooldown: {cooldown:F1}s\n";
        // Derived classes should override this to add specific stats (Damage, Duration etc.)
        return txt;
    }

    // Keep enum definition accessible
    public enum SkillType { Active, Passive, Ultimate }
}