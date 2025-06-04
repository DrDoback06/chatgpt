using System;
using UnityEngine;

// Represents an instance of an active or defined status effect (buff/debuff).
// Contains logic for applying/removing its *direct* stat changes.
// More complex behaviors (stun, DoT) require interaction with owner's controller.
[System.Serializable]
public class StatusEffect
{
    [Header("Definition")]
    [Tooltip("Unique ID (e.g., 'BUFF_HASTE', 'DEBUFF_STUN'). Used for logic lookup.")]
    [SerializeField] private string effectID;
    [Tooltip("Display name shown in UI.")]
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [Tooltip("Icon representing the effect in the UI.")]
    [SerializeField] private Sprite icon;

    [Header("Effect Parameters")]
    [Tooltip("Duration in seconds (<= 0 for permanent until removed explicitly).")]
    [SerializeField] private float duration;
    [Tooltip("Is this generally positive (buff) or negative (debuff)?")]
    [SerializeField] private bool isBuff = false;
    [Tooltip("Can multiple instances of this effect exist simultaneously? (Requires stacking logic in StatusEffectController)")]
    [SerializeField] private bool isStackable = false; // TODO: Implement stacking logic
    [Tooltip("The numeric value associated with the effect (e.g., +15 Agility, 5 damage per second).")]
    [SerializeField] private float effectValue = 0; // Use float for flexibility (e.g., percentages)
    // Add MaxStacks?
    // Add TickInterval? public float tickInterval = 1.0f;

    // --- Runtime Data (Managed by Controller) ---
    // Mark NonSerialized if StatusEffect instances are saved directly as part of another component (like Character),
    // but usually they are recreated on load based on saved IDs/modifiers.
    [NonSerialized] private float timeApplied;
    [NonSerialized] private float effectEndTime = float.PositiveInfinity;
    [NonSerialized] private bool isActive = false;
    // [NonSerialized] public float nextTickTime; // For periodic effects

    // --- Public Properties ---
    public string EffectID => effectID;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public float Duration => duration;
    public bool IsBuff => isBuff;
    public bool IsActive => isActive; // Is this specific instance currently applied and running?
    public bool IsStackable => isStackable;
    public float Value => effectValue; // Public accessor for the magnitude
    public float RemainingDuration => (duration > 0 && isActive) ? Mathf.Max(0f, effectEndTime - Time.time) : (duration <= 0 ? float.PositiveInfinity : 0f);

    // --- Constructors ---

    /// <summary>
    /// Parameterless constructor needed for serialization (e.g., JsonUtility).
    /// </summary>
    public StatusEffect() { }

    /// <summary>
    /// Constructor primarily used for defining effect templates (e.g., in a database or skill).
    /// </summary>
    public StatusEffect(string id, string name, string desc, float dur, bool buff, bool stackable, Sprite ico = null, float value = 0f)
    {
        this.effectID = id;
        this.displayName = name;
        this.description = desc; // Base description
        this.duration = dur;
        this.isBuff = buff;
        this.isStackable = stackable;
        this.icon = ico;
        this.effectValue = value; // Store the value/magnitude
    }

    // --- Core Logic Methods ---

    /// <summary>
    /// Called by StatusEffectController when this effect instance starts.
    /// Applies initial modifications based on effectID and stored Value.
    /// </summary>
    /// <param name="targetStats">The CharacterAttributes of the affected entity.</param>
    /// <param name="ownerController">Optional: Reference to the owner's controller for complex effects like stun.</param>
    public virtual void ApplyEffect(CharacterAttributes targetStats, MonoBehaviour ownerController = null)
    {
        if (isActive) return; // Don't double-apply

        timeApplied = Time.time;
        effectEndTime = (duration > 0) ? timeApplied + duration : float.PositiveInfinity;
        isActive = true;
        // Debug.Log($"Status Effect '{DisplayName}' applied. Value: {effectValue}, Duration: {duration}s");

        // --- Apply Direct Stat Modifications ---
        ModifyStats(targetStats, 1); // Apply (multiplier = 1)

        // --- Apply Behavioural Effects (Needs Owner Controller) ---
        if (ownerController != null) {
             ApplyBehaviouralEffect(ownerController, true); // Apply behaviour change
        }
    }

    /// <summary>
    /// Called by StatusEffectController when this effect instance ends or is removed.
    /// Reverts direct stat modifications.
    /// </summary>
    /// <param name="targetStats">The CharacterAttributes of the affected entity.</param>
    /// <param name="ownerController">Optional: Reference to the owner's controller.</param>
    public virtual void RemoveEffect(CharacterAttributes targetStats, MonoBehaviour ownerController = null)
    {
        if (!isActive) return; // Already removed

        isActive = false;
        // Debug.Log($"Status Effect '{DisplayName}' removed.");

        // --- Revert Direct Stat Modifications ---
        ModifyStats(targetStats, -1); // Revert (multiplier = -1)

         // --- Revert Behavioural Effects ---
        if (ownerController != null) {
             ApplyBehaviouralEffect(ownerController, false); // Revert behaviour change
        }
    }

