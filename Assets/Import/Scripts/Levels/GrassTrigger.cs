using UnityEngine;

public class GrassTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<SecMainCharacter>();
        if (player != null && player.sound != null)
        {
            player.sound.SetGrassState(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<SecMainCharacter>();
        if (player != null && player.sound != null)
        {
            player.sound.SetGrassState(false);
        }
    }
}
