using UnityEngine;

public class QuietZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var sound = other.GetComponent<PlayerSoundComponent>() ?? other.GetComponentInParent<PlayerSoundComponent>();
        if (sound != null)
        {
            sound.Volume = 0.185f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var sound = other.GetComponent<PlayerSoundComponent>() ?? other.GetComponentInParent<PlayerSoundComponent>();
        if (sound != null)
        {
            sound.Volume = 1f;
        }
    }
}
