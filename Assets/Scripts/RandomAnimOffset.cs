using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimOffset : MonoBehaviour
{
	[SerializeField] float minSpeed = 0.85f;
	[SerializeField] float maxSpeed = 1.15f;

	void Start()
	{
		var anim = GetComponent<Animator>();
		// Randomize the normalized time so instances are out of sync
		anim.Play(0, 0, Random.value);
		// Small speed variance for a more organic feel
		anim.speed = Random.Range(minSpeed, maxSpeed);
	}
}


