using UnityEngine;

public class TeleportActivator : MonoBehaviour
{
    public GameObject teleportObject;
    public bool activateOnEnter = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activateOnEnter && (collision.GetComponent<MainCharacter>() != null || collision.GetComponent<SecMainCharacter>() != null))
        {
            ActivateTeleport();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activateOnEnter && (collision.GetComponent<MainCharacter>() != null || collision.GetComponent<SecMainCharacter>() != null))
        {
            ActivateTeleport();
        }
    }

    private void ActivateTeleport()
    {
        if (teleportObject != null)
        {
            var collider = teleportObject.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
            teleportObject.SetActive(true);
            Debug.Log("[TeleportActivator] Телепорт активирован: " + teleportObject.name);
        }
        Destroy(gameObject);
    }
}