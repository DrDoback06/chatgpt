using System; // Needed for [System.Serializable]
using System.Collections.Generic;
using UnityEngine; // Still needed for Debug and potentially ScriptableObject later

// Represents the definition and current state of a single quest.
// Changed from MonoBehaviour to a standard class for data holding.
[System.Serializable]
public class Quest
{
    // --- Static Data (Quest Definition) ---
    [Tooltip("Unique identifier for this quest (e.g., 'main_quest_01', 'kill_10_boars').")]
    public string questID;
    [Tooltip("Display name of the quest shown in the UI.")]
    public string questName;
    [Tooltip("Detailed description of the quest's story or purpose.")]
    [TextArea] public string questDescription;
    [Tooltip("List of objectives required to complete the quest.")]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    [Tooltip("Rewards given upon completion (e.g., XP amount, Gold amount, Item ID string).")]
    public List<QuestReward> questRewards = new List<QuestReward>();
    [Tooltip("Minimum player level suggested or required to start this quest.")]
    public int requiredLevel = 1;
    // Optional: Prerequisite Quest ID? Chain quests together.
    // public string prerequisiteQuestID;

    // --- Dynamic Data (Quest State) ---
    [Tooltip("Current status of the quest for the player.")]
    public QuestStatus currentStatus = QuestStatus.NotStarted;

    // Keep track of which objectives are complete
    // Serializing List<bool> is fine with JsonUtility
    private List<bool> objectiveCompletionStatus = new List<bool>();

    // --- Enums ---
    public enum QuestStatus
    {
        NotStarted,   // Quest is available but not accepted by the player.
        InProgress,   // Player has accepted the quest.
        RequirementsMet, // All objectives complete, ready to turn in (if applicable).
        Completed,    // Quest objectives met and turned in / rewards claimed.
        Failed        // Quest can no longer be completed (optional).
    }

    // --- Constructor ---
    // Default constructor needed for serialization if others are added.
    public Quest() { }

    // Optional: Constructor for easier creation in code (e.g., for Scriptable Objects)
    public Quest(string id, string name, string desc, int reqLevel = 1)
    {
        this.questID = id;
        this.questName = name;
        this.questDescription = desc;
        this.requiredLevel = reqLevel;
        this.currentStatus = QuestStatus.NotStarted;
        InitializeObjectiveStatus();
    }


    // --- Methods ---

    /// <summary>
    /// Initializes the completion status list based on the number of objectives.
    /// Should be called when the quest is first created or loaded if status list is empty.
    /// </summary>
    private void InitializeObjectiveStatus()
    {
        // Only initialize if the list doesn't match the objective count (e.g., on load or first creation)
        if (objectiveCompletionStatus == null || objectiveCompletionStatus.Count != objectives.Count)
        {
             objectiveCompletionStatus = new List<bool>(new bool[objectives.Count]);
        }
    }

    /// <summary>
    /// Marks the quest as started by the player.
    /// </summary>
    /// <returns>True if the quest status was successfully changed to InProgress, false otherwise.</returns>
    public bool StartQuest()
    {
        if (currentStatus == QuestStatus.NotStarted)
        {
            currentStatus = QuestStatus.InProgress;
            InitializeObjectiveStatus(); // Ensure status list is ready
            Debug.Log($"Quest '{questName}' ({questID}) started.");
            return true;
        }
        Debug.LogWarning($"Cannot start quest '{questName}' ({questID}) - current status is {currentStatus}.");
        return false;
    }

    /// <summary>
    /// Attempts to mark a specific objective as complete by its index.
    /// Checks if the overall quest requirements are now met.
    /// </summary>
    /// <param name="objectiveIndex">The zero-based index of the objective to complete.</param>
    /// <returns>True if the objective was successfully marked complete, false otherwise.</returns>
    public bool CompleteObjective(int objectiveIndex)
    {
        InitializeObjectiveStatus(); // Ensure list exists, crucial if loaded without it

        if (currentStatus != QuestStatus.InProgress)
        {
            Debug.LogWarning($"Cannot complete objective for quest '{questName}' - Quest not InProgress (Status: {currentStatus}).");
            return false;
        }

        if (objectiveIndex >= 0 && objectiveIndex < objectiveCompletionStatus.Count)
        {
            if (!objectiveCompletionStatus[objectiveIndex]) // Only mark if not already complete
            {
                objectiveCompletionStatus[objectiveIndex] = true;
                Debug.Log($"Quest '{questName}': Objective {objectiveIndex} ('{objectives[objectiveIndex].description}') completed.");
                CheckOverallCompletion(); // Check if quest is now fully complete
                return true;
            }
            else
            {
                 Debug.Log($"Quest '{questName}': Objective {objectiveIndex} was already complete.");
                 return false; // Wasn't newly completed
            }
        }
        else
        {
            Debug.LogError($"Invalid objective index {objectiveIndex} for quest '{questName}'.");
            return false;
        }
    }

