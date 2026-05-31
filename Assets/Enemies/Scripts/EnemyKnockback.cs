using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Урон при касании")]
    public float damage = 3f;
    [Tooltip("Расстояние отталкивания героя в метрах")]
    public float knockbackDistance = 1f;
    [Tooltip("Задержка между ударами (секунды)")]
    public float cooldown = 0.5f;
    [Tooltip("Радиус обнаружения игрока (для скольжения и даша)")]
    public float checkRadius = 1.5f;

    [Header("Звук (опционально)")]
    public AudioSource hitSound;

    private float lastHitTime = -999f;
    private Collider2D enemyCollider;

    private void Start()
    {
        enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            // Увеличиваем радиус проверки, чтобы учесть размер врага
            checkRadius = Mathf.Max(checkRadius, enemyCollider.bounds.extents.magnitude);
        }
    }

    private void FixedUpdate()
    {
        // Проверяем игроков в радиусе, даже если их коллайдер отключен (скольжение) 5 из 5
        var hits = Physics2D.OverlapCircleAll(transform.position, checkRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            // Игнорируем объекты на слое Enemy (чтобы враги не атаковали друг друга)
            if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                continue;

            var player = GetPlayerFromHit(hit);
            if (player != null)
            {
                RegisterHit(player);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = GetPlayerFromHit(other);
        if (player == null) return;
        RegisterHit(player);
    }

    private SecMainCharacter GetPlayerFromHit(Component hit)
    {
        // Игнорируем объекты на слое Enemy (чтобы враги не атаковали друг друга)
        if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return null;

        // Сначала проверяем сам компонент
        var player = hit.GetComponent<SecMainCharacter>();
        if (player != null) return player;

        // Проверяем SlideDamageHandler (для скольжения)
        var slideHandler = hit.GetComponent<SlideDamageHandler>();
        if (slideHandler != null)
        {
            // Получаем SecMainCharacter через GetComponentInParent (как в SlideDamageHandler.Awake)
            return hit.GetComponentInParent<SecMainCharacter>();
        }

        // Проверяем через родителя
        return hit.GetComponentInParent<SecMainCharacter>();
    }

    void RegisterHit(SecMainCharacter player)
    {
        if (Time.time - lastHitTime < cooldown) return;
        lastHitTime = Time.time;

        // Наносим урон (передаём null, чтобы не телепортировать игрока к врагу)
        player.Damage(damage, null);

        if (hitSound != null)
            hitSound.Play();

        // Отталкиваем игрока строго по горизонтали (назад от врага)
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        
        // Используем центр коллайдера врага, а не transform.position (учитываем возможное смещение спрайта)
        Collider2D col = GetComponent<Collider2D>();
        Vector3 enemyCenter = col != null ? (Vector3)col.bounds.center : transform.position;
        
        // Направление: если игрок справа от центра врага, толкаем вправо; если слева - влево
        float dirX = Mathf.Sign(player.transform.position.x - enemyCenter.x);
        
        Vector2 newPos = new Vector2(
            player.transform.position.x + dirX * knockbackDistance,
            player.transform.position.y  // Y не меняем
        );

        // ОТЛАДКА: Какая сторона и куда толкаем?
        Debug.Log($"[EnemyKnockback] EnemyCenter: {enemyCenter.x}, Player: {player.transform.position.x}. " +
                  $"DirX: {dirX}. NewPosX: {newPos.x}. EnemyScaleX: {transform.localScale.x}");

        if (rb != null)
            rb.MovePosition(newPos);
        else
            player.transform.position = newPos;
    }
}
