using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityAudio : MonoBehaviour
{
	[SerializeField] Transform player;
	[SerializeField] float minDistance = 2f;
	[SerializeField] float maxDistance = 15f;
	[SerializeField] AnimationCurve volumeByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
	[SerializeField] bool use2DVolumeCurve = true; // false = use 3D spatial audio settings only
	[SerializeField] bool hardMuteBeyondMax = true; // if true, force volume 0 and optionally pause when > maxDistance
	[SerializeField] bool pauseWhenBeyondMax = false; // only used if hardMuteBeyondMax
	[SerializeField] bool debugLogs = false;

	AudioSource audioSource;
	bool warnedNoClip;
	bool warnedNoPlayer;

	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (use2DVolumeCurve)
		{
			// 2D volume controlled by script
			audioSource.spatialBlend = 0f;
			// Ensure no initial leak before first Update
			audioSource.playOnAwake = true;
			audioSource.volume = 0f;
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
		if (audioSource.clip == null && !warnedNoClip && debugLogs)
		{
			Debug.LogWarning($"ProximityAudio on {name}: AudioSource.clip is not assigned.");
			warnedNoClip = true;
		}
		if (!audioSource.isPlaying) audioSource.Play();
	}

	void Update()
	{
		if (!use2DVolumeCurve || player == null)
		{
			if (player == null && !warnedNoPlayer && debugLogs)
			{
				Debug.LogWarning($"ProximityAudio on {name}: no player Transform assigned and none found with tag 'Player'.");
				warnedNoPlayer = true;
			}
			return;
		}

		float distance = Vector2.Distance((Vector2)transform.position, (Vector2)player.position);
		if (distance >= maxDistance)
		{
			if (hardMuteBeyondMax)
			{
				audioSource.volume = 0f;
				if (pauseWhenBeyondMax && audioSource.isPlaying)
				{
					audioSource.Pause();
				}
			}
			if (debugLogs)
			{
				Debug.Log($"ProximityAudio on {name}: distance={distance:F2} >= max={maxDistance:F2}, volume={(hardMuteBeyondMax ? 0f : audioSource.volume):F2}");
			}
			return;
		}

		if (pauseWhenBeyondMax && !audioSource.isPlaying)
		{
			audioSource.UnPause();
		}

		float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
		audioSource.volume = Mathf.Clamp01(volumeByDistance.Evaluate(t));
		if (!audioSource.isPlaying && audioSource.clip != null)
		{
			audioSource.UnPause();
			audioSource.Play();
		}
		if (debugLogs)
		{
			Debug.Log($"ProximityAudio on {name}: distance={distance:F2}, t={t:F2}, volume={audioSource.volume:F2}");
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0f, 1f, 0.6f, 0.35f);
		UnityEditor.Handles.color = new Color(0f, 1f, 0.6f, 0.75f);
		#if UNITY_EDITOR
		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, minDistance);
		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, maxDistance);
		if (debugLogs && Application.isPlaying && player != null)
		{
			float d = Vector2.Distance((Vector2)transform.position, (Vector2)player.position);
			UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"d={d:F1} v={audioSource.volume:F2}");
		}
		#endif
	}
}


