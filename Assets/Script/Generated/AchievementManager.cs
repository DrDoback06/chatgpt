using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// Manages player achievements: tracking progress, unlocking, saving/loading.
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance; // Optional Singleton

    [Header("Database")]
    [SerializeField] private AchievementDatabase achievementDatabase; // Assign SO asset

    // Runtime state - Store achievement ID and its current state (instance)
    private Dictionary<string, Achievement> playerAchievements = new Dictionary<string, Achievement>();

    // Event fired when an achievement is unlocked
    public event Action<Achievement> OnAchievementUnlocked;

    void Awake()
    {
        // Singleton Setup
        if (Instance == null) { Instance = this; /* Optional DontDestroyOnLoad */ }
        else { Destroy(gameObject); return; }

        // Load achievements state
        LoadAchievements();
    }

    void Start() {
         // Subscribe to game events AFTER managers holding the data are ready
         SubscribeToGameEvents();
    }
    void OnDestroy() {
         UnsubscribeFromGameEvents();
    }

     /// <summary>
     /// Subscribes to relevant game events that can trigger achievements.
     /// </summary>
    private void SubscribeToGameEvents() {
         Debug.Log("AchievementManager subscribing to game events...");
         // Example Subscriptions (Ensure target events exist!)
         if (CharacterManager.Instance?.character != null) {
             CharacterManager.Instance.character.OnLevelUp += HandleLevelUp;
             // CharacterManager.Instance.character.OnEnemyKilled += HandleEnemyKilled; // Need this event defined
         }
         if (QuestManager.Instance != null) {
             QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
         }
         // Subscribe to EnemyController.OnEnemyDefeated? (Can be many instances) - Better if GameController relays event
          if (GameController.Instance != null) {
              // Add an event to GameController or Character for generic enemy kill notification
              // GameController.Instance.OnEnemyDefeatedGlobally += HandleEnemyDefeated;
          }

          // Subscribe to Inventory changes, Item crafting, etc.
    }

     private void UnsubscribeFromGameEvents() {
          // Unsubscribe from all events here to prevent errors on scene change/destroy
          if (CharacterManager.Instance?.character != null) {
             CharacterManager.Instance.character.OnLevelUp -= HandleLevelUp;
          }
         if (QuestManager.Instance != null) {
             QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
         }
          // ... and so on
     }

     // --- Event Handlers ---

     private void HandleLevelUp() {
         if (CharacterManager.Instance?.character == null) return;
         int currentLevel = CharacterManager.Instance.character.Level;
         Debug.Log($"AchievementManager: Player reached level {currentLevel}");
         CheckAchievement("reach_level_5", currentLevel >= 5);
         CheckAchievement("reach_level_10", currentLevel >= 10);
     }

     private void HandleQuestCompleted(Quest quest) {
          if (quest == null) return;
          Debug.Log($"AchievementManager: Quest '{quest.questID}' completed.");
          CheckAchievement("complete_first_quest", true); // Simple boolean check example
          CheckAchievement($"complete_{quest.questID}", true); // Achievement for specific quest
     }

     // Example for enemy kills (Needs event source)
     // private Dictionary<string, int> killCounts = new Dictionary<string, int>();
     // private void HandleEnemyDefeated(string enemyID) {
     //    if (!killCounts.ContainsKey(enemyID)) killCounts[enemyID] = 0;
     //    killCounts[enemyID]++;
     //    Debug.Log($"AchievementManager: Killed {enemyID} ({killCounts[enemyID]} total)");
     //    CheckAchievement("kill_10_imps", enemyID == "imp_melee" && killCounts[enemyID] >= 10);
     //    CheckAchievement("kill_1_brute", enemyID == "brute_demon" && killCounts[enemyID] >= 1);
     // }

    // --- Achievement Checking & Unlocking ---

    /// <summary>
    /// Checks if conditions are met for a specific achievement and unlocks it if not already unlocked.
    /// </summary>
    /// <param name="achievementID">The ID of the achievement to check.</param>
    /// <param name="conditionMet">Boolean indicating if the specific unlock condition is currently true.</param>
    public void CheckAchievement(string achievementID, bool conditionMet)
    {
        if (string.IsNullOrEmpty(achievementID)) return;

        if (playerAchievements.TryGetValue(achievementID, out Achievement achievement))
        {
            // Only proceed if condition is met AND achievement is not already unlocked
            if (conditionMet && !achievement.IsUnlocked)
            {
                UnlockAchievement(achievement);
            }
        }
         else {
              Debug.LogWarning($"CheckAchievement: Achievement ID '{achievementID}' not found in player data. Was it initialized on load?");
         }
    }

     // Could add variants like CheckAchievementProgress(string id, int current, int required)

    private void UnlockAchievement(Achievement achievement)
    {
         if (achievement == null || achievement.IsUnlocked) return;

         achievement.IsUnlocked = true; // Update runtime state
         Debug.Log($"<color=yellow>ACHIEVEMENT UNLOCKED: {achievement.AchievementName}</color>");
         OnAchievementUnlocked?.Invoke(achievement); // Fire event for UI popups etc.

         // TODO: Save Achievement State
         SaveAchievements();
    }


    // --- Save & Load ---
    // Needs AchievementSaveData struct and save logic (e.g., save Dictionary<string, bool> of unlocked status)
    private void LoadAchievements() {
         if (achievementDatabase == null || achievementDatabase.allAchievements == null) {
             Debug.LogError("AchievementManager: Cannot load achievements - Database not assigned or empty!");
             return;
         }

         // TODO: Load saved unlock status (e.g., from PlayerPrefs or save file)
         // Dictionary<string, bool> loadedUnlockStatus = LoadUnlockStatusFromFile(); // Placeholder

         playerAchievements.Clear();
         // Initialize player achievement dictionary from database definitions
         foreach(Achievement definition in achievementDatabase.allAchievements) {
             if (definition != null && !playerAchievements.ContainsKey(definition.AchievementID)) {
                  // Create instance/copy (if Achievement is class not SO)
                  Achievement instance = CreateAchievementInstance(definition);
                  // Apply loaded status
                  // if(loadedUnlockStatus.TryGetValue(instance.AchievementID, out bool unlocked)) {
                  //    instance.IsUnlocked = unlocked;
                  // }
                  playerAchievements.Add(instance.AchievementID, instance);
             }
         }
         Debug.Log($"Initialized/Loaded {playerAchievements.Count} player achievements.");
    }

    private Achievement CreateAchievementInstance(Achievement source) {
         // Simple JSON copy
         try { return JsonUtility.FromJson<Achievement>(JsonUtility.ToJson(source)); }
         catch (Exception e) { Debug.LogError($"Failed achievement copy {source?.AchievementID}: {e}"); return null; }
    }


    private void SaveAchievements() {
         // TODO: Save the unlocked status of playerAchievements
         // Example: Create Dictionary<string, bool> with unlocked status and serialize it
         Debug.LogWarning("AchievementManager SaveAchievements() Not Implemented!");
    }

}
