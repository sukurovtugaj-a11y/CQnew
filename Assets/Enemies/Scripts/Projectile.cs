using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float activateDelay;
    public bool playerOnly = true;
	public GameObject onHitVFX;
	public Vector3 flyVector = Vector3.zero;
	public Collider2D myCollider;

	public void SetFlyVector(Vector3 value)
	{
		flyVector = value;
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(activateDelay);
		myCollider.enabled = true;

		Destroy(gameObject, 5f);
	}

	public IEnumerator GoToTarget(Vector3 target)
	{
		Vector3 a = transform.position;
		for (float t = 0; t < 1; t += Time.deltaTime * 2)
		{
			transform.position = Vector3.Lerp(a, target, t);
			yield return null;
		}

		Destroy(gameObject);
	}

	private void Update()
	{
		transform.position += flyVector * Time.deltaTime;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ИГНОРИРУЕМ ВРАГОВ - проверка по слою Enemy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        // ИГНОРИРУЕМ ВРАГОВ - проверка по компонентам
        if (collision.GetComponentInParent<IdleEnemy>() != null ||
            collision.GetComponentInParent<AngerEnemy>() != null)
            return;

        // ИГНОРИРУЕМ TeleportObj по тегу
        if (collision.CompareTag("TeleportObj"))
            return;

        // Ищем игрока
        var player = collision.gameObject.GetComponentInParent<SecMainCharacter>();
        
        if (player != null)
        {
            if (!playerOnly) return;
            
            player.Damage(damage, this.gameObject.transform);
            
            if (onHitVFX != null)
                Instantiate(onHitVFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
            return;
        }

        // Если это не игрок, ищем HP (для врагов)
        HP victim = collision.gameObject.GetComponentInParent<HP>();
        if (victim != null)
        {
            if (playerOnly) return;

            // Дополнительная проверка: не является ли жертва врагом
            if (victim.GetComponentInParent<IdleEnemy>() != null ||
                victim.GetComponentInParent<AngerEnemy>() != null)
                return;
            
            victim.TakeDamage(damage, this.gameObject);

            if (onHitVFX != null)
                Instantiate(onHitVFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

	private void OnDestroy()
	{
		CancelInvoke();
	}
}
