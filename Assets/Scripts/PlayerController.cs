using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f; // how fast the player moves
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        // get the rigidbody component so we can move the player
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // freeze rotation so player doesn't spin around
        rb.freezeRotation = true;
    }
    
    void Update()
    {
        // get input from WASD keys
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D keys
        movement.y = Input.GetAxisRaw("Vertical");   // W/S keys
        
        // flip sprite based on direction (simple left/right flip)
        if (movement.x > 0)
        {
            spriteRenderer.flipX = false; // facing right
        }
        else if (movement.x < 0)
        {
            spriteRenderer.flipX = true; // facing left
        }
    }
    
    void FixedUpdate()
    {
        // use velocity for smooth movement - no more glitching!
        rb.velocity = movement * moveSpeed;
    }
    
    // this will be useful later for checking if player is near something they can interact with
    void OnTriggerEnter2D(Collider2D other)
    {
        // placeholder for now - we'll add interaction logic later
        if (other.CompareTag("Interactable"))
        {
            Debug.Log("Player can interact with: " + other.name);
        }
    }
}