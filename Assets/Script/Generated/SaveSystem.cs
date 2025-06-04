using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; // Optional
using System.Collections.Generic;

public static class SaveSystem
{
    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
    private static string GetSaveFilePath(string saveName) => Path.Combine(SaveDirectory, saveName + ".sav");

    // --- Core Save Data Structure --- ** MADE PUBLIC **
    [System.Serializable]
    public class GameSaveData // <<< CHANGED TO PUBLIC
    {
        public string characterName;
        public string characterJson;
        public string currentSceneName;
        public float playerPositionX;
        public float playerPositionY;
        public string questManagerJson; // Placeholder
    }

    // --- Save Operation ---
    public static bool SaveGame(string saveName)
    {
        // Debug.Log($"Saving game to '{saveName}'...");
        Character character = CharacterManager.Instance?.character;
        PlayerController playerController = GameObject.FindObjectOfType<PlayerController>(); // Okay for save time usually

        if (character == null) { Debug.LogError("Save Failed: Character missing."); return false; }

        GameSaveData saveData = new GameSaveData();

        // --- Populate Data ---
        saveData.characterName = character.characterName;

        // *** Refactored: Get JSON directly from Character ***
        // Ensure Character has: public string GetSaveDataJson() { ... return JsonUtility.ToJson(saveData); }
        // saveData.characterJson = character.GetSaveDataJson(); // <<< PREFERRED WAY
        // Temporary Workaround (If GetSaveDataJson not implemented):
         character.SaveCharacter(); // Saves to PlayerPrefs
         saveData.characterJson = PlayerPrefs.GetString($"CharacterData_{character.characterName}", "");
         PlayerPrefs.DeleteKey($"CharacterData_{character.characterName}"); // Clean up temp key
         if(string.IsNullOrEmpty(saveData.characterJson)) { Debug.LogError("Save Error: Failed to get character JSON."); return false; }
        // *** End Refactor section ***

        saveData.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (playerController != null) {
            saveData.playerPositionX = playerController.transform.position.x;
            saveData.playerPositionY = playerController.transform.position.y;
        } else { saveData.playerPositionX = 0; saveData.playerPositionY = 0; } // Default pos

        // TODO: Get QuestManager JSON
        // QuestManager.Instance?.SavePlayerQuests(); // Assuming saves to temp location or returns JSON
        // saveData.questManagerJson = GetQuestManagerSaveJson(); // Example

        // --- Serialize & Write ---
        try {
            string jsonToSave = JsonUtility.ToJson(saveData, false); // Save compact (false) usually
            Directory.CreateDirectory(SaveDirectory);
            File.WriteAllText(GetSaveFilePath(saveName), jsonToSave);
            Debug.Log($"Game saved: {GetSaveFilePath(saveName)}");
            return true;
        } catch (Exception e) { Debug.LogError($"Save Failed: {e}"); return false; }
    }

    // --- Load Operation ---
    public static bool LoadGame(string saveName)
    {
        string filePath = GetSaveFilePath(saveName);
        if (!File.Exists(filePath)) { Debug.LogError($"Load Failed: File not found '{filePath}'."); return false; }

        try {
            string jsonToLoad = File.ReadAllText(filePath);
            GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(jsonToLoad);

            if (loadedData == null) { Debug.LogError("Load Failed: Invalid save data format."); return false; }

            // --- Trigger Scene Load ---
            // Store data temporarily, then load the scene. PostLoadApplier will handle the rest.
            TemporarySaveDataHolder.DataToLoad = loadedData;
             Debug.Log($"Loading scene '{loadedData.currentSceneName}'...");
             UnityEngine.SceneManagement.SceneManager.LoadScene(loadedData.currentSceneName);
             // Indicate success for initiating the load process
             return true; // Loading continues after scene switch

        } catch (Exception e) { Debug.LogError($"Load Failed: {e}"); return false; }
    }

    /// <summary>
    /// Applies loaded data AFTER the correct scene is loaded. Called by PostLoadApplier.
    /// ** Parameter type GameSaveData must be public now **
    /// </summary>
    public static void ApplyLoadedData(GameSaveData loadedData)
    {
        if (loadedData == null) { Debug.LogError("ApplyLoadData: Null data!"); return; }
        Debug.Log($"Applying loaded data for character '{loadedData.characterName}'...");

        // Find managers - Should be ready after scene load
        CharacterManager charManager = CharacterManager.Instance;
        QuestManager questManager = QuestManager.Instance; // Placeholder
        PlayerController playerController = GameObject.FindObjectOfType<PlayerController>(); // Okay after scene load

        if (charManager == null) { Debug.LogError("ApplyLoadData Failed: CharacterManager missing!"); return; }

        // --- Load Character ---
        // TODO: Ideally, have CharacterManager handle loading/creating the correct character
        // instance based on loadedData.characterName BEFORE calling LoadCharacterFromJson.
        if (charManager.character == null || charManager.character.characterName != loadedData.characterName) {
            Debug.LogError($"ApplyLoadData Error: Character instance mismatch or missing! Expected '{loadedData.characterName}'. Character load may fail.");
            // Need logic here to ensure the correct Character object exists in CharacterManager.Instance.character
            // This might involve destroying the current one and creating/loading the correct one based on save data.
        }

        // *** Refactored: Load Character directly from JSON ***
        // Ensure Character has: public bool LoadCharacterFromJson(string json) { ... }
        // bool charLoadSuccess = charManager.character?.LoadCharacterFromJson(loadedData.characterJson) ?? false; // <<< PREFERRED WAY
        // Temporary Workaround:
         PlayerPrefs.SetString($"CharacterData_{loadedData.characterName}", loadedData.characterJson); // Put JSON back temp
         bool charLoadSuccess = charManager.character?.LoadCharacter() ?? false; // Use old method
         PlayerPrefs.DeleteKey($"CharacterData_{loadedData.characterName}"); // Clean up
         // *** End Refactor Section ***

        if (!charLoadSuccess) { Debug.LogError("ApplyLoadData Failed: Character data could not be applied!"); return; }


        // --- Apply Position ---
        if (playerController != null && charManager.character != null && !charManager.character.IsDead()) // Don't teleport if dead?
        {
            playerController.transform.position = new Vector3(loadedData.playerPositionX, loadedData.playerPositionY, playerController.transform.position.z);
        } else if (playerController == null) { Debug.LogWarning("ApplyLoadData Warning: PlayerController missing, cannot set position."); }


        // --- Apply Quest State ---
        // TODO: Ensure QuestManager has LoadPlayerQuestsFromJson(string json)
        // questManager?.LoadPlayerQuestsFromJson(loadedData.questManagerJson);


        // --- Apply Other States ---
        // ...

        Debug.Log("Save data applied successfully.");
        TemporarySaveDataHolder.DataToLoad = null; // Clear temp holder
    }

    // --- Utility ---
    public static bool DoesSaveExist(string saveName) => File.Exists(GetSaveFilePath(saveName));
    public static void DeleteSave(string saveName) { string p = GetSaveFilePath(saveName); if (File.Exists(p)) File.Delete(p); }

} // End SaveSystem class


// Helper to hold data during scene load
public static class TemporarySaveDataHolder
{
    // --- Use the PUBLIC GameSaveData defined in SaveSystem ---
    public static SaveSystem.GameSaveData DataToLoad = null;
}