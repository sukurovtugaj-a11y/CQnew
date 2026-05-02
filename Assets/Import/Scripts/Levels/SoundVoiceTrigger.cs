using UnityEngine;

public class SoundVoiceTrigger : MonoBehaviour
{
    private bool hasPlayed;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SecMainCharacter>() == null || hasPlayed) return;

        hasPlayed = true;
        if (audioSource != null)
            audioSource.Play();
    }
}
