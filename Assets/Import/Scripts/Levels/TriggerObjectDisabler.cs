using UnityEngine;

public class TriggerObjectDisabler : MonoBehaviour
{
    public GameObject objectToDisable;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SecMainCharacter>() != null && objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }
}