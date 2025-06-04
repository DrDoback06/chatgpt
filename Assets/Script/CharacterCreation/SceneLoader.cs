using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Button

[RequireComponent(typeof(Button))] // Attach this to the Button GameObject
public class SceneLoader : MonoBehaviour
{
    [Tooltip("The name of the scene to load when the button is clicked.")]
    [SerializeField] private string sceneToLoad = "Town"; // Make configurable

    [Header("Button Reference (Optional)")]
    [Tooltip("Button to control interactability. If null, tries GetComponent<Button>().")]
    [SerializeField] private Button loadSceneButton;

    private bool isCharacterReady = false; // Track readiness

    void Awake()
    {
        // Get button reference if not assigned
        if (loadSceneButton == null)
        {
            loadSceneButton = GetComponent<Button>();
        }

        if (loadSceneButton == null)
        {
             Debug.LogError("SceneLoader: Button component not found!", this);
             enabled = false; // Cannot function without a button
             return;
        }

        // Add listener programmatically
        loadSceneButton.onClick.RemoveAllListeners(); // Clear existing
        loadSceneButton.onClick.AddListener(LoadTargetScene);

        // Initial state: Button disabled until character ready
        loadSceneButton.interactable = false;
    }

    void Update()
    {
        // Continuously check if the character is ready ONLY IF it wasn't ready before.
        if (!isCharacterReady)
        {
            // Check CharacterManager instance and its character data
            if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
            {
                 isCharacterReady = true;
                 // Enable the button ONLY IF the component is still enabled
                 if (enabled && loadSceneButton != null) {
                      loadSceneButton.interactable = true;
                 }
            }
        }
        // No need for an 'else' block to disable it again, assume character remains ready.
    }

    /// <summary>
    /// Loads the specified scene. Called by the button's onClick event.
    /// </summary>
    public void LoadTargetScene()
    {
        // Double-check character exists before loading (safety)
        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
             // Perform any pre-scene-load actions here if needed (e.g., saving state)
             Debug.Log($"Loading scene: {sceneToLoad}...");
             SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Character not ready or lost. Cannot load target scene.", this);
            // Optionally show a message to the player
        }
    }
}