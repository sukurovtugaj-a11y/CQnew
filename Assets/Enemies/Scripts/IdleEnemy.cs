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
        lastAngerTime = Time.time;
    }

    public void MakeAngry(bool flag, GameObject damageSource)
    {
        if (isStarting) return;

        if (angry == flag)
            return;

        if (flag == true && Time.time - lastAngerTime < angerCooldown)
            return;

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

    // ВОТ ЗДЕСЬ ИСПРАВЛЕНИЕ: проверка на скрипт игрока
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<SecMainCharacter>() != null)
        {
            MakeAngry(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (angry)
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