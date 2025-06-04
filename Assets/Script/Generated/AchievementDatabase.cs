using UnityEngine;
using System.Collections.Generic; // Needed for List<>

// Defines the ScriptableObject asset that holds all achievement definitions.
[CreateAssetMenu(fileName = "AchievementDatabase", menuName = "ScriptableObjects/Achievement Database", order = 5)]
public class AchievementDatabase : ScriptableObject
{
    [Tooltip("List of all base achievement definitions.")]
    public List<Achievement> allAchievements; // Assign Achievement assets/definitions here

    // Optional: Add helper method to find by ID if needed often
    public Achievement GetAchievementByID(string id) {
        return allAchievements?.Find(ach => ach != null && ach.AchievementID == id);
    }
}