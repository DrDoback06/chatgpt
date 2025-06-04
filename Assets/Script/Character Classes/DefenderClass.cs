using System.Collections.Generic;
using UnityEngine;

// Changed to static class - CANNOT inherit from MonoBehaviour
public static class DefenderClass
{
    // Load icons or assign in Skill constructors/SOs
    public static Sprite shieldBashIcon = Resources.Load<Sprite>("SkillIcons/shieldBashIcon");
    public static Sprite toughSkinIcon = Resources.Load<Sprite>("SkillIcons/toughSkinIcon");
    public static Sprite shieldWallIcon = Resources.Load<Sprite>("SkillIcons/shieldWallIcon");
    public static Sprite resilienceIcon = Resources.Load<Sprite>("SkillIcons/resilienceIcon");
    public static Sprite earthquakeIcon = Resources.Load<Sprite>("SkillIcons/earthquakeIcon");

    public static Character CreateDefender(string characterName)
    {
        SkillTree defenderSkillTree = new SkillTree();

        // --- Instantiate SPECIFIC Skill Classes ---
        defenderSkillTree.AddSkill(new ShieldBashSkill() { icon = shieldBashIcon }); // Needs ShieldBashSkill.cs
        defenderSkillTree.AddSkill(new ToughSkinSkill() { icon = toughSkinIcon }); // Needs ToughSkinSkill.cs (Likely Passive)
        defenderSkillTree.AddSkill(new ShieldWallSkill() { icon = shieldWallIcon }); // Needs ShieldWallSkill.cs (Active Buff?)
        defenderSkillTree.AddSkill(new ResilienceSkill() { icon = resilienceIcon }); // Needs ResilienceSkill.cs (Passive?)
        defenderSkillTree.AddSkill(new EarthquakeSkill() { icon = earthquakeIcon }); // Needs EarthquakeSkill.cs (Ultimate?)

        CharacterAttributes defenderAttributes = new CharacterAttributes(10, 5, 5, 15); // Example base stats

        Character newChar = new Character(
            characterName,
            "Defender",
            "defender_sprite",
            defenderSkillTree,
            defenderAttributes
        );
        // newChar.inventory?.SetOwner(newChar); // Done in Character constructor

        return newChar;
    }
}