using UnityEngine;
using System.Collections.Generic; // Make sure this is present

// Defines the ScriptableObject asset that holds all quest definitions.
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "ScriptableObjects/Quest Database", order = 1)]
public class QuestDatabase : ScriptableObject
{
    [Tooltip("List of all quest definitions available in the game. Assign Quest ScriptableObjects or configured Quest instances here.")]
    public List<Quest> allQuests; // You'll drag your actual Quest assets/definitions here in the Inspector

    /// <summary>
    /// Finds a quest definition by its unique ID within this database.
    /// Note: If using ScriptableObjects for quests, this returns the shared asset reference.
    /// If storing plain Quest class instances, consider returning a copy.
    /// </summary>
    /// <param name="id">The Quest ID to search for.</param>
    /// <returns>The Quest definition object, or null if not found.</returns>
    public Quest GetQuestByID(string id)
    {
        if (allQuests == null)
        {
            Debug.LogWarning("QuestDatabase: 'allQuests' list is null!");
            return null;
        }
        Quest foundQuest = allQuests.Find(q => q != null && q.questID == id);
        if (foundQuest == null)
        {
            // Debug.LogWarning($"QuestDatabase: Quest with ID '{id}' not found."); // Can be spammy
        }
        return foundQuest;
    }
}