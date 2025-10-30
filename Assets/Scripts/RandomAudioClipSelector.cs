using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioClipSelector : MonoBehaviour
{
	[SerializeField] AudioClip[] candidateClips;
	[SerializeField] bool pickOnAwake = true;
	[SerializeField] bool shuffleStartTime = true;

	AudioSource audioSource;

	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (pickOnAwake) Pick();
	}

	public void Pick()
	{
		if (candidateClips == null || candidateClips.Length == 0) return;
		int idx = Random.Range(0, candidateClips.Length);
		audioSource.clip = candidateClips[idx];
		if (shuffleStartTime && audioSource.clip != null)
		{
			float start = Random.Range(0f, Mathf.Max(0.01f, audioSource.clip.length - 0.05f));
			// Ensure the source is ready
			if (!audioSource.isPlaying)
			{
				audioSource.time = start;
				audioSource.Play();
			}
			else
			{
				audioSource.time = start;
			}
		}
	}
}


