using UnityEngine;
using TMPro;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f; // how fast the player moves

    public TextMeshProUGUI livesTextTMP; // drag the tmp 
    
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

    [Header("Lives Settings")] // simple chase rules: 3 lives and brief invulnerability after a hit
    public int maxLives = 3;
    public float invulnerabilityDuration = 1.5f;
    public float blinkInterval = 0.1f;
    
    [Header("Audio")]
    public AudioClip hitSound;              // sound when player gets hit by cop
    public AudioSource audioSource;         // audio source for playing sounds
    
    [Header("Boogie Bomb")]
    public GameObject boogieBombPrefab;     // assign your boogie bomb prefab
    public int boogieBombs = 3;            // starting bombs (increased)
    public float throwCooldown = 6f;       // cooldown between throws
    public float throwForce = 8f;          // how hard to throw

    private int currentLives;
    private bool isInvulnerable;
    private float lastThrowTime;
    private void Awake()
    {
        animator = GetComponent<Animator>(); 
    }
    void Start()
    {
        // get the rigidbody component so we can move the player
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // initialize lives first so UI shows the correct value at spawn
        maxLives = Mathf.Max(1, maxLives);
        currentLives = maxLives;
        UpdateLivesUI();

        // freeze rotation so player doesn't spin around
        rb.freezeRotation = true;

        //init stamina
        currentStamina = staminaMax;
        
        // setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        if (StaminaBar != null)
        {
            StaminaBar.minValue = 0f;
            StaminaBar.maxValue = staminaMax;
            StaminaBar.value = currentStamina;
        }

        // ensure base speed matches current inspector value on first frame
        baseMoveSpeed = moveSpeed;
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
        
        // handle boogie bomb throw (simple E key)
        if (Input.GetKeyDown(KeyCode.E) && boogieBombs > 0 && Time.time - lastThrowTime > throwCooldown)
        {
            ThrowBoogieBomb();
        }

        if (!isMoving)
        {
            movement.x = 0;
            movement.y = 0;
            // Force reset the animator parameters
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", 0);
        }

       // Debug.Log("isMoving: " + isMoving + ", movement: " + movement);

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
        // Note: "Interactable" tag needs to be created in Project Settings if you want to use this
        // if (other.CompareTag("Interactable"))
        // {
        //     Debug.Log("Player can interact with: " + other.name);
        // }

        // lose a life if a cop touches us
        if (other.CompareTag("Cop"))
        {
            TakeHit();
        }
    }

    // fallback for non-trigger colliders: if the cop uses a normal Collider2D,
    // we still take a hit on contact
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Cop"))
        {
            TakeHit();
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

    // apply one hit of damage and start invulnerability window
    private void TakeHit()
    {
        if (isInvulnerable) return;

        // play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        currentLives--;
        Debug.Log("Hit by cop! Lives left: " + currentLives);

        // update HUD immediately so 1->0 is visible
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        isInvulnerable = true;
        StartCoroutine(InvulnerabilityBlink());
    }

    // quick blink so it's obvious we can't be hit again right away
    private System.Collections.IEnumerator InvulnerabilityBlink()
    {
        float elapsed = 0f;
        bool visible = true;
        while (elapsed < invulnerabilityDuration)
        {
            visible = !visible;
            if (spriteRenderer != null)
            {
                var c = spriteRenderer.color;
                c.a = visible ? 1f : 0.3f;
                spriteRenderer.color = c;
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
        {
            var c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
        isInvulnerable = false;
    }

    private void GameOver()
    {
        // stop blink and restore visuals
        StopAllCoroutines();
        isInvulnerable = false;
        if (spriteRenderer != null)
        {
            var c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        // freeze and prevent further collisions
        rb.velocity = Vector2.zero;
        if (rb != null) rb.simulated = false;
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        enabled = false;
        Debug.Log("Game Over");

        // notify GameManager if present so it can show final score and restart hint
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.OnGameOver();
        }
    }

    private void UpdateLivesUI()
{
    if (livesTextTMP != null) livesTextTMP.text = "Lives: " + currentLives;
}

    private void ThrowBoogieBomb()
    {
        if (boogieBombPrefab == null) return;
        
        // create bomb at player position
        GameObject bomb = Instantiate(boogieBombPrefab, transform.position, Quaternion.identity);
        
        // get throw direction (toward mouse or movement direction)
        Vector2 throwDirection;
        if (Camera.main != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            throwDirection = (mousePos - transform.position).normalized;
        }
        else
        {
            // fallback to movement direction or right
            throwDirection = movement.magnitude > 0.1f ? movement.normalized : Vector2.right;
        }
        
        // add velocity to bomb
        Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
        if (bombRb != null)
        {
            bombRb.velocity = throwDirection * throwForce;
        }
        
        // consume bomb and set cooldown
        boogieBombs--;
        lastThrowTime = Time.time;
        
        Debug.Log("Boogie bomb thrown! Remaining: " + boogieBombs);
    }
    
    // called by boogie bomb pickup to add bombs
    public void AddBoogieBombs(int amount)
    {
        boogieBombs += amount;
        Debug.Log("Picked up boogie bomb! Total: " + boogieBombs);
    }
}