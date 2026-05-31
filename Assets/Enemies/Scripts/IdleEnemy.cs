using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class IdleEnemy : MonoBehaviour
{
    public float patroolRange;
    public float patroolSpeed;
    public HP hp;
    public AngerEnemy angryBehavior;
    public float angerCooldown;

    private bool angry = false;
    private Vector3 moveVector;
    private float patroolPointA;
    private float patroolPointB;

    private float lastAngerTime;
    private bool isStarting = true;

    private void Start()
    {
        moveVector = Vector3.right * patroolSpeed;

        patroolPointA = transform.position.x - patroolRange;
        patroolPointB = transform.position.x + patroolRange;

        hp.onHit.AddListener(MakeAngry);
        lastAngerTime = -angerCooldown;
        angryBehavior.enabled = false;
        Invoke("Ready", 1f);
    }

    private void Ready()
    {
        isStarting = false;
    }

    public void MakePeace()
    {
        enabled = true;
        angry = false;
    }

    public void MakeAngry(bool flag, GameObject damageSource)
    {
        Debug.Log($"[IdleEnemy] MakeAngry(flag={flag}, src={damageSource?.name}) | isStarting={isStarting}, angry={angry}, lastAngerTime={lastAngerTime}, cooldown={angerCooldown}, time={Time.time}");

        if (isStarting) { Debug.Log("[IdleEnemy] BLOCKED: isStarting"); return; }

        if (angry == flag) { Debug.Log("[IdleEnemy] BLOCKED: angry == flag"); return; }

        if (flag == true && Time.time - lastAngerTime < angerCooldown)
        {
            Debug.Log($"[IdleEnemy] BLOCKED: cooldown (lastAngerTime={lastAngerTime}, diff={Time.time - lastAngerTime}, cd={angerCooldown})");
            return;
        }

        // ТОЛЬКО ИГРОК! Игнорируем врагов
        if (flag == true && (damageSource == null || damageSource.GetComponent<SecMainCharacter>() == null))
        {
            Debug.Log("[IdleEnemy] BLOCKED: not player");
            return;
        }

        Debug.Log("[IdleEnemy] MakeAngry: ACTIVATING!");
        angry = flag;
        this.enabled = !flag;
        angryBehavior.enabled = flag;

        if (flag)
            angryBehavior.TurnOn(damageSource.transform);
    }

    public void MakeAngry(GameObject damageSource)
    {
        MakeAngry(true, damageSource);
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;

        if (transform.position.x < patroolPointA)
            moveVector.x = patroolSpeed;
        else if (transform.position.x > patroolPointB)
            moveVector.x = -patroolSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[IdleEnemy] OnTriggerEnter2D! self.enabled={enabled}, angry={angry}, collider={collision.name}, tag={collision.tag}, layer={collision.gameObject.layer}");

        // Игнорируем врагов (тег TeleportableObject)
        if (collision.CompareTag("TeleportObj"))
            return;

        // Игнорируем объекты на слое Enemy (чтобы враги не атаковали друг друга)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        var player = collision.GetComponent<SecMainCharacter>();
        Debug.Log($"[IdleEnemy] SecMainCharacter found: {player != null}");

        if (player != null)
        {
            Debug.Log($"[IdleEnemy] Calling MakeAngry(true, {collision.name})");
            MakeAngry(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (angry)
            return;

        // ИГНОРИРУЕМ ВРАГОВ - проверка по слою Enemy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        // ИГНОРИРУЕМ ВРАГОВ - проверка по компоненту IdleEnemy
        if (collision.gameObject.GetComponent<IdleEnemy>() != null)
            return;

        if (collision.gameObject.TryGetComponent<HP>(out HP victim))
        {
            victim.TakeDamage(float.PositiveInfinity, this.gameObject);
        }
    }

#if UNITY_EDITOR
    [SerializeField, HideInInspector] private Vector3 startPosition;
    private Color gizmosColor = new Color(0, 0, 1, 0.5f);
    private void OnValidate()
    {
        startPosition = transform.position + Vector3.up / 2;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmosColor;
        if (Application.isPlaying)
            Gizmos.DrawCube(startPosition, new Vector3(patroolRange * 2, 1, 1));
        else
            Gizmos.DrawCube(transform.position + Vector3.up / 2, new Vector3(patroolRange * 2, 1, 1));
    }
#endif
}