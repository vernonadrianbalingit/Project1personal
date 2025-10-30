using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityAudio : MonoBehaviour
{
	[SerializeField] Transform player;
	[SerializeField] float minDistance = 2f;
	[SerializeField] float maxDistance = 15f;
	[SerializeField] AnimationCurve volumeByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
	[SerializeField] bool use2DVolumeCurve = false; // false = use 3D spatial audio settings only

	AudioSource audioSource;

	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (use2DVolumeCurve)
		{
			// 2D volume controlled by script
			audioSource.spatialBlend = 0f;
		}
		else
		{
			// Use 3D audio; volume rolloff handled by AudioSource
			audioSource.spatialBlend = 1f;
		}
		audioSource.loop = true;
	}

	void Start()
	{
		if (player == null)
		{
			var p = GameObject.FindGameObjectWithTag("Player");
			if (p != null) player = p.transform;
		}
		if (!audioSource.isPlaying) audioSource.Play();
	}

	void Update()
	{
		if (!use2DVolumeCurve || player == null) return;

		float distance = Vector2.Distance((Vector2)transform.position, (Vector2)player.position);
		float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
		audioSource.volume = volumeByDistance.Evaluate(t);
	}
}


