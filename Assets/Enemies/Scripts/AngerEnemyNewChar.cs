using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyNewCHAR : AngerEnemy
{
    [Header("Дистанции")]
    public float closeRange = 7f;
    public float farRange = 12f;
    public float aggroRadius = 20f;

    [Header("Настройки")]
    public float moveSpeed = 3f;
    public float chargeSpeed = 6f;
    public float attackCooldown = 2f;

    [Header("Ссылки на формы (дети)")]
    public GameObject formO;
    public GameObject formG;
    public GameObject formSeven;
    public GameObject formZH;

    [Header("Визуал и физика")]
    public SpriteRenderer mainSprite;
    public Rigidbody2D rb;
    public BoxCollider2D baseCollider;

    [Header("Звуки")]
    public AudioSource zigzagAudioSource;    // Звук для формы Ж
    public AudioSource chargeAudioSource;    // Звук для формы О

    private float lastAttackTime = -100f;
    private bool isAttacking = false;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (mainSprite == null) mainSprite = GetComponent<SpriteRenderer>();
        if (baseCollider == null) baseCollider = GetComponent<BoxCollider2D>();

        DeactivateAllForms();
        StopAllSounds();
    }

    private void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.position, transform.position);
        if (distance > aggroRadius)
        {
            StopAllSounds();
        }

        if (isAttacking) return;

        FaceTarget();

        bool playerOnRight = target.position.x >= transform.position.x;

        if (distance > aggroRadius)
        {
            MoveTowards(target.position);
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            MoveTowards(target.position);
            return;
        }

        // Близко: выстрел (Г или 7)
        if (distance <= closeRange)
        {
            lastAttackTime = Time.time;
            if (playerOnRight)
                StartCoroutine(AttackShoot(formG));
            else
                StartCoroutine(AttackShoot(formSeven));
            return;
        }

        // Далеко: О или Ж
        if (distance <= farRange)
        {
            lastAttackTime = Time.time;
            if (!playerOnRight)
                StartCoroutine(AttackCharge(formO));
            else
                StartCoroutine(AttackZigzag(formZH));
            return;
        }

        MoveTowards(target.position);
    }

    private void FaceTarget()
    {
        if (mainSprite != null && target != null)
            mainSprite.flipX = target.position.x < transform.position.x;
    }

    private void MoveTowards(Vector3 targetPos)
    {
        float dir = Mathf.Sign(targetPos.x - transform.position.x);
        Vector2 newPos = rb.position + new Vector2(dir * moveSpeed * Time.deltaTime, 0);
        rb.MovePosition(newPos);
    }

    private void DeactivateAllForms()
    {
        DeactivateForm(formO);
        DeactivateForm(formG);
        DeactivateForm(formSeven);
        DeactivateForm(formZH);

        if (mainSprite != null) mainSprite.enabled = true;
        if (baseCollider != null) baseCollider.enabled = true;
    }

    private void DeactivateForm(GameObject form)
    {
        if (form == null) return;
        form.SetActive(false);

        foreach (var col in form.GetComponents<Collider2D>())
            col.enabled = false;

        foreach (var ds in form.GetComponents<DamageSource>())
            ds.SetWork(false);
    }

    private void ActivateForm(GameObject form)
    {
        DeactivateAllForms();
        if (form == null) return;

        form.SetActive(true);

        foreach (var col in form.GetComponents<Collider2D>())
            col.enabled = true;

        foreach (var ds in form.GetComponents<DamageSource>())
            ds.SetWork(true);

        if (mainSprite != null) mainSprite.enabled = false;
        if (baseCollider != null) baseCollider.enabled = false;
    }

    private IEnumerator AttackShoot(GameObject form)
    {
        isAttacking = true;
        ActivateForm(form);

        yield return new WaitForSeconds(0.3f);

        Gun gun = form.GetComponentInChildren<Gun>();
        if (gun != null && target != null)
        {
            gun.Shoot(target.position + Vector3.up);
        }

        yield return new WaitForSeconds(0.5f);
        DeactivateAllForms();
        isAttacking = false;
    }

    private IEnumerator AttackCharge(GameObject form)
    {
        isAttacking = true;

        // Воспроизводим звук для формы О
        if (chargeAudioSource != null)
            chargeAudioSource.Play();

        ActivateForm(form);

        Vector3 dir = (target.position.x < transform.position.x) ? Vector3.left : Vector3.right;
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position - dir * 2f;
        endPos.y = startPos.y;

        float rotateDirection = dir.x > 0 ? -1f : 1f;
        float rotateSpeed = 300f;

        for (float t = 0; t < 1; t += Time.deltaTime * chargeSpeed * 0.2f)
        {
            rb.MovePosition(Vector3.Lerp(startPos, endPos, t));

            if (form != null)
                form.transform.Rotate(0, 0, rotateDirection * rotateSpeed * Time.deltaTime);

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        DeactivateAllForms();
        isAttacking = false;
    }

    private IEnumerator AttackZigzag(GameObject form)
    {
        isAttacking = true;

        // Воспроизводим звук для формы Ж
        if (zigzagAudioSource != null)
            zigzagAudioSource.Play();

        ActivateForm(form);

        Vector3 dir = (target.position.x < transform.position.x) ? Vector3.left : Vector3.right;
        Vector3 startPos = transform.position;

        // ФАЗА 1: ПОДЪЁМ ВВЕРХ
        float climbHeight = 8f;
        float climbSpeed = 12f;
        float climbElapsed = 0f;

        while (climbElapsed < 1f)
        {
            climbElapsed += Time.deltaTime * climbSpeed * 0.5f;
            float t = Mathf.Clamp01(climbElapsed);
            Vector3 climbPos = startPos + Vector3.up * climbHeight * t;
            rb.MovePosition(climbPos);
            yield return null;
        }

        // ФАЗА 2: ЗИГЗАГ ВНИЗ
        float duration = 2f;
        float frequency = 13f;
        float amplitude = 6f;
        float elapsed = 0f;
        Vector3 peakPos = rb.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float targetY = Mathf.Lerp(peakPos.y, target.position.y, t);
            float zigzag = Mathf.Sin(elapsed * frequency) * amplitude;

            Vector3 newPos = peakPos + dir * chargeSpeed * elapsed + Vector3.up * (targetY - peakPos.y + zigzag);
            rb.MovePosition(newPos);
            yield return null;
        }

        DeactivateAllForms();
        isAttacking = false;
    }

    private void OnDisable()
    {
        StopAllSounds();
        isAttacking = false;
        DeactivateAllForms();
    }

    private void StopAllSounds()
    {
        if (zigzagAudioSource != null && zigzagAudioSource.isPlaying)
            zigzagAudioSource.Stop();
        if (chargeAudioSource != null && chargeAudioSource.isPlaying)
            chargeAudioSource.Stop();
    }
}