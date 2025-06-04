using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For Linq queries like Any, Where

// Manages active status effects on a character or enemy.
public class StatusEffectController : MonoBehaviour
{
    // Runtime list of currently active status effect instances
    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    // Reference to the character data (if on player) or enemy stats
    private Character characterOwner; // If attached to player controlled by CharacterManager
    private EnemyController enemyOwner; // If attached to an enemy prefab

    // Event (Optional): Fired when an effect is added or removed
    public event System.Action<StatusEffect, bool> OnEffectChanged; // bool is true if added, false if removed

    void Awake()
    {
        // Try to get owner reference
        characterOwner = GetComponent<PlayerController>() ? CharacterManager.Instance?.character : null;
        enemyOwner = GetComponent<EnemyController>();

        // Basic check - Could need refinement if multiple controllers possible
        if (characterOwner == null && enemyOwner == null)
        {
            Debug.LogWarning($"StatusEffectController on {gameObject.name} could not find a Character or EnemyController owner.", this);
        }
    }

    void Update()
    {
        // Iterate backwards for safe removal while looping
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            if (effect.CheckExpiration()) // Check if duration ran out
            {
                RemoveEffect(effect); // Handles reverting stats and removing from list
            }
            // --- TODO: Handle periodic effects ---
            // Example: Apply Poison damage tick
            // if (effect.EffectID == "Poison" && Time.time >= effect.nextTickTime) {
            //    ApplyTickEffect(effect);
            //    effect.nextTickTime = Time.time + effect.tickInterval;
            // }
        }
    }

    /// <summary>
    /// Applies a new status effect or refreshes an existing one.
    /// </summary>
    /// <param name="effectData">The base definition/data of the effect to apply.</param>
    public void ApplyStatusEffect(StatusEffect effectData)
    {
        if (effectData == null) return;

        // --- Stacking Logic ---
        // TODO: Implement proper stacking (check if effect is stackable, max stacks etc.)
        StatusEffect existingEffect = FindEffectByID(effectData.EffectID);

        if (existingEffect != null)
        {
            // Effect already exists - Refresh duration? Add stack? Override?
            // Simple approach: Refresh duration
            existingEffect.RefreshEffect();
            Debug.Log($"Refreshed Status Effect: {effectData.DisplayName}");
             OnEffectChanged?.Invoke(existingEffect, true); // Notify listeners (though it wasn't newly added)
        }
        else
        {
            // Apply a new instance of the effect
            // Create a copy to manage runtime state independently
             StatusEffect newInstance = CreateEffectInstance(effectData);
             if (newInstance == null) return; // Failed to copy

            activeEffects.Add(newInstance);
            newInstance.ApplyEffect(GetTargetStats()); // Pass target stats to the effect
            OnEffectChanged?.Invoke(newInstance, true); // Notify listeners effect was added
        }
    }

     /// <summary>
     /// Creates a runtime instance copy of a status effect definition.
     /// </summary>
     private StatusEffect CreateEffectInstance(StatusEffect source) {
         if (source == null) return null;
         // Simple copy using JsonUtility (Ensure StatusEffect is Serializable)
         try {
            string json = JsonUtility.ToJson(source);
            StatusEffect instance = JsonUtility.FromJson<StatusEffect>(json);
             // Reset runtime state if necessary (might be done in ApplyEffect)
             return instance;
         } catch (System.Exception e) {
             Debug.LogError($"Failed to create StatusEffect instance for {source.EffectID}: {e}");
             return null;
         }
     }

    /// <summary>
    /// Removes a specific status effect instance immediately.
    /// </summary>
    /// <param name="effectToRemove">The instance to remove.</param>
    public void RemoveEffect(StatusEffect effectToRemove)
    {
        if (effectToRemove != null && activeEffects.Contains(effectToRemove))
        {
            effectToRemove.RemoveEffect(GetTargetStats()); // Revert stats
            activeEffects.Remove(effectToRemove);
            OnEffectChanged?.Invoke(effectToRemove, false); // Notify listeners effect was removed
            // Debug.Log($"Removed Status Effect: {effectToRemove.DisplayName}");
        }
    }

    /// <summary>
    /// Removes all effects matching a specific ID.
    /// </summary>
    public void RemoveEffectByID(string effectID)
    {
        // Remove in reverse to avoid index issues
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].EffectID == effectID)
            {
                RemoveEffect(activeEffects[i]);
            }
        }
    }

    /// <summary>
    /// Checks if an effect with the given ID is currently active.
    /// </summary>
    public bool HasEffect(string effectID)
    {
        return activeEffects.Any(fx => fx.EffectID == effectID && fx.IsActive);
    }

    /// <summary>
    /// Finds the first active effect instance with the given ID.
    /// </summary>
    public StatusEffect FindEffectByID(string effectID)
    {
        return activeEffects.FirstOrDefault(fx => fx.EffectID == effectID && fx.IsActive);
    }

    /// <summary>
    /// Gets a list of all currently active effects (useful for UI).
    /// </summary>
    public List<StatusEffect> GetActiveEffects()
    {
        // Return a copy to prevent external modification? Or allow direct access?
        return new List<StatusEffect>(activeEffects);
    }


    /// <summary>
    /// Helper to get the stats object of the owner (Character Attributes or potentially Enemy Stats).
    /// Used by StatusEffect Apply/Remove methods. Modify if EnemyController gets its own stats class.
    /// </summary>
    /// <returns>CharacterAttributes if applicable, otherwise null.</returns>
    private CharacterAttributes GetTargetStats()
    {
        return characterOwner?.attributes;
        // If enemies have separate stats:
        // return characterOwner?.attributes ?? enemyOwner?.GetStatsComponent();
    }

     // --- TODO ---
     // Implement Periodic Tick Logic in Update if needed.
     // Implement actual stat modifications within StatusEffect Apply/Remove methods based on EffectID.
     // Implement Stacking Logic.
     // Add Dispel logic (RemoveEffect based on IsBuff flag).
}