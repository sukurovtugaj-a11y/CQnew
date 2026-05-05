using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyCHAR : AngerEnemy
{
    [Header("Дистанции")]
    public float chargeRange = 12f;
    public float aggroRadius = 20f;

    [Header("Настройки")]
    public float attackCooldown = 2f;
    public float moveSpeed = 4f;
    public float chargeDamage = 2f;
    public float chargeStopingDistance = 2f;

    [Header("Ссылки")]
    public LineRenderer line;
    public Animator animator;
    public Gun gun;
    public Vector3[] pointsO;
    public Vector3[] pointsB;
    public Vector3[] pointsI;

    [Header("Физика")]
    public Rigidbody2D rb;

    private float lastAttackTime;
    private bool onCharge = false;
    private Vector3 basePosition;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // ВАЖНО: LineRenderer должен быть в локальных координатах, чтобы масштаб работал
        if (line != null) line.useWorldSpace = false;

        if (pointsI == null || pointsI.Length == 0)
        {
            if (line != null)
            {
                pointsI = new Vector3[line.positionCount];
                line.GetPositions(pointsI);
            }
        }
        lastAttackTime = -100f;
        basePosition = transform.position;

        // Сразу смотрим в сторону игрока при старте
        FaceTarget();
    }

    private void Update()
    {
        if (target == null) return;

        // Разворот каждый кадр (работает для LineRenderer)
        FaceTarget();

        if (onCharge) return;

        float distance = Vector3.Distance(target.position, transform.position);
        basePosition = transform.position;

        if (distance > aggroRadius)
        {
            MoveTowards(target.position);
            return;
        }

        if (distance > chargeRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                StartCoroutine(AttackCharge());
            }
            else
            {
                MoveTowards(target.position);
            }
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackGun());
        }
        else
        {
            MoveTowards(target.position);
        }
    }

    /// <summary>
    /// Разворот LineRenderer влево/вправо
    /// </summary>
    private void FaceTarget()
    {
        if (target == null) return;
        // 1 = вправо, -1 = влево. Избегаем 0, чтобы не схлопнуть объект
        float dir = target.position.x >= transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(dir, 1, 1);
    }

    private void MoveTowards(Vector3 targetPos)
    {
        float direction = Mathf.Sign(targetPos.x - transform.position.x);
        basePosition += direction * moveSpeed * Time.deltaTime * Vector3.right;
        if (rb != null) rb.MovePosition(basePosition);
        else transform.position = basePosition;
    }

    private IEnumerator AttackCharge()
    {
        onCharge = true;
        FaceTarget();

        if (line != null && pointsO != null)
            yield return StartCoroutine(GoToForm(pointsO));

        yield return StartCoroutine(ChargeToTarget());

        if (target != null && target.TryGetComponent<HP>(out HP hp))
            hp.TakeDamage(chargeDamage, this.gameObject);

        if (line != null && pointsI != null)
            yield return StartCoroutine(GoToForm(pointsI));

        onCharge = false;
    }

    private IEnumerator AttackGun()
    {
        onCharge = true;
        FaceTarget();

        if (line != null && pointsB != null)
            yield return StartCoroutine(GoToForm(pointsB));

        yield return new WaitForSeconds(0.5f);

        if (gun != null && target != null)
        {
            gun.Shoot(target.position + Vector3.up);
        }

        if (line != null && pointsI != null)
            yield return StartCoroutine(GoToForm(pointsI));

        onCharge = false;
    }

    private IEnumerator ChargeToTarget()
    {
        if (target == null) yield break;

        Vector3 moveDir = (target.position.x < transform.position.x) ? Vector3.left : Vector3.right;
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position - moveDir * chargeStopingDistance;
        endPos.y = startPos.y;

        for (float t = 0; t < 1; t += Time.deltaTime * 2f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;
    }

    private IEnumerator GoToForm(Vector3[] targetForm)
    {
        if (line == null || targetForm == null) yield break;

        int count = line.positionCount;
        Vector3[] current = new Vector3[count];
        line.GetPositions(current);

        for (float t = 0; t < 1; t += Time.deltaTime * 2f)
        {
            for (int i = 0; i < count; i++)
                current[i] = Vector3.Lerp(current[i], targetForm[i], t);
            line.SetPositions(current);
            yield return null;
        }
        line.SetPositions(targetForm);
    }
}