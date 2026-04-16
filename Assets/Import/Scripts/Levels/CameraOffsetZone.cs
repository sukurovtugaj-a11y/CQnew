using UnityEngine;

public class CameraOffsetZone : MonoBehaviour
{
    [Tooltip("Смещение камеры при входе")]
    public Vector3 offset;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<SecMainCharacter>();
        if (player != null && player.cameraController != null)
        {
            player.cameraController.AddOffset(offset);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<SecMainCharacter>();
        if (player != null && player.cameraController != null)
        {
            player.cameraController.RemoveOffset(offset);
        }
    }
}
