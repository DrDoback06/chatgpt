using UnityEngine;

public class UiToggleOnOff : MonoBehaviour
{
    [Header("Target UI")]
    [Tooltip("The UI Panel (GameObject) to toggle active state.")]
    [SerializeField] private GameObject uiPanelToToggle; // Renamed for clarity

    [Header("Control")]
    [Tooltip("The key used to toggle the UI panel.")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I; // Example: 'I' for Inventory

    [Header("Time Scale (Optional)")]
    [Tooltip("Should opening this UI pause the game time? Uses UIManager.")]
    [SerializeField] private bool pauseTimeWhenOpen = true;

    private bool isPanelOpen = false; // Track the state

    void Start()
    {
        // Input Validation
        if (uiPanelToToggle == null)
        {
            Debug.LogError($"UiToggleOnOff on '{gameObject.name}' is missing its UI Panel reference! Disabling script.", this);
            enabled = false; // Disable script if no target
            return;
        }

        // Initialize UI state (start closed by default)
        isPanelOpen = uiPanelToToggle.activeSelf;
        // Optional: Force closed on start?
        // uiPanelToToggle.SetActive(false);
        // isPanelOpen = false;
    }

    void Update()
    {
        // Check for toggle key press
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }

    /// <summary>
    /// Toggles the associated UI panel's visibility and handles time scale changes.
    /// </summary>
    public void TogglePanel()
    {
        if (uiPanelToToggle == null) return; // Safety check

        isPanelOpen = !isPanelOpen;
        uiPanelToToggle.SetActive(isPanelOpen);
        Debug.Log($"Toggled UI Panel '{uiPanelToToggle.name}'. New state: {(isPanelOpen ? "Open" : "Closed")}");

        // Handle Time Scale via UIManager if configured
        if (pauseTimeWhenOpen && UIManager.Instance != null)
        {
            if (isPanelOpen)
            {
                UIManager.Instance.PauseGameplayTime();
            }
            else
            {
                UIManager.Instance.ResumeGameplayTime();
            }
        } else if (pauseTimeWhenOpen && UIManager.Instance == null) {
             Debug.LogWarning($"UiToggleOnOff on '{gameObject.name}' wants to pause time, but UIManager.Instance is not found!");
        }
    }
}