using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Visuals (Optional)")]
    [SerializeField] private TMP_Text quantityText;

    // Data
    private Item itemData;
    private int amount;
    private bool canPickup = true;

    void Awake() {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    /// <summary>
    /// Initializes the pickup with item data, quantity, and sets the correct WORLD sprite.
    /// </summary>
    public void Initialize(Item item, int quantity) {
        itemData = item;
        amount = Mathf.Max(1, quantity);
        canPickup = true;

        // --- Update Visuals ---
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && itemData != null) {
            // *** Use the Item's helper method to get the WORLD sprite based on its ROLLED rarity ***
            sr.sprite = itemData.GetWorldSpriteForCurrentRarity(); // <<< GET WORLD SPRITE
        }

        gameObject.name = $"Pickup_{itemData?.itemName ?? "UNKNOWN"}{(amount > 1 ? $"_x{amount}" : "")}";

        // Update quantity text
        if (quantityText != null) {
            quantityText.enabled = (amount > 1);
            if(quantityText.enabled) quantityText.text = amount.ToString();
        }
    }

    // --- Player Interaction ---
    private void OnTriggerEnter2D(Collider2D other) {
        if (canPickup && other.CompareTag("Player") && itemData != null) {
            // Debug.Log($"Player touched pickup: {itemData.itemName} x{amount}");
            Inventory playerInventory = CharacterManager.Instance?.character?.inventory;
            if (playerInventory != null) {
                if (playerInventory.AddItem(itemData, amount)) { // Try adding to inventory
                    // Debug.Log($"Picked up {amount} x {itemData.itemName}.");
                    canPickup = false; // Prevent double pickup
                    AudioManager.Instance?.PlaySoundEffect(null); // TODO: Assign pickup SFX
                    Destroy(gameObject); // Destroy pickup object
                }
                 // else { Debug.Log($"Inventory full for {itemData.itemName}"); }
            }
             // else { Debug.LogError("Cannot pickup - Player inventory not found!"); }
        }
    }
}