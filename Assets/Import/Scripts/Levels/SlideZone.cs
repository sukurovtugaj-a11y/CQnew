using UnityEngine;

public class SlideZone : MonoBehaviour
{
    [Tooltip("Скорость скольжения в этой зоне")]
    public float slideSpeed = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var mc = other.GetComponent<SecMainCharacter>() ?? other.GetComponentInParent<SecMainCharacter>();
        if (mc == null) return;
        mc.EnterSlideZone(slideSpeed, this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var mc = other.GetComponent<SecMainCharacter>() ?? other.GetComponentInParent<SecMainCharacter>();
        if (mc == null) return;
        mc.ExitSlideZone(this);
    }
}