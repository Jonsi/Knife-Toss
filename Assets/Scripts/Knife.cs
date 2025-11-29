using UnityEngine;

public class Knife : MonoBehaviour
{
    private KnifeThrower thrower;
    private float speed;
    private bool isResolved;
    private bool isStuck;
    private Rigidbody2D rb2d;
    
    [Header("Stick Settings")]
    [SerializeField] private float embedOffset = 0.02f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip hitTargetSound;
    [SerializeField] private AudioClip hitKnifeSound;
    [SerializeField] private float soundVolume = 1f;
    
    public void Initialize(KnifeThrower knifeThrower, float throwSpeed)
    {
        thrower = knifeThrower;
        speed = throwSpeed;
        isResolved = false;
        isStuck = false;
        
        // Get or add Rigidbody2D and configure it
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.gravityScale = 0f; // Disable gravity for manual movement
            rb2d.isKinematic = false; // Keep dynamic for collision detection
        }
    }
    
    void Update()
    {
        if (isResolved || isStuck) return;
        
        // Use Rigidbody2D movement if available, otherwise manual transform
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.up * speed;
        }
        else
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.transform, collision.gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.transform, collision.gameObject);
    }
    
    private void HandleCollision(Transform collisionTransform, GameObject collisionObject)
    {
        if (isResolved || isStuck) return;
        
        Debug.Log($"Knife collision detected with: {collisionObject.name} (Tag: {collisionObject.tag})");
        
        // Check if collided with target
        if (collisionObject.CompareTag("Target"))
        {
            StickToTarget(collisionTransform);
            return;
        }
        
        // Check if collided with another knife - fail on ANY knife collision
        if (collisionObject.CompareTag("Knife"))
        {
            Knife otherKnife = collisionObject.GetComponent<Knife>();
            if (otherKnife != null)
            {
                Debug.Log($"Knife hit another knife! Other knife isStuck: {otherKnife.IsStuck}");
                // Always fail when hitting another knife (stuck or in flight)
                // Only trigger once per knife by checking if we're not already resolved
                TriggerFail();
                return;
            }
        }
    }
    
    private void StickToTarget(Transform targetTransform)
    {
        isStuck = true;
        isResolved = true;
        
        // Stop movement
        speed = 0f;
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
        }
        
        // Calculate direction from target center to knife
        Vector2 targetCenter = targetTransform.position;
        Vector2 knifePosition = transform.position;
        Vector2 direction = (knifePosition - targetCenter).normalized;
        
        // Get target's circle collider to get radius
        CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
        float targetRadius = targetCollider != null ? targetCollider.radius : 0.5f;
        
        // Calculate embedded position at target edge
        Vector2 embeddedPosition = targetCenter + direction * (targetRadius + embedOffset);
        
        // Set world position first
        transform.position = embeddedPosition;
        
        // Parent knife to target (will maintain world position automatically)
        transform.SetParent(targetTransform);
        
        // Notify thrower that knife is resolved
        if (thrower != null)
        {
            thrower.OnKnifeResolved();
        }
        
        // Log successful stick
        Debug.Log("SUCCESS: Knife stuck to target!");
        
        // Play hit target sound
        if (hitTargetSound != null)
        {
            AudioSource.PlayClipAtPoint(hitTargetSound, transform.position, soundVolume);
        }
        
        // Trigger successful stick event (placeholder for future GameController)
        OnKnifeStuck?.Invoke();
    }
    
    private void TriggerFail()
    {
        isResolved = true;
        
        // Stop movement
        speed = 0f;
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
        }
        
        // Log failure
        Debug.Log("FAIL: Knife hit another knife!");
        
        // Play hit knife sound
        if (hitKnifeSound != null)
        {
            AudioSource.PlayClipAtPoint(hitKnifeSound, transform.position, soundVolume);
        }
        
        // Notify thrower that knife is resolved (failed)
        if (thrower != null)
        {
            thrower.OnKnifeResolved();
        }
        
        // Trigger fail event (placeholder for future GameController)
        OnKnifeFailed?.Invoke();
    }
    
    public void Resolve()
    {
        if (isResolved) return;
        
        isResolved = true;
        
        if (thrower != null)
        {
            thrower.OnKnifeResolved();
        }
    }
    
    // Events for future GameController integration
    public static System.Action OnKnifeStuck;
    public static System.Action OnKnifeFailed;
    
    public bool IsStuck => isStuck;
}
