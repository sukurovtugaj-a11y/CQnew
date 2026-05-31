using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyINT : AngerEnemy
{
    [Header("��������� �����")]
    public float attackCooldown = 2f;
    public float delayBeforeAttack = 0.5f;
    public float detectionRadius = 20f; // ����. ��������� ��������

    [Header("��������� �������")]
    [Tooltip("���������, �� ������� ���� ���� �������� (� ������ ��� �������)")]
    public float clubDistance = 4.5f;

    [Header("��������� ��������")]
    public float moveSpeed = 3f;
    public float stopingDistance = 8f;   // ���������, � ������� �� ��������� ����� � ��������

    [Header("�������")]
    public AudioSource motorSound;
    public AudioSource clubSound;
    public AudioSource laserSound;
    public AudioSource rocketSound;

    [Header("������")]
    public TextMeshProUGUI commantText;
    public Animator animator;

    private Rigidbody2D rb;
    // ���������� ������� ��� ����������
    private float lastClubTime;
    private float lastRangedTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // �������������� ������� � �������, ����� ����� ���� ��������� �����
        lastClubTime = -100f;
        lastRangedTime = -100f;
        StopAllSounds();
    }

    private void OnDisable()
    {
        StopAllSounds();
    }

    private void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.position, (transform.position + Vector3.up * 2));

        if (distance > detectionRadius)
        {
            StopAllSounds();
            MoveTowards(target.position);
            return;
        }

        if (motorSound != null && !motorSound.isPlaying)
            motorSound.Play();

        if (distance < clubDistance)
        {
            if (Time.time - lastClubTime >= attackCooldown)
            {
                lastClubTime = Time.time;
                lastRangedTime = Time.time;

                AttackClub();
            }
            return;
        }

        if (distance > stopingDistance)
            MoveTowards(target.position);

        if (Time.time - lastRangedTime >= attackCooldown)
        {
            lastRangedTime = Time.time;
            lastClubTime = Time.time;

            if (Random.Range(0, 100) > 50) AttackLazer();
            else AttackRocket();
        }
    }

    // ������� ��������
    private void MoveTowards(Vector3 targetPos)
    {
        float speedScale = (targetPos.x < transform.position.x) ? -1f : 1f;
        Vector2 newPos = rb.position + new Vector2(speedScale * moveSpeed * Time.deltaTime, 0f);
        rb.MovePosition(newPos);
    }

    private void AttackClub()
    {
        if (clubSound != null) clubSound.Play();
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
        if (laserSound != null) laserSound.Play();
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
        if (rocketSound != null) rocketSound.Play();
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

    private void StopAllSounds()
    {
        if (motorSound != null && motorSound.isPlaying)
            motorSound.Stop();
        if (clubSound != null && clubSound.isPlaying)
            clubSound.Stop();
        if (laserSound != null && laserSound.isPlaying)
            laserSound.Stop();
        if (rocketSound != null && rocketSound.isPlaying)
            rocketSound.Stop();
    }

    private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(trigger);
    }
}