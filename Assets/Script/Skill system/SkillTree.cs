using System.Collections.Generic;
using UnityEngine; // Needed for Debug
using System; // Needed for [System.Serializable]

[System.Serializable] // Make sure this attribute is present
public class SkillTree
{
    // Store the skills available in this tree (main class or subclass)
    public List<Skill> skills;

    public SkillTree()
    {
        skills = new List<Skill>();
    }

    /// <summary>
    /// Adds a skill to the tree. Checks for duplicates based on name (optional).
    /// </summary>
    public void AddSkill(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("Attempted to add a null skill to the SkillTree.");
            return;
        }

        // Optional: Check if a skill with the same name already exists
        // bool exists = skills.Exists(s => s.name == skill.name);
        // if (!exists) {
        //    skills.Add(skill);
        // } else {
        //    Debug.LogWarning($"Skill '{skill.name}' already exists in this tree.");
        // }

        skills.Add(skill); // Simpler: Allow duplicates if needed, or handle elsewhere
    }

    /// <summary>
    /// Retrieves all skills currently in this skill tree.
    /// </summary>
    public List<Skill> GetAllSkills()
    {
        return skills;
    }

    /// <summary>
    /// Finds a skill by name within this tree.
    /// </summary>
    /// <param name="skillName">The name of the skill to find (case-sensitive).</param>
    /// <returns>The Skill object if found, otherwise null.</returns>
    public Skill GetSkillByName(string skillName)
    {
        return skills.Find(s => s.name == skillName);
    }
}