using UnityEngine;

// Represents a room or area within the generated dungeon layout.
public class Room : MonoBehaviour
{
    [Tooltip("Default spawn point position within this room for enemies or items.")]
    [SerializeField] private Transform spawnPoint; // Assign in Room prefab inspector

    /// <summary>
    /// Gets the world position of the designated spawn point for this room.
    /// Returns the room's own position if spawnPoint is not assigned.
    /// </summary>
    /// <returns>The spawn point's world position.</returns>
    public Vector3 GetSpawnPoint()
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }
        else
        {
            Debug.LogWarning($"Room '{gameObject.name}' is missing its Spawn Point reference. Using room center as fallback.", this);
            return transform.position; // Fallback to room's center
        }
    }

    // Potential future additions (Keep commented out for now):
    // public enum RoomType { Normal, Treasure, Hazard, Boss, Start, End }
    // public RoomType roomType = RoomType.Normal;
    // public List<Transform> enemySpawnPoints;
    // public List<string> allowedEnemyTypes; // For themed rooms
}