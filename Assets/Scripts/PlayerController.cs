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

    public LayerMask solidObjectsLayer;
    public LayerMask interactablesLayer;

    // Stamina/Dash
    public float baseMoveSpeed = 6f;          // keep your current move speed here
    public float dashSpeedMultiplier = 1.9f;  // how much faster when dashing
    public float staminaMax = 2f;             // seconds of full dash
    public float staminaRegenPerSec = 0.8f;   // regen when not dashing
    public float dashDrainPerSec = 1.0f;      // drain while holding space
    public float exhaustedSlowMultiplier = 0.65f; // brief slow if you fully drain
    public float exhaustedPenaltyTime = 0.75f;

    public UnityEngine.UI.Slider StaminaBar;

    // internal state
    private float currentStamina;
    private bool isDashing;
    private bool isExhausted;
    private float exhaustedTimer;
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

        //init stamina
        currentStamina = staminaMax;
        if (StaminaBar != null)
        {
            StaminaBar.minValue = 0f;
            StaminaBar.maxValue = staminaMax;
            StaminaBar.value = currentStamina;
        }
    }
    
    void Update()
    {
        // get input from WASD keys
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D keys
        movement.y = Input.GetAxisRaw("Vertical");   // W/S keys

       // Debug.Log("This is input.x" + movement.x);
       // Debug.Log("This is input.y" + movement.y);

        



        if (movement.x !=0) movement.y = 0; //if we going right or left we cant go up or down

        isMoving = movement.magnitude > 0.1f; //tell it when we are moving or stopped

        // exhausted penalty countdown
        if (isExhausted)
        {
            exhaustedTimer -= Time.deltaTime;
            if (exhaustedTimer <= 0f) isExhausted = false;
        }

        // dash input
        bool dashHeld = Input.GetKey(KeyCode.Space);
        isDashing = dashHeld && currentStamina > 0.01f && !isExhausted;

        // stamina drain/regen
        if (isDashing)
        {
            currentStamina -= dashDrainPerSec * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isDashing = false;
                isExhausted = true;
                exhaustedTimer = exhaustedPenaltyTime;
            }
        }
        else
        {
            currentStamina += staminaRegenPerSec * Time.deltaTime;
            if (currentStamina > staminaMax) currentStamina = staminaMax;
        }

        // update stamina UI (if assigned)
        if (StaminaBar != null) StaminaBar.value = currentStamina;

        if (!isMoving)
        {
            movement.x = 0;
            movement.y = 0;
            // Force reset the animator parameters
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", 0);
        }

        Debug.Log("isMoving: " + isMoving + ", movement: " + movement);

        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);

        if(movement != Vector2.zero)
        {
            var targetPos = transform.position;
            targetPos.x += movement.x;
            targetPos.y += movement.y;
            if (IsWalkable(targetPos))
            {
                isMoving = movement.magnitude > .1f;
                animator.SetBool("isMoving", isMoving);
            }
        }

       

        

    }
    
    void FixedUpdate()
    {
        // use velocity for smooth movement - no more glitching!
        float speed = baseMoveSpeed;
        if (isDashing) speed *= dashSpeedMultiplier;
        if (isExhausted) speed *= exhaustedSlowMultiplier;

        rb.velocity = movement * speed;
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

    private bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactablesLayer) != null)
        {
            return false;
        } else
        {
            return true;
        }
    }
}