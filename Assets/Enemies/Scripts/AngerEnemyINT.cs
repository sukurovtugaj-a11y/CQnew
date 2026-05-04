using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyINT : AngerEnemy
{
    [Header("Настройки Атаки")]
    public float attackCooldown = 2f;
    public float delayBeforeAttack = 0.5f;
    public float detectionRadius = 20f; // Макс. дистанция триггера

    [Header("Настройки Кувалды")]
    [Tooltip("Дистанция, на которой враг БИТЬ кувалдой (с учетом его размера)")]
    public float clubDistance = 4.5f;

    [Header("Настройки Движения")]
    public float moveSpeed = 3f;
    public float stopingDistance = 8f;   // Дистанция, с которой он перестает ехать и стреляет

    [Header("Ссылки")]
    public TextMeshProUGUI commantText;
    public Animator animator;

    private Rigidbody2D rb;
    // Раздельные таймеры для приоритета
    private float lastClubTime;
    private float lastRangedTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Инициализируем таймеры в прошлом, чтобы можно было атаковать сразу
        lastClubTime = -100f;
        lastRangedTime = -100f;
    }

    private void Update()
    {
        if (target == null) return;

        // Считаем реальную дистанцию (в метрах)
        // Учитываем смещение + Vector3.up * 2, как было в оригинале
        float distance = Vector3.Distance(target.position, (transform.position + Vector3.up * 2));

        // === ПРИОРИТЕТНАЯ МАШИНА СОСТОЯНИЙ ===

        // ПРИОРИТЕТ 1: КУВАЛДА (Ближний бой)
        // Если игрок в зоне удара — ВСЁ БРОСАЕМ И БЬЕМ
        if (distance < clubDistance)
        {
            // Проверяем кулдаун ТОЛЬКО для кувалды
            if (Time.time - lastClubTime >= attackCooldown)
            {
                lastClubTime = Time.time;
                // Сбрасываем таймер стрельбы, чтобы при отходе не стрелял мгновенно
                lastRangedTime = Time.time;

                AttackClub();
                return; // Завершаем Update, чтобы не сработало движение или стрельба
            }
        }

        // ПРИОРИТЕТ 2: СТРЕЛЬБА (Средний/Дальний бой)
        // Если не бьем кувалдой, но в зоне поражения — стреляем
        else if (distance <= detectionRadius)
        {
            // Если мы еще далеко до "боевой позиции" — доезжаем
            if (distance > stopingDistance)
            {
                MoveTowards(target.position);
            }

            // Стреляем, если прошел кулдаун стрельбы
            if (Time.time - lastRangedTime >= attackCooldown)
            {
                lastRangedTime = Time.time;
                // При стрельбе сбрасываем таймер кувалды, чтобы не махал ею сразу после выстрела
                lastClubTime = Time.time;

                if (Random.Range(0, 100) > 50) AttackLazer();
                else AttackRocket();
            }
        }

        // ПРИОРИТЕТ 3: ПРЕСЛЕДОВАНИЕ
        // Если игрок дальше зоны поражения (хотя триггер должен был отключить скрипт, но на всякий случай)
        else
        {
            MoveTowards(target.position);
        }
    }

    // Функция движения
    private void MoveTowards(Vector3 targetPos)
    {
        float speedScale = (targetPos.x < transform.position.x) ? -1f : 1f;
        Vector2 newPos = rb.position + new Vector2(speedScale * moveSpeed * Time.deltaTime, 0f);
        rb.MovePosition(newPos);
    }

    private void AttackClub()
    {
        if (target.position.x < transform.position.x)
        {
            commantText.text = "195";
            StartCoroutine(DelayedAnimatorTrigger("leftClub", delayBeforeAttack));
        }
        else
        {
            commantText.text = "100";
            StartCoroutine(DelayedAnimatorTrigger("rightClub", delayBeforeAttack));
        }
    }

    private void AttackLazer()
    {
        if (target.position.x < transform.position.x)
        {
            commantText.text = "5";
            StartCoroutine(DelayedAnimatorTrigger("leftLazer", delayBeforeAttack));
        }
        else
        {
            commantText.text = "10";
            StartCoroutine(DelayedAnimatorTrigger("rightLazer", delayBeforeAttack));
        }
    }

    private void AttackRocket()
    {
        if (target.position.x < transform.position.x)
        {
            commantText.text = "18";
            StartCoroutine(DelayedAnimatorTrigger("leftRocket", delayBeforeAttack));
        }
        else
        {
            commantText.text = "15";
            StartCoroutine(DelayedAnimatorTrigger("rightRocket", delayBeforeAttack));
        }
    }

    private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(trigger);
    }
}