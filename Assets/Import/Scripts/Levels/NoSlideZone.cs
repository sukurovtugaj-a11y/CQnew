using UnityEngine;

public class NoSlideZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var mc = other.GetComponent<SecMainCharacter>() ?? other.GetComponentInParent<SecMainCharacter>();
        if (mc != null)
        {
            mc.noSlideCount++;
            if (mc.IsSliding) mc.StopSlide();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var mc = other.GetComponent<SecMainCharacter>() ?? other.GetComponentInParent<SecMainCharacter>();
        if (mc != null)
        {
            mc.noSlideCount--;
            if (mc.noSlideCount <= 0) mc.noSlideCooldown = 0.1f;
        }
    }
}
