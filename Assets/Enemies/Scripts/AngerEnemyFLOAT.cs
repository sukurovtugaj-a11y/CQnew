using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyFLOAT : AngerEnemy
{
    [Header("Дистанции атак")]
    public float laserMaxRange = 17f;   // Лазер: работает в пределах 17м
    public float stopingDistance = 7.5f; // Остановиться и целиться, если ближе 7.5м

    [Header("Настройки")]
    public float moveSpeed = 4f;
    public float aggroRadius = 20f;     // Радиус обнаружения игрока
    public float attackCooldown = 2f;
    public float timeToAim = 5f;        // Время прицеливания
    public float delayBeforeAttack = 0.5f;
    public float floatAmplitude = 1.5f; // Амплитуда парения (визуальная)
    public float floatSpeed = 2f;       // Скорость парения

    [Header("Ссылки")]
    public TextMeshProUGUI commantText;
    public Animator animator;
    public GameObject explosionVFX;
    public Transform lazerGun;

    private Rigidbody2D rb;
    private float lastLaserTime;
    private bool isAiming = false;
    private Vector3 basePosition;       // «Истинная» позиция для физики
    private float floatOffsetStart;     // Случайный сдвиг фазы для парения

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        floatOffsetStart = Random.Range(0f, Mathf.PI * 2);
        lastLaserTime = -100f;
        basePosition = transform.position;
    }

    public override void TurnOn(Transform newTarget)
    {
        base.TurnOn(newTarget);
        basePosition = transform.position;
    }

    private void Update()
    {
        if (target == null) return;

        // === 1. Считаем дистанцию (с небольшим смещением по Y для консистентности) ===
        float distance = Vector3.Distance(target.position, transform.position + Vector3.up * 2);

        // === 2. Если игрок слишком далеко — просто летим к нему ===
        if (distance > aggroRadius)
        {
            MoveTowards(target.position);
            ApplyFloatAnimation();
            return;
        }

        // === 3. ЛАЗЕР: приоритетная атака ===
        // Атакуем, если: игрок в радиусе лазера + не на КД + не в процессе прицеливания
        if (distance <= laserMaxRange && !isAiming && Time.time - lastLaserTime >= attackCooldown)
        {
            lastLaserTime = Time.time;
            isAiming = true;
            StartCoroutine(AttackLazer());
            // Не делаем return — пусть применяется анимация парения
        }
        // === 4. Движение: если игрок дальше «боевой дистанции» ===
        else if (distance > stopingDistance)
        {
            MoveTowards(target.position);
        }
        // === 5. Если в зоне атаки, но лазер на КД — просто «висим» на месте ===
        // (не вызываем MoveTowards, чтобы не дёргаться)

        // === 6. Применяем визуальное парение поверх базовой позиции ===
        ApplyFloatAnimation();
    }

    /// <summary>
    /// Движение через Rigidbody2D (физика)
    /// </summary>
    private void MoveTowards(Vector3 targetPos)
    {
        float speedScale = (targetPos.x < basePosition.x) ? -1f : 1f;
        basePosition += speedScale * moveSpeed * Time.deltaTime * Vector3.right;

        // Плавное выравнивание по высоте (опционально)
        float heightDiff = targetPos.y - basePosition.y;
        if (Mathf.Abs(heightDiff) > 0.5f)
        {
            basePosition += Vector3.up * Mathf.Sign(heightDiff) * moveSpeed * 0.5f * Time.deltaTime;
        }

        rb.MovePosition(basePosition);
    }

    /// <summary>
    /// Визуальное парение: не влияет на физику, только на отрисовку
    /// </summary>
    private void ApplyFloatAnimation()
    {
        float floatX = Mathf.Cos(Time.time * floatSpeed + floatOffsetStart) * floatAmplitude;
        float floatY = Mathf.Sin(Time.time * floatSpeed * 0.7f + floatOffsetStart) * floatAmplitude * 0.5f;
        transform.position = basePosition + new Vector3(floatX, floatY, 0);
    }

    /// <summary>
    /// Атака лазером — 1 в 1 как у DOUBLE, но без лишних флагов
    /// </summary>
    private IEnumerator AttackLazer()
    {
        commantText.text = "6.875";

        Vector2 direction;
        float targetAngle;
        float startAngle = lazerGun.eulerAngles.z;

        // Прицеливание
        for (float t = 0; t < 1; t += Time.deltaTime / timeToAim)
        {
            direction = target.position - lazerGun.position + Vector3.up;
            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            lazerGun.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        // Финальная наводка
        direction = target.position - lazerGun.position + Vector3.up;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        lazerGun.rotation = Quaternion.Euler(0, 0, targetAngle);

        // Запуск анимации выстрела
        StartCoroutine(DelayedAnimatorTrigger("lazer", delayBeforeAttack));

        // Ждём завершения анимации перед сбросом флага
        yield return new WaitForSeconds(delayBeforeAttack + 0.1f);
        isAiming = false;
    }

    private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(trigger);
    }

    // Опционально: визуальный эффект при уничтожении (как у DOUBLE)
    public void OnDeath()
    {
        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
    }
}