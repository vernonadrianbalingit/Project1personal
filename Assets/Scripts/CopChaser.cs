using UnityEngine;

// Simple chaser AI: patrol-idle until it can see the player, then chase
// LOS is blocked by SolidObjects layer using a Linecast
public class CopChaser : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 3.5f;
    public float visionRange = 12f;
    public float loseSightTime = 1.25f;
    public LayerMask solidLayerMask; // assign SolidObjects layer in Inspector
    
    [Header("Avoidance / Steering")]
    public float probeDistance = 0.6f;      // how far ahead to check for walls
    public float sideProbeOffset = 0.25f;   // left/right probe offset from center
    public float unstickNudge = 0.5f;       // small sideways nudge when stuck

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private float lostTimer;
    private bool hasLOS;
    
    [Header("Boogie State")]
    private bool isBoogied;
    private float boogieTimer;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        // keep cops from spinning or falling due to collisions/physics
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.angularVelocity = 0f;
        }
    }

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
        
        // store original color for boogie effect
        if (sprite != null) originalColor = sprite.color;
    }

    void Update()
    {
        if (player == null) { rb.velocity = Vector2.zero; return; }

        // handle boogie state - cops dance and ignore player
        if (isBoogied)
        {
            boogieTimer -= Time.deltaTime;
            if (boogieTimer <= 0f)
            {
                // end boogie state
                isBoogied = false;
                if (sprite != null) sprite.color = originalColor;
            }
            else
            {
                // dance effect - small random movement and color flash
                rb.velocity = new Vector2(
                    Mathf.Sin(Time.time * 8f) * 0.5f,
                    Mathf.Cos(Time.time * 6f) * 0.3f
                );
                if (sprite != null)
                {
                    sprite.color = Color.Lerp(originalColor, Color.magenta, 
                        Mathf.PingPong(Time.time * 4f, 1f));
                }
            }
            return; // skip normal chase logic while boogied
        }

        // LOS: within range and no wall between
        Vector2 dir = (player.position - transform.position);
        float dist = dir.magnitude;
        bool inRange = dist <= visionRange;
        bool blocked = Physics2D.Linecast(transform.position, player.position, solidLayerMask);
        bool seePlayer = inRange && !blocked;

        if (seePlayer)
        {
            hasLOS = true;
            lostTimer = 0f;
        }
        else if (hasLOS)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= loseSightTime) hasLOS = false;
        }

        // Move with simple wall-slide avoidance
        if (hasLOS || seePlayer)
        {
            Vector2 desired = dir.normalized * moveSpeed;

            // forward probe
            Vector2 origin = (Vector2)transform.position;
            RaycastHit2D hitF = Physics2D.Raycast(origin, desired.normalized, probeDistance, solidLayerMask);

            if (hitF.collider != null)
            {
                // slide along surface: remove component into the wall normal
                Vector2 slide = desired - Vector2.Dot(desired, hitF.normal) * hitF.normal;
                desired = slide;

                // if still tiny, use side probes to pick a bias
                if (desired.sqrMagnitude < 0.01f)
                {
                    Vector2 perp = new Vector2(-dir.y, dir.x).normalized; // left vector relative to target
                    Vector2 leftOrigin = origin + perp * sideProbeOffset;
                    Vector2 rightOrigin = origin - perp * sideProbeOffset;
                    bool leftBlocked = Physics2D.Raycast(leftOrigin, dir.normalized, probeDistance, solidLayerMask);
                    bool rightBlocked = Physics2D.Raycast(rightOrigin, dir.normalized, probeDistance, solidLayerMask);
                    desired = rightBlocked && !leftBlocked ? perp * moveSpeed : (!rightBlocked && leftBlocked ? -perp * moveSpeed : perp * moveSpeed);
                }
            }

            // unstick: if we're chasing but barely moving, add a tiny perpendicular nudge
            if (rb.velocity.sqrMagnitude < 0.0004f)
            {
                Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
                desired += perp * unstickNudge;
            }

            rb.velocity = desired;
            if (sprite != null) sprite.flipX = rb.velocity.x < 0f;
        }
        else
        {
            rb.velocity = Vector2.zero; // idle when no LOS
        }
    }

    // called by boogie bomb to make cop dance
    public void EnterBoogie(float duration)
    {
        isBoogied = true;
        boogieTimer = duration;
        hasLOS = false; // stop chasing
        lostTimer = 0f;
    }
}
 
