using UnityEngine;
using UnityEngine.UI; // Needed for Image, Button
using TMPro; // Use TextMeshPro
using UnityEngine.EventSystems; // Needed for EventTrigger logic

// Manages the UI elements and interactions for a single skill button in the Skill Tree.
// Attached to the Skill Button Prefab.
[RequireComponent(typeof(Button))] // Might need Button if upgradeButton isn't the root object
[RequireComponent(typeof(EventTrigger))] // EventTrigger is added in Start
public class SkillButtonUI : MonoBehaviour
{
    [Header("UI References (Assign in Prefab)")]
    [Tooltip("Image component to display the skill's icon.")]
    [SerializeField] private Image skillIcon;
    [Tooltip("Text component to display the skill's current/max level.")]
    [SerializeField] private TMP_Text skillLevelText; // Renamed for clarity
    [Tooltip("Button component used for upgrading the skill.")]
    [SerializeField] private Button upgradeButton;

    // Data & Controller References (Set via Initialize)
    private Skill skill;
    private SkillTreeUIController skillTreeUIController; // Main Skill Tree UI Manager
    private SkillDetailsToggle skillDetailsToggle;    // UI Panel for showing details
    private Character characterData; // Added reference to check requirements directly

    /// <summary>
    /// Initializes the button with references and updates the display.
    /// Called by SkillTreeUIController when creating buttons.
    /// </summary>
    public void Initialize(Skill skillData, SkillTreeUIController controller, SkillDetailsToggle detailsPanel, Character character)
    {
        this.skill = skillData;
        this.skillTreeUIController = controller;
        this.skillDetailsToggle = detailsPanel;
        this.characterData = character; // Store character reference

        // --- Validate References ---
        if (skill == null || skillTreeUIController == null || skillDetailsToggle == null || characterData == null ||
            skillIcon == null || skillLevelText == null || upgradeButton == null)
        {
            Debug.LogError($"SkillButtonUI Initialize failed on '{gameObject.name}': Missing reference(s). Disabling.", this);
            gameObject.SetActive(false);
            return;
        }

        // --- Setup Button Listener ---
        upgradeButton.onClick.RemoveAllListeners(); // Clear previous listeners
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick); // Add current listener

        // --- Setup Hover Events ---
        SetupHoverEvents();

        // --- Initial Visual Update ---
        UpdateUI();

        // --- Subscribe to Changes (Driven by Controller Now) ---
        // No longer subscribing to skillTreeUIController.OnSkillPointsChanged here.
        // The SkillTreeUIController will call UpdateAllButtonStates or UpdateSkillButtonState as needed.
    }

    /// <summary>
    /// Gets the Skill associated with this button.
    /// </summary>
    public Skill GetSkill() => skill;


    /// <summary>
    /// Sets up Pointer Enter/Exit events using EventTrigger component.
    /// </summary>
    private void SetupHoverEvents()
    {
        EventTrigger trigger = GetComponent<EventTrigger>() ?? gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear(); // Clear existing triggers to prevent duplicates

        // --- Pointer Enter ---
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnterEntry.callback.AddListener((eventData) =>
        {
            if (skillDetailsToggle != null && skill != null)
                skillDetailsToggle.ShowSkillDetails(skill);
        });
        trigger.triggers.Add(pointerEnterEntry);

        // --- Pointer Exit ---
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExitEntry.callback.AddListener((eventData) =>
        {
            if (skillDetailsToggle != null)
                skillDetailsToggle.HideSkillDetails();
        });
        trigger.triggers.Add(pointerExitEntry);
    }

    /// <summary>
    /// Called when the upgrade button (+) is clicked.
    /// Tells the SkillTreeUIController to try upgrading this skill.
    /// </summary>
    public void OnUpgradeButtonClick() // <<< Ensure only ONE of this method exists
    {
        if (skillTreeUIController != null && skill != null)
        {
            skillTreeUIController.RequestSkillUpgrade(skill);
            // UI update is handled by SkillTreeUIController triggering UpdateAllButtonStates or UpdateSkillButtonState
        }
        else
        {
            Debug.LogError("OnUpgradeButtonClick: Controller or Skill is null!", this);
        }
    }

    /// <summary>
    /// Updates the visual elements: Icon, Level Text, Upgrade Button Interactability.
    /// Called by Initialize and potentially by SkillTreeUIController.
    /// </summary>
    public void UpdateUI() // <<< Ensure only ONE of this method exists
    {
        // --- Safety Checks ---
        if (skill == null || characterData == null || skillIcon == null || skillLevelText == null || upgradeButton == null)
        {
            // Debug.LogWarning($"UpdateUI called on {gameObject.name} but references are missing.", this); // Can be spammy
            return; // Exit if essential references are missing
        }

        // --- Update Icon ---
        // Use icon assigned in Skill definition (loaded by BlademasterClass etc.)
        skillIcon.sprite = skill.icon;
        skillIcon.enabled = (skill.icon != null);
        // Removed Resources.Load - icons should be assigned when Skill is created.

        // --- Update Level Text ---
        string maxLevelStr = skill.maxLevel.HasValue ? skill.maxLevel.Value.ToString() : "∞";
        skillLevelText.text = $"{skill.currentLevel} / {maxLevelStr}";

        // --- Update Upgrade Button Interactability ---
        bool canUpgrade = true;
        // Check 1: Can the skill intrinsically level up?
        if (!skill.CanLevelUp()) canUpgrade = false;
        // Check 2: Does character meet level requirement?
        if (characterData.Level < skill.requiredLevel) canUpgrade = false;
        // Check 3: Does character have enough skill points?
        if (characterData.SkillPoints < skill.requiredSkillPoints) canUpgrade = false;

        upgradeButton.interactable = canUpgrade;
    }

    // Removed Start() - Setup is done in Initialize
    // Removed OnDestroy() - Event subscription handled by controller / unnecessary now

} // <<< Make sure this is the final closing brace for the class