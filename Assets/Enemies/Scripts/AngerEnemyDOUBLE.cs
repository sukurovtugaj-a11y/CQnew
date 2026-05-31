using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyDOUBLE : AngerEnemy
{
    [Header("��������� ����")]
    public float fireRange = 7.5f;      // ������: < 7.5� (��������� 1)
    public float laserMaxRange = 17f;   // �����: 7.5� - 17� (��������� 2)
    public float chargeMinRange = 17f;  // �����: > 17� (��������� 3)

    [Header("���������")]
    public float moveSpeed = 5f;
    public float aggroRadius = 20f;     // ������������ ������ ����
    public float attackCooldown = 2f;
    public float timeToAim = 5f;
    public float delayBeforeAttack = 0.5f;
    public float chargeStopingDistance = 2f;
    public float knockbackDistance = 1.5f;

    [Header("�������")]
    public AudioSource motorSound;
    public AudioSource fireSound;
    public AudioSource laserSound;
    public AudioSource chargeSound;

    [Header("������")]
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

    // ����� ���������� (������ ��� ������ �� ���������� ������� ������ �����)
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
        StopAllSounds();
    }

    private void OnDisable()
    {
        StopAllSounds();
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

        // ������� ��������� (� ��� �� ���������, ��� � ����, ��� ���������������)
        float distance = Vector3.Distance(target.position, (transform.position + Vector3.up * 2));

        if (distance > aggroRadius)
        {
            StopAllSounds();
            MoveTowards(target.position);
            return;
        }

        if (motorSound != null && !motorSound.isPlaying)
            motorSound.Play();

        // === ��������� 1: ����̨� (< 7.5�) ===
        if (distance < fireRange)
        {
            // ���������� �����, ����� ����� �� ������ ����, ���� ����� ����� �������
            isCharging = false;
            isAiming = false;

            if (Time.time - lastFireTime >= attackCooldown)
            {
                lastFireTime = Time.time;
                AttackFire();
            }
            // �� ������ return � ���� ������ �� ��, ��� ���� ������ �����������
        }
        // === ��������� 2: ����� (7.5� - 17�) ===
        else if (distance <= laserMaxRange)
        {
            if (!isAiming && Time.time - lastLaserTime >= attackCooldown)
            {
                lastLaserTime = Time.time;
                isAiming = true; // ��������� ����������, �� �� ���� Update
                StartCoroutine(AttackLazer());
            }
            // ���� ����� �� �� ��� ��� �������� � ������ ����� (MoveTowards �� ��������)
        }
        // === ��������� 3: ����� (> 17�) ===
        else
        {
            if (!isCharging && Time.time - lastChargeTime >= attackCooldown)
            {
                lastChargeTime = Time.time;
                StartCoroutine(AttackCharge());
            }
            else
            {
                // ���� ����� �� �� ��� ��� ����� � ������ ��������
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
        if (chargeSound != null) chargeSound.Play();
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

        // ������ � ������ (����� rb.MovePosition ��� ������)
        for (float t = 0; t < 1; t += Time.deltaTime * 2f)
        {
            Vector3 lerpPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(lerpPos);
            yield return null;
        }

        // ����� ����� �����
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
        if (laserSound != null) laserSound.Play();
        commantText.text = "6.875";

        Vector2 direction;
        float targetAngle;
        float startAngle = lazerGun.eulerAngles.z;

        // ������������: ���� �� ��������� (isAiming = true ��������� ����������)
        for (float t = 0; t < 1; t += Time.deltaTime / timeToAim)
        {
            direction = target.position - lazerGun.position + Vector3.up;
            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            lazerGun.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        // ��������� �������
        direction = target.position - lazerGun.position + Vector3.up;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        lazerGun.rotation = Quaternion.Euler(0, 0, targetAngle);

        StartCoroutine(DelayedAnimatorTrigger("lazer", delayBeforeAttack));

        // ������� ���������� ������ ����� ���������� ���� ������
        yield return new WaitForSeconds(delayBeforeAttack);
        isAiming = false;
    }

    private void AttackFire()
    {
        if (fireSound != null) fireSound.Play();
        commantText.text = "1";
        StartCoroutine(DelayedAnimatorTrigger("fire", delayBeforeAttack));
    }

    private void StopAllSounds()
    {
        if (motorSound != null && motorSound.isPlaying)
            motorSound.Stop();
        if (fireSound != null && fireSound.isPlaying)
            fireSound.Stop();
        if (laserSound != null && laserSound.isPlaying)
            laserSound.Stop();
        if (chargeSound != null && chargeSound.isPlaying)
            chargeSound.Stop();
    }

    private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(trigger);
    }
}