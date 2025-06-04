using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseHealth = 50;
    [SerializeField] private int baseExperienceValue = 25;
    [SerializeField] private int baseArmor = 5;

    [Header("Scaling Per Level")]
    [SerializeField] private int healthPerLevel = 10;
    [SerializeField] private int experiencePerLevel = 5;
    [SerializeField] private int armorPerLevel = 2;

    [Header("Loot")]
    [Tooltip("Loot table asset defining potential drops for this enemy.")]
    [SerializeField] private LootTable lootTable;
    [Tooltip("Prefab for the item pickup object to spawn in the world.")]
    [SerializeField] private GameObject itemPickupPrefab;

    [Header("Runtime State (Read Only)")]
    [SerializeField] private int level;
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentArmor;
    private bool isDefeated = false;

    // --- Events ---
    public event Action<int, EnemyController> OnEnemyDefeated;

    // --- Public Accessors ---
    public bool IsDefeated() => isDefeated;
    public int Level => level; // Allow reading level externally

    // --- Initialization ---
    public void InitializeEnemy(int playerLevelContext) // Renamed param for clarity
    {
        this.level = Mathf.Max(1, playerLevelContext + UnityEngine.Random.Range(-1, 2));
        maxHealth = baseHealth + (healthPerLevel * (this.level - 1));
        currentHealth = maxHealth;
        currentArmor = baseArmor + (armorPerLevel * (this.level - 1));
        isDefeated = false;
        gameObject.name = $"{gameObject.name.Replace("(Clone)", "").Trim()} (Lvl {this.level})";
        // Debug.Log($"{gameObject.name} initialized. Lvl: {this.level}, HP: {currentHealth}/{maxHealth}, Armor: {currentArmor}");
    }

    // --- Combat ---
    public void TakeDamage(float damageAmount)
    {
        if (isDefeated || damageAmount <= 0) return;

        // Mitigation
        float armorCalculationConstant = 50f;
        float damageReduction = (armorCalculationConstant > 0) ? currentArmor / (currentArmor + armorCalculationConstant) : 0f;
        float mitigatedDamage = damageAmount * (1f - damageReduction);
        float finalDamage = Mathf.Max(0, mitigatedDamage);

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Debug.Log($"{gameObject.name} took {finalDamage} damage ({damageAmount} raw). HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            DefeatEnemy();
        }
    }

    private int GetScaledExperiencePoints(int playerLevel)
    {
        int levelBasedXP = baseExperienceValue + (experiencePerLevel * (this.level - 1));
        int levelDifference = playerLevel - this.level;
        float scalingFactor = 1.0f; // Default
        if (levelDifference > 5) scalingFactor = 0.5f;
        else if (levelDifference > 2) scalingFactor = 0.8f;
        else if (levelDifference < -5) scalingFactor = 1.5f;
        else if (levelDifference < -2) scalingFactor = 1.2f;
        return Mathf.Max(1, Mathf.RoundToInt(levelBasedXP * scalingFactor));
    }

    // --- Defeat Sequence ---
    private void DefeatEnemy()
    {
        if (isDefeated) return;
        isDefeated = true;
        Debug.Log($"{gameObject.name} defeated!");

        // Grant Experience
        int playerLevel = CharacterManager.Instance?.character?.Level ?? 1;
        int scaledXP = GetScaledExperiencePoints(playerLevel);
        OnEnemyDefeated?.Invoke(scaledXP, this); // Notify listeners (e.g., GameController)

        // Trigger Loot Drop
        DropLoot();

        // Disable Components & Schedule Destruction
        GetComponent<Collider2D>().enabled = false;
        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false; // Disable AI script
        var rb = GetComponent<Rigidbody2D>();
        if(rb != null) rb.velocity = Vector2.zero; // Stop movement
        this.enabled = false; // Disable this script
        Destroy(gameObject, 2.5f); // Allow time for death anim/effects
    }


    // --- Loot Dropping Logic ---
    private void DropLoot()
    {
        if (lootTable == null || itemPickupPrefab == null) return;

        float playerMagicFind = CharacterManager.Instance?.character?.GetMagicFindValue() ?? 0f;
        List<ItemDropResult> dropResults = lootTable.EvaluateDrops(this.level, playerMagicFind);

        if (dropResults.Count > 0)
        {
            ItemDatabase db = FindObjectOfType<ItemDatabase>(); // TODO: Optimize access
            AffixManager am = FindObjectOfType<AffixManager>(); // Find AffixManager
            if (db == null) { Debug.LogError("DropLoot: ItemDatabase not found!"); return; }
            if (am == null) { Debug.LogError("DropLoot: AffixManager not found!"); return; }
            ItemGenerator.Initialize(db, am); // Ensure generator is ready with both DB and AffixMgr

            Vector3 dropBasePosition = transform.position;

            foreach (ItemDropResult result in dropResults) {
                 Item itemToDrop = null;
                 if (result.isGenerated) {
                     // Pass context level and MF to the main generator
                     itemToDrop = ItemGenerator.GenerateItem(result.generationLevel, playerMagicFind);
                 } else {
                      // For non-generated (specific ID drops), use CreateItemInstance
                      itemToDrop = db.CreateItemInstance(result.itemID);
                 }

                 if (itemToDrop != null && result.amount > 0) {
                     Vector3 spawnPos = dropBasePosition + (Vector3)UnityEngine.Random.insideUnitCircle * 0.7f;
                     SpawnPickupInstance(itemToDrop, result.amount, spawnPos);
                 }
                 // else { Log failure }
            }
        }
        // else { Log no loot }
    }

     private void SpawnPickupInstance(Item itemData, int amount, Vector3 position) {
     {
         if(itemPickupPrefab == null) return;

         GameObject pickupGO = Instantiate(itemPickupPrefab, position, Quaternion.identity);
         ItemPickup pickupScript = pickupGO.GetComponent<ItemPickup>();
         if (pickupScript != null)
         {
             pickupScript.Initialize(itemData, amount);
         }
         else
         {
             Debug.LogError($"ItemPickupPrefab '{itemPickupPrefab.name}' is missing ItemPickup script!", itemPickupPrefab);
             Destroy(pickupGO);
         }
     }

}
} // End of EnemyController Class

// --- Required Addition to Character.cs ---
// Add this placeholder method inside your Character class:
/*
public float GetMagicFindValue() {
    float baseMF = 0f;
    // TODO: Calculate MF from equipped gear affixes (ItemAffix.AffixType.MagicFind?)
    // float gearMF = inventory?.GetTotalEquippedStatModifier(ItemAffix.AffixType.MagicFind) ?? 0f;
    // TODO: Calculate MF from temporary buffs (Status Effects?)
    // float buffMF = StatusEffectController?.GetTotalValue(StatusEffectType.MagicFind) ?? 0f;
    Debug.Log("Placeholder GetMagicFindValue returning 0");
    return baseMF; // + gearMF + buffMF;
}
*/