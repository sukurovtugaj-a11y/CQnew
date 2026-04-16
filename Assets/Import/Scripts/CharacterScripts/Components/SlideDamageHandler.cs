using UnityEngine;

public class SlideDamageHandler : MonoBehaviour
{
    private SecMainCharacter _mainCharacter;

    private void Awake()
    {
        _mainCharacter = GetComponentInParent<SecMainCharacter>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var trap = other.GetComponent<TrapDamageZone>();
        if (trap != null && _mainCharacter != null && !_mainCharacter.IsInvulnerable())
        {
            _mainCharacter.StopSlide();
            _mainCharacter.Damage(trap.damage, trap.respawnPoint);
        }
    }
}