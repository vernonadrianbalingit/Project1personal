using UnityEngine;

// Coin that can be collected by the player to add to score
public class Coin : MonoBehaviour
{
    [Header("Score Value")]
    public int scoreValue = 10;  // how many points this coin is worth
    
    [Header("Audio")]
    public AudioClip collectSound;  // sound when coin is collected
    
    [Header("Visual Feedback")]
    public float rotationSpeed = 90f;  // degrees per second
    public float bobSpeed = 2f;
    public float bobAmount = 0.2f;
    
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Make sure it's set up as a trigger
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Simple spinning animation
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }
    
    void CollectCoin()
    {
        // Add score to GameManager
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
        }
        
        // play collect sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Destroy the coin
        Destroy(gameObject);
    }
}
