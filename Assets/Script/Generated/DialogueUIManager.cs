using UnityEngine;
using TMPro; // Use TextMeshPro
using UnityEngine.UI; // Need Button
using System.Collections.Generic; // Need List
using System; // Need Action

// Manages displaying dialogue text and handling player response options.
public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance; // Optional Singleton

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // The root panel
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Transform optionsContainer; // Parent for option buttons
    [SerializeField] private GameObject optionButtonPrefab; // Prefab for an option button

    private List<GameObject> currentOptionButtons = new List<GameObject>();
    private Action currentCleanupAction; // Action to call when dialogue closes

    void Awake() {
        // Singleton setup
        if (Instance == null) { Instance = this; /* Optional DontDestroyOnLoad */ }
        else { Destroy(gameObject); return; }

        // Start hidden
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
         else Debug.LogError("DialogueUIManager: Dialogue Panel reference not set!");
    }

    /// <summary>
    /// Shows dialogue with optional response buttons.
    /// </summary>
    /// <param name="npcName">Name of the speaking NPC.</param>
    /// <param name="text">The dialogue text.</param>
    /// <param name="options">List of options (text + action callback).</param>
     /// <param name="onCloseAction">Action to run when dialogue naturally closes (no option picked).</param>
    public void ShowDialogue(string npcName, string text, List<DialogueOption> options = null, Action onCloseAction = null)
    {
         if (dialoguePanel == null) return;

        // --- Pause Game? ---
        UIManager.Instance?.PauseGameplayTime(); // Optional: Pause game during dialogue

        npcNameText.text = npcName;
        dialogueText.text = text;
        currentCleanupAction = onCloseAction;

        // Clear previous options
        foreach (GameObject button in currentOptionButtons) Destroy(button);
        currentOptionButtons.Clear();

        // Create new option buttons
        if (options != null && options.Count > 0 && optionButtonPrefab != null && optionsContainer != null)
        {
            foreach (DialogueOption option in options)
            {
                GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer);
                TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>(); // Find text on button prefab
                Button buttonComp = buttonGO.GetComponent<Button>();

                if (buttonText != null) buttonText.text = option.Text;
                if (buttonComp != null)
                {
                    buttonComp.onClick.AddListener(() => {
                        CloseDialogue(); // Close panel first
                        option.OnSelectCallback?.Invoke(); // Then execute option action
                    });
                }
                currentOptionButtons.Add(buttonGO);
            }
            // Simple: just enable container. Could use LayoutGroup for better arrangement.
            optionsContainer.gameObject.SetActive(true);
        } else if (optionsContainer != null) {
            optionsContainer.gameObject.SetActive(false); // Hide container if no options
        }


        dialoguePanel.SetActive(true);

         // TODO: Handle advancing dialogue (if text is long, or multiple pages)
         // Maybe require player click or key press to continue/close if no options?
    }

    /// <summary>
    /// Closes the dialogue panel and resumes game time.
    /// </summary>
    public void CloseDialogue()
    {
        if (dialoguePanel == null) return;

        dialoguePanel.SetActive(false);
        currentCleanupAction?.Invoke(); // Run cleanup if any specified
        currentCleanupAction = null; // Clear cleanup action

         // --- Resume Game ---
        UIManager.Instance?.ResumeGameplayTime(); // Resume time if dialogue paused it
    }
}

// Helper class for dialogue options
public class DialogueOption
{
    public string Text { get; private set; }
    public Action OnSelectCallback { get; private set; } // What happens when selected

    public DialogueOption(string text, Action onSelect)
    {
        Text = text;
        OnSelectCallback = onSelect;
    }
}