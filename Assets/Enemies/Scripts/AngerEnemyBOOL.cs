using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyBOOL : AngerEnemy
{
    [Header("���������")]
    public float aggroRange = 10f;         
    public float looseRange = 20f;         

    [Header("����� �����")]
    public float attackTime = 15f;        
    public float vulnerableTime = 5f;

    [Header("���������")]
    public float runSpeed = 4f;
    public float collisionDamage = 3f;
    public float bounceTime = 0.3f;

    [Header("������")]
    public Animator anim;
    public IdleEnemy idleBehavior;

    private Rigidbody2D rb;
    private Vector3 targetPosition;

    // ������ �����
    private bool run = false;                 
    private bool isVulnerablePhase = false;   // false = �����, true = ������
    private float cycleTimer = 0f;
    private bool isActive = false;            // ���� ������� ������ � ���� �����

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void TurnOn(Transform newTarget)
    {
        Debug.Log($"[AngerEnemyBOOL] TurnOn called! Target: {newTarget.name}, isActive was: {isActive}");
        base.TurnOn(newTarget);
        targetPosition = newTarget.position;
        isActive = true;
        cycleTimer = 0f;
        isVulnerablePhase = false;
        
        // Принудительно будим физику
        if (rb != null)
        {
            rb.WakeUp();
            Debug.Log("[AngerEnemyBOOL] Rigidbody2D wake up called");
        }
        
        Debug.Log($"[AngerEnemyBOOL] After TurnOn: target={(target != null ? target.name : "NULL")}, isActive={isActive}");
    }

    private void OnEnable()
    {
        Debug.Log($"[AngerEnemyBOOL] OnEnable called, enabled: {enabled}, target: {(target != null ? target.name : "NULL")}");
    }

    private void OnDisable()
    {
        Debug.Log($"[AngerEnemyBOOL] OnDisable called, enabled: {enabled}");
    }

    private void Update()
    {
        if (target == null) return;
        targetPosition = target.position;

        // ���� ����� ������� ������ � ���������� ����������
        float sqrDist = Vector3.SqrMagnitude(targetPosition - transform.position);
        if (sqrDist > looseRange * looseRange)
        {
            isActive = false;
            run = false;
            anim.SetBool("state", false);
            rb.velocity = Vector2.zero;
            if (idleBehavior != null)
            {
                idleBehavior.MakePeace();
                idleBehavior.enabled = true;
            }
            this.enabled = false;
            return;
        }

        // ���� ����� ��� �� � ���� ����� � ������ ���
        if (!isActive && sqrDist > aggroRange * aggroRange)
        {
            return;
        }

        // ����� � ���� � ���������/���������� ����
        isActive = true;
        cycleTimer += Time.deltaTime;

        // === ���� 1: ����� (run = true) ===
        if (!isVulnerablePhase)
        {
            if (cycleTimer >= attackTime)
            {
                // ������� � �������� ����
                isVulnerablePhase = true;
                run = false;
                anim.SetBool("state", false);
                rb.velocity = Vector2.zero;
                cycleTimer = 0f;
            }
            else
            {
                run = true;
                anim.SetBool("state", true);
                ChaseTarget();
            }
        }
        // === ���� 2: ���������� (run = false) ===
        else
        {
            run = false;
            anim.SetBool("state", false);

            if (cycleTimer >= vulnerableTime)
            {
                // ������� � ���� �����
                isVulnerablePhase = false;
                cycleTimer = 0f;
            }
        }
    }

    private void ChaseTarget()
    {
        float direction = Mathf.Sign(targetPosition.x - transform.position.x);
        Vector2 newPos = rb.position + new Vector2(direction * runSpeed * Time.deltaTime, 0f);
        rb.MovePosition(newPos);
    }

    private IEnumerator BounceOut(float time, bool isRight)
    {
        rb.velocity = Vector2.zero;
        Vector2 dir = isRight ? Vector2.right : Vector2.left;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float speed = runSpeed * (1f - t);
            rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<HP>(out HP victim))
        {
            // ���� ��������� ������, �� � ���� ����� � ������
            float damage = isVulnerablePhase ? collisionDamage * 0.5f : collisionDamage;
            victim.TakeDamage(damage, this.gameObject);

            bool isRight = transform.position.x > targetPosition.x;
            StartCoroutine(BounceOut(bounceTime, isRight));
        }
    }
}