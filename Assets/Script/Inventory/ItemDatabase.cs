using UnityEngine;
using System.Collections.Generic; // Make sure this is present
using System; // Needed for JsonUtility if CreateItemInstance uses it (currently does)

// Defines the ScriptableObject asset that holds all item definitions.
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/Item Database", order = 2)]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("List of all item definitions available in the game. Assign Item ScriptableObjects or configured Item instances here.")]
    public List<Item> allItems; // Drag actual Item assets/definitions here

    /// <summary>
    /// Finds an item definition by its unique ID within this database.
    /// Note: This currently returns a DEEP COPY using JsonUtility.
    /// If your 'allItems' list holds ScriptableObject assets, you might want to return the direct reference instead.
    /// </summary>
    /// <param name="id">The Item ID to search for.</param>
    /// <returns>A copy of the Item definition object, or null if not found.</returns>
    public Item GetItemByID(string id)
    {
        if (allItems == null) return null;
        Item foundItem = allItems.Find(item => item != null && item.itemID == id);

        if (foundItem != null)
        {
            // Create a copy to prevent modifying the database definition indirectly
            // Ensure Item and any nested custom classes are [System.Serializable]
            try {
                 string json = JsonUtility.ToJson(foundItem);
                 return JsonUtility.FromJson<Item>(json);
            } catch (Exception e) {
                 Debug.LogError($"Failed to create copy of item '{id}' from database using JSON: {e.Message}");
                 return null;
            }
        }
        // Debug.LogWarning($"ItemDatabase: Item with ID '{id}' not found."); // Can be spammy
        return null;
    }

    /// <summary>
    /// Creates a new instance (deep copy) of an item from the database using JsonUtility.
    /// Useful for loot drops/rewards.
    /// </summary>
    /// <param name="id">The Item ID of the definition to instance.</param>
    /// <returns>A new Item instance, or null if definition not found or copying failed.</returns>
    public Item CreateItemInstance(string id)
    {
        Item itemDefinition = allItems.Find(item => item != null && item.itemID == id);
        if (itemDefinition != null)
        {
            try {
                 string json = JsonUtility.ToJson(itemDefinition);
                 Item instance = JsonUtility.FromJson<Item>(json);
                 // Reset any runtime variables if they were part of the saved definition (shouldn't be)
                 return instance;
            } catch (Exception e) {
                 Debug.LogError($"Failed to create instance of item '{id}' using JSON: {e.Message}");
                 return null;
            }
        }
        Debug.LogWarning($"ItemDatabase: Could not find item definition for ID '{id}' to create instance.");
        return null;
    }
}