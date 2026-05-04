using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngerEnemy : MonoBehaviour
{
	protected Transform target;
	public virtual void TurnOn(Transform target)
	{
		this.enabled = true;
		this.target = target;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}
}
