using UnityEngine;

/// <summary>
/// Скрипт для ловушек (шипы и т.д.)
/// Наносит урон игроку и телепортирует в безопасную точку (если ещё жив)
/// </summary>
public class TrapDamageZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Куда телепортировать игрока после урона (оставь пустым, если не нужен телепорт)")]
    public Transform respawnPoint;

    [Tooltip("Сколько жизней снять при контакте")]
    public int damage = 1;

    [Tooltip("Наносить урон только один раз за контакт")]
    public bool oneTimeDamage = true;

    [Header("Блокировка управления")]
    [Tooltip("Время блокировки управления после урона/телепорта")]
    public float controlLockDuration = 0.5f;

    private bool hasDamaged = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var secChar = collision.GetComponent<SecMainCharacter>();
        var player = secChar as MonoBehaviour;
        if (player != null)
        {
            bool isInvuln = secChar.IsInvulnerable();

            if (oneTimeDamage && hasDamaged) return;

            if (isInvuln)
            {
                if (respawnPoint != null)
                {
                    player.transform.position = respawnPoint.position;
                    var rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = Vector2.zero;
                    secChar.LockControls(controlLockDuration);
                }
            }
            else
            {
                secChar.Damage(damage, respawnPoint);
                secChar.LockControls(controlLockDuration);
            }

            hasDamaged = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<SecMainCharacter>() != null)
        {
            hasDamaged = false;
        }
    }
}