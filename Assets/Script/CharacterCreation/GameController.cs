using UnityEngine;
using UnityEngine.UI; // Needed for Slider
using TMPro; // Use TextMeshPro
using System.Collections.Generic; // If you add enemy management lists later

public class GameController : MonoBehaviour
{
    // Singleton Instantiation
    public static GameController Instance;

    [Header("Player UI References")]
    [Tooltip("Text element displaying the player's current level.")] // <<< MOVED Tooltip 1
    [SerializeField] private TMP_Text levelText;                     // <<< MOVED levelText field here
    [Tooltip("Text element displaying the player's current XP / Required XP.")]
    [SerializeField] private TMP_Text experienceText;
    [Tooltip("Slider visually representing XP progress towards the next level.")]
    [SerializeField] private Slider experienceBar;
    [Tooltip("Text element displaying the player's available Skill Points.")]
    [SerializeField] private TMP_Text skillPointsText;
    [Tooltip("Text element displaying the player's available Attribute Points.")]
    [SerializeField] private TMP_Text attributePointsText;

    [Header("Player Resource UI")] // <<< Header correctly placed
    [Tooltip("Slider visually representing Health.")] // <<< Tooltip 2 correctly placed
    [SerializeField] private Slider healthBar;
    [Tooltip("Text displaying current/max Health (e.g., 'HP: 100 / 120').")]
    [SerializeField] private TMP_Text healthText;
    [Tooltip("Slider visually representing Mana/Resource.")]
    [SerializeField] private Slider manaBar;
    [Tooltip("Text displaying current/max Mana (e.g., 'MP: 50 / 75').")]
    [SerializeField] private TMP_Text manaText;


    [Header("Enemy Settings")]
    [Tooltip("Prefab of the basic enemy to spawn (for testing, replace with dynamic system later).")]
    [SerializeField] private GameObject enemyPrefab;

    // Reference to the player character data
    private Character character;

    // --- Unity Lifecycle Methods ---

