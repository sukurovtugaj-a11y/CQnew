using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public float damage;
    public float knockbackDistance = 1f;
    public bool work = false;
    public bool playerOnly = true;

    public void SetWork(bool flag)
    {
        work = flag;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!work)
            return;

        var player = collision.gameObject.GetComponentInParent<SecMainCharacter>();

        if (playerOnly)
        {
            if (player == null)
                return;
        }

        if (player != null)
        {
            player.Damage(damage, transform);

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

            // Инверсия: вектор от игрока к врагу = притягивание
            Vector3 knockbackDir = (transform.position - player.transform.position).normalized;

            Vector2 knockbackPos = new Vector2(
                player.transform.position.x + knockbackDir.x * knockbackDistance,
                player.transform.position.y + knockbackDir.y * knockbackDistance
            );

            if (playerRb != null)
                playerRb.MovePosition(knockbackPos);
            else
                player.transform.position = knockbackPos;

            return;
        }

        HP victim = collision.gameObject.GetComponentInParent<HP>();
        if (victim != null)
        {
            victim.TakeDamage(damage, this.gameObject);

            Rigidbody2D victimRb = victim.GetComponent<Rigidbody2D>();

            // Инверсия: вектор от жертвы к источнику = притягивание
            Vector3 knockbackDir = (transform.position - victim.transform.position).normalized;

            Vector2 knockbackPos = new Vector2(
                victim.transform.position.x + knockbackDir.x * knockbackDistance,
                victim.transform.position.y + knockbackDir.y * knockbackDistance
            );

            if (victimRb != null)
                victimRb.MovePosition(knockbackPos);
            else
                victim.transform.position = knockbackPos;
        }
    }
}