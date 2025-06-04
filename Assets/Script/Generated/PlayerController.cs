using UnityEngine;
using System; // Needed for Action event if used here

// Add required components to ensure they exist on the GameObject
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] // BoxCollider2D or CapsuleCollider2D recommended
[RequireComponent(typeof(SpriteRenderer))]
// Add Animator if you have animations
// [RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 5f;
    // Add jump force, ground check variables etc. if implementing jumping

    [Header("Combat Settings")]
    [Tooltip("Base damage added to every basic attack.")]
    [SerializeField] public float baseAttackDamage = 5f; // Lowered base, more reliance on stats/weapon
    [Tooltip("Time in seconds between allowed basic attacks.")]
    [SerializeField] public float attackCooldown = 0.8f;
    [Tooltip("How much damage each point of Strength adds to basic attacks.")]
    [SerializeField] public float strengthDamageModifier = 0.5f;
    [Tooltip("Range of the basic attack check in front of the player.")]
    [SerializeField] public float attackRange = 1.5f;
    [Tooltip("How far offset the attack check origin is from the player's center.")]
    [SerializeField] public float attackOffset = 0.75f;
    [Tooltip("Layers that contain enemies.")]
    [SerializeField] public LayerMask enemyLayer; // Assign Enemy layer in Inspector
    [Tooltip("Base critical hit chance (0.0 to 1.0).")]
    [SerializeField] [Range(0f, 1f)] private float baseCritChance = 0.05f; // 5% base chance
    [Tooltip("Critical hit damage multiplier (e.g., 1.5 for 150% damage).")]
    [SerializeField] private float critMultiplier = 1.5f;

    [Header("Input Settings")]
    [Tooltip("Key for basic attack.")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0; // Left Mouse Button
    [Tooltip("Key for interaction.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    // Add keys for skills etc.

    // Component References (populated in Awake/Start)
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    protected StatusEffectController statusEffectController;
    // private Animator animator;

    // Reference to the character data
    private Character characterData;
    private bool isControlEnabled = true; // Controls whether input is processed

    // --- Combat State Variables ---
    private float nextAttackTime = 0f; // Tracks when the player can attack again

    // --- Unity Lifecycle Methods ---

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        statusEffectController = GetComponent<StatusEffectController>();
        // animator = GetComponent<Animator>();
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            characterData = CharacterManager.Instance.character;
            Debug.Log($"PlayerController initialized for {characterData.characterName}.");
            characterData.OnDeath += HandlePlayerDeath;

            if (characterData.IsDead()) HandlePlayerDeath();
            else isControlEnabled = true;
        }
        else
        {
            Debug.LogError("PlayerController could not find Character data! Disabling.", this);
            isControlEnabled = false;
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (characterData != null)
        {
            characterData.OnDeath -= HandlePlayerDeath;
        }
    }

    void Update()
    {
        if (!isControlEnabled || characterData == null) return;

        HandleInput();
        HandleSpriteFlip();
        // UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!isControlEnabled || characterData == null) return;
        HandleMovement();
    }

    // --- Input & Movement Handling ---

    private float horizontalInput;

    private void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(attackKey)) TriggerAttack();
        if (Input.GetKeyDown(interactionKey)) TriggerInteraction();
        if (Input.GetKeyDown(KeyCode.Alpha1)) TriggerSkill(0);
        // Add more skill inputs...
    }

    private void HandleMovement()
    {
        Vector2 currentVelocity = rb.velocity;
        currentVelocity.x = horizontalInput * moveSpeed;
        rb.velocity = currentVelocity;
    }

    private void HandleSpriteFlip()
    {
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            spriteRenderer.flipX = (horizontalInput < 0);
        }
    }

    // --- Action Triggers ---

    private void TriggerAttack()
    {
        if (Time.time < nextAttackTime) return; // Cooldown check
        if (characterData == null || !isControlEnabled) return; // Added isControlEnabled check

        // Debug.Log($"{characterData.characterName} attempts a basic attack!");
        // animator?.SetTrigger("Attack");

        nextAttackTime = Time.time + GetAttackCooldown(); // Use calculated cooldown

        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackOrigin = (Vector2)transform.position + (attackDirection * attackOffset);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(attackOrigin, attackRange / 2f, attackDirection, 0.1f, enemyLayer);
        Debug.DrawRay(attackOrigin, attackDirection * attackRange, Color.red, 0.5f);

        int enemiesHit = 0;
        foreach (RaycastHit2D hit in hits)
        {
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDefeated()) // Check if enemy is already defeated
            {
                // Debug.Log($"Attack hit {enemy.gameObject.name}");
                enemiesHit++;

                // Calculate Base Damage (Stats + Weapon)
                float baseDamage = CalculatePlayerDamage();
                // Apply Damage with Crit Check
                ApplyDamageToEnemy(enemy, baseDamage);
            }
        }
        // Optional: Play sounds based on enemiesHit > 0
    }


