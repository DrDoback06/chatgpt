using System;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ operations in Load
using UnityEngine; // For Debug only
using TMPro; 

// Holds the player's item collection (inventory and equipped items).
// This is a data container, usually owned by the Character class.
[System.Serializable]
public class Inventory
{
    [Header("Configuration")]
    [Tooltip("Number of slots in the main inventory grid.")]
    [SerializeField] private int inventorySlotCount = 20;

    [Header("Runtime Data")]
    [Tooltip("Items currently in the inventory slots.")]
    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    [Tooltip("Items currently equipped, keyed by slot type.")]
    [SerializeField] private Dictionary<Item.EquipmentSlot, InventorySlot> equippedItems = new Dictionary<Item.EquipmentSlot, InventorySlot>();

    // --- Events ---
    // Systems like InventoryUI subscribe to these to update visuals.
    public event Action<InventorySlot> OnInventoryChanged;
    public event Action<Item.EquipmentSlot> OnEquipmentChanged;

    // --- Character Reference ---
    [System.NonSerialized] // Prevents serialization cycle
    private Character ownerCharacter; // Set via SetOwner()

    // --- Constructor ---
    public Inventory(int invSize = 20)
    {
        inventorySlotCount = Mathf.Max(1, invSize); // Ensure at least 1 slot
        InitializeSlots();
    }

    /// <summary>
    /// Establishes the link back to the character who owns this inventory.
    /// Essential for applying/removing stats on equip/unequip.
    /// </summary>
    public void SetOwner(Character owner)
    {
        this.ownerCharacter = owner;
    }

    /// <summary>
    /// Sets up the initial empty inventory and equipment slot structures.
    /// </summary>
    private void InitializeSlots()
    {
        inventorySlots = new List<InventorySlot>(inventorySlotCount);
        for (int i = 0; i < inventorySlotCount; i++)
        {
            // Create empty slots, passing 'this' Inventory as parent for potential event calls
            inventorySlots.Add(new InventorySlot(i, this));
        }

        equippedItems = new Dictionary<Item.EquipmentSlot, InventorySlot>();
        foreach (Item.EquipmentSlot slotType in Enum.GetValues(typeof(Item.EquipmentSlot)))
        {
            if (slotType != Item.EquipmentSlot.None)
            {
                // Create empty equip slots, passing 'this' Inventory as parent
                equippedItems.Add(slotType, new InventorySlot(slotType, this));
            }
        }
        // Debug.Log($"Inventory initialized: {inventorySlotCount} slots, {equippedItems.Count} equip slots.");
    }

    // --- Accessors ---
    public List<InventorySlot> GetInventorySlots() => inventorySlots;
    public Dictionary<Item.EquipmentSlot, InventorySlot> GetEquippedItems() => equippedItems;
    public InventorySlot GetEquippedSlot(Item.EquipmentSlot slotType)
    {
        equippedItems.TryGetValue(slotType, out InventorySlot slot);
        return slot; // Returns null if key not found (shouldn't happen after init)
    }
    public int GetInventorySize() => inventorySlotCount;


    // --- Item Management ---

