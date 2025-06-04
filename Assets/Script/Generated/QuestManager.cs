using System;
using System.Collections.Generic;
using System.Linq; // Needed for searching lists
using UnityEngine;

// Manages the player's quests, tracking active and completed quests.
// Should persist across scenes if quest progress needs to be maintained.
public class QuestManager : MonoBehaviour
{
    // Singleton Instantiation (Optional but common for managers)
    public static QuestManager Instance;

    [Header("Quest Database (Reference)")]
    [Tooltip("Reference to a ScriptableObject or prefab containing definitions of ALL available quests.")]
    [SerializeField] private QuestDatabase questDatabase; // NEEDS TO BE CREATED - see below

    // Player's Quest State (This needs saving/loading)
    // Store the full Quest object now, not just names. Key: QuestID, Value: Quest object instance.
    private Dictionary<string, Quest> playerQuests = new Dictionary<string, Quest>();

    // Public accessors
    public QuestDatabase GetQuestDatabase() => questDatabase;
    public Dictionary<string, Quest> GetPlayerQuests() => playerQuests;

    // --- Events ---
    /// <summary>
    /// Fired when a quest is added to the player's active list. Passes the Quest object.
    /// </summary>
    public event Action<Quest> OnQuestAccepted;
    /// <summary>
    /// Fired when an objective within an active quest progresses. Passes Quest + Objective Index.
    /// </summary>
    public event Action<Quest, int> OnObjectiveCompleted;
     /// <summary>
    /// Fired when a quest's status changes to RequirementsMet. Passes the Quest object.
    /// </summary>
    public event Action<Quest> OnQuestRequirementsMet;
    /// <summary>
    /// Fired when a quest is fully completed (turned in). Passes the Quest object.
    /// </summary>
    public event Action<Quest> OnQuestCompleted;


