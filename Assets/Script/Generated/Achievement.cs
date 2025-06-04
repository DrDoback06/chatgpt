using System;
using UnityEngine;
// Removed UnityEngine.Events as UnityEvents aren't easily serializable with standard methods
// Actions/callbacks would be handled by the AchievementManager instead.

[System.Serializable] // Make serializable for saving/manager use
public class Achievement
{
    [Tooltip("Unique identifier for the achievement (e.g., 'kill_100_demons', 'first_quest_complete').")]
    [SerializeField] private string achievementID; // Use property for controlled access

    [Tooltip("Name displayed in the UI.")]
    [SerializeField] private string achievementName;

    [Tooltip("Description shown in the UI.")]
    [TextArea] [SerializeField] private string achievementDescription;

    [Tooltip("Description of how to unlock it (for player reference).")]
    [SerializeField] private string unlockConditionsDescription; // Renamed from unlockConditions

    // --- Runtime State ---
    private bool isUnlocked;
    // Consider adding unlock timestamp: public DateTime? unlockTimestamp;

    // --- Properties ---
    public string AchievementID => achievementID;
    public string AchievementName => achievementName;
    public string AchievementDescription => achievementDescription;
    public string UnlockConditionsDescription => unlockConditionsDescription;
    public bool IsUnlocked { get => isUnlocked; internal set => isUnlocked = value; } // Allow manager to set

    // --- Constructor ---
    // Default constructor for serialization
    public Achievement() { }

    // Constructor for potential creation in code/database
    public Achievement(string id, string name, string desc, string unlockDesc)
    {
        this.achievementID = id;
        this.achievementName = name;
        this.achievementDescription = desc;
        this.unlockConditionsDescription = unlockDesc;
        this.isUnlocked = false;
    }

    // Note: Removed CheckUnlockConditions and UnlockAchievement methods.
    // The logic for checking conditions and unlocking belongs in a dedicated
    // AchievementManager, which listens to game events and updates the state
    // of specific achievement instances.
    // The UnityEvent was also removed as it complicates serialization and is better
    // handled by the manager invoking its own events upon unlock.
}

// --- Next Steps ---
// 1. Create an AchievementDatabase ScriptableObject (similar to QuestDatabase)
//    - Holds a list of all base Achievement definitions.
// 2. Create an AchievementManager MonoBehaviour (Singleton):
//    - Holds player's achievement state (e.g., Dictionary<string, Achievement>).
//    - References the AchievementDatabase.
//    - Subscribes to game events (enemy killed, quest completed, item looted, etc.).
//    - Contains the logic in `CheckUnlockConditions` for each achievement type based on events.
//    - Calls `achievementInstance.IsUnlocked = true` when conditions are met.
//    - Fires its own event like `OnAchievementUnlocked(Achievement achievement)`.
//    - Handles saving/loading of the player's achievement state.