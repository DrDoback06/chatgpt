using System;
using System.Collections.Generic; // <<< ADD this using directive
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
// using System.Diagnostics; // Remove if not used

public class CharacterCreation : MonoBehaviour
{
    public Button createCharacterButton;
    public Button playButton;

    [Tooltip("List of possible factions")]
    public List<string> factions = new List<string>();
    [Tooltip("List of possible pets")]
    public List<string> pets = new List<string>();
    [Tooltip("List of possible base attributes to potentially boost slightly?")]
    public List<string> attributes = new List<string>(); // Keep list for potential future use

    void Start()
    {
        createCharacterButton.onClick.AddListener(CreateRandomCharacter);
        // Play button logic is handled by SceneLoader now
        // playButton.onClick.AddListener(LoadNextScene);
    }

    void CreateRandomCharacter()
    {
        Debug.Log("CreateRandomCharacter called");
        string characterName = "Adventurer"; // More generic name
        // Ensure you have BlademasterClass/DefenderClass etc defined
        int randomMainClassIndex = UnityEngine.Random.Range(0, 2); // Only 0 (Blade) or 1 (Def) for now

        // Subclass placeholder
        int randomSubClassIndex = UnityEngine.Random.Range(0, 2); // Example indices

        Character character = CreateCharacterWithClassIndex(characterName, randomMainClassIndex, randomSubClassIndex);
        Debug.Log($"Character '{character?.characterName}' created. Class: {character?.mainClass}");

        if (character != null)
        {
            CharacterManager.Instance.character = character;
             // Enable play button via SceneLoader check (it monitors CharacterManager)
             // Or explicitly find SceneLoader and enable its button? Less ideal coupling.
        }
         else {
            Debug.LogError("Failed to create character!");
         }
    }


    Character CreateCharacterWithClassIndex(string characterName, int mainClassIndex, int subclassIndex)
    {
        Character character = null;

        // Main class creation using factories
        switch (mainClassIndex)
        {
            case 0:
                character = BlademasterClass.CreateBlademaster(characterName);
                break;
            case 1:
                character = DefenderClass.CreateDefender(characterName);
                break;
            // Add cases for other classes...
            default:
                Debug.LogWarning($"Invalid main class index: {mainClassIndex}. Defaulting to Blademaster.");
                character = BlademasterClass.CreateBlademaster(characterName);
                break;
        }

        if (character == null) return null; // Stop if creation failed

        // Subclass assignment (Placeholder - Needs proper implementation)
        switch (subclassIndex)
        {
            case 0: character.subClass = "Subclass_A"; break;
            case 1: character.subClass = "Subclass_B"; break;
            default: character.subClass = "DefaultSubclass"; break;
        }

        // Assign Faction
        if (factions != null && factions.Count > 0) {
            int randomFactionIndex = UnityEngine.Random.Range(0, factions.Count);
            character.AssignFaction(factions[randomFactionIndex]);
        }

        // Assign Pet
        if (pets != null && pets.Count > 0) {
             int randomPetIndex = UnityEngine.Random.Range(0, pets.Count);
             character.AssignPet(pets[randomPetIndex]);
        }


        // --- REMOVED BoostAttribute Call ---
        // Boosting base stats directly is usually done via spending points later.
        // If you want STARTING variations, adjust the base stats in the Class Factory scripts.
        /*
        if (attributes != null && attributes.Count > 0) {
            int randomAttributeIndex = UnityEngine.Random.Range(0, attributes.Count);
            int randomAmount = UnityEngine.Random.Range(1, 4); // Small starting boost
            // character.BoostAttribute(attributes[randomAttributeIndex], randomAmount); // <<< OLD LINE REMOVED
            // Replace with status effect or modify base in factory if needed.
             Debug.Log($"TEMP REMOVED: Character creation attribute boost.");
        }
        */

        return character;
    }

    // LoadNextScene is handled by SceneLoader.cs attached to the Play button now
    // void LoadNextScene() { ... }
}