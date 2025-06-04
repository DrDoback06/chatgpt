using UnityEngine;
using System.Collections.Generic; // To potentially hold instantiated rooms

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation Parameters")]
    [Tooltip("Minimum number of rooms to generate.")]
    [SerializeField] private int minRoomsBase = 3;
    [Tooltip("Maximum number of rooms to generate.")]
    [SerializeField] private int maxRoomsBase = 6;
    [Tooltip("How many extra potential rooms to add per character level.")]
    [SerializeField] [Range(0f, 1f)] private float roomsPerLevel = 0.5f;

    [Header("Prefabs")]
    [Tooltip("Array of possible room prefabs.")]
    [SerializeField] private GameObject[] roomPrefabs;
    [Tooltip("Array of possible corridor prefabs connecting rooms.")]
    [SerializeField] private GameObject[] corridorPrefabs;
    [Tooltip("Prefab for the starting room/entrance.")]
    [SerializeField] private GameObject startRoomPrefab; // Optional specific start room
    [Tooltip("Prefab for the final room/exit/boss room.")]
    [SerializeField] private GameObject endRoomPrefab; // Optional specific end room

    [Header("Layout Settings")]
    [Tooltip("Spacing along the X-axis between the center of rooms/corridors.")]
    [SerializeField] private float horizontalSpacing = 15f; // Adjust based on prefab sizes

    // Keep track of generated objects if needed for other systems
    private List<GameObject> generatedObjects = new List<GameObject>();

    /// <summary>
    /// Generates the dungeon layout sequentially based on character level.
    /// </summary>
    /// <param name="characterLevel">The player's current level.</param>
    public void GenerateDungeon(int characterLevel)
    {
        // --- Input Validation ---
        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("DungeonGenerator: Room Prefabs array is empty or not assigned!");
            return;
        }
        if (corridorPrefabs == null || corridorPrefabs.Length == 0)
        {
            Debug.LogError("DungeonGenerator: Corridor Prefabs array is empty or not assigned!");
            // You could potentially proceed without corridors if designed that way
            // return;
        }

        // --- Clear Old Dungeon (if any) ---
        ClearDungeon();

        // --- Calculate Dungeon Size ---
        int minRooms = minRoomsBase + Mathf.FloorToInt(characterLevel * roomsPerLevel);
        int maxRooms = maxRoomsBase + Mathf.FloorToInt(characterLevel * roomsPerLevel);
        int numberOfRooms = Random.Range(minRooms, maxRooms + 1);
        Debug.Log($"Generating dungeon for Level {characterLevel}: Target rooms = {numberOfRooms} (Range: {minRooms}-{maxRooms})");


        // --- Generation Logic ---
        float currentOffsetX = 0.0f;
        GameObject previousInstance = null; // Keep track of the last instantiated object

        // 1. Start Room (Optional)
        GameObject startPrefab = startRoomPrefab != null ? startRoomPrefab : roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        previousInstance = InstantiateRoomOrCorridor(startPrefab, currentOffsetX);
        currentOffsetX += horizontalSpacing; // Use fixed spacing for simplicity


        // 2. Middle Rooms & Corridors
        for (int i = 0; i < numberOfRooms; i++)
        {
            // Add a corridor (if available and not the last connection)
            if (corridorPrefabs.Length > 0)
            {
                GameObject corridorPrefab = corridorPrefabs[Random.Range(0, corridorPrefabs.Length)];
                 previousInstance = InstantiateRoomOrCorridor(corridorPrefab, currentOffsetX);
                currentOffsetX += horizontalSpacing;
            }

            // Add a regular room
             GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
             previousInstance = InstantiateRoomOrCorridor(roomPrefab, currentOffsetX);
            currentOffsetX += horizontalSpacing;
        }

        // 3. End Room (Optional)
         // Add a final corridor if needed before the end room
         if (corridorPrefabs.Length > 0 && endRoomPrefab != null)
         {
             GameObject corridorPrefab = corridorPrefabs[Random.Range(0, corridorPrefabs.Length)];
             previousInstance = InstantiateRoomOrCorridor(corridorPrefab, currentOffsetX);
             currentOffsetX += horizontalSpacing;
         }

         // Add the end room
        GameObject finalPrefab = endRoomPrefab != null ? endRoomPrefab : roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        previousInstance = InstantiateRoomOrCorridor(finalPrefab, currentOffsetX);


        Debug.Log($"Dungeon generation complete. Total objects: {generatedObjects.Count}");
    }

     /// <summary>
     /// Instantiates a room or corridor prefab at the specified X offset.
     /// </summary>
     /// <param name="prefab">The GameObject prefab to instantiate.</param>
     /// <param name="offsetX">The horizontal position offset.</param>
     /// <returns>The instantiated GameObject instance.</returns>
     private GameObject InstantiateRoomOrCorridor(GameObject prefab, float offsetX)
     {
         if (prefab == null) return null;

         // Instantiate at position (offsetX, 0, 0) relative to the generator's position
         Vector3 spawnPosition = transform.position + new Vector3(offsetX, 0, 0);
         GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, this.transform); // Parent to generator

         generatedObjects.Add(instance);
         // Debug.Log($"Instantiated {prefab.name} at X={offsetX}"); // Optional detailed log
         return instance;

         // --- Note on more advanced connection: ---
         // If using connection points:
         // 1. Find connection point on `previousInstance` (e.g., "Connection_E")
         // 2. Find compatible point on `prefab` (e.g., "Connection_W")
         // 3. Calculate required offset and rotation to align points
         // 4. Instantiate `prefab` at the calculated position/rotation
     }


    /// <summary>
    /// Destroys all previously generated dungeon objects.
    /// </summary>
    public void ClearDungeon()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        generatedObjects.Clear();
        // Alternatively, destroy all children of this transform:
        // foreach (Transform child in transform) { Destroy(child.gameObject); }
        Debug.Log("Cleared previous dungeon layout.");
    }
}