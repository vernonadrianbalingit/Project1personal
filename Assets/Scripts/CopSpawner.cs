using System.Collections.Generic;
using UnityEngine;

// Spawns cops from predefined spawn points. Interval ramps down over time.
public class CopSpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject copPrefab;            // assign CopPrefab
    public List<Transform> spawnPoints;     // assign SpawnPoint children
    public Transform player;                // assign Player (or auto-find)
    public LayerMask solidLayerMask;        // SolidObjects layer to validate space

    [Header("Rules")]
    public int maxCops = 5;                 // increased from 4 - start with more cops
    public float initialInterval = 4f;      // decreased from 6f - spawn faster initially
    public float minInterval = 1.5f;        // decreased from 2f - minimum spawn interval
    public float rampEverySeconds = 15f;    // decreased from 20f - ramp difficulty faster
    public int maxCopsCap = 10;             // increased from 8 - allow more cops at max difficulty
    public float minDistanceFromPlayer = 5f; // decreased from 6f - can spawn closer to player
    public float overlapRadius = 0.35f;     // avoid spawning inside walls

    private float nextSpawnTime;
    private float nextRampTime;
    private float currentInterval;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        currentInterval = initialInterval;
        nextSpawnTime = Time.time + currentInterval;
        nextRampTime = Time.time + rampEverySeconds;
    }

    void Update()
    {
        if (copPrefab == null || spawnPoints == null || spawnPoints.Count == 0 || player == null) return;

        // difficulty ramp
        if (Time.time >= nextRampTime)
        {
            currentInterval = Mathf.Max(minInterval, currentInterval - 0.5f);
            if (maxCops < maxCopsCap) maxCops += 1; // slowly allow more cops
            nextRampTime = Time.time + rampEverySeconds;
        }

        // spawn
        if (Time.time >= nextSpawnTime)
        {
            int currentCount = CountActiveCops();
            if (currentCount < maxCops)
            {
                TrySpawn();
            }
            nextSpawnTime = Time.time + currentInterval;
        }
    }

    int CountActiveCops()
    {
        // lightweight: count by tag; alternatively track instances
        return GameObject.FindGameObjectsWithTag("Cop").Length;
    }

    void TrySpawn()
    {
        // pick a random valid spawn point not too close and not inside walls
        const int attempts = 16;
        for (int i = 0; i < attempts; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (sp == null) continue;

            Vector3 pos = sp.position;
            if (Vector2.Distance(pos, player.position) < minDistanceFromPlayer) continue;

            // reject if overlapping SolidObjects at spawn radius
            if (Physics2D.OverlapCircle(pos, overlapRadius, solidLayerMask)) continue;

            Instantiate(copPrefab, pos, Quaternion.identity);
            return;
        }
        // if we get here, no valid spawn this cycle; will try next tick
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (spawnPoints != null)
        {
            foreach (var t in spawnPoints)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.position, 0.35f);
            }
        }
    }
}
 
