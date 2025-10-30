using System.Collections.Generic;
using UnityEngine;

// Spawns coins randomly on the map at spawn points
public class CoinSpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject coinPrefab;            // assign your coin prefab
    public List<Transform> spawnPoints;      // can reuse same spawn points as cops
    public LayerMask solidLayerMask;        // SolidObjects layer to validate space
    
    [Header("Spawn Rules")]
    public float spawnInterval = 5f;        // spawn a coin every X seconds
    public int maxCoins = 5;                // maximum coins on map at once
    public float overlapRadius = 0.3f;     // avoid spawning inside walls
    public float minDistanceFromPlayer = 3f; // don't spawn too close to player
    
    private Transform player;
    private float nextSpawnTime;
    
    void Start()
    {
        // Find player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        
        // Find spawn points if not assigned
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
            
            // debug log to help troubleshoot
            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning("CoinSpawner: No spawn points found! Coins won't spawn. Either add SpawnPoint components or assign spawn points in the inspector.");
            }
            else
            {
                Debug.Log("CoinSpawner: Found " + spawnPoints.Count + " spawn points");
            }
        }
        
        nextSpawnTime = Time.time + spawnInterval;
    }
    
    void Update()
    {
        if (coinPrefab == null || spawnPoints == null || spawnPoints.Count == 0) return;
        
        // Check if we should spawn a new coin
        if (Time.time >= nextSpawnTime)
        {
            int currentCoinCount = CountActiveCoins();
            if (currentCoinCount < maxCoins)
            {
                TrySpawnCoin();
            }
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    int CountActiveCoins()
    {
        // Count coins by tag (make sure your coin prefab has "Coin" tag)
        return GameObject.FindGameObjectsWithTag("Coin").Length;
    }
    
    void TrySpawnCoin()
    {
        const int attempts = 16;
        for (int i = 0; i < attempts; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (sp == null) continue;
            
            Vector3 pos = sp.position;
            
            // Don't spawn too close to player
            if (player != null && Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                continue;
            
            // Reject if overlapping solid objects
            if (Physics2D.OverlapCircle(pos, overlapRadius, solidLayerMask)) 
                continue;
            
            // Don't spawn on top of another coin
            if (Physics2D.OverlapCircle(pos, overlapRadius) != null)
            {
                var overlap = Physics2D.OverlapCircle(pos, overlapRadius);
                if (overlap.CompareTag("Coin")) continue;
            }
            
            Instantiate(coinPrefab, pos, Quaternion.identity);
            return;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
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
