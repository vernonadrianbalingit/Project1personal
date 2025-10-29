using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f; // how fast the player moves
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    private bool isMoving;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>(); 
    }
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

        Debug.Log("This is input.x" + movement.x);
        Debug.Log("This is input.y" + movement.y);

        



        if (movement.x !=0) movement.y = 0; //if we going right or left we cant go up or down
        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);

        isMoving = movement.magnitude > .1f;
        animator.SetBool("isMoving", isMoving);

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