    /// <summary>
    /// Helper method to apply or revert direct stat modifications based on multiplier.
    /// </summary>
    protected virtual void ModifyStats(CharacterAttributes targetStats, int multiplier) {
        if (targetStats == null) return;
        int valueToInt = Mathf.RoundToInt(effectValue * multiplier); // Apply multiplier here

        switch (effectID) // Logic based on ID
        {
            // Direct Attribute Modifiers
            case "Haste": // Example using Agility
                targetStats.Agility += valueToInt; break;
            case "Weakness": // Example using Strength
                targetStats.Strength += valueToInt; break; // valueToInt will be negative on remove
            case "Might":
                targetStats.Strength += valueToInt; break;
            case "Fortitude":
                targetStats.Vitality += valueToInt; break;
             // Add other direct stat buffs/debuffs...

             // NOTE: Effects modifying derived stats like MaxHP, Armor, Damage are generally
             // handled by the Character recalculating those stats when base attributes (like Vit, Agi) change.
             // If an effect gives FLAT MaxHP bonus, handle it in Character.RecalculateCombatStats
             // by checking if the effect is active via ownerController.GetComponent<StatusEffectController>().HasEffect(...)

            // --- Placeholder for other effect types ---
             // case "Poison": // DoT would be handled in Controller Update, not direct stat mod
             // case "Regeneration": // HoT handled in Controller Update
             // case "ArmorBuff": // Flat armor buff - modify Character.GetArmorValue() to check for this active effect
             // break;
        }
        // Changes to CharacterAttributes automatically trigger Character.RecalculateCombatStats via events.
    }


     /// <summary>
     /// Helper method to apply/revert behavioral effects (like stun, silence) using the owner's controller.
     /// </summary>
     protected virtual void ApplyBehaviouralEffect(MonoBehaviour ownerController, bool apply) {
        // --- Handle effects that change behaviour ---
        switch (effectID) {
            case "Stun":
                // Find the correct controller type and enable/disable actions
                 PlayerController player = ownerController as PlayerController;
                 EnemyAI enemy = ownerController as EnemyAI;
                 if (player != null) {
                      if (apply) player.DisableControl(); else player.EnableControl(); // Basic stun control
                      Debug.Log($"Player Stun State: {apply}");
                 } else if (enemy != null) {
                     enemy.enabled = !apply; // Simple way: disable AI script entirely while stunned
                      if(apply) enemy.GetComponent<Rigidbody2D>().velocity = Vector2.zero; // Stop movement
                      Debug.Log($"Enemy ({enemy.gameObject.name}) Stun State: {apply}");
                 }
                 break;

             // Add cases for Silence (disable skill usage), Root (disable movement but allow actions), etc.
             // case "Silence":
             //    // PlayerController pc = ownerController as PlayerController;
             //    // if(pc != null) pc.SetSilenced(apply);
             //    break;
         }
     }


    /// <summary>
    /// Called by StatusEffectController if this effect is reapplied while active.
    /// Default: Refreshes duration. Override for stacking value or other behaviors.
    /// </summary>
    public virtual void RefreshEffect()
    {
        if (!isActive) { ApplyEffect(null); return; } // Should ideally be applied by Controller if inactive

        timeApplied = Time.time;
        effectEndTime = (duration > 0) ? timeApplied + duration : float.PositiveInfinity;
        Debug.Log($"Status Effect '{DisplayName}' refreshed. Duration Reset.");
        // TODO: Add logic here if refreshing should also re-apply/stack intensity
    }

    /// <summary>
    /// Checks if the effect's duration has run out.
    /// </summary>
    public bool CheckExpiration()
    {
        if (!isActive || duration <= 0) return false; // Permanent effects don't expire by time
        return Time.time >= effectEndTime;
    }

    // --- Placeholder methods for periodic effects (DoT/HoT) ---
    // These would be called from StatusEffectController.Update()
    // public virtual void ApplyTickEffect(CharacterAttributes targetStats, MonoBehaviour ownerController) {
    //    if (!isActive) return;
    //    switch(effectID) {
    //       case "Poison":
    //          // targetStats.Owner.TakeDamage(effectValue); // Need Owner reference or pass damage method
    //          Debug.Log($"Poison tick: {effectValue} damage"); break;
    //       case "Regeneration":
    //          // targetStats.Owner.Heal(effectValue);
    //           Debug.Log($"Regen tick: {effectValue} heal"); break;
    //    }
    // }
    // public virtual bool IsTickReady(float tickInterval) => isActive && Time.time >= nextTickTime;
    // public virtual void UpdateTickTimer(float tickInterval) => nextTickTime = Time.time + tickInterval;

} // End of StatusEffect Class