using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum AIBehavior { MeleeChase, RangedKite, PatrolThenChase } // Add more behaviors later

    [Header("Core Behavior")]
    [Tooltip("The primary behavior pattern for this AI.")]
    [SerializeField] private AIBehavior behavior = AIBehavior.MeleeChase;

    [Header("Detection & Targeting")]
    [Tooltip("Range within which the enemy detects the player.")]
    [SerializeField] private float detectionRange = 8f;
    [Tooltip("Layers containing the player.")]
    [SerializeField] private LayerMask playerLayer; // Assign Player layer in Inspector
    private Transform playerTarget;
    private bool hasDetectedPlayer = false; // Has the player been seen at least once?

    [Header("Movement")]
    [Tooltip("Speed when chasing the player.")]
    [SerializeField] private float chaseSpeed = 2.5f;
    [Tooltip("Speed when patrolling (if applicable).")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [Tooltip("Distance to maintain from player (used for kiting or stopping melee).")]
    [SerializeField] private float stoppingDistance = 1.2f; // Stopping distance for melee, minimum kite distance for ranged

    [Header("Melee Attack (If Applicable)")]
    [Tooltip("Range within which the enemy can initiate a melee attack.")]
    [SerializeField] private float meleeAttackRange = 1.5f;
    [Tooltip("Base damage dealt by melee attack.")]
    [SerializeField] private float meleeAttackDamage = 8f;
    [Tooltip("Time in seconds between melee attacks.")]
    [SerializeField] private float meleeAttackCooldown = 1.8f;
    [Tooltip("Chance (0.0 to 1.0) for melee attack to be critical.")]
    [SerializeField] [Range(0f, 1f)] private float meleeCritChance = 0.05f; // 5%
    [Tooltip("Damage multiplier for melee critical hits.")]
    [SerializeField] private float meleeCritMultiplier = 1.5f;
    private float nextMeleeAttackTime = 0f;

    [Header("Ranged Attack (If Applicable)")]
    [Tooltip("Prefab of the projectile to shoot.")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Transform point where projectiles spawn.")]
    [SerializeField] private Transform projectileSpawnPoint;
    [Tooltip("Minimum distance to attempt ranged attacks.")]
    [SerializeField] private float minRangedAttackRange = 3f;
    [Tooltip("Maximum distance to attempt ranged attacks.")]
    [SerializeField] private float maxRangedAttackRange = 10f; // Must be within detectionRange
    [Tooltip("Base damage dealt by ranged projectile.")]
    [SerializeField] private float rangedAttackDamage = 6f;
    [Tooltip("Speed of the projectile.")]
    [SerializeField] private float projectileSpeed = 7f;
    [Tooltip("Time in seconds between ranged attacks.")]
    [SerializeField] private float rangedAttackCooldown = 2.5f;
    [Tooltip("Does the enemy try to back away if player gets too close (kiting)?")]
    [SerializeField] private bool kiteWhenClose = true;
    [Tooltip("How close the player needs to be for the enemy to start kiting (backing away).")]
    [SerializeField] private float kiteDistance = 4f; // Should be > stoppingDistance
    [Tooltip("Chance (0.0 to 1.0) for ranged attack to be critical.")]
    [SerializeField] [Range(0f, 1f)] private float rangedCritChance = 0.05f;
    [Tooltip("Damage multiplier for ranged critical hits.")]
    [SerializeField] private float rangedCritMultiplier = 1.5f;
    private float nextRangedAttackTime = 0f;


    [Header("Patrol (If Applicable)")]
    [Tooltip("Array of points defining the patrol path (Transforms). Uses local positions if parented.")]
    [SerializeField] private Transform[] patrolPoints;
    [Tooltip("Time to wait at each patrol point.")]
    [SerializeField] private float patrolWaitTime = 1.5f;
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private bool isPatrolling = false;

    // Components & State
    private EnemyController enemyController;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // For flipping

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get renderer
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        FindPlayer();
        if (behavior == AIBehavior.PatrolThenChase)
        {
            isPatrolling = (patrolPoints != null && patrolPoints.Length > 0);
            if (!isPatrolling) {
                Debug.LogWarning($"Enemy {gameObject.name} set to PatrolThenChase but has no patrol points. Defaulting to MeleeChase.", this);
                behavior = AIBehavior.MeleeChase; // Fallback if patrol not set up
            }
        }
    }

    void FindPlayer() {
        // Simple find - consider a manager for robustness
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTarget = (playerObject != null) ? playerObject.transform : null;
        if (playerTarget == null) {
            Debug.LogWarning($"EnemyAI on {gameObject.name}: Could not find Player tag. AI inactive.", this);
            enabled = false;
        }
    }


    void Update()
    {
        if (playerTarget == null || enemyController.IsDefeated())
        {
            rb.velocity = Vector2.zero;
            return; // Exit if no player or defeated
        }

        // --- Core Logic ---
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        // --- Detection ---
         bool canSeePlayer = CanSeePlayer(distanceToPlayer); // Check range and potentially line of sight
         if(canSeePlayer) hasDetectedPlayer = true; // Player has been seen


        // --- Behavior State Machine ---
        if (!hasDetectedPlayer && isPatrolling) {
            HandlePatrolState();
        }
        else if (hasDetectedPlayer) { // Player has been detected at least once
            isPatrolling = false; // Stop patrolling once player is seen
            HandleCombatState(distanceToPlayer);
        } else {
            // Idle state if not patrolling and player not detected
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

         HandleSpriteFlip(); // Flip based on movement or player direction
    }


     bool CanSeePlayer(float distance) {
         if (distance > detectionRange) return false;

         // Optional Line of Sight Check:
         // RaycastHit2D hit = Physics2D.Linecast(transform.position, playerTarget.position, obstacleLayerMask);
         // return hit.collider == null || hit.transform == playerTarget; // Can see if no obstacles or hit player

         return true; // Simple range check for now
     }

     // --- State Handling Methods ---

    void HandlePatrolState()
    {
        if (patrolPoints.Length == 0) return;

         // Check wait timer
         if(patrolWaitTimer > 0) {
            patrolWaitTimer -= Time.deltaTime;
             rb.velocity = new Vector2(0, rb.velocity.y); // Stop while waiting
             return;
         }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        float distanceToPatrolPoint = Vector2.Distance(transform.position, targetPoint.position);

        if (distanceToPatrolPoint < 0.5f) // Reached the patrol point
        {
            patrolWaitTimer = patrolWaitTime; // Start waiting
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // Move to next point (looping)
             // Debug.Log($"{gameObject.name} reached patrol point {currentPatrolIndex}, waiting.");
        }
        else
        {
            // Move towards patrol point
            float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);
        }
    }


    void HandleCombatState(float distanceToPlayer)
    {
        // Determine primary action based on behavior
        switch (behavior)
        {
            case AIBehavior.MeleeChase:
                PerformMeleeChase(distanceToPlayer);
                break;
            case AIBehavior.RangedKite:
                PerformRangedKite(distanceToPlayer);
                break;
            case AIBehavior.PatrolThenChase: // Switches to MeleeChase once player detected
                 PerformMeleeChase(distanceToPlayer);
                 break;
            // Add cases for other behaviors
        }
    }

     // --- Behavior Action Methods ---

     void PerformMeleeChase(float distanceToPlayer) {
         // Movement
        if (distanceToPlayer > stoppingDistance)
        {
            float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop when close
        }

         // Attack
         if (distanceToPlayer <= meleeAttackRange && Time.time >= nextMeleeAttackTime)
        {
            MeleeAttack();
            nextMeleeAttackTime = Time.time + meleeAttackCooldown;
        }
     }

    void PerformRangedKite(float distanceToPlayer) {
         // Movement (Kiting)
         float moveDirection = 0f;
         if (kiteWhenClose && distanceToPlayer < kiteDistance) {
             // Move away from player if too close
             moveDirection = -Mathf.Sign(playerTarget.position.x - transform.position.x);
             rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y); // Kite at chase speed? Or patrol speed?
         } else if (distanceToPlayer > stoppingDistance) { // Maintain minimum distance (stoppingDistance)
              // Move towards player if further than stoppingDistance
              moveDirection = Mathf.Sign(playerTarget.position.x - transform.position.x);
              rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
         } else {
              // Within sweet spot, stop moving horizontally
              rb.velocity = new Vector2(0, rb.velocity.y);
         }


         // Attack
         if (distanceToPlayer >= minRangedAttackRange && distanceToPlayer <= maxRangedAttackRange && Time.time >= nextRangedAttackTime) {
            RangedAttack();
             nextRangedAttackTime = Time.time + rangedAttackCooldown;
         }
    }

    // --- Attack Implementation ---

    void MeleeAttack()
    {
        Debug.Log($"{gameObject.name} MELEE attacks player!");
        // animator?.SetTrigger("MeleeAttack");
        Character playerCharacter = CharacterManager.Instance?.character;
        if (playerCharacter != null && !playerCharacter.IsDead())
        {
            // --- Calculate Damage (with Crit) ---
            float finalDamage = meleeAttackDamage; // TODO: Scale base damage with level?
            bool isCritical = Random.value < meleeCritChance;

            if (isCritical) {
                finalDamage *= meleeCritMultiplier;
                Debug.Log($"Enemy MELEE Crit! Final Damage: {finalDamage}");
                 // TODO: Show Crit indicator over player?
            }

            playerCharacter.TakeDamage(finalDamage); // Apply final damage
            // AudioManager.Instance?.PlaySoundEffect(meleeSound);
        }
    }

    void RangedAttack()
    {
         if(projectilePrefab == null || projectileSpawnPoint == null) {
            Debug.LogWarning($"{gameObject.name} cannot perform ranged attack - prefab or spawn point missing.", this);
            return;
         }

         Debug.Log($"{gameObject.name} RANGED attacks player!");
         // animator?.SetTrigger("RangedAttack");

         // Determine direction towards player (for aiming)
         Vector2 aimDirection = (playerTarget.position - projectileSpawnPoint.position).normalized;
         GameObject projectileGO = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
         Projectile projectileScript = projectileGO.GetComponent<Projectile>();

         if (projectileScript != null) {
             // --- Calculate Damage (with Crit) ---
             float finalDamage = rangedAttackDamage; // TODO: Scale base damage with level?
             bool isCritical = Random.value < rangedCritChance;

             if (isCritical) {
                 finalDamage *= rangedCritMultiplier;
                  Debug.Log($"Enemy RANGED Crit! Final Damage: {finalDamage}");
                 // Note: The projectile itself doesn't know it's a crit, only the damage is increased.
                 // For visual crit indicators on projectile hit, need more complex system.
             }

            projectileScript.Initialize(aimDirection, projectileSpeed, finalDamage, playerLayer); // Pass final damage
             // AudioManager.Instance?.PlaySoundEffect(rangedAttackSound);
         } else { /* ... LogError and Destroy ... */ }
    }

    // --- Utility ---
    void HandleSpriteFlip() {
         if (spriteRenderer != null && rb != null && Mathf.Abs(rb.velocity.x) > 0.1f) // Flip based on movement
         {
             spriteRenderer.flipX = (rb.velocity.x < 0);
         } else if (spriteRenderer != null && playerTarget != null && hasDetectedPlayer) // Flip based on player position if idle
         {
            spriteRenderer.flipX = (playerTarget.position.x < transform.position.x);
         }
    }
}