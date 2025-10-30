using UnityEngine;

// Empty marker so it's easy to find/validate spawn points
public class SpawnPoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
        Gizmos.DrawSphere(transform.position, 0.12f);
    }
}
 
