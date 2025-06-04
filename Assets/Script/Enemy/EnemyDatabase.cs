using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For Linq methods like Where, ToList

[System.Serializable]
public class EnemyDatabaseEntry
{
    [Tooltip("Unique identifier for this enemy type (e.g., 'goblin_melee', 'skeleton_archer').")]
    public string enemyID;
    [Tooltip("Reference to the configured enemy prefab.")]
    public GameObject enemyPrefab; // Assign prefab here
    [Tooltip("Difficulty or level range where this enemy typically appears.")]
    public int minLevel = 1;
    public int maxLevel = 5;
    [Tooltip("Optional category tags for themed spawning (e.g., 'Forest', 'Cave', 'Undead').")]
    public List<string> categoryTags;
}


[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "ScriptableObjects/Enemy Database", order = 4)]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyDatabaseEntry> allEnemies;

    /// <summary>
    /// Gets a specific enemy prefab by its ID.
    /// </summary>
    public GameObject GetEnemyPrefabByID(string id)
    {
        EnemyDatabaseEntry entry = allEnemies?.Find(e => e.enemyID == id);
        return entry?.enemyPrefab;
    }

    /// <summary>
    /// Gets a list of suitable enemy prefabs based on level and optional category tags.
    /// </summary>
    /// <param name="requiredLevel">The level context (e.g., player level or area level).</param>
    /// <param name="requiredTags">Optional list of tags that *must* be present on the enemy.</param>
    /// <returns>A list of matching enemy prefabs.</returns>
    public List<GameObject> GetEnemyPrefabsByCriteria(int requiredLevel, List<string> requiredTags = null)
    {
        if (allEnemies == null) return new List<GameObject>();

        IEnumerable<EnemyDatabaseEntry> query = allEnemies;

        // Filter by level
        query = query.Where(e => requiredLevel >= e.minLevel && requiredLevel <= e.maxLevel);

        // Filter by tags (if provided)
        if (requiredTags != null && requiredTags.Count > 0)
        {
            query = query.Where(e => requiredTags.All(reqTag => e.categoryTags != null && e.categoryTags.Contains(reqTag)));
            // Use .Any if only *one* tag needs to match:
            // query = query.Where(e => requiredTags.Any(reqTag => e.categoryTags != null && e.categoryTags.Contains(reqTag)));
        }

        // Select the prefab from the matching entries
        return query.Select(e => e.enemyPrefab).Where(prefab => prefab != null).ToList();
    }

     /// <summary>
     /// Gets a single random enemy prefab matching the criteria.
     /// </summary>
     public GameObject GetRandomEnemyPrefabByCriteria(int requiredLevel, List<string> requiredTags = null) {
          List<GameObject> matchingPrefabs = GetEnemyPrefabsByCriteria(requiredLevel, requiredTags);
          if(matchingPrefabs.Count == 0) {
             // Fallback? Return a default enemy? Or null?
             Debug.LogWarning($"EnemyDatabase: No matching enemies found for Level {requiredLevel} and Tags: [{string.Join(", ", requiredTags ?? new List<string>())}].");
             // Optional Fallback: Return ANY enemy within level range regardless of tags?
             matchingPrefabs = GetEnemyPrefabsByCriteria(requiredLevel, null);
              if (matchingPrefabs.Count == 0) return null; // No enemies at all for this level
          }
         // Return a random one from the filtered list
          int randomIndex = Random.Range(0, matchingPrefabs.Count);
          return matchingPrefabs[randomIndex];
     }
}