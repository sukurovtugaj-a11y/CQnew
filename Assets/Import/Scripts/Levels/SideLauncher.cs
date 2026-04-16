using UnityEngine;

public class SideLauncher : MonoBehaviour
{
    [Header("Настройки отскока")]
    [Tooltip("Сила отскока влево")]
    public float launchForce = 15f;

    [Tooltip("Множитель вертикальной скорости (0 = строго влево, 1 = сохранение инерции)")]
    [Range(0f, 1f)]
    public float verticalRetention = 0.5f;

    [Tooltip("Задержка перед повторным срабатыванием (сек)")]
    public float cooldown = 0.5f;

    [Tooltip("Визуальный эффект (опционально)")]
    public GameObject launchEffect;

    // Внутренняя переменная для кулдауна
    private float lastTriggerTime = -999f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - lastTriggerTime < cooldown) return;

        MonoBehaviour player = collision.GetComponent<MainCharacter>() as MonoBehaviour ?? collision.GetComponent<SecMainCharacter>();
        if (player == null) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.velocity = new Vector2(-launchForce, rb.velocity.y * verticalRetention);

        lastTriggerTime = Time.time;

        if (launchEffect != null)
            Instantiate(launchEffect, transform.position, Quaternion.identity);

        Debug.Log($"[SideLauncher] ???????? ?????! ????: {launchForce}");
    }
}