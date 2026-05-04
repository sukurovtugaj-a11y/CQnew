using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyDOUBLE : AngerEnemy
{
    [Header("Дистанции атак")]
    public float fireRange = 7.5f;      // Огнемёт: < 7.5м (ПРИОРИТЕТ 1)
    public float laserMaxRange = 17f;   // Лазер: 7.5м - 17м (ПРИОРИТЕТ 2)
    public float chargeMinRange = 17f;  // Таран: > 17м (ПРИОРИТЕТ 3)

    [Header("Настройки")]
    public float moveSpeed = 5f;
    public float aggroRadius = 20f;     // Максимальный радиус агро
    public float attackCooldown = 2f;
    public float timeToAim = 5f;
    public float delayBeforeAttack = 0.5f;
    public float chargeStopingDistance = 2f;
    public float knockbackDistance = 1.5f;

    [Header("Ссылки")]
    public TextMeshProUGUI commantText;
    public Animator animator;
    public HP hp;
    public GameObject floatPrefab;
    public Transform floatPrefabSpawnPoint;
    public GameObject explosionVFX;
    public Transform lazerGun;

    private Rigidbody2D rb;
    private float lastFireTime;
    private float lastLaserTime;
    private float lastChargeTime;
    private float lowHPLevel;

    // Флаги блокировки (только для защиты от повторного запуска внутри атаки)
    private bool isCharging = false;
    private bool isAiming = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lowHPLevel = hp.maxHP * 0.3f;
        hp.onChangeHp.AddListener(LowHPCheck);

        lastFireTime = -100f;
        lastLaserTime = -100f;
        lastChargeTime = -100f;
    }

    public void LowHPCheck(float currentHP)
    {
        if (currentHP <= lowHPLevel)
        {
            Instantiate(floatPrefab, floatPrefabSpawnPoint.position, Quaternion.identity);
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (target == null) return;

        // Считаем дистанцию (с тем же смещением, что и было, для консистентности)
        float distance = Vector3.Distance(target.position, (transform.position + Vector3.up * 2));

        // Если игрок слишком далеко — просто идём к нему, не атакуем
        if (distance > aggroRadius)
        {
            MoveTowards(target.position);
            return;
        }

        // === ПРИОРИТЕТ 1: ОГНЕМЁТ (< 7.5м) ===
        if (distance < fireRange)
        {
            // Сбрасываем флаги, чтобы выйти из других атак, если игрок резко подошёл
            isCharging = false;
            isAiming = false;

            if (Time.time - lastFireTime >= attackCooldown)
            {
                lastFireTime = Time.time;
                AttackFire();
            }
            // НЕ делаем return — если огнемёт на КД, даём шанс другим приоритетам
        }
        // === ПРИОРИТЕТ 2: ЛАЗЕР (7.5м - 17м) ===
        else if (distance <= laserMaxRange)
        {
            if (!isAiming && Time.time - lastLaserTime >= attackCooldown)
            {
                lastLaserTime = Time.time;
                isAiming = true; // Блокируем ПЕРЕЗАПУСК, но не весь Update
                StartCoroutine(AttackLazer());
            }
            // Если лазер на КД или уже стреляем — просто стоим (MoveTowards не вызываем)
        }
        // === ПРИОРИТЕТ 3: ТАРАН (> 17м) ===
        else
        {
            if (!isCharging && Time.time - lastChargeTime >= attackCooldown)
            {
                lastChargeTime = Time.time;
                StartCoroutine(AttackCharge());
            }
            else
            {
                // Если таран на КД или уже бежим — просто догоняем
                MoveTowards(target.position);
            }
        }
    }

    private void MoveTowards(Vector3 targetPos)
    {
        float speedScale = (targetPos.x < transform.position.x) ? -1f : 1f;
        Vector2 newPos = rb.position + new Vector2(speedScale * moveSpeed * Time.deltaTime, 0f);
        rb.MovePosition(newPos);
    }

    private IEnumerator AttackCharge()
    {
        isCharging = true;
        Vector3 moveVector = Vector3.right;
        if (target.position.x < transform.position.x)
        {
            commantText.text = "8.7";
            moveVector = Vector3.left;
        }
        else
            commantText.text = "14.95";

        Vector3 startPos = transform.position;
        Vector3 endPos = target.position - moveVector * chargeStopingDistance;
        endPos.y = startPos.y;

        // Разгон к игроку (через rb.MovePosition для физики)
        for (float t = 0; t < 1; t += Time.deltaTime * 2f)
        {
            Vector3 lerpPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(lerpPos);
            yield return null;
        }

        // Откат после удара
        Vector3 knockbackEnd = transform.position + moveVector * knockbackDistance;
        for (float t = 0; t < 1; t += Time.deltaTime * 3f)
        {
            Vector3 lerpPos = Vector3.Lerp(endPos, knockbackEnd, t);
            rb.MovePosition(lerpPos);
            yield return null;
        }

        isCharging = false;
    }

    private IEnumerator AttackLazer()
    {
        commantText.text = "6.875";

        Vector2 direction;
        float targetAngle;
        float startAngle = lazerGun.eulerAngles.z;

        // Прицеливание: враг НЕ двигается (isAiming = true блокирует перезапуск)
        for (float t = 0; t < 1; t += Time.deltaTime / timeToAim)
        {
            direction = target.position - lazerGun.position + Vector3.up;
            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            lazerGun.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        // Финальный выстрел
        direction = target.position - lazerGun.position + Vector3.up;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        lazerGun.rotation = Quaternion.Euler(0, 0, targetAngle);

        StartCoroutine(DelayedAnimatorTrigger("lazer", delayBeforeAttack));

        // Снимаем блокировку только после завершения всей логики
        yield return new WaitForSeconds(delayBeforeAttack);
        isAiming = false;
    }

    private void AttackFire()
    {
        commantText.text = "1";
        StartCoroutine(DelayedAnimatorTrigger("fire", delayBeforeAttack));
    }

    private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(trigger);
    }
}