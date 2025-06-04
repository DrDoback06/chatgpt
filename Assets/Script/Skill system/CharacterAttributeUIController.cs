using UnityEngine;
using UnityEngine.UI; // Original uses Text, switch if using TMPro
using TMPro; // Recommended for better text

public class CharacterAttributesUIController : MonoBehaviour
{
    // Use EITHER Text OR TMP_Text based on your UI setup
    [Header("UI References (TextMeshPro)")]
    [Tooltip("Text element to display Strength value.")]
    [SerializeField] private TMP_Text strengthText;
    [Tooltip("Text element to display Agility value.")]
    [SerializeField] private TMP_Text agilityText;
    [Tooltip("Text element to display Intelligence value.")]
    [SerializeField] private TMP_Text intelligenceText;
    [Tooltip("Text element to display Vitality value.")]
    [SerializeField] private TMP_Text vitalityText;
    [Tooltip("Text element to display remaining Stat Points.")] // Added Stat Points display here
    [SerializeField] private TMP_Text statPointsText;

    // Reference to Character - Can be set via Inspector OR fetched from Manager
    // Option 1: Assign manually if this UI is specific to one character context
    // [SerializeField] private Character character;
    // Option 2 (More common): Get from CharacterManager
    private Character character;

    void Start()
    {
        // Try to get character reference
        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            character = CharacterManager.Instance.character;

            // Subscribe to relevant character events
            character.OnAttributesChanged += UpdateCharacterAttributesUI; // Base attribute changes
             character.OnStatsChanged += UpdateStatPointsUI; // Stat point changes

            // Initial UI Update
            UpdateCharacterAttributesUI();
            UpdateStatPointsUI(); // Update points display initially too
        }
        else
        {
            Debug.LogError("CharacterAttributesUIController: Character data not found! UI cannot initialize.", this);
            // Optionally disable the panel or clear text fields
             ClearUIFields();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events when this UI object is destroyed
        if (character != null)
        {
            character.OnAttributesChanged -= UpdateCharacterAttributesUI;
             character.OnStatsChanged -= UpdateStatPointsUI;
        }
    }

    /// <summary>
    /// Updates the UI text elements displaying the character's base attributes.
    /// Called by event handler when base attributes change.
    /// </summary>
    public void UpdateCharacterAttributesUI() // Keep public if called externally? Usually private if only event-driven.
    {
        if (character == null || character.attributes == null) return; // Safety check

        // Update text fields only if they are assigned
        if (strengthText != null) strengthText.text = "Strength: " + character.attributes.Strength;
        if (agilityText != null) agilityText.text = "Agility: " + character.attributes.Agility;
        if (intelligenceText != null) intelligenceText.text = "Intelligence: " + character.attributes.Intelligence;
        if (vitalityText != null) vitalityText.text = "Vitality: " + character.attributes.Vitality;
    }

    /// <summary>
    /// Updates the UI text element displaying the character's available Stat Points.
    /// Called by event handler when Stat Points change.
    /// </summary>
     private void UpdateStatPointsUI() // Should likely be private if only event-driven
     {
        if (character != null && statPointsText != null)
        {
            statPointsText.text = $"Stat Points Available: {character.StatPoints}";
            // Optionally add visual indicator if points > 0
            // statPointsText.color = (character.StatPoints > 0) ? Color.yellow : Color.white;
        }
     }

     /// <summary>
     /// Clears the UI text fields, e.g., if character data is unavailable.
     /// </summary>
     private void ClearUIFields()
     {
         if (strengthText != null) strengthText.text = "Strength: -";
         if (agilityText != null) agilityText.text = "Agility: -";
         if (intelligenceText != null) intelligenceText.text = "Intelligence: -";
         if (vitalityText != null) vitalityText.text = "Vitality: -";
          if (statPointsText != null) statPointsText.text = "Stat Points Available: -";
     }
}