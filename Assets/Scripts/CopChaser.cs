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

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private float lostTimer;
    private bool hasLOS;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    void Update()
    {
        if (player == null) { rb.velocity = Vector2.zero; return; }

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

        // Move
        if (hasLOS || seePlayer)
        {
            Vector2 v = dir.normalized * moveSpeed;
            rb.velocity = v;
            if (sprite != null) sprite.flipX = v.x < 0f;
        }
        else
        {
            rb.velocity = Vector2.zero; // idle when no LOS
        }
    }
}
 
