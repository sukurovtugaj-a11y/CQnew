using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
	public Vector3 flyVector = Vector3.zero;
	public Projectile projectilePrefab;

	public void Shoot()
	{
		Instantiate(projectilePrefab, transform.position, Quaternion.identity).SetFlyVector(flyVector);
	}

	public void Shoot(Vector3 targetPoint)
	{
		Projectile proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
		

		proj.StartCoroutine(proj.GoToTarget(targetPoint + (targetPoint - transform.position)));
	}
}
