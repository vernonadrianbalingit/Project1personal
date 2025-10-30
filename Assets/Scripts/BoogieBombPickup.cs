using UnityEngine;

// Boogie bomb pickup that player can collect
public class BoogieBombPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int bombAmount = 1;  // how many bombs this pickup gives
    
    [Header("Audio")]
    public AudioClip collectSound;  // sound when collected
    
    [Header("Visual Feedback")]
    public float rotationSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.2f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        // make sure collider is a trigger
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // spin it
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectBomb();
        }
    }
    
    void CollectBomb()
    {
        // add bombs to player
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.AddBoogieBombs(bombAmount);
        }
        
        // play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        Destroy(gameObject);
    }
}

