using UnityEngine;
using UnityEngine.UI; // Needed for Image, maybe Layout Groups
using UnityEngine.EventSystems; // Needed for drag/drop context
using TMPro; // Needed for TextMeshPro components
using System.Collections.Generic; // Needed for Dictionary and List
using System.Linq; // Needed for FirstOrDefault etc.

// Manages the visual representation and interaction of the player's inventory panel.
public class InventoryUI : MonoBehaviour
{
    [Header("UI Configuration")]
    [Tooltip("Prefab for the individual inventory slot UI element.")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [Tooltip("Parent transform where main inventory grid slots will be created (Ideally has GridLayoutGroup).")]
    [SerializeField] private Transform inventorySlotsParent;
    [Tooltip("Parent transform for the Head equipment slot UI element.")]
    [SerializeField] private Transform headSlotParent;
    [Tooltip("Parent transform for the Chest equipment slot UI element.")]
    [SerializeField] private Transform chestSlotParent;
    [Tooltip("Parent transform for the Weapon equipment slot UI element.")]
    [SerializeField] private Transform weaponSlotParent;
    [Tooltip("Parent transform for the OffHand equipment slot UI element.")]
    [SerializeField] private Transform offHandSlotParent;
    [Tooltip("Parent transform for the Boots equipment slot UI element.")]
    [SerializeField] private Transform bootsSlotParent;
    [Tooltip("Parent transform for the Gloves equipment slot UI element.")]
    [SerializeField] private Transform glovesSlotParent;
    [Tooltip("Parent transform for the Belt equipment slot UI element.")]
    [SerializeField] private Transform beltSlotParent;
    [Tooltip("Parent transform for the Amulet equipment slot UI element.")]
    [SerializeField] private Transform amuletSlotParent;
    [Tooltip("Parent transform for the Ring1 equipment slot UI element.")]
    [SerializeField] private Transform ring1SlotParent;
    [Tooltip("Parent transform for the Ring2 equipment slot UI element.")]
    [SerializeField] private Transform ring2SlotParent;
    // Add more parents if you add more equipment slots to the Item enum

    [Header("Drag & Drop Visuals")]
    [Tooltip("UI Image used to show the icon being dragged. Should be high in hierarchy / on top canvas layer.")]
    [SerializeField] private Image draggedItemIcon;

    [Header("Tooltip (Optional)")]
    [Tooltip("Reference to the Tooltip UI panel GameObject.")]
    [SerializeField] private GameObject tooltipPanel;
    [Tooltip("Reference to the TextMeshPro component within the Tooltip Panel.")]
    [SerializeField] private TextMeshProUGUI tooltipText; // Use TextMeshProUGUI for UI Text

    // Data Reference
    private Inventory characterInventory; // The actual inventory data

    // Runtime References to UI Elements
    private List<InventorySlotUI> inventorySlotUIs = new List<InventorySlotUI>();
    private Dictionary<Item.EquipmentSlot, InventorySlotUI> equipmentSlotUIs = new Dictionary<Item.EquipmentSlot, InventorySlotUI>();
    private InventorySlotUI currentlyDraggedSlotUI = null; // Track the UI slot being dragged

    // --- Initialization & Event Handling ---

    void Start()
    {
        // Get Inventory Data and Subscribe to Events
        if (CharacterManager.Instance?.character?.inventory != null)
        {
            characterInventory = CharacterManager.Instance.character.inventory;
            characterInventory.OnInventoryChanged += HandleInventorySlotChanged; // Method name updated for clarity
            characterInventory.OnEquipmentChanged += HandleEquipmentSlotChanged; // Method name updated

            CreateSlotUIs(); // Create the visual slots

            // Initial state for drag icon and tooltip
            if (draggedItemIcon != null) draggedItemIcon.enabled = false;
            HideTooltip();
        }
        else
        {
            Debug.LogError("InventoryUI: Character inventory not found! Disabling panel.", this);
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (characterInventory != null)
        {
            characterInventory.OnInventoryChanged -= HandleInventorySlotChanged;
            characterInventory.OnEquipmentChanged -= HandleEquipmentSlotChanged;
        }
    }

    /// <summary>
    /// Instantiates and initializes all the UI slot elements based on Inventory data.
    /// </summary>
    private void CreateSlotUIs()
    {
        if (inventorySlotPrefab == null) { Debug.LogError("InventoryUI: Slot Prefab missing!", this); return; }

        // Clear existing UI slots if any (e.g., if re-initializing)
        ClearAllSlotUIs();

        // Create Main Inventory Slots
        List<Inventory.InventorySlot> invDataSlots = characterInventory.GetInventorySlots();
        for (int i = 0; i < invDataSlots.Count; i++)
        {
            InventorySlotUI slotUI = CreateSingleSlotUI(invDataSlots[i], inventorySlotsParent, $"InvSlot_{i}");
            if (slotUI != null) inventorySlotUIs.Add(slotUI);
        }

        // Create Equipment Slots
        Dictionary<Item.EquipmentSlot, Inventory.InventorySlot> equipDataSlots = characterInventory.GetEquippedItems();
        foreach (var kvp in equipDataSlots)
        {
            Item.EquipmentSlot slotType = kvp.Key;
            Inventory.InventorySlot dataSlot = kvp.Value;
            Transform parentTransform = GetParentForEquipmentSlot(slotType);

            if (parentTransform != null)
            {
                InventorySlotUI slotUI = CreateSingleSlotUI(dataSlot, parentTransform, $"EquipSlot_{slotType}");
                if (slotUI != null) equipmentSlotUIs.Add(slotType, slotUI);
            }
            else { Debug.LogWarning($"InventoryUI: No parent assigned for Equip Slot '{slotType}'."); }
        }
        // Debug.Log($"InventoryUI created {inventorySlotUIs.Count} inv slots, {equipmentSlotUIs.Count} equip slots.");
    }

     /// <summary>
    /// Helper to instantiate and initialize a single UI slot.
    /// </summary>
     private InventorySlotUI CreateSingleSlotUI(Inventory.InventorySlot dataSlot, Transform parent, string gameObjectName) {
         GameObject slotGO = Instantiate(inventorySlotPrefab, parent);
         slotGO.name = gameObjectName;
         InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
         if (slotUI != null) {
             slotUI.Initialize(dataSlot, this); // Initialize with data and parent UI
         } else {
              Debug.LogError($"Slot Prefab for {gameObjectName} missing InventorySlotUI script!", slotGO);
              Destroy(slotGO);
         }
         return slotUI;
     }

      /// <summary>
    /// Clears all existing UI slots from the containers.
    /// </summary>
     private void ClearAllSlotUIs() {
        foreach(var slotUI in inventorySlotUIs) if (slotUI != null) Destroy(slotUI.gameObject);
        inventorySlotUIs.Clear();
        foreach(var slotUI in equipmentSlotUIs.Values) if (slotUI != null) Destroy(slotUI.gameObject);
        equipmentSlotUIs.Clear();
        // Also clear containers manually if needed
         foreach (Transform child in inventorySlotsParent) Destroy(child.gameObject);
         // ... clear other parent transforms ...
     }

    /// <summary>
    /// Gets the appropriate parent Transform for a given Equipment Slot type based on Inspector assignments.
    /// </summary>
    private Transform GetParentForEquipmentSlot(Item.EquipmentSlot slotType)
    {
        // Ensure all cases match your Item.EquipmentSlot enum and Inspector fields
        switch (slotType)
        {
            case Item.EquipmentSlot.Head: return headSlotParent;
            case Item.EquipmentSlot.Chest: return chestSlotParent;
            case Item.EquipmentSlot.Weapon: return weaponSlotParent;
            case Item.EquipmentSlot.OffHand: return offHandSlotParent;
            case Item.EquipmentSlot.Boots: return bootsSlotParent;
            case Item.EquipmentSlot.Gloves: return glovesSlotParent;
            case Item.EquipmentSlot.Belt: return beltSlotParent;
            case Item.EquipmentSlot.Amulet: return amuletSlotParent;
            case Item.EquipmentSlot.Ring1: return ring1SlotParent;
            case Item.EquipmentSlot.Ring2: return ring2SlotParent;
            // Add other cases...
            default: return null;
        }
    }

    // --- Event Handlers ---

    /// <summary>
    /// Called when an InventorySlot's data changes. Updates the corresponding UI element.
    /// </summary>
    private void HandleInventorySlotChanged(Inventory.InventorySlot changedDataSlot) // <<< Corrected parameter type
    {
        if (changedDataSlot == null || changedDataSlot.isEquipmentSlot) return; // Ignore if null or equipment slot

        if (changedDataSlot.inventoryIndex >= 0 && changedDataSlot.inventoryIndex < inventorySlotUIs.Count)
        {
            inventorySlotUIs[changedDataSlot.inventoryIndex]?.UpdateDisplay(); // Find UI slot by index and update
        }
    }

    /// <summary>
    /// Called when an EquipmentSlot's data changes. Updates the corresponding UI element.
    /// </summary>
    private void HandleEquipmentSlotChanged(Item.EquipmentSlot changedEquipSlotType)
    {
        if (equipmentSlotUIs.TryGetValue(changedEquipSlotType, out InventorySlotUI slotUI))
        {
            slotUI?.UpdateDisplay(); // Find UI slot by type and update
        }
    }

    // --- Tooltip Management ---

    public void ShowTooltip(Item item, RectTransform slotRectTransform)
    {
        if (tooltipPanel == null || tooltipText == null || item == null) return;
        tooltipText.text = item.GetTooltipInfo(); // Use item's method to get description/stats
        tooltipPanel.SetActive(true);
        PositionTooltip(slotRectTransform); // Use helper to position
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

     /// <summary>
     /// Positions the tooltip relative to a slot, trying to stay on screen. (Basic implementation)
     /// </summary>
     private void PositionTooltip(RectTransform slotRect) {
          if(tooltipPanel == null) return;
         Canvas canvas = GetComponentInParent<Canvas>();
         RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();

         // Convert slot position to canvas space
         Vector3 slotWorldPos = slotRect.position + new Vector3(slotRect.rect.width * slotRect.lossyScale.x, 0, 0); // Top-right corner roughly
         Vector2 slotCanvasPos;
         RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(slotWorldPos), canvas.worldCamera, out slotCanvasPos);

         // Set tooltip pivot to bottom-left (0,0) so position is its corner
         tooltipRect.pivot = new Vector2(0, 0);
         // Position tooltip slightly offset from slot's canvas position
         tooltipRect.anchoredPosition = slotCanvasPos + new Vector2(10, 10); // Adjust offset

         // --- TODO: Add screen clamping logic ---
         // Check if tooltipRect goes off screen boundaries and adjust anchoredPosition accordingly.
     }

    // --- Drag and Drop Management ---

    public void StartDraggingItem(InventorySlotUI sourceSlotUI)
    {
        if (sourceSlotUI == null || !sourceSlotUI.HasItem || draggedItemIcon == null) return;
        currentlyDraggedSlotUI = sourceSlotUI;
        draggedItemIcon.sprite = sourceSlotUI.RepresentedSlot.Item.icon;
        draggedItemIcon.enabled = true;
        UpdateDraggedItemPosition(Input.mousePosition); // Initial position
        HideTooltip();
    }

    public void UpdateDraggedItemPosition(Vector2 screenPosition)
    {
        if (draggedItemIcon == null || !draggedItemIcon.enabled) return;
        // Convert screen position to Canvas local position for the drag icon
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            draggedItemIcon.rectTransform.parent as RectTransform, // Assumes icon is child of main canvas/panel
            screenPosition,
            GetComponentInParent<Canvas>().worldCamera, // Or Camera.main if overlay canvas
            out Vector2 localPoint);
        draggedItemIcon.rectTransform.localPosition = localPoint;
    }

    public void StopDraggingItem(GameObject droppedOnObject)
    {
        bool droppedOnValidSlot = false;
        if (droppedOnObject != null) {
             // Use GetComponentInParent because the drop might register on the icon/text child of the slot UI
             InventorySlotUI dropTargetSlotUI = droppedOnObject.GetComponentInParent<InventorySlotUI>();
             if (dropTargetSlotUI != null && dropTargetSlotUI != currentlyDraggedSlotUI) { // Check it's a different slot
                  droppedOnValidSlot = true;
                   // OnDrop on the target slot (dropTargetSlotUI) handles calling HandleItemDrop
             }
        }

        if (!droppedOnValidSlot && currentlyDraggedSlotUI != null) {
            // Dropped somewhere invalid, reset source slot's visuals (data didn't change yet)
             currentlyDraggedSlotUI.UpdateDisplay();
             // Debug.Log("Drag ended on invalid target.");
        }

        // Hide drag icon
        if (draggedItemIcon != null) draggedItemIcon.enabled = false;
        currentlyDraggedSlotUI = null; // Clear drag state
    }

    /// <summary>
    /// Called by InventorySlotUI.OnDrop when a dragged item is dropped onto another slot.
    /// Determines the type of transfer (Inv->Inv, Inv->Equip, Equip->Inv) and calls Inventory methods.
    /// </summary>
    public void HandleItemDrop(InventorySlotUI sourceSlotUI, InventorySlotUI targetSlotUI)
    {
        if (characterInventory == null || sourceSlotUI == null || targetSlotUI == null) return;

        // Use the qualified nested class name: Inventory.InventorySlot
        Inventory.InventorySlot sourceDataSlot = sourceSlotUI.RepresentedSlot;
        Inventory.InventorySlot targetDataSlot = targetSlotUI.RepresentedSlot;

        if (sourceDataSlot == null || targetDataSlot == null || sourceDataSlot == targetDataSlot) return; // Invalid slots or dropping on self

        // Debug.Log($"HandleItemDrop: From {(sourceDataSlot.isEquipmentSlot?sourceDataSlot.equipmentSlotType.ToString():sourceDataSlot.inventoryIndex.ToString())} To {(targetDataSlot.isEquipmentSlot?targetDataSlot.equipmentSlotType.ToString():targetDataSlot.inventoryIndex.ToString())}");

        // --- Logic based on target and source slot types ---

        if (targetDataSlot.isEquipmentSlot) // Target is Equipment Slot
        {
            if (!sourceDataSlot.isEquipmentSlot) // Source is Inventory Slot -> Equip Action
            {
                // Check if item type matches equipment slot
                if (sourceDataSlot.Item != null && sourceDataSlot.Item.equipSlot == targetDataSlot.equipmentSlotType)
                {
                    characterInventory.EquipItemFromSlot(sourceDataSlot.inventoryIndex);
                } else { sourceSlotUI.UpdateDisplay(); /* Invalid drop */ } // Reset source display
            }
             else // Source is also Equipment Slot -> Swap Equipment? (Advanced)
             {
                  Debug.LogWarning("Swapping directly between equipment slots not implemented yet.");
                  sourceSlotUI.UpdateDisplay(); targetSlotUI.UpdateDisplay(); // Reset both displays
             }
        }
        else // Target is Inventory Slot
        {
            if (sourceDataSlot.isEquipmentSlot) // Source is Equipment Slot -> Unequip Action
            {
                // Attempt to unequip into the specific target slot if possible
                 // NOTE: Current UnequipItemToInventory just finds *any* empty slot.
                 // Requires Inventory class modification for targeted unequip/swap.
                 bool success = characterInventory.UnequipItemToInventory(sourceDataSlot.equipmentSlotType);
                 if(!success) sourceSlotUI.UpdateDisplay(); // Reset source if unequip failed (inventory full)
            }
            else // Source is Inventory Slot -> Inventory Swap/Merge Action
            {
                characterInventory.SwapOrMergeInventorySlots(sourceDataSlot, targetDataSlot);
                 // Events should handle UI updates, but forcing update ensures immediate visual feedback
                 sourceSlotUI.UpdateDisplay();
                 targetSlotUI.UpdateDisplay();
            }
        }
    }

} // End of InventoryUI Class