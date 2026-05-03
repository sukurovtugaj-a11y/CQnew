using UnityEngine;

public class Sollers : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Сколько касаний нужно для исчезновения")]
    public int hitsNeeded = 3;

    [Tooltip("Расстояние отталкивания героя в метрах")]
    public float knockbackDistance = 1f;

    [Tooltip("Задержка между регистрацией касаний (секунды)")]
    public float cooldown = 0.5f;

    [Tooltip("Радиус обнаружения игрока (для скольжения и даша)")]
    public float checkRadius = 1.5f;

    [Header("Звук")]
    public AudioSource hitSound;

    private int hitCount;
    private float lastHitTime = -999f;
    private Collider2D sollersCollider;

    private void Start()
    {
        sollersCollider = GetComponent<Collider2D>();
        if (sollersCollider != null)
        {
            // Увеличиваем радиус проверки, чтобы учесть размер Soller'а
            checkRadius = Mathf.Max(checkRadius, sollersCollider.bounds.extents.magnitude);
        }
    }

    private void FixedUpdate()
    {
        // Проверяем игроков в радиусе, даже если их коллайдер отключен (скольжение)
        var players = Physics2D.OverlapCircleAll(transform.position, checkRadius, LayerMask.GetMask("Player"));
        foreach (var hit in players)
        {
            var player = hit.GetComponent<SecMainCharacter>();
            if (player != null)
            {
                RegisterHit(player);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<SecMainCharacter>();
        if (player == null) return;
        RegisterHit(player);
    }

    public void RegisterHit(SecMainCharacter player)
    {
        if (Time.time - lastHitTime < cooldown) return;

        lastHitTime = Time.time;
        hitCount++;

        Debug.Log($"[Sollers] Касание {hitCount}/{hitsNeeded}");

        if (hitSound != null)
            hitSound.Play();

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
        Vector2 newPos = new Vector2(
            player.transform.position.x + knockbackDir.x * knockbackDistance,
            player.transform.position.y + knockbackDir.y * knockbackDistance
        );

        if (rb != null)
            rb.MovePosition(newPos);
        else
            player.transform.position = newPos;

        if (hitCount >= hitsNeeded)
        {
            gameObject.SetActive(false);
        }
    }
}