     /// <summary>
     /// Checks if all objectives are complete and updates the quest status accordingly.
     /// </summary>
    private void CheckOverallCompletion()
    {
        if (currentStatus != QuestStatus.InProgress) return; // Only check if currently active

        InitializeObjectiveStatus();

        bool allComplete = true;
        foreach (bool status in objectiveCompletionStatus)
        {
            if (!status)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            currentStatus = QuestStatus.RequirementsMet;
            Debug.Log($"Quest '{questName}' ({questID}) - All requirements met. Ready for turn-in.");
            // Optional: Trigger an event via QuestManager if needed
            // QuestManager.Instance?.NotifyQuestRequirementsMet(this.questID);
        }
    }


    /// <summary>
    /// Marks the quest as fully completed (usually after turn-in).
    /// </summary>
    /// <returns>True if status changed to Completed, false otherwise.</returns>
    public bool CompleteQuest()
    {
        if (currentStatus == QuestStatus.RequirementsMet)
        {
            currentStatus = QuestStatus.Completed;
            Debug.Log($"Quest '{questName}' ({questID}) officially completed.");
            // Rewards should be granted by the QuestManager or NPC upon turn-in
            return true;
        }
        else if (currentStatus == QuestStatus.InProgress) {
            // Allow completing quests with no objectives directly? Or force RequirementsMet first?
            // Let's assume objectives must be met first for most quests.
            Debug.LogWarning($"Cannot complete quest '{questName}' - Requirements not yet met (Status: {currentStatus}). Call CompleteObjective first.");
            return false;
        }
         else {
            Debug.LogWarning($"Cannot complete quest '{questName}' - Current status is {currentStatus}.");
            return false;
         }
    }

    /// <summary>
    /// Gets the completion status of a specific objective.
    /// </summary>
    /// <param name="objectiveIndex">The index of the objective.</param>
    /// <returns>True if complete, false otherwise.</returns>
    public bool IsObjectiveComplete(int objectiveIndex)
    {
         InitializeObjectiveStatus();
        if (objectiveIndex >= 0 && objectiveIndex < objectiveCompletionStatus.Count)
        {
            return objectiveCompletionStatus[objectiveIndex];
        }
        return false;
    }

    // Note: Claiming rewards is typically handled externally by the system
    // that calls CompleteQuest (e.g., NPC dialogue, Quest Manager).
    // public void ClaimRewards() { ... } // Removed - Logic belongs elsewhere.

}

// --- Helper Structures (Define within Quest or separately) ---

[System.Serializable]
public class QuestObjective
{
    [Tooltip("Description shown in the Quest Log (e.g., 'Kill 10 Slimes', 'Collect 5 Herbs', 'Speak to Guard Captain').")]
    public string description;
    [Tooltip("Type of objective (helps determine how progress is tracked).")]
    public ObjectiveType type = ObjectiveType.Other;
    [Tooltip("Target identifier (e.g., 'enemy_slime_id', 'item_herb_id', 'npc_guard_captain_id').")]
    public string targetID;
    [Tooltip("Amount required (e.g., 10 for kills, 5 for collection).")]
    public int requiredAmount = 1;

    // Optional runtime progress tracking (may be better managed in QuestManager)
    // [NonSerialized] public int currentAmount = 0;

    public enum ObjectiveType
    {
        Kill,        // Defeat specific enemies
        Collect,     // Gather items
        Reach,       // Go to a specific location/trigger
        Interact,    // Talk to an NPC or interact with an object
        Other        // Custom objective type
    }
}

[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public int amount;       // e.g., XP amount, Gold amount
    public string itemID;   // ID of item reward (if type is Item)

    public enum RewardType
    {
        Experience,
        Gold,
        Item,
        SkillPoint, // Added reward types
        AttributePoint
    }
}