private void TriggerSkill(int skillSlotIndex)
    {
        if (characterData == null || !isControlEnabled) return;

        // 1. Get Skill Instance from Hotbar/Tree (Needs proper mapping system)
        Skill skillToUse = null;
        // Basic example: Get from main tree by index
        if (characterData.mainClassSkillTree != null && skillSlotIndex >= 0 && skillSlotIndex < characterData.mainClassSkillTree.skills.Count)
        {
            skillToUse = characterData.mainClassSkillTree.skills[skillSlotIndex];
        }
        // TODO: Expand to check subclass tree, use actual hotbar mapping

        if (skillToUse == null) { Debug.LogWarning($"TriggerSkill: No skill found at index {skillSlotIndex}."); return; }

        // 2. Check Requirements (Cooldown, Level, Mana)
        // TODO: Check Cooldown: if (SkillCooldownManager.IsOnCooldown(skillToUse.skillID)) return;
        if (characterData.Level < skillToUse.requiredLevel) { Debug.Log("Level req not met."); return; }
        if (!characterData.SpendMana(skillToUse.GetCurrentManaCost())) { Debug.Log("Not enough mana."); return; }

        // 3. EXECUTE the Skill Polymorphically!
        Debug.Log($"{characterData.characterName} using skill '{skillToUse.skillName}'...");
        bool success = skillToUse.Execute(this, characterData, statusEffectController); // <<< CALL SKILL'S OWN EXECUTE

        if (success) {
             // TODO: Start Cooldown: SkillCooldownManager.StartCooldown(skillToUse);
             Debug.Log($"Skill '{skillToUse.skillName}' executed.");
        } else {
             Debug.LogWarning($"Skill '{skillToUse.skillName}' execution failed or aborted.");
             characterData.RestoreMana(skillToUse.GetCurrentManaCost()); // Refund cost on failure?
        }
    }

    private void TriggerInteraction()
    {
        if (characterData == null || !isControlEnabled) return;

        Debug.Log($"{characterData.characterName} attempts to interact!");
        float interactionRadius = 1.0f; // How close player needs to be
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, interactionRadius); // Add layer mask for interactables

        GameObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach(Collider2D col in nearbyObjects)
        {
             // Check if object has an "Interact" method via interface or component check
             NPCManager npc = col.GetComponent<NPCManager>();
             // ItemPickup pickup = col.GetComponent<ItemPickup>(); // Pickups handled by trigger enter usually
             // ChestController chest = col.GetComponent<ChestController>(); // Example

             if (npc != null /* || pickup != null || chest != null etc. */) {
                 float distance = Vector2.Distance(transform.position, col.transform.position);
                 if(distance < closestDistance) {
                    closestDistance = distance;
                     closestObject = col.gameObject;
                 }
             }
        }

        if (closestObject != null) {
            Debug.Log($"Interacting with {closestObject.name}");
            // Call interaction method - needs specific component check
             NPCManager npcToInteract = closestObject.GetComponent<NPCManager>();
             if (npcToInteract != null) npcToInteract.Interact();
             // else if (chestToInteract != null) chestToInteract.Interact();
        } else {
             Debug.Log("Nothing interactable in range.");
        }
    }

    // --- Damage Calculation & Application ---

    private float CalculatePlayerDamage()
    {
        if (characterData == null) return 0f;
        float strengthBonus = characterData.attributes.Strength * strengthDamageModifier;
        float weaponDamage = characterData.GetWeaponDamage();
        return baseAttackDamage + strengthBonus + weaponDamage;
    }

     // Gets actual attack cooldown, potentially modified by stats/buffs later
     private float GetAttackCooldown() {
         // TODO: Adjust cooldown based on Agility or Haste effects?
         // float agilityFactor = 1.0f - (characterData.attributes.Agility * 0.005f); // Example
         // return attackCooldown * agilityFactor;
         return attackCooldown;
     }


    // Calculate actual crit chance, potentially modified by stats/buffs later
     private float GetCritChance() {
          // TODO: Adjust crit chance based on Agility or gear?
          // float agilityBonus = characterData.attributes.Agility * 0.001f; // Example
          // return baseCritChance + agilityBonus;
          return baseCritChance;
     }

    public void ApplyDamageToEnemy(EnemyController enemy, float baseDamage)
    {
        if (enemy == null) return;

        float finalDamage = baseDamage;
        bool isCritical = UnityEngine.Random.value < GetCritChance(); // Use calculated chance

        if (isCritical)
        {
            finalDamage *= critMultiplier; // Use crit multiplier field
            Debug.Log($"CRITICAL HIT! Base: {baseDamage}, Final: {finalDamage}");
            // TODO: Show "Critical!" text effect over enemy (Visual Effect system needed)
            // DamagePopupManager.Instance.ShowCritPopup(enemy.transform.position, finalDamage);
        }

        enemy.TakeDamage(finalDamage);
    }

    // --- Death Handling ---
    private void HandlePlayerDeath()
    {
        if (!isControlEnabled && characterData.IsDead()) return; // Prevent re-entry if already dead handled

        Debug.Log("PlayerController reacting to player death. Disabling controls.");
        isControlEnabled = false;
        rb.velocity = Vector2.zero;
        horizontalInput = 0f;
        // animator?.SetBool("IsDead", true); // Or animator?.SetTrigger("Die");
    }

    // --- Public Control Methods ---
    public void DisableControl()
    {
        isControlEnabled = false;
        rb.velocity = Vector2.zero;
        horizontalInput = 0f;
    }

    public void EnableControl()
    {
        if (characterData != null && !characterData.IsDead())
        {
            isControlEnabled = true;
        }
    }
} // End of PlayerController Class