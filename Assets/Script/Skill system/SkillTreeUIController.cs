using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Keep for potential future use with Layout Groups etc.
using TMPro; // Use TextMeshPro for better text rendering

public class SkillTreeUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Prefab for individual skill buttons.")]
    [SerializeField] private GameObject skillButtonPrefab;
    [Tooltip("Parent Transform where main class skill buttons will be instantiated.")]
    [SerializeField] private Transform mainSkillButtonContainer;
    [Tooltip("Parent Transform where sub class skill buttons will be instantiated.")]
    [SerializeField] private Transform subSkillButtonContainer; // Separate container for subclass
    [Tooltip("Reference to the panel that shows skill details on hover.")]
    [SerializeField] private SkillDetailsToggle skillDetailsToggle; // Assign in Inspector
    [Tooltip("Text element to display the player's available skill points.")]
    [SerializeField] private TMP_Text skillPointsText; // Assign in Inspector

    [Header("Attribute Display (Optional)")] // Moved attribute text here if this UI manages both
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text agilityText;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text vitalityText;


    // Character data reference
    private Character character;
    private List<SkillButtonUI> spawnedButtons = new List<SkillButtonUI>(); // Keep track of buttons to update them

    // No need for local skillPoints variable, get directly from character.
    // public event Action OnSkillPointsChanged; // We don't need this event; UI updates based on Character events

    void Start()
    {
        // Attempt to initialize immediately if character data is ready
        TryInitialize();

        // Subscribe to scene loaded event as a fallback
        // (SkillTreeUIInitializer might handle this, but belt-and-suspenders)
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
         UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
         // Unsubscribe from character events if subscribed
         if (character != null)
         {
            character.OnStatsChanged -= UpdateSkillPointsUI;
            character.OnLevelUp -= UpdateAllButtonStates; // Level affects requirements
            character.OnAttributesChanged -= UpdateCharacterAttributesUI; // If handling attributes here
         }
    }

     private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
     {
         // Try to initialize after a scene load, in case the character is ready now
         TryInitialize();
     }

     /// <summary>
     /// Attempts to get character data and initialize the UI if not already done.
     /// </summary>
     private void TryInitialize()
     {
        // Only initialize once
        if (this.character != null) return;

        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            InitializeFromCharacter(CharacterManager.Instance.character);
        }
        else
        {
             Debug.LogWarning("SkillTreeUIController: Character data not ready yet.");
             // Consider disabling the panel until initialized: gameObject.SetActive(false);
        }
     }


    /// <summary>
    /// Sets up the skill tree UI based on the provided character data.
    /// </summary>
    public void InitializeFromCharacter(Character character)
    {
        if (this.character != null) {
            Debug.LogWarning("SkillTreeUIController already initialized.");
            return; // Avoid double initialization
        }

        this.character = character;
        Debug.Log($"SkillTreeUIController Initializing for: {character.characterName}");
        // gameObject.SetActive(true); // Ensure panel is visible if disabled previously

        // Clear any old buttons (important if re-initializing)
        ClearSkillButtons();

        // Populate buttons for both main and subclass trees
        PopulateSkillButtons(character.mainClassSkillTree, mainSkillButtonContainer);
        PopulateSkillButtons(character.subClassSkillTree, subSkillButtonContainer); // Populate subclass skills

        // Initial UI updates
        UpdateSkillPointsUI();
        UpdateCharacterAttributesUI(); // If handling attributes here

        // Subscribe to character events to keep UI updated
        character.OnStatsChanged += UpdateSkillPointsUI; // Catches changes in skill points
         character.OnStatsChanged += UpdateAllButtonStates; // Also update buttons if points change
        character.OnLevelUp += UpdateAllButtonStates; // Level up affects requirements
        character.OnAttributesChanged += UpdateCharacterAttributesUI; // If handling attributes here

         // Ensure buttons reflect initial state correctly
         UpdateAllButtonStates();
    }

    private void ClearSkillButtons()
    {
        foreach (var buttonUI in spawnedButtons)
        {
            if (buttonUI != null) Destroy(buttonUI.gameObject);
        }
        spawnedButtons.Clear();

        // Also clear containers directly if buttons weren't tracked properly
        foreach (Transform child in mainSkillButtonContainer) Destroy(child.gameObject);
        foreach (Transform child in subSkillButtonContainer) Destroy(child.gameObject);
    }

    /// <summary>
    /// Instantiates skill buttons for a given skill tree and places them in the container.
    /// </summary>
    private void PopulateSkillButtons(SkillTree skillTree, Transform container)
    {
        if (skillTree == null || container == null || skillButtonPrefab == null)
        {
             Debug.LogError("Cannot populate skill buttons - Missing references (SkillTree, Container, or Prefab)");
             return;
        }

        // Optional: Sort skills first (e.g., by required level)
        // skillTree.skills.Sort((s1, s2) => s1.requiredLevel.CompareTo(s2.requiredLevel));

        foreach (Skill skill in skillTree.GetAllSkills())
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, container);
            SkillButtonUI skillButtonUI = buttonGO.GetComponent<SkillButtonUI>();

            if (skillButtonUI != null && skillDetailsToggle != null)
            {
                // Pass the Character reference to the button UI
                skillButtonUI.Initialize(skill, this, skillDetailsToggle, this.character);
                spawnedButtons.Add(skillButtonUI); // Track the button
            }
            else
            {
                 Debug.LogError($"Skill Button Prefab missing SkillButtonUI component, or SkillDetailsToggle reference is null on {gameObject.name}!", buttonGO);
                 Destroy(buttonGO); // Destroy invalid button
            }
        }
    }

    /// <summary>
    /// Called by SkillButtonUI when its upgrade button is clicked.
    /// Handles the logic for checking requirements and spending points.
    /// </summary>
    public void RequestSkillUpgrade(Skill skill)
    {
        if (character == null || skill == null) return;

        // 1. Check Character Level
        if (character.Level < skill.requiredLevel)
        {
            Debug.Log($"Upgrade failed: Character level {character.Level} too low for {skill.name} (requires {skill.requiredLevel}).");
            // Optionally provide player feedback (e.g., sound effect, UI message)
            return;
        }

        // 2. Check if Skill Can Be Leveled (Intrinsic check)
        if (!skill.CanLevelUp())
        {
            Debug.Log($"Upgrade failed: Skill {skill.name} is already at max level ({skill.currentLevel}).");
            return;
        }

        // 3. Check Skill Point Cost and Attempt to Spend
        // The Character.SpendSkillPoints method already checks if points are sufficient
        if (character.SpendSkillPoints(skill)) // This also checks skill.CanLevelUp internally now (redundant check removed from SpendSkillPoints recommended)
        {
            // 4. If points were spent successfully, actually upgrade the skill
            bool upgraded = skill.Upgrade(); // Increment skill level

            if (upgraded)
            {
                Debug.Log($"Skill '{skill.name}' successfully upgraded to level {skill.currentLevel}.");

                // 5. Update UI - Specific button and points display
                UpdateSkillPointsUI(); // Update the points text
                // Find the specific button and update its UI (or update all buttons)
                UpdateSkillButtonState(skill); // Update just the one that changed
                // UpdateAllButtonStates(); // Or update all if easier

                // Optional: Play success sound effect
            }
            else
            {
                 // This case shouldn't happen if SpendSkillPoints succeeded and CanLevelUp was true, but good practice.
                 Debug.LogError($"Skill point spending succeeded for {skill.name}, but skill.Upgrade() failed!");
            }
        }
        else
        {
            Debug.Log($"Upgrade failed: Insufficient skill points for {skill.name} (cost {skill.requiredSkillPoints}, have {character.SkillPoints}).");
            // Optionally provide feedback
        }
    }

     /// <summary>
    /// Updates the state (interactability, text) of all spawned skill buttons.
    /// </summary>
    private void UpdateAllButtonStates()
    {
         if (spawnedButtons == null) return;
         foreach (var buttonUI in spawnedButtons)
         {
             if (buttonUI != null) // Check if button still exists
             {
                 buttonUI.UpdateUI(); // Tell the button to update itself based on current character/skill state
             }
         }
    }

    /// <summary>
    /// Finds and updates the UI for a specific skill button.
    /// </summary>
    private void UpdateSkillButtonState(Skill skill)
    {
        if (spawnedButtons == null || skill == null) return;
        SkillButtonUI targetButton = spawnedButtons.Find(b => b.GetSkill() == skill);
        if (targetButton != null)
        {
            targetButton.UpdateUI();
        }
    }


    /// <summary>
    /// Updates the UI text displaying available skill points.
    /// Called by event handler.
    /// </summary>
    private void UpdateSkillPointsUI()
    {
        if (character != null && skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {character.SkillPoints}";
        }
    }

    // --- Attribute UI Handling (Keep if this panel shows attributes) ---
    /// <summary>
    /// Updates the UI text elements displaying character attributes.
    /// Called by event handler.
    /// </summary>
    public void UpdateCharacterAttributesUI()
    {
        if (character == null || character.attributes == null) return;

        if (strengthText != null) strengthText.text = "" + character.attributes.Strength;
        if (agilityText != null) agilityText.text = "" + character.attributes.Agility;
        if (intelligenceText != null) intelligenceText.text = "" + character.attributes.Intelligence;
        if (vitalityText != null) vitalityText.text = "" + character.attributes.Vitality;
    }
}