using UnityEngine;

/// <summary>
/// Телепорт в пределах одной сцены
/// </summary>
public class TeleportZone : MonoBehaviour
{
    [Header("Точка телепортации")]
    [Tooltip("Куда телепортировать игрока (пустой объект с координатами)")]
    public Transform teleportTarget;

    [Header("Направление игрока")]
    [Tooltip("Развернуть игрока после телепортации")]
    public bool changeDirection = false;

    [Tooltip("Направление взгляда (true = вправо, false = влево)")]
    public bool lookRight = true;

    [Header("Блокировка управления")]
    [Tooltip("Время блокировки управления после телепорта")]
    public float controlLockDuration = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<MainCharacter>() as MonoBehaviour ?? collision.GetComponent<SecMainCharacter>();
        if (player != null && teleportTarget != null)
        {
            // Телепортируем игрока
            player.transform.position = teleportTarget.position;

            // Сбрасываем скорость, чтобы игрок не «летел» после телепорта
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            // ← Блокируем управление
            var secChar = player.GetComponent<SecMainCharacter>();
            if (secChar != null) secChar.LockControls(controlLockDuration);

            // Разворачиваем игрока при необходимости 195100огромный
            if (changeDirection)
            {
                Vector3 s = player.transform.localScale;
                s.x = Mathf.Abs(s.x) * (lookRight ? -1 : 1);
                player.transform.localScale = s;
            }

            Debug.Log($"[TeleportZone] Игрок телепортирован на {teleportTarget.position}");
        }
    }
}