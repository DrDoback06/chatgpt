using UnityEngine;
using UnityEngine.UI; // Required for Button

[RequireComponent(typeof(Button))] // Ensure Button component exists
public class AttributeButton : MonoBehaviour
{
    [Tooltip("The exact name of the attribute this button increases (e.g., 'Strength', 'Vitality'). Case-sensitive.")]
    [SerializeField] private string attributeName; // Assign this in Inspector for each button

    private Button button;

    void Awake() // Use Awake to get component reference
    {
        button = GetComponent<Button>();
    }

    void Start() // Add listener in Start after potential UI setup
    {
        // Add listener ONLY if attributeName is set
        if (!string.IsNullOrEmpty(attributeName))
        {
            button.onClick.AddListener(AttemptAddAttributePoint);
        }
        else
        {
            Debug.LogError($"AttributeButton on '{gameObject.name}' is missing 'attributeName'! Button will be disabled.", this);
            button.interactable = false; // Disable button if not configured
        }

         // Optional: Subscribe here to update interactability, or rely on a parent UI controller
         // if (CharacterManager.Instance?.character != null)
         // {
         //    CharacterManager.Instance.character.OnStatsChanged += UpdateInteractability;
         //    UpdateInteractability(); // Initial check
         // }
    }

    // Optional: Unsubscribe if needed
     // void OnDestroy()
     // {
     //     if (CharacterManager.Instance?.character != null)
     //     {
     //         CharacterManager.Instance.character.OnStatsChanged -= UpdateInteractability;
     //     }
     // }


    /// <summary>
    /// Called when the button is clicked. Attempts to increase the assigned attribute.
    /// </summary>
    private void AttemptAddAttributePoint()
    {
        // Check if CharacterManager and character exist
        if (CharacterManager.Instance == null || CharacterManager.Instance.character == null)
        {
            Debug.LogError($"AttributeButton cannot find Character data from CharacterManager!", this);
            return;
        }

        Character character = CharacterManager.Instance.character;

        // Attempt to increase the attribute using the character method
        bool success = character.IncreaseAttribute(attributeName, 1); // Increase by 1 point

        if (success)
        {
            Debug.Log($"Attribute '{attributeName}' point added via button.");
            // Play success sound effect?
            // UIManager.Instance?.PlayUISound(successSound);

            // UI Update Notes:
            // - The CharacterAttributesUIController (or similar) should listen to
            //   Character.OnAttributesChanged to update the attribute value display.
            // - The UI displaying remaining Stat Points (e.g., in GameController or an Attributes Panel)
            //   should listen to Character.OnStatsChanged to update its text.
            // - This button's interactability (can we afford the point?) should also update based on StatPoints.
             // UpdateInteractability(); // Update this button immediately
        }
        else
        {
            Debug.LogWarning($"Failed to add attribute point to '{attributeName}'. Insufficient points ({character.StatPoints}) or invalid attribute name?", this);
            // Play failure sound effect?
             // UIManager.Instance?.PlayUISound(failureSound);
        }
    }

     /// <summary>
     /// Updates whether this button can be clicked based on available stat points.
     /// Should be called initially and whenever stat points change.
     /// </summary>
     // private void UpdateInteractability()
     // {
     //     if (button == null || CharacterManager.Instance?.character == null) return;
     //
     //     button.interactable = (CharacterManager.Instance.character.StatPoints > 0);
     // }
}