using UnityEngine;
using System.Collections.Generic;

public class DungeonSpawningSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the DungeonGenerator script that creates the layout.")]
    [SerializeField] private DungeonGenerator dungeonGenerator; // Assign in Inspector
    [Tooltip("Reference to the EnemyDatabase asset containing all enemy definitions.")]
    [SerializeField] private EnemyDatabase enemyDatabase; // Assign EnemyDatabase asset

    [Header("Spawning Parameters")]
    [Tooltip("Array of possible enemy prefabs to spawn.")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [Tooltip("Minimum number of enemies to potentially spawn in a room (if spawn chance succeeds).")]
    [SerializeField] private int minEnemiesPerRoom = 0;
    [Tooltip("Maximum number of enemies to potentially spawn in a room (if spawn chance succeeds).")]
    [SerializeField] private int maxEnemiesPerRoom = 3;
    [Tooltip("Chance (0-100) that any enemies will spawn in a given room.")]
    [SerializeField] [Range(0, 100)] private int spawnChancePercentage = 75; // Use percentage for clarity
    [Tooltip("Optional list of category tags to filter enemies for this specific dungeon/area.")]
    [SerializeField] private List<string> requiredEnemyTags; // e.g., ["Cave", "Undead"]


    private Character characterData; // Reference to player data

    // --- Unity Lifecycle ---

    void Start()
    {
        // Get Player Data Reference
        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            characterData = CharacterManager.Instance.character;
        }
        else
        {
            Debug.LogError("DungeonSpawningSystem: Cannot find Character data! Enemies will spawn at default level.", this);
            // Proceed, but enemies won't scale correctly
        }

        // --- Initialization Flow ---
        // It's crucial that Dungeon Generation happens BEFORE Spawning.
        // Option 1 (Recommended): Generator calls Spawner.
        //    - Add a method `StartSpawning()` here.
        //    - In DungeonGenerator.GenerateDungeon(), after it finishes, call:
        //      FindObjectOfType<DungeonSpawningSystem>()?.StartSpawning(); // Or use direct reference
        // Option 2 (Simple): Assume Generator runs first (based on Script Execution Order or calling order).
        //    - The code below uses this assumption.

        if (dungeonGenerator == null) { /* ... error ... */ return; }
        if (enemyDatabase == null) { // Add check for database
            Debug.LogError("DungeonSpawningSystem: EnemyDatabase reference not assigned!", this);
            return;
        }

        // Assuming DungeonGenerator has already run (either in its Awake/Start or called externally)
        // Trigger the spawning process
        SpawnEnemiesInGeneratedDungeon();
    }

    /// <summary>
    /// Finds all rooms created by the generator and attempts to spawn enemies in them.
    /// </summary>
    public void SpawnEnemiesInGeneratedDungeon()
    {
        // Check if database has any enemies at all
         if (enemyDatabase.allEnemies == null || enemyDatabase.allEnemies.Count == 0)
         {
             Debug.LogWarning("DungeonSpawningSystem: EnemyDatabase is empty. Cannot spawn enemies.", this);
             return;
         }

        int playerLevel = (characterData != null) ? characterData.Level : 1;
        Room[] rooms = dungeonGenerator.GetComponentsInChildren<Room>();
        if (rooms.Length == 0) { /* ... error ... */ return; }

        Debug.Log($"DungeonSpawningSystem: Found {rooms.Length} rooms. Attempting to spawn enemies (Player Level: {playerLevel}, Tags: [{string.Join(", ", requiredEnemyTags)}])...");

        int totalEnemiesSpawned = 0;
        foreach (Room room in rooms)
        {
            if (Random.Range(0, 100) < spawnChancePercentage)
            {
                int enemiesToSpawn = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    // --- Select Enemy Prefab from Database ---
                    GameObject enemyPrefab = enemyDatabase.GetRandomEnemyPrefabByCriteria(playerLevel, requiredEnemyTags);
                    // --- End Selection ---

                    if (enemyPrefab != null) // Check if a suitable prefab was found
                    {
                        Vector3 spawnPosition = room.GetSpawnPoint();
                        spawnPosition += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                        EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
                        if (enemyController != null)
                        {
                            enemyController.InitializeEnemy(playerLevel); // Initialize level/stats
                            totalEnemiesSpawned++;
                             // Subscribe GameController to defeat event
                            if (GameController.Instance != null) {
                                enemyController.OnEnemyDefeated += GameController.Instance.HandleEnemyDefeated;
                            }
                             // Optional: EnemyAI might need reference to player target set here if Start() is unreliable across scenes
                             // enemyInstance.GetComponent<EnemyAI>()?.SetTarget(CharacterManager.Instance?.character?.transform); // If you add SetTarget method
                        } else { /* ... error cleanup ... */ }
                    } else {
                        // Log if no suitable enemy found for this room attempt
                        // Debug.LogWarning($"Could not find suitable enemy prefab for room (Level {playerLevel}, Tags [{string.Join(", ", requiredEnemyTags)}]). Skipping spawn.");
                    }
                }
            }
        }
        Debug.Log($"DungeonSpawningSystem: Finished spawning. Total enemies created: {totalEnemiesSpawned}");
    }
}