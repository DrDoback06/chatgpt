// ItemGeneratorInitializer.cs (New Script)
using UnityEngine;
public class ItemGeneratorInitializer : MonoBehaviour {
    public ItemDatabase itemDatabase; // Assign ItemDatabase_Asset here
    public AffixManager affixManager; // Assign AffixManager_Asset or scene instance here

    void Awake() {
        if (itemDatabase != null && affixManager != null) ItemGenerator.Initialize(itemDatabase, affixManager);
        else Debug.LogError("ItemGeneratorInitializer: ItemDatabase or AffixManager not assigned!");
    }
}