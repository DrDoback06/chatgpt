using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Singleton Instantiation
    public static UIManager Instance;

    [Header("Managed UI Elements")]
    [Tooltip("Array of UI Canvases that should persist across scene loads. Ensure these are top-level canvases.")]
    [SerializeField] private Canvas[] persistentUiCanvases; // Renamed for clarity

    [Header("Inventory Time Scale")]
    [Tooltip("Time scale value when inventory (or any UI that pauses gameplay) is opened.")]
    [SerializeField] [Range(0f, 1f)] private float pausedTimeScale = 0f; // Often set to 0 for full pause

    private float previousTimeScale = 1f; // Store the time scale before pausing

    // --- Unity Lifecycle Methods ---

    private void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist the UIManager itself

            // Persist designated canvases
            foreach (Canvas canvas in persistentUiCanvases)
            {
                if (canvas != null)
                {
                    // Check if the canvas root GameObject needs DontDestroyOnLoad
                    // Usually best if the canvas has its own root GameObject
                    DontDestroyOnLoad(canvas.gameObject);
                }
            }
            Debug.Log("UIManager Instance created and persistent canvases marked.");
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Duplicate UIManager detected. Destroying this instance.", this.gameObject);
            Destroy(gameObject);
            return;
        }
    }

    // --- Time Scale Management ---

    /// <summary>
    /// Pauses or slows down game time. Call this when opening UI like Inventory, Skill Tree, Menu.
    /// Stores the previous time scale.
    /// </summary>
    public void PauseGameplayTime()
    {
        // Only store previous time scale if not already paused
        if (Time.timeScale > pausedTimeScale)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = pausedTimeScale;
            Debug.Log($"Gameplay time paused. TimeScale set to {pausedTimeScale}");
        } else {
            Debug.LogWarning("Attempted to pause time, but it's already paused or slower.");
        }
    }

    /// <summary>
    /// Resumes game time to the state it was in before pausing.
    /// Call this when closing UI that paused the game.
    /// </summary>
    public void ResumeGameplayTime()
    {
        // Only resume if currently paused by this manager
        if (Time.timeScale == pausedTimeScale)
        {
            Time.timeScale = previousTimeScale;
            Debug.Log($"Gameplay time resumed. TimeScale set to {previousTimeScale}");
        } else {
             Debug.LogWarning("Attempted to resume time, but it wasn't paused by UIManager or was changed externally.");
             // Force resume to 1f as a fallback?
             // Time.timeScale = 1f;
        }
    }

    // --- Potential Future Additions ---
    // - Methods to Show/Hide specific UI panels (e.g., ShowConfirmationPopup, HideLoadingScreen)
    // - Centralized handling of UI sound effects
    // - Management of screen resolution or UI scaling options
}