    // --- Unity Lifecycle ---
    private void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
             // Decide if this manager needs to persist across scenes
             // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // TODO: Load Player Quest Data here
        LoadPlayerQuests();
    }

    // --- Quest Management ---

    /// <summary>
    /// Starts a quest for the player if available and requirements are met.
    /// </summary>
    /// <param name="questID">The unique ID of the quest to start.</param>
    /// <returns>True if the quest was successfully accepted, false otherwise.</returns>
    public bool AcceptQuest(string questID)
    {
        if (questDatabase == null) {
             Debug.LogError("QuestManager: Quest Database not assigned!");
             return false;
        }

        // Check if player already has this quest
        if (playerQuests.ContainsKey(questID))
        {
             Debug.LogWarning($"Player already has quest '{questID}' (Status: {playerQuests[questID].currentStatus}). Cannot accept again.");
            return false;
        }

        // Find the quest definition in the database
        Quest questPrefab = questDatabase.GetQuestByID(questID);
        if (questPrefab == null)
        {
            Debug.LogError($"Quest '{questID}' not found in the database.");
            return false;
        }

        // Check requirements (e.g., player level)
        Character character = CharacterManager.Instance?.character;
        if (character == null) {
             Debug.LogError("Cannot accept quest - Player character data not found.");
             return false;
        }
        if (character.Level < questPrefab.requiredLevel)
        {
             Debug.Log($"Cannot accept quest '{questPrefab.questName}'. Player Level {character.Level} too low (Requires {questPrefab.requiredLevel}).");
             return false;
        }
        // Check prerequisite quest completion if implemented
        // if (!string.IsNullOrEmpty(questPrefab.prerequisiteQuestID) && !IsQuestCompleted(questPrefab.prerequisiteQuestID)) { ... }


        // Create a *new instance* of the quest for the player state
        // IMPORTANT: Don't modify the prefab/database version directly!
        Quest newQuestInstance = CreateQuestInstance(questPrefab);

        // Attempt to start the quest instance
        if (newQuestInstance.StartQuest())
        {
            playerQuests.Add(questID, newQuestInstance);
            OnQuestAccepted?.Invoke(newQuestInstance);
             // Hook up objective listeners? Handled by Notify methods below now.
            Debug.Log($"Quest '{newQuestInstance.questName}' accepted.");
             // TODO: Save Player Quest Data
             // SavePlayerQuests();
            return true;
        }
        return false;
    }


    /// <summary>
    /// Turns in a completed quest and grants rewards.
    /// </summary>
    /// <param name="questID">The ID of the quest to turn in.</param>
    /// <returns>True if turn-in was successful, false otherwise.</returns>
    public bool TurnInQuest(string questID)
    {
        if (!playerQuests.TryGetValue(questID, out Quest questInstance))
        {
             Debug.LogWarning($"Cannot turn in quest '{questID}'. Player does not have this quest active.");
             return false;
        }

        if (questInstance.currentStatus == Quest.QuestStatus.RequirementsMet)
        {
            if (questInstance.CompleteQuest()) // Mark as officially completed
            {
                 GrantQuestRewards(questInstance);
                 OnQuestCompleted?.Invoke(questInstance);
                 Debug.Log($"Quest '{questInstance.questName}' turned in successfully.");
                 // TODO: Save Player Quest Data
                 // SavePlayerQuests();
                 return true;
            }
             else {
                 // This shouldn't happen if status was RequirementsMet, but log anyway
                 Debug.LogError($"Quest '{questID}' had RequirementsMet status, but CompleteQuest() returned false.");
                 return false;
             }
        }
        else
        {
             Debug.LogWarning($"Cannot turn in quest '{questInstance.questName}'. Requirements not met or already completed (Status: {questInstance.currentStatus}).");
            return false;
        }
    }

    /// <summary>
    /// Grants rewards for a completed quest to the player.
    /// </summary>
    private void GrantQuestRewards(Quest completedQuest)
    {
        Character character = CharacterManager.Instance?.character;
        if (character == null)
        {
            Debug.LogError($"Cannot grant rewards for quest '{completedQuest.questName}' - Player character data not found.");
            return;
        }

        Debug.Log($"Granting rewards for quest '{completedQuest.questName}':");
        foreach (var reward in completedQuest.questRewards)
        {
            switch (reward.type)
            {
                case QuestReward.RewardType.Experience:
                    character.AddExperience(reward.amount);
                    Debug.Log($"- Added {reward.amount} Experience.");
                    break;
                case QuestReward.RewardType.Gold:
                    // character.AddGold(reward.amount); // Assuming character has Gold property/method
                    Debug.Log($"- Added {reward.amount} Gold (Implementation needed).");
                    break;
                case QuestReward.RewardType.Item:
                    // InventoryManager.Instance.AddItem(reward.itemID, reward.amount); // Assuming InventoryManager exists
                    Debug.Log($"- Added Item '{reward.itemID}' x{reward.amount} (Implementation needed).");
                    break;
                 case QuestReward.RewardType.SkillPoint:
                     // character.AddSkillPoints(reward.amount); // Assuming method exists
                     Debug.Log($"- Added {reward.amount} Skill Point(s) (Implementation needed).");
                     break;
                 case QuestReward.RewardType.AttributePoint:
                      // character.AddStatPoints(reward.amount); // Assuming method exists
                     Debug.Log($"- Added {reward.amount} Attribute Point(s) (Implementation needed).");
                     break;
            }
        }
    }


    // --- Progress Notification Methods (Called by other systems) ---

    /// <summary>
    /// Called by other systems (e.g., EnemyController, InventoryManager) to notify quest progress.
    /// </summary>
    /// <param name="objectiveType">The type of action that occurred.</param>
    /// <param name="targetID">The specific ID related to the action (enemy ID, item ID, location ID).</param>
    /// <param name="amount">The amount related to the action (usually 1).</param>
    public void NotifyProgress(QuestObjective.ObjectiveType objectiveType, string targetID, int amount = 1)
    {
         if (amount <= 0) return;

        // Find all active quests that could be progressed by this action
        foreach (var questEntry in playerQuests)
        {
            Quest quest = questEntry.Value;
            if (quest.currentStatus == Quest.QuestStatus.InProgress)
            {
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    QuestObjective objective = quest.objectives[i];

                    // Check if this objective matches the action and is not yet complete
                    if (!quest.IsObjectiveComplete(i) && objective.type == objectiveType && objective.targetID == targetID)
                    {
                         // --- Simple Completion (Requires 1 Amount) ---
                         // For now, assume requiredAmount is 1 for simplicity for Kill/Collect/Interact/Reach
                         // More complex logic needed here for objectives like "Kill 10 Slimes"
                        if (objective.requiredAmount == 1 && amount >= 1) // Basic case: complete if action occurs once
                        {
                             if(quest.CompleteObjective(i)) // Mark objective complete in the Quest instance
                             {
                                OnObjectiveCompleted?.Invoke(quest, i); // Notify listeners
                                 // CheckOverallCompletion is called internally by quest.CompleteObjective

                                if(quest.currentStatus == Quest.QuestStatus.RequirementsMet) {
                                     OnQuestRequirementsMet?.Invoke(quest);
                                }
                                 // TODO: Save Player Quest Data after progress
                                 // SavePlayerQuests();
                             }
                        }
                         // --- TODO: Add logic for objectives requiring > 1 amount ---
                         // Example: Track currentAmount in QuestManager or modify QuestObjective
                         // else if (objective.requiredAmount > 1) { ... update current amount ... }
                    }
                }
            }
        }
    }


    // --- Status Checks ---

    /// <summary>
    /// Gets the current status of a specific quest for the player.
    /// </summary>
    public Quest.QuestStatus GetQuestStatus(string questID)
    {
        if (playerQuests.TryGetValue(questID, out Quest questInstance))
        {
            return questInstance.currentStatus;
        }
        // Check if quest exists in database but isn't accepted yet?
         else if (questDatabase != null && questDatabase.GetQuestByID(questID) != null) {
            return Quest.QuestStatus.NotStarted; // Available but not taken
         }

        return Quest.QuestStatus.NotStarted; // Default: treat as unavailable or not started
    }

    public bool IsQuestActive(string questID) => GetQuestStatus(questID) == Quest.QuestStatus.InProgress;
    public bool IsQuestCompleted(string questID) => GetQuestStatus(questID) == Quest.QuestStatus.Completed;
    public bool IsQuestReadyToTurnIn(string questID) => GetQuestStatus(questID) == Quest.QuestStatus.RequirementsMet;

     /// <summary>
    /// Gets a list of all quests currently in progress.
    /// </summary>
    public List<Quest> GetActiveQuests()
    {
        return playerQuests.Values.Where(q => q.currentStatus == Quest.QuestStatus.InProgress || q.currentStatus == Quest.QuestStatus.RequirementsMet).ToList();
    }

    /// <summary>
    /// Gets a list of all completed quests.
    /// </summary>
    public List<Quest> GetCompletedQuests()
    {
        return playerQuests.Values.Where(q => q.currentStatus == Quest.QuestStatus.Completed).ToList();
    }

     /// <summary>
    /// Creates a deep copy instance of a quest from a prefab/definition.
    /// Needed to ensure player state doesn't modify the shared quest definition.
    /// </summary>
    private Quest CreateQuestInstance(Quest questPrefab)
    {
         // Using JsonUtility for a simple deep copy (requires Quest and nested types to be Serializable)
         // More robust copy methods exist if needed (reflection, manual copy constructor).
         try {
             string json = JsonUtility.ToJson(questPrefab);
             Quest instance = JsonUtility.FromJson<Quest>(json);
             instance.currentStatus = Quest.QuestStatus.NotStarted; // Ensure starts as NotStarted before AcceptQuest sets it
             return instance;
         } catch (Exception e) {
             Debug.LogError($"Failed to create quest instance from '{questPrefab?.questID}': {e}");
             return null; // Or handle error appropriately
         }
    }

    // --- Save/Load ---
    // TODO: Implement saving playerQuests dictionary to a file (JSON recommended)
    public void SavePlayerQuests() { Debug.LogWarning("QuestManager SavePlayerQuests() Not Implemented!"); }
    public void LoadPlayerQuests() { Debug.LogWarning("QuestManager LoadPlayerQuests() Not Implemented!"); }

}