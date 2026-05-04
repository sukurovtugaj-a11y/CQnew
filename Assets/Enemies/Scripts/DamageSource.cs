using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
	public float damage;
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

		if (playerOnly)
			if (!collision.gameObject.CompareTag("Player"))
				return;

		if (collision.gameObject.TryGetComponent<HP>(out HP victim))
		{
			victim.TakeDamage(damage, this.gameObject);
		}
	}
}