    private void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
            // Optional: Keep GameController across scenes if it manages persistent game state
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return; // Prevent rest of Awake/Start running on duplicate instance
        }
    }

    void Start()
    {
        // Get Character reference - Ensure CharacterManager runs first or is in same scene
        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            character = CharacterManager.Instance.character;

            // Subscribe to Character events for UI Updates
            character.OnExperienceChanged += UpdateExperienceUI;
            character.OnLevelUp += UpdateLevelAndExperienceUI; // Update level text specifically on level up
            character.OnStatsChanged += UpdatePointDisplays; // Update skill/attribute points when they change

            // Subscribe to death event for potential UI changes
            character.OnDeath += HandlePlayerDeathUI;
            // Initial UI state setup
            InitializeUI();

            // TEMP: Spawn an enemy for testing purposes - Remove/replace later
             // if(enemyPrefab != null) SpawnEnemy();
        }
        else
        {
            Debug.LogError("GameController: Character data not found in CharacterManager! UI will not function correctly.");
            // Disable UI elements or show an error state?
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when GameController is destroyed
        if (character != null)
        {
            character.OnExperienceChanged -= UpdateExperienceUI;
            character.OnLevelUp -= UpdateLevelAndExperienceUI;
            character.OnStatsChanged -= UpdatePointDisplays;
            character.OnDeath -= HandlePlayerDeathUI;
        }
        // Note: Unsubscribing from enemy events needs careful handling if enemies persist
    }

    // --- UI Initialization and Update ---

    /// <summary>
    /// Sets the initial state of all managed UI elements.
    /// </summary>
    private void InitializeUI()
    {
        UpdateLevelText();
        UpdateExperienceUI();
        UpdateSkillPointsUI();
        UpdateAttributePointsUI();
        UpdateCharacterStatsUI();  // Call consolidated method initially

    }

    /// <summary>
    /// Updates level text and experience UI. Called on level up.
    /// </summary>
    private void UpdateLevelAndExperienceUI()
    {
        UpdateLevelText();
        UpdateExperienceUI(); // XP requirement changes on level up
        UpdatePointDisplays(); // Points are awarded on level up
    }

     /// <summary>
    /// Updates the display of Skill and Attribute points.
    /// Called when Character.OnStatsChanged is invoked.
    /// </summary>
    private void UpdatePointDisplays()
    {
        UpdateSkillPointsUI();
        UpdateAttributePointsUI();
    }


    private void UpdateLevelText()
    {
        if (character != null && levelText != null)
        {
            levelText.text = $"Level: {character.Level}";
        }
    }

    private void UpdateExperienceUI()
    {
        if (character != null)
        {
            if (experienceText != null)
            {
                experienceText.text = $"XP: {character.ExperiencePoints} / {character.RequiredExperienceForNextLevel}";
            }
            if (experienceBar != null)
            {
                // Avoid division by zero if required XP calculation ever yields 0
                float requiredXP = character.RequiredExperienceForNextLevel;
                experienceBar.value = (requiredXP > 0) ? (float)character.ExperiencePoints / requiredXP : 0f;
            }
        }
    }

     public void UpdateSkillPointsUI() // Make public if other systems need to trigger (e.g. quest reward)
    {
        if (character != null && skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {character.SkillPoints}";
        }
    }

    public void UpdateAttributePointsUI() // Make public if other systems need to trigger
    {
        if (character != null && attributePointsText != null)
        {
            attributePointsText.text = $"Attribute Points: {character.StatPoints}";
        }
    }

    private void UpdateCharacterStatsUI()
    {
         UpdateLevelText(); // Level changes trigger OnStatsChanged via AddExperience now
         UpdateHealthUI();
         UpdateManaUI();
         UpdatePointDisplays(); // Handles Skill/Attribute points
    }

    private void UpdateHealthUI()
    {
        if (character != null)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = character.MaxHealth;
                healthBar.value = character.CurrentHealth;
            }
            if (healthText != null)
            {
                 // Format health text: round to integer or show decimals?
                 healthText.text = $"HP: {character.CurrentHealth:F0} / {character.MaxHealth:F0}";
            }
        }
    }

    private void UpdateManaUI()
    {
        if (character != null)
        {
            if (manaBar != null)
            {
                manaBar.maxValue = character.MaxMana;
                manaBar.value = character.CurrentMana;
            }
            if (manaText != null)
            {
                manaText.text = $"MP: {character.CurrentMana:F0} / {character.MaxMana:F0}"; // Assuming MP for Mana
            }
        }
    }

     // --- Handle Player Death UI ---
    private void HandlePlayerDeathUI()
    {
         Debug.Log("GameController reacting to player death for UI purposes.");
         // Example: Show Game Over screen, hide HUD elements etc.
         // gameOverPanel.SetActive(true);
         // hudPanel.SetActive(false);
    }


    // --- Enemy Handling ---

    /// <summary>
    /// Spawns a single enemy instance (basic example).
    /// Should be replaced by DungeonSpawningSystem later.
    /// </summary>
    private void SpawnEnemy()
    {
         if (enemyPrefab == null) {
             Debug.LogError("Enemy Prefab not assigned in GameController!");
             return;
         }
        // Instantiate at a default position (e.g., off-screen or a set spawn point)
        GameObject enemyInstance = Instantiate(enemyPrefab, new Vector3(10, 0, 0), Quaternion.identity);
        EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();

        if (enemyController != null)
        {
             // Set enemy level based on player level (or area level later)
             if (character != null)
             {
                 enemyController.InitializeEnemy(character.Level); // Use Initialize instead of just SetLevel now
             }
             else {
                 enemyController.InitializeEnemy(1); // Default to level 1 if player data missing
             }

            // Subscribe to the OnEnemyDefeated event for THIS specific instance
            enemyController.OnEnemyDefeated += HandleEnemyDefeated;
            Debug.Log($"Spawned enemy '{enemyInstance.name}' and subscribed to its defeat event.");
        }
         else {
            Debug.LogError($"Spawned enemy prefab '{enemyPrefab.name}' is missing EnemyController script!");
            Destroy(enemyInstance); // Clean up invalid instance
         }
    }

    /// <summary>
    /// Event handler called when an enemy's OnEnemyDefeated event is invoked.
    /// </summary>
    /// <param name="experienceValue">Experience points awarded by the defeated enemy.</param>
    /// <param name="defeatedEnemy">The EnemyController instance that was defeated.</param>
    public void HandleEnemyDefeated(int experienceValue, EnemyController defeatedEnemy)
    {
        Debug.Log($"Enemy defeated! Granting {experienceValue} XP.");

        // Grant the experience points to the player's character
        if (character != null)
        {
            character.AddExperience(experienceValue);
        }

        // We no longer need to listen to this specific enemy's event
        // Best practice: Always unsubscribe when the event source is destroyed
        if (defeatedEnemy != null) {
           defeatedEnemy.OnEnemyDefeated -= HandleEnemyDefeated;
        }

        // Note: UI updates for XP/Level are handled automatically by the Character's
        // OnExperienceChanged and OnLevelUp events, which this controller subscribes to.
        // No need to call UpdateExperienceUI() here directly.

        // Optional: Trigger loot drop, quest progress check, etc.
        // TriggerLootDrop(defeatedEnemy.transform.position);
        // QuestManager.Instance?.NotifyEnemyDefeated(defeatedEnemy.enemyTypeID); // If quests track enemy types
    }

    // --- Save Trigger (Example) ---
    // Could be called from a Save button in a Pause Menu
    public void SaveGame()
    {
        if (character != null)
        {
            Debug.Log("GameController triggering character save...");
            character.SaveCharacter();
            // Also need to save other game state: current scene, player position, quest states etc.
            // PlayerPrefs.SetString("CurrentScene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            // PlayerPrefs.SetFloat("PlayerPosX", PlayerControllerInstance.transform.position.x); // Need reference to player instance
            // QuestManager.Instance?.SaveQuests();
        }
         else {
             Debug.LogError("Cannot save game - Character data is missing!");
         }
    }
}