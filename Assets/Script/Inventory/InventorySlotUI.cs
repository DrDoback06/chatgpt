using UnityEngine;
using UnityEngine.UI; // Need Image, Button potentially
using UnityEngine.EventSystems; // Need interfaces for Drag, Drop, Hover
using TMPro; // Use TextMeshPro

// Manages the visuals and interaction for a single UI slot (inventory or equipment).
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler // Added Click Handler
{
    [Header("UI References (Assign in Prefab)")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text stackAmountText;
    [SerializeField] private Image highlightImage; // Optional: For showing selection/hover

    // Data Reference (Set by InventoryUI)
    private Inventory.InventorySlot representedInventorySlot; // The actual data slot this UI represents
    private InventoryUI parentInventoryUI; // Reference to the main UI manager

    // Properties to access internal data safely
    public Inventory.InventorySlot RepresentedSlot => representedInventorySlot;
    public bool HasItem => representedInventorySlot != null && !representedInventorySlot.IsEmpty();

    // --- Initialization ---

    /// <summary>
    /// Links this UI element to its corresponding data slot and parent UI.
    /// </summary>
    public void Initialize(Inventory.InventorySlot dataSlot, InventoryUI parentUI)
    {
        representedInventorySlot = dataSlot;
        parentInventoryUI = parentUI;

        // Subscribe to changes *for this specific slot* if the Inventory class provides such events,
        // OR rely on the parent InventoryUI to tell this slot to UpdateDisplay when needed.
        // For simplicity now, we rely on parentInventoryUI.UpdateSlotDisplay(this).

        UpdateDisplay(); // Set initial visuals
    }

    /// <summary>
    /// Updates the icon, stack text, and other visuals based on the represented data slot.
    /// </summary>
    public void UpdateDisplay()
    {
        if (itemIconImage == null || stackAmountText == null) return; // Check references

        if (representedInventorySlot != null && !representedInventorySlot.IsEmpty())
        {
            // Slot has an item
            itemIconImage.sprite = representedInventorySlot.Item.icon; // Make sure item has an icon assigned!
            itemIconImage.enabled = true;

            // Show stack amount only if > 1
            if (representedInventorySlot.Item.isStackable && representedInventorySlot.Amount > 1)
            {
                stackAmountText.text = representedInventorySlot.Amount.ToString();
                stackAmountText.enabled = true;
            }
            else
            {
                stackAmountText.text = ""; // Don't show "1"
                stackAmountText.enabled = false;
            }
        }
        else
        {
            // Slot is empty
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            stackAmountText.text = "";
            stackAmountText.enabled = false;
        }

        // Hide highlight by default
        if (highlightImage != null) highlightImage.enabled = false;
    }

    // --- Pointer Hover Events (for Tooltips) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parentInventoryUI != null && HasItem)
        {
            parentInventoryUI.ShowTooltip(representedInventorySlot.Item, GetComponent<RectTransform>());
        }
        // Optional: Show highlight on hover
        // if (highlightImage != null) highlightImage.enabled = HasItem; // Only highlight if item present?
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (parentInventoryUI != null)
        {
            parentInventoryUI.HideTooltip();
        }
         // Optional: Hide highlight on exit
        // if (highlightImage != null) highlightImage.enabled = false;
    }

    // --- Drag and Drop Implementation ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (HasItem && parentInventoryUI != null)
        {
            // Tell the parent UI that dragging has started from this slot
            parentInventoryUI.StartDraggingItem(this);
            itemIconImage.raycastTarget = false; // Disable raycast on icon while dragging
            // Debug.Log($"Begin Drag: {representedInventorySlot.Item.itemName}");
        }
        else
        {
            eventData.pointerDrag = null; // Cancel drag if slot is empty
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // The parent UI will move the drag icon representation
        if (parentInventoryUI != null)
        {
             parentInventoryUI.UpdateDraggedItemPosition(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
         if (parentInventoryUI != null) {
             // Tell parent UI drag ended. Parent handles logic if dropped outside valid slot.
             parentInventoryUI.StopDraggingItem(eventData.pointerEnter); // Pass what it was dropped on
             itemIconImage.raycastTarget = true; // Re-enable raycast
             // Debug.Log($"End Drag");
         }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // This method is called on the SLOT WHERE THE ITEM WAS DROPPED.
        GameObject draggedObject = eventData.pointerDrag; // The object being dragged (which has the source InventorySlotUI)
        if (draggedObject != null)
        {
            InventorySlotUI sourceSlotUI = draggedObject.GetComponent<InventorySlotUI>();
            if (sourceSlotUI != null && sourceSlotUI != this && parentInventoryUI != null) // Ensure it's a valid slot drag and not dropping on itself
            {
                 // Tell parent UI to handle the swap/merge between source and this slot
                 parentInventoryUI.HandleItemDrop(sourceSlotUI, this);
                 // Debug.Log($"Dropped item from slot {sourceSlotUI.representedInventorySlot?.inventoryIndex ?? -1} onto slot {representedInventorySlot?.inventoryIndex ?? -1}");
            }
        }
    }

    // --- Handle Clicks ---
    public void OnPointerClick(PointerEventData eventData)
    {
         if (!HasItem) return; // Ignore clicks on empty slots

         // Right Mouse Button Click (Common for 'Use' or context menu)
         if (eventData.button == PointerEventData.InputButton.Right)
         {
             Debug.Log($"Right-clicked on {representedInventorySlot.Item.itemName}");
             TryUseItem();
         }
         // Left Mouse Button Click (Could be for equipping if not dragging)
         // else if (eventData.button == PointerEventData.InputButton.Left)
         // {
              // Check for double click? Or handle equip here if not dragging?
              // if (eventData.clickCount == 2) { EquipOrUse(); }
         // }
    }

    private void TryUseItem()
    {
        if (representedInventorySlot != null && !representedInventorySlot.IsEmpty())
        {
             // Check if item is consumable or has another 'Use' action
            Item item = representedInventorySlot.Item;
            if (item.itemType == Item.ItemType.Consumable) // Or any other type you want usable
            {
                 // 1. Get character reference
                 Character character = CharacterManager.Instance?.character;
                 if (character != null) {
                     // 2. Call the item's Use method
                     item.Use(character); // Item logic applies effect

                     // 3. Remove ONE instance from the inventory slot's stack
                     // The Inventory data class should handle this removal
                     Inventory inventory = character.inventory;
                     if (inventory != null) {
                          // How to best reference the slot? Use its index or equipment type.
                          if (representedInventorySlot.inventoryIndex >= 0) {
                             // Need a way to remove from a specific slot index, or just by itemID
                             inventory.RemoveItem(item.itemID, 1); // Simple removal by ID
                             // OR create Inventory.RemoveFromSlot(index, amount) method
                             // inventory.RemoveFromSlot(representedInventorySlot.inventoryIndex, 1);
                          } else {
                               Debug.LogWarning("Cannot 'Use' item directly from equipment slot via right click usually.");
                          }
                     }
                     // Note: UI update happens via OnInventoryChanged event triggered by RemoveItem/RemoveFromSlot
                 }
            }
             else {
                Debug.Log($"{item.itemName} is not a usable item type.");
             }
        }
    }

     // Optional: Combine Equip/Use on left double-click or specific keybind
     // private void EquipOrUse() { ... }
}