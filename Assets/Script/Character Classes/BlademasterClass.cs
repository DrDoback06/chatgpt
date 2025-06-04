using System.Collections.Generic;
using UnityEngine;

// Changed to static class - CANNOT inherit from MonoBehaviour
public static class BlademasterClass
{
    // Load Icons or assign them in Skill constructors/SOs
    // These Load calls only work if icons are in a "Resources/SkillIcons" folder
    public static Sprite slashIcon = Resources.Load<Sprite>("SkillIcons/slashIcon");
    public static Sprite spinAttackIcon = Resources.Load<Sprite>("SkillIcons/spinAttackIcon");
    public static Sprite focusIcon = Resources.Load<Sprite>("SkillIcons/focusIcon");
    public static Sprite parryIcon = Resources.Load<Sprite>("SkillIcons/parryIcon");
    public static Sprite ultimateBladeIcon = Resources.Load<Sprite>("SkillIcons/ultimateBladeIcon");

    public static Character CreateBlademaster(string characterName)
    {
        SkillTree blademasterSkillTree = new SkillTree();

        // Add instances of specific skill classes (assuming they exist)
        // Pass necessary parameters or set them after creation if needed
        blademasterSkillTree.AddSkill(new SlashSkill() { icon = slashIcon, skillName = "Slash" }); // Use constructor defaults or set properties
        blademasterSkillTree.AddSkill(new SpinAttackSkill() { icon = spinAttackIcon, skillName = "Spin Attack" }); // Example: Need SpinAttackSkill.cs
        blademasterSkillTree.AddSkill(new FocusSkill() { icon = focusIcon, skillName = "Focus" });
        blademasterSkillTree.AddSkill(new ParrySkill() { icon = parryIcon, skillName = "Parry" }); // Example: Need ParrySkill.cs
        blademasterSkillTree.AddSkill(new UltimateBladeSkill() { icon = ultimateBladeIcon, skillName = "Ultimate Blade" }); // Example: Need UltimateBladeSkill.cs


        CharacterAttributes blademasterAttributes = new CharacterAttributes(12, 10, 5, 8); // Example base stats

        Character newChar = new Character(
            characterName,
            "Blademaster",
            "blademaster_sprite", // Sprite identifier
            blademasterSkillTree,
            blademasterAttributes
            // Optional: initialInventorySize
        );
        // Ensure inventory owner is set right after creation
        // newChar.inventory?.SetOwner(newChar); // Already done inside Character constructor now

        return newChar;
    }
}

// TODO: Create the missing specific skill classes like SpinAttackSkill, ParrySkill, UltimateBladeSkill, etc. inheriting from Skill.