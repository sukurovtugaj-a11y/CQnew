using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AngerEnemyBOOL : AngerEnemy
{
    [Header("Дистанции")]
    public float aggroRange = 10f;         
    public float looseRange = 20f;         

    [Header("Время циклов")]
    public float attackTime = 15f;        
    public float vulnerableTime = 5f;

    [Header("Настройки передвижения")]
    public float runSpeed = 4f;

    [Header("Ссылки")]
    public Animator anim;
    public IdleEnemy idleBehavior;

    [Header("Звуки фаз")]
    [Tooltip("Звук для фазы TRUE (уязвимость)")]
    public AudioSource truePhaseSound;
    [Tooltip("Звук для фазы FALSE (атака)")]
    public AudioSource falsePhaseSound;

    [Header("Объекты для фаз")]
    [Tooltip("Объекты, которые включаются в фазе АТАКИ (TRUE)")]
    public GameObject[] trueObjects;
    [Tooltip("Объекты, которые включаются в фазе УЯЗВИМОСТИ (FALSE)")]
    public GameObject[] falseObjects;

    private Rigidbody2D rb;
    private Vector3 targetPosition;

    // Циклы врага
    private bool run = false;                 
    private bool isVulnerablePhase = false;   // false = атака, true = уязвимость
    private float cycleTimer = 0f;
    private bool isActive = false;            // флаг активности врага
    private float lastPollTime;

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
        StopAllSounds();
        Debug.Log($"[AngerEnemyBOOL] OnDisable called, enabled: {enabled}");
    }

    private void Update()
    {
        if (!isActive)
        {
            PollForPlayer();
            return;
        }

        if (target == null) return;
        targetPosition = target.position;

        // Проверка выхода за пределы досягаемости
        float sqrDist = Vector3.SqrMagnitude(targetPosition - transform.position);
        if (sqrDist > looseRange * looseRange)
        {
            isActive = false;
            run = false;
            anim.SetBool("state", false);
            rb.velocity = Vector2.zero;
            StopAllSounds();
            if (idleBehavior != null)
            {
                idleBehavior.MakePeace();
                idleBehavior.enabled = true;
            }
            return;
        }

        // Враг активен — работаем
        isActive = true;
        cycleTimer += Time.deltaTime;

        // === Фаза 1: Атака (run = true, FALSE) ===
        if (!isVulnerablePhase)
        {
            if (cycleTimer >= attackTime)
            {
                // Переход в фазу уязвимости (TRUE)
                isVulnerablePhase = true;
                run = false;
                anim.SetBool("state", false);
                rb.velocity = Vector2.zero;
                cycleTimer = 0f;
                
                // Включаем TRUE объекты (уязвимость), выключаем FALSE
                SetObjectsActive(trueObjects, true);
                SetObjectsActive(falseObjects, false);

                // ОСТАНАВЛИВАЕМ звук FALSE, если он играет
                if (falsePhaseSound != null && falsePhaseSound.isPlaying) 
                    falsePhaseSound.Stop();

                // Играем звук фазы TRUE
                if (truePhaseSound != null) truePhaseSound.Play();
            }
            else
            {
                run = true;
                anim.SetBool("state", true);
                ChaseTarget();
                
                // Включаем FALSE объекты (атака), выключаем TRUE
                SetObjectsActive(trueObjects, false);
                SetObjectsActive(falseObjects, true);

                // Играем звук FALSE на протяжении всей фазы
                if (falsePhaseSound != null && !falsePhaseSound.isPlaying)
                    falsePhaseSound.Play();
            }
        }
        // === Фаза 2: Уязвимость (run = false, TRUE) ===
        else
        {
            run = false;
            anim.SetBool("state", false);

            if (cycleTimer >= vulnerableTime)
            {
                // Возврат к атаке (FALSE)
                isVulnerablePhase = false;
                cycleTimer = 0f;

                // Включаем FALSE объекты (атака), выключаем TRUE
                SetObjectsActive(trueObjects, false);
                SetObjectsActive(falseObjects, true);

                // ОСТАНАВЛИВАЕМ звук TRUE, если он играет
                if (truePhaseSound != null && truePhaseSound.isPlaying) 
                    truePhaseSound.Stop();

                // Играем звук фазы FALSE
                if (falsePhaseSound != null) falsePhaseSound.Play();
            }
            else
            {
                // Включаем TRUE объекты (уязвимость), выключаем FALSE
                SetObjectsActive(trueObjects, true);
                SetObjectsActive(falseObjects, false);

                // Играем звук TRUE на протяжении всей фазы
                if (truePhaseSound != null && !truePhaseSound.isPlaying)
                    truePhaseSound.Play();
            }
        }
    }

    private void PollForPlayer()
    {
        if (Time.time - lastPollTime < 1f) return;
        lastPollTime = Time.time;

        Collider2D[] hits = new Collider2D[10];
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position, aggroRange, hits);

        for (int i = 0; i < count; i++)
        {
            if (hits[i].gameObject.layer == LayerMask.NameToLayer("Enemy"))
                continue;
            if (hits[i].GetComponent<SecMainCharacter>() != null)
            {
                if (idleBehavior != null)
                    idleBehavior.MakeAngry(hits[i].gameObject);
                return;
            }
        }
    }

    private void StopAllSounds()
    {
        if (truePhaseSound != null && truePhaseSound.isPlaying)
            truePhaseSound.Stop();
        if (falsePhaseSound != null && falsePhaseSound.isPlaying)
            falsePhaseSound.Stop();
    }

    private void ChaseTarget()
    {
        float direction = Mathf.Sign(targetPosition.x - transform.position.x);
        Vector2 newPos = rb.position + new Vector2(direction * runSpeed * Time.deltaTime, 0f);
        rb.MovePosition(newPos);
    }

    private void SetObjectsActive(GameObject[] objects, bool isActive)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(isActive);
        }
    }
}
