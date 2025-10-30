using System.Collections.Generic;
using UnityEngine;

// Spawns boogie bomb pickups at spawn points
public class BoogieBombSpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject bombPickupPrefab;  // assign your boogie bomb pickup prefab
    public List<Transform> spawnPoints;
    public LayerMask solidLayerMask;
    
    [Header("Spawn Rules")]
    public float spawnInterval = 15f;  // spawn a bomb every X seconds
    public int maxBombs = 2;  // maximum bomb pickups on map at once
    public float overlapRadius = 0.3f;
    public float minDistanceFromPlayer = 3f;
    
    private Transform player;
    private float nextSpawnTime;
    
    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        
        // find spawn points if not assigned
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            spawnPoints = new List<Transform>();
            
            // first try to find SpawnPoint components
            var spawnPointObjs = FindObjectsOfType<SpawnPoint>();
            foreach (var sp in spawnPointObjs)
            {
                if (sp != null && sp.transform != null)
                    spawnPoints.Add(sp.transform);
            }
            
            // if no SpawnPoint components, try to reuse CopSpawner's spawn points
            if (spawnPoints.Count == 0)
            {
                var copSpawner = FindObjectOfType<CopSpawner>();
                if (copSpawner != null && copSpawner.spawnPoints != null && copSpawner.spawnPoints.Count > 0)
                {
                    foreach (var sp in copSpawner.spawnPoints)
                    {
                        if (sp != null)
                            spawnPoints.Add(sp);
                    }
                }
            }
        }
        
        nextSpawnTime = Time.time + spawnInterval;
    }
    
    void Update()
    {
        if (bombPickupPrefab == null || spawnPoints == null || spawnPoints.Count == 0) return;
        
        if (Time.time >= nextSpawnTime)
        {
            int currentBombCount = CountActiveBombs();
            if (currentBombCount < maxBombs)
            {
                TrySpawnBomb();
            }
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    int CountActiveBombs()
    {
        // count by tag (make sure your bomb pickup prefab has "BoogieBombPickup" tag or search by component)
        var allBombs = FindObjectsOfType<BoogieBombPickup>();
        return allBombs.Length;
    }
    
    void TrySpawnBomb()
    {
        const int attempts = 16;
        for (int i = 0; i < attempts; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (sp == null) continue;
            
            Vector3 pos = sp.position;
            
            // not too close to player
            if (player != null && Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                continue;
            
            // dont spawn in walls
            if (Physics2D.OverlapCircle(pos, overlapRadius, solidLayerMask)) 
                continue;
            
            // dont spawn on another pickup
            if (Physics2D.OverlapCircle(pos, overlapRadius) != null)
            {
                var overlap = Physics2D.OverlapCircle(pos, overlapRadius);
                if (overlap.CompareTag("Coin") || overlap.GetComponent<BoogieBombPickup>() != null) 
                    continue;
            }
            
            Instantiate(bombPickupPrefab, pos, Quaternion.identity);
            return;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        if (spawnPoints != null)
        {
            foreach (var t in spawnPoints)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.position, overlapRadius);
            }
        }
    }
}

