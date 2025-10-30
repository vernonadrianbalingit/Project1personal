using UnityEngine;

// Simple script to force an animation to play and loop
public class ForceAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public string animationName = "BlueBuildingLightsFlashing";
    public bool playOnStart = true;
    public bool loop = true;
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator found on " + gameObject.name);
            return;
        }
        
        if (playOnStart)
        {
            PlayAnimation();
        }
    }
    
    void Update()
    {
        // Force the animation to keep playing if it stops
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            PlayAnimation();
        }
    }
    
    public void PlayAnimation()
    {
        if (animator != null)
        {
            animator.Play(animationName, 0, 0f);
            Debug.Log("Forcing animation: " + animationName);
        }
    }
    
    // Call this from other scripts if needed
    public void RestartAnimation()
    {
        PlayAnimation();
    }
}
