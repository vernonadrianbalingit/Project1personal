using UnityEngine;

// Fortnite-style boogie bomb: arcs, fuses, explodes in AoE to make cops dance
public class BoogieBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public float fuseTime = 0.8f;           // time before explosion
    public float explosionRadius = 3.5f;    // AoE radius
    public float boogieDuration = 3.5f;     // how long cops dance
    public LayerMask copLayer = -1;         // layer mask for cops (or use tag)
    
    [Header("Effects")]
    public GameObject explosionEffect;       // optional particle effect
    public AudioClip explosionSound;        // optional explosion SFX
    
    private float timer;
    private bool hasExploded;
    private AudioSource audioSource;
    
    void Start()
    {
        timer = fuseTime;
        audioSource = GetComponent<AudioSource>();
        
        // add some spin for visual flair
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().angularVelocity = 180f;
        }
        
        // ensure bomb doesn't interfere with building animations
        // make bomb a trigger so it doesn't physically collide with anything
        var bombCol = GetComponent<Collider2D>();
        if (bombCol != null)
        {
            bombCol.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (hasExploded) return;
        
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Explode();
        }
    }
    
    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // find all cops in radius and make them dance
        var allCops = GameObject.FindGameObjectsWithTag("Cop");
        foreach (var cop in allCops)
        {
            float distance = Vector2.Distance(transform.position, cop.transform.position);
            if (distance <= explosionRadius)
            {
                var chaser = cop.GetComponent<CopChaser>();
                if (chaser != null)
                {
                    chaser.EnterBoogie(boogieDuration);
                }
            }
        }
        
        // play explosion effects
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // play sound before destroying
        if (audioSource != null && explosionSound != null)
        {
            // set the clip and start from a specific time (trim the beginning)
            audioSource.clip = explosionSound;
            audioSource.time = 1.5f; // start 0.2 seconds into the clip
            audioSource.Play();
            
            // destroy after the remaining sound finishes
            float remainingLength = explosionSound.length - 0.2f;
            Destroy(gameObject, Mathf.Max(remainingLength, 0.5f));
        }
        else
        {
            // destroy immediately if no sound
            Destroy(gameObject, 0.1f);
        }
    }
    
    // visual gizmo to show explosion radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
