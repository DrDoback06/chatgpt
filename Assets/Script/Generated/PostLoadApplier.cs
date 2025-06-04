using UnityEngine;

/// <summary>
/// Simple MonoBehaviour that runs logic immediately after a scene has loaded.
/// Specifically used here to apply saved game data held temporarily during a scene change.
/// Attach this to a persistent GameObject like your main GameManager or Managers object.
/// </summary>
public class PostLoadApplier : MonoBehaviour
{
    void Start()
    {
        // Check if there's any saved game data waiting to be applied
        if (TemporarySaveDataHolder.DataToLoad != null)
        {
            Debug.Log("PostLoadApplier running ApplyLoadedData...");
            SaveSystem.ApplyLoadedData(TemporarySaveDataHolder.DataToLoad);
            // Data is now applied, clear the temporary holder
            TemporarySaveDataHolder.DataToLoad = null;
        }
         else {
            // Debug.Log("PostLoadApplier: No saved data to apply on scene start."); // Optional log
         }
    }
}