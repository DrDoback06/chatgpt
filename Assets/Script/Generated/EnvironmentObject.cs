using UnityEngine;

// Ensure object has a Collider2D set to be a Trigger
[RequireComponent(typeof(Collider2D))]
public class EnvironmentObject : MonoBehaviour
{
    public enum ObjectType { Trap, HealingZone, InteractableShrine, Other } // Added example type

    [Tooltip("Type of the environmental object.")]
    [SerializeField] private ObjectType objectType = ObjectType.Other;

    [Tooltip("Effects that this object can trigger (e.g., damage amount, healing rate, status effect name). Use appropriate values based on Object Type.")]
    [SerializeField] private float effectValue = 10f; // Example: 10 damage for trap, 10 healing per second for zone
    [SerializeField] private string statusEffectName = "Poison"; // Example: If it applies a status effect

    // Removed interactionRange - Trigger interaction is better for 2D
    // [Tooltip("Interaction range for this object")]
    // public float interactionRange;

    private bool playerInside = false; // Track if player is currently in the trigger zone (for continuous effects)
    private Character playerCharacterData; // Reference to the player's data if needed for effects

    void Start()
    {
        // Ensure the Collider2D is set to be a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"EnvironmentObject '{gameObject.name}' requires its Collider2D to be a Trigger. Setting it now.", this);
            col.isTrigger = true;
        }
        else if (col == null)
        {
             Debug.LogError($"EnvironmentObject '{gameObject.name}' is missing a Collider2D component!", this);
        }
    }

    // --- Trigger Interaction (Recommended Method) ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the Player
        // Assumes player GameObject has the "Player" tag
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered EnvironmentObject zone: {gameObject.name} ({objectType})");
            playerInside = true;

            // Try to get player data (might need PlayerController reference)
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && CharacterManager.Instance != null)
            {
                playerCharacterData = CharacterManager.Instance.character;
            }


            // Trigger instant effects based on type
            switch (objectType)
            {
                case ObjectType.Trap:
                    TriggerTrap(playerController);
                    break;
                case ObjectType.InteractableShrine:
                    // Potentially show an "Interact [E]" prompt here
                    break;
                 // Healing zones often apply effects over time (handled in Update)
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited EnvironmentObject zone: {gameObject.name} ({objectType})");
            playerInside = false;
            playerCharacterData = null; // Clear reference when player leaves
            // Stop any continuous effects if needed
        }
    }

    // --- Continuous Effects / Update Logic ---

    void Update()
    {
        // Apply effects that happen over time while player is inside
        if (playerInside)
        {
            switch (objectType)
            {
                case ObjectType.HealingZone:
                    ApplyHealingOverTime();
                    break;
                // Other continuous effects? (e.g., continuous damage aura)
            }
        }

        // --- Interaction Prompt Handling (Example) ---
        // if (objectType == ObjectType.InteractableShrine && playerInside)
        // {
        //    // Show UI Prompt
        //    // Check if player presses interaction key (e.g., 'E' in PlayerController)
        //    if (Input.GetKeyDown(KeyCode.E)) // Note: Input handling is better in PlayerController
        //    {
        //        ActivateShrine();
        //    }
        // }
    }

    // --- Effect Implementations (Placeholders) ---

    private void TriggerTrap(PlayerController playerController)
    {
        if (playerCharacterData != null)
        {
            Debug.Log($"TRAP ACTIVATED on {playerCharacterData.characterName}! Dealing {effectValue} damage.");
            // --- Actual Implementation ---
            // playerCharacterData.TakeDamage(effectValue); // Need a TakeDamage method on Character
            // ApplyStatusEffect(playerCharacterData, statusEffectName);
        }
         else {
             Debug.LogWarning($"Trap activated, but couldn't get player character data for {gameObject.name}");
         }

        // Optional: Play sound/visual effect, destroy trap after activation?
         // Destroy(gameObject, 0.5f); // Self-destruct after triggering
    }

    private void ApplyHealingOverTime()
    {
        if (playerCharacterData != null)
        {
            // Calculate healing amount based on delta time
            float healAmount = effectValue * Time.deltaTime;
             // Ensure player character has a method to receive healing
             // playerCharacterData.Heal(healAmount);
             if (playerCharacterData.CurrentHealth < playerCharacterData.MaxHealth) {
                // Only log if actually healing needed to avoid spam
                Debug.Log($"Healing {playerCharacterData.characterName} for {healAmount:F2} HP.");
             }
        }
    }

     private void ActivateShrine()
     {
         if (playerCharacterData != null)
         {
             Debug.Log($"Shrine Activated by {playerCharacterData.characterName}! Applying effect '{statusEffectName}' for {effectValue} seconds.");
             // Apply temporary buff, grant item, restore resources etc.
             // ApplyStatusEffect(playerCharacterData, statusEffectName, effectValue); // Pass duration?
         }
         // Optional: Play effect, disable shrine temporarily
         // gameObject.GetComponent<Collider2D>().enabled = false; // Disable interaction
         // Invoke(nameof(ReEnableShrine), 60f); // Re-enable after 60 seconds
     }

     // Placeholder for applying status effects (needs a Status Effect system)
     // private void ApplyStatusEffect(Character character, string effectName, float duration = 0) { ... }

     // Placeholder for re-enabling shrine
     // private void ReEnableShrine() { gameObject.GetComponent<Collider2D>().enabled = true; }

}