using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyNewBOOL : AngerEnemy
{
    [Header("���������")]
    public float aggroRange = 10f;
    public float looseRange = 20f;

    [Header("����")]
    public float attackTime = 15f;
    public float restTime = 5f;

    [Header("���������")]
    public float moveSpeed = 4f;
    public float bounceTime = 0.3f;

    [Header("���� �� ����������")]
    public float damageFalse = 10f;
    public float damageTrue = 3f;
    public float knockbackFalse = 1.5f;
    public float knockbackTrue = 0.8f;

    [Header("������")]
    public SpriteRenderer trueSprite;
    public GameObject falseLight;
    public GameObject trueLight;

    [Header("������� ��� ������������")]
    public bool enableBlink = true;
    public float blinkDuration = 0.5f;
    public int blinkCount = 4;

    [Header("������")]
    public IdleEnemy idleBehavior;
    public HP hp;
    public DamageSource damageSource;
    public AudioSource switchAudioSource;

    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private bool isAttacking = false;
    private float cycleTimer = 0f;
    private bool isActive = false;
    private bool isBlinking = false;
    private bool isBouncing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (hp == null) hp = GetComponent<HP>();
        if (damageSource == null) damageSource = GetComponent<DamageSource>();
        SetVisualState(false);
    }

    public override void TurnOn(Transform newTarget)
    {
        base.TurnOn(newTarget);
        targetPosition = newTarget.position;
        isActive = true;
        isAttacking = true;
        cycleTimer = 0f;
        SwitchState(true);
    }

    private void Update()
    {
        if (target == null || isBlinking || isBouncing) return;

        targetPosition = target.position;
        float sqrDist = Vector3.SqrMagnitude(targetPosition - transform.position);

        if (sqrDist > looseRange * looseRange)
        {
            ResetEnemy();
            return;
        }

        if (!isActive && sqrDist > aggroRange * aggroRange) return;

        isActive = true;
        cycleTimer += Time.deltaTime;

        if (isAttacking)
        {
            if (cycleTimer >= attackTime) SwitchState(false);
            else ChaseTarget();
        }
        else
        {
            if (cycleTimer >= restTime) SwitchState(true);
        }
    }

    private void SwitchState(bool toAttack)
    {
        isAttacking = toAttack;
        cycleTimer = 0f;

        if (enableBlink && !isBlinking)
            StartCoroutine(BlinkAndSwitch(toAttack));
        else
            ApplyState(toAttack);
    }

    private IEnumerator BlinkAndSwitch(bool toAttack)
    {
        isBlinking = true;
        if (switchAudioSource != null) switchAudioSource.Play();

        float blinkInterval = blinkDuration / (blinkCount * 2f);
        for (int i = 0; i < blinkCount; i++)
        {
            SetVisualInstant(toAttack);
            yield return new WaitForSeconds(blinkInterval);
            SetVisualInstant(!toAttack);
            yield return new WaitForSeconds(blinkInterval);
        }
        ApplyState(toAttack);
        isBlinking = false;
    }

    private void SetVisualInstant(bool isAttack)
    {
        if (trueSprite != null) trueSprite.enabled = isAttack;
        if (falseLight != null) falseLight.SetActive(!isAttack);
        if (trueLight != null) trueLight.SetActive(isAttack);
    }

    private void ApplyState(bool isAttack)
    {
        SetVisualInstant(isAttack);
        if (damageSource != null)
        {
            damageSource.damage = isAttack ? damageTrue : damageFalse;
            damageSource.knockbackDistance = isAttack ? knockbackTrue : knockbackFalse;
            damageSource.SetWork(true);
        }
        if (hp != null) hp.enabled = !isAttack;
        if (!isAttack) rb.velocity = Vector2.zero;
    }

    private void SetVisualState(bool isAttack)
    {
        if (trueSprite != null) trueSprite.enabled = isAttack;
        if (falseLight != null) falseLight.SetActive(!isAttack);
        if (trueLight != null) trueLight.SetActive(isAttack);
        if (damageSource != null)
        {
            damageSource.damage = isAttack ? damageTrue : damageFalse;
            damageSource.knockbackDistance = isAttack ? knockbackTrue : knockbackFalse;
            damageSource.SetWork(true);
        }
        if (hp != null) hp.enabled = !isAttack;
    }

    private void ChaseTarget()
    {
        float dir = Mathf.Sign(targetPosition.x - transform.position.x);
        Vector2 newPos = rb.position + new Vector2(dir * moveSpeed * Time.deltaTime, 0);
        rb.MovePosition(newPos);
    }

    private IEnumerator BounceOut(float time, bool isRight)
    {
        isBouncing = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = isRight ? Vector2.right : Vector2.left;
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float speed = moveSpeed * (1f - t);
            rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
            yield return null;
        }
        isBouncing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ИГНОРИРУЕМ ВРАГОВ - проверка по слою Enemy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        // ИГНОРИРУЕМ ВРАГОВ - проверка по компонентам
        if (collision.gameObject.GetComponent<IdleEnemy>() != null || 
            collision.gameObject.GetComponent<AngerEnemy>() != null ||
            collision.gameObject.GetComponent<AngerEnemyNewBOOL>() != null)
            return;

        // ИГНОРИРУЕМ TeleportObj по тегу
        if (collision.gameObject.CompareTag("TeleportObj"))
            return;

        if (collision.gameObject.TryGetComponent<HP>(out HP victim))
        {
            bool isRight = transform.position.x > targetPosition.x;
            StartCoroutine(BounceOut(bounceTime, isRight));
        }
    }

    private void ResetEnemy()
    {
        isActive = false;
        isAttacking = false;
        cycleTimer = 0f;
        rb.velocity = Vector2.zero;
        SetVisualState(false);
        if (idleBehavior != null)
        {
            idleBehavior.MakePeace();
            idleBehavior.enabled = true;
        }
        this.enabled = false;
    }

    private void OnDisable()
    {
        SetVisualState(false);
    }
}