    public bool AddItem(Item itemToAdd, int amount = 1)
    {
        if (itemToAdd == null || amount <= 0) return false;
        if (ownerCharacter == null) { Debug.LogWarning("Inventory owner not set, events might not fire correctly."); }

        // --- Stacking Logic ---
        if (itemToAdd.isStackable)
        {
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.CanStack(itemToAdd)) // Check if slot can accept this item stack
                {
                    int amountPossibleToAdd = itemToAdd.maxStackSize - slot.Amount;
                    int amountToAdd = Mathf.Min(amount, amountPossibleToAdd);

                    if (amountToAdd > 0) {
                        slot.AddToStack(amountToAdd);
                        amount -= amountToAdd;
                        // OnInventoryChanged?.Invoke(slot); // Slot's method will notify now
                        if (amount <= 0) return true;
                    }
                }
            }
        }

        // --- Adding to New Slots ---
        while (amount > 0)
        {
             InventorySlot emptySlot = FindEmptyInventorySlot();
             if (emptySlot != null)
             {
                 int amountToPlace = itemToAdd.isStackable ? Mathf.Min(amount, itemToAdd.maxStackSize) : 1;
                 Item itemInstance = CreateItemInstanceForSlot(itemToAdd); // Create instance
                 if (itemInstance == null) return false;

                 emptySlot.AddItem(itemInstance, amountToPlace);
                 // OnInventoryChanged?.Invoke(emptySlot); // Slot notifies
                 amount -= amountToPlace;

                 // If adding a non-stackable item, we're done with this item instance
                 if (!itemToAdd.isStackable)
                 {
                     if(amount > 0) continue; // Need to find more slots for remaining amount
                     else return true; // Added the single item
                 }
                 // If stackable and amount > 0, loop continues automatically
             }
             else
             {
                 Debug.LogWarning($"Inventory full! Failed to add {amount}x {itemToAdd.itemName}.");
                 return false; // No empty slots left
             }
        }
        return true; // All successfully added
    }

    // Removes by ID from first available stacks
    public void RemoveItem(string itemIDToRemove, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemIDToRemove) || amount <= 0) return;

        int amountStillToRemove = amount;
        for (int i = inventorySlots.Count - 1; i >= 0; i--) // Iterate backwards
        {
            InventorySlot slot = inventorySlots[i];
            if (!slot.IsEmpty() && slot.Item.itemID == itemIDToRemove)
            {
                int amountToRemoveFromSlot = Mathf.Min(amountStillToRemove, slot.Amount);
                slot.RemoveFromStack(amountToRemoveFromSlot); // Slot handles clearing and notifying
                amountStillToRemove -= amountToRemoveFromSlot;
                // OnInventoryChanged?.Invoke(slot); // Slot notifies

                if (amountStillToRemove <= 0) return;
            }
        }
        // Warning if not enough removed handled by slot logic potentially
    }

    // Removes from a specific inventory slot index
    public void RemoveFromSlot(int inventorySlotIndex, int amount = 1)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= inventorySlots.Count || amount <= 0) return;
        inventorySlots[inventorySlotIndex]?.RemoveFromStack(amount); // Let slot handle logic & notification
    }

    private InventorySlot FindEmptyInventorySlot() => inventorySlots.FirstOrDefault(slot => slot.IsEmpty());

    private Item CreateItemInstanceForSlot(Item sourceItem)
    {
        // Creates a copy using JsonUtility - ensures instances are unique
        if (sourceItem == null) return null;
        try { return JsonUtility.FromJson<Item>(JsonUtility.ToJson(sourceItem)); }
        catch (Exception e) { Debug.LogError($"Inventory JSON Copy Failed for '{sourceItem.itemID}': {e}"); return null; }
    }

    // --- Equipment Handling ---

    public void EquipItemFromSlot(int inventorySlotIndex)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= inventorySlots.Count) return;

        InventorySlot sourceSlot = inventorySlots[inventorySlotIndex];
        if (sourceSlot.IsEmpty() || sourceSlot.Item.equipSlot == Item.EquipmentSlot.None) return;

        Item itemToEquip = sourceSlot.Item;
        Item.EquipmentSlot targetSlotType = itemToEquip.equipSlot;

        if (!equippedItems.TryGetValue(targetSlotType, out InventorySlot targetEquipSlot))
        { Debug.LogError($"Equip target slot {targetSlotType} dictionary error!"); return; }

        Item currentlyEquippedItem = targetEquipSlot.Item;
        int currentlyEquippedAmount = targetEquipSlot.Amount; // Usually 1

        // --- IMPORTANT: The order matters for stat calculation triggers ---

        // 1. Temporarily store data from slots
        Item sourceItemInstance = sourceSlot.Item; // Keep reference
        int sourceAmount = sourceSlot.Amount;

        // 2. Clear source slot (will trigger OnInventoryChanged)
        sourceSlot.Clear();

        // 3. Clear target equipment slot (will trigger OnEquipmentChanged for old item removal implicitly later)
        targetEquipSlot.Clear(); // Clear data only for now

        // 4. Place *new* item data into equipment slot
        targetEquipSlot.AddItem(sourceItemInstance, sourceAmount);
        // **This AddItem should trigger targetEquipSlot.NotifyChanged() which calls OnEquipmentChanged**

        // 5. Place *old* equipped item (if any) data into the now empty source inventory slot
        if (currentlyEquippedItem != null)
        {
            sourceSlot.AddItem(currentlyEquippedItem, currentlyEquippedAmount);
            // **This AddItem should trigger sourceSlot.NotifyChanged() which calls OnInventoryChanged**
        }

        // Stats are recalculated automatically by Character because HandleEquipmentChange is subscribed

        Debug.Log($"Equipped {itemToEquip.itemName} to {targetSlotType}.");
    }


    public bool UnequipItemToInventory(Item.EquipmentSlot slotToUnequip)
    {
        if (!equippedItems.TryGetValue(slotToUnequip, out InventorySlot equipSlot) || equipSlot.IsEmpty()) return false;

        Item itemToUnequip = equipSlot.Item;
        int amountToUnequip = equipSlot.Amount;

        InventorySlot targetInventorySlot = FindEmptyInventorySlot();
        if (targetInventorySlot != null)
        {
            // --- Order matters for stat calculation ---

            // 1. Store item reference
             Item itemInstance = equipSlot.Item; // Keep reference

            // 2. Clear equipment slot (triggers OnEquipmentChanged via NotifyChanged)
             equipSlot.Clear();

            // 3. Add item to the empty inventory slot (triggers OnInventoryChanged via NotifyChanged)
             targetInventorySlot.AddItem(itemInstance, amountToUnequip);

            // Stats recalculate automatically in Character due to HandleEquipmentChange

            Debug.Log($"Unequipped {itemInstance.itemName} from {slotToUnequip} to inv slot {targetInventorySlot.inventoryIndex}.");
            return true;
        }
        else { Debug.LogWarning($"Inventory full! Cannot unequip {itemToUnequip?.itemName ?? "item"}."); return false; }
    }

    // --- Dynamic Stat Calculation Helper ---
    public int GetTotalEquippedStatModifier(ItemAffix.AffixType type)
    {
        float total = 0f;
        if (equippedItems != null)
        {
            foreach (InventorySlot slot in equippedItems.Values)
            {
                if (slot != null && !slot.IsEmpty())
                {
                    total += slot.Item.GetTotalStatModifier(type); // Use Item's method
                }
            }
        }
        return Mathf.RoundToInt(total); // Explicitly convert float to int
    }

    // --- Swapping / Merging ---
    public void SwapOrMergeInventorySlots(InventorySlot slotA, InventorySlot slotB) // Renamed for clarity
    {
        if (slotA == null || slotB == null || slotA.isEquipmentSlot || slotB.isEquipmentSlot) return;

        Item itemA = slotA.Item; int amountA = slotA.Amount;
        Item itemB = slotB.Item; int amountB = slotB.Amount;

        // Try Merge
        if (itemA != null && itemB != null && itemA.itemID == itemB.itemID && itemA.isStackable)
        {
            int spaceInB = itemB.maxStackSize - amountB;
            int amountToMove = Mathf.Min(amountA, spaceInB);
            if (amountToMove > 0)
            {
                slotB.AddToStack(amountToMove); // Will notify
                slotA.RemoveFromStack(amountToMove); // Will notify
                return;
            }
        }

        // Swap if no merge or merge failed
        slotA.Clear(); slotB.Clear(); // Clear data only first
        if (itemB != null) slotA.AddItem(itemB, amountB); // Add B data to A
        if (itemA != null) slotB.AddItem(itemA, amountA); // Add A data to B
        // AddItem calls NotifyChanged, triggering events for both slots if they weren't empty
        if (slotA.IsEmpty() && itemA != null) slotA.NotifyChanged(); // Ensure notify if A became empty then filled
        if (slotB.IsEmpty() && itemB != null) slotB.NotifyChanged(); // Ensure notify if B became empty then filled
    }


    // --- Save/Load ---
    [System.Serializable]
    private class InventorySaveData { public List<InventorySlot> inventorySlots; public List<InventorySlot> equipmentSlots; }

    public string GetSaveDataJson()
    {
        InventorySaveData saveData = new InventorySaveData
        {
            inventorySlots = this.inventorySlots,
            equipmentSlots = new List<InventorySlot>(this.equippedItems.Values)
        };
        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromSaveDataJson(string json)
    {
        if (string.IsNullOrEmpty(json)) { InitializeSlots(); return; }
        try
        {
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            // --- Load Inventory Slots ---
            this.inventorySlots = saveData.inventorySlots ?? new List<InventorySlot>();
            this.inventorySlotCount = this.inventorySlots.Count;
            for (int i = 0; i < this.inventorySlots.Count; ++i) {
                if (this.inventorySlots[i] != null) {
                     this.inventorySlots[i].inventoryIndex = i;
                     this.inventorySlots[i].SetParentInventory(this); // << Re-link parent on load
                }
            }

            // --- Reconstruct Equipment Dictionary ---
            // Initialize first to ensure all keys exist
            InitializeSlots(); // This re-creates the dictionary with empty slots & sets parent Inventory
            if (saveData.equipmentSlots != null)
            {
                foreach (InventorySlot savedEquipSlot in saveData.equipmentSlots)
                {
                    if (savedEquipSlot != null && savedEquipSlot.isEquipmentSlot && this.equippedItems.ContainsKey(savedEquipSlot.equipmentSlotType))
                    {
                        if (!savedEquipSlot.IsEmpty()) // Only load if it contains an item
                        {
                            savedEquipSlot.SetParentInventory(this); // << Re-link parent on load
                            this.equippedItems[savedEquipSlot.equipmentSlotType] = savedEquipSlot;
                        }
                    }
                }
            }

            // Trigger UI update events for all slots AFTER loading is complete
            ForceNotifyAllSlots();

            Debug.Log($"Inventory loaded. Slots:{inventorySlots.Count}, Equip:{equippedItems.Values.Count(s => !s.IsEmpty())}");
        }
        catch (Exception e) { Debug.LogError($"Inventory Load Failed: {e.Message}"); InitializeSlots(); }
    }

    /// <summary>
    /// Manually triggers update events for all slots. Useful after loading.
    /// </summary>
    public void ForceNotifyAllSlots() {
        if(inventorySlots != null) foreach(var slot in inventorySlots) NotifyInventoryChanged(slot);
        if(equippedItems != null) foreach(var type in equippedItems.Keys) NotifyEquipmentChanged(type);
    }

    // Internal methods for slots to notify this manager
    internal void NotifyInventoryChanged(InventorySlot slot) => OnInventoryChanged?.Invoke(slot);
    internal void NotifyEquipmentChanged(Item.EquipmentSlot slotType) => OnEquipmentChanged?.Invoke(slotType);


    // +------------------------------------------------------+
    // | NESTED CLASS: InventorySlot                          |
    // +------------------------------------------------------+
    // Represents a single slot (inventory or equipment).
    [System.Serializable]
    public class InventorySlot
    {
        public bool isEquipmentSlot = false;
        public Item.EquipmentSlot equipmentSlotType = Item.EquipmentSlot.None;
        public int inventoryIndex = -1; // Index if !isEquipmentSlot

        [SerializeField] private Item item = null;
        [SerializeField] private int amount = 0;

        // --- Transient: Link back to parent for notifications ---
        [NonSerialized] private Inventory parentInventory; // Avoids circular serialization

        // --- Public Accessors ---
        public Item Item => item;
        public int Amount => amount;
        public bool IsEmpty() => item == null || amount <= 0;

        // Constructor for inventory slot
        public InventorySlot(int index, Inventory parent)
        {
            this.inventoryIndex = index;
            this.isEquipmentSlot = false;
            this.equipmentSlotType = Item.EquipmentSlot.None;
            SetParentInventory(parent);
            Clear();
        }
        // Constructor for equipment slot
        public InventorySlot(Item.EquipmentSlot type, Inventory parent)
        {
            this.inventoryIndex = -1;
            this.isEquipmentSlot = true;
            this.equipmentSlotType = type;
            SetParentInventory(parent);
            Clear();
        }

        // Sets the parent reference (needed after deserialization)
        public void SetParentInventory(Inventory inventory) => parentInventory = inventory;

        // --- Slot Modification Methods ---
        public void AddItem(Item newItem, int addAmount)
        {
            if (!IsEmpty()) { Debug.LogError($"Slot AddItem Error: Slot {Identifier()} not empty."); return; }
            if (newItem == null || addAmount <= 0) { Debug.LogWarning($"Slot AddItem Warning: Invalid item or amount on slot {Identifier()}."); return; }

            item = newItem; // Assign the item instance
            amount = Mathf.Clamp(addAmount, 1, newItem.isStackable ? newItem.maxStackSize : 1);
            NotifyChanged(); // Notify parent inventory
        }

        public void AddToStack(int addAmount)
        {
            if (IsEmpty() || addAmount <= 0 || !item.isStackable) return;
            int oldAmount = amount;
            amount = Mathf.Min(amount + addAmount, item.maxStackSize); // Add and clamp
            if (amount != oldAmount) NotifyChanged(); // Notify only if amount changed
        }

        public void RemoveFromStack(int removeAmount)
        {
            if (IsEmpty() || removeAmount <= 0) return;
            int oldAmount = amount;
            amount -= removeAmount;
            if (amount <= 0) Clear(); // Clear calls NotifyChanged
            else if (amount != oldAmount) NotifyChanged(); // Notify if amount just decreased
        }

        public void Clear()
        {
            bool wasEmpty = IsEmpty();
            item = null;
            amount = 0;
            if (!wasEmpty) NotifyChanged(); // Notify only if it wasn't already empty
        }

        /// <summary>
        /// Can this slot stack the specified item? (Checks ID and max stack size)
        /// </summary>
        public bool CanStack(Item itemToCheck) {
             return !IsEmpty()
                 && itemToCheck != null
                 && item.isStackable // Slot item must be stackable
                 && item.itemID == itemToCheck.itemID // Items must match
                 && amount < item.maxStackSize; // Slot must have space
        }

        // Helper to easily notify the parent Inventory manager
        public void NotifyChanged()
        {
            if (parentInventory != null)
            {
                if (isEquipmentSlot) parentInventory.NotifyEquipmentChanged(equipmentSlotType);
                else parentInventory.NotifyInventoryChanged(this);
            } else {
                 // Debug.LogWarning($"Slot {Identifier()} has no parent Inventory reference to notify.");
            }
        }

        // Helper for Debugging
        public string Identifier() => isEquipmentSlot ? equipmentSlotType.ToString() : $"Inv[{inventoryIndex}]";

    } // End of Nested InventorySlot Class

} // End of Inventory Class