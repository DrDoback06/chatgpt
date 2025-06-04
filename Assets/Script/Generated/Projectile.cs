using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed;
    private float damage;
    private LayerMask targetLayer; // Layer the projectile should hit (e.g., Player)
    private bool initialized = false;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();
        if (rb.gravityScale != 0) {
             // Debug.LogWarning($"Projectile '{gameObject.name}' should probably have Gravity Scale 0.", this);
            // rb.gravityScale = 0; // Optionally force it
        }
        if(col != null) col.isTrigger = true; // Ensure trigger for detection
    }

    /// <summary>
    /// Sets up the projectile's properties after instantiation.
    /// </summary>
    public void Initialize(Vector2 direction, float moveSpeed, float damageAmount, LayerMask layerToHit)
    {
        this.moveDirection = direction.normalized;
        this.speed = moveSpeed;
        this.damage = damageAmount;
        this.targetLayer = layerToHit;
        this.initialized = true;

        // Optional: Rotate sprite to face movement direction
        if (direction != Vector2.zero) {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

         // Destroy after some time if it doesn't hit anything
         Destroy(gameObject, 5f); // Destroy after 5 seconds (adjust as needed)
    }

    void FixedUpdate() // Use FixedUpdate for rigidbody movement
    {
        if (initialized)
        {
            // Move the projectile
            rb.velocity = moveDirection * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        // Check if the hit object is on the target layer
        // Compare layer of 'other' GameObject with 'targetLayer' mask
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Debug.Log($"Projectile hit {other.gameObject.name} on target layer.");

            // Try to apply damage to the Character component on the hit object
            Character character = other.GetComponent<Character>(); // Direct check (if Character is MonoBehaviour)
            if (character == null) {
                 // If Character is not MB, check PlayerController for data access
                 PlayerController pc = other.GetComponent<PlayerController>();
                 if(pc != null) character = CharacterManager.Instance?.character;
            }


            if (character != null && !character.IsDead())
            {
                character.TakeDamage(damage);
            }

            // --- Hit Effect ---
            // Instantiate hit particle effect
            // Play hit sound
            // AudioManager.Instance?.PlaySoundEffect(hitSound);

            // Destroy the projectile immediately upon hitting a valid target
            Destroy(gameObject);
        }
        // Optional: Check for collision with environment/obstacles
        // else if (((1 << other.gameObject.layer) & environmentLayer) != 0) {
        //     Debug.Log("Projectile hit environment.");
        //     // Instantiate hit particle effect
        //     Destroy(gameObject);
        // }
    }
}