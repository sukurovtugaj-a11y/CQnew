using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AngerEnemyFLOAT : AngerEnemy
{
	public float timeToAim;
	public float attackCooldown;
	public float delayBeforeAttack;
	public float minRangeToAttack;
	public float moveSpeed;
	public float stopingDistance;
	public float chargeStopingDistance;
	public TextMeshProUGUI commantText;
	public Animator animator;
	public GameObject explosionVFX;
	public Transform lazerGun;

	private float lastAttackTime;
	private float sqrtMinRangeToAttack;
	private float sqrtStopingDistance;
	private bool onCharge = false;
	private Vector3 nativePosition;

	private void Start()
	{
		sqrtMinRangeToAttack = minRangeToAttack * minRangeToAttack;
		sqrtStopingDistance = stopingDistance * stopingDistance;

		nativePosition = transform.position;
	}

	public override void TurnOn(Transform target)
	{
		base.TurnOn(target);

		nativePosition = transform.position;
		StartCoroutine(FirstCharge());
	}

	private IEnumerator FirstCharge()
	{
		onCharge = true;

		for (float t = 0; t < 1; t += Time.deltaTime * 2)
		{
			transform.position = Vector3.Lerp(nativePosition, 
				nativePosition + new Vector3(Mathf.Cos(Time.time) * 5, Mathf.Sin(Time.time * 2f), 0),
				t);

			yield return null;
		}

		onCharge = false;
	}

	private void Update()
	{
		if (onCharge)
			return;

		float sqrtRange = Vector3.SqrMagnitude(target.position - transform.position);

		if (sqrtRange > sqrtStopingDistance)
		{
			float speedScale = 1;
			if (target.position.x < transform.position.x)
				speedScale = -1;

			nativePosition += speedScale * moveSpeed * Time.deltaTime * Vector3.right;

			if (target.position.y + 4 < nativePosition.y)
				nativePosition += Vector3.down * moveSpeed * Time.deltaTime;
			else if (target.position.y + 2 > nativePosition.y)
				nativePosition += Vector3.up * moveSpeed * Time.deltaTime;
		}


		transform.position = nativePosition + new Vector3(Mathf.Cos(Time.time) * 5, Mathf.Sin(Time.time * 2f), 0);

		if (Time.time - lastAttackTime < attackCooldown)
			return;

		lastAttackTime = Time.time;

		//if (sqrtRange > sqrtMinRangeToAttack)
		//	StartCoroutine(AttackCharge());
		//else
			StartCoroutine(AttackLazer());
	}

	private IEnumerator AttackCharge()
	{
		yield return null;
	}

	private IEnumerator AttackLazer()
	{
		commantText.text = "6.875";

		Vector2 direction;
		float targetAngle;
		float startAngle = lazerGun.eulerAngles.z;

		for (float t = 0; t < 1; t += Time.deltaTime / timeToAim)
		{
			direction = target.position - lazerGun.position + Vector3.up;
			targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
			float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
			lazerGun.rotation = Quaternion.Euler(0, 0, currentAngle);

			yield return null;
		}

		direction = target.position - lazerGun.position + Vector3.up;
		targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
		lazerGun.rotation = Quaternion.Euler(0, 0, targetAngle);

		StartCoroutine(DelayedAnimatorTrigger("lazer", delayBeforeAttack));

		for (float t = 0; t < 0.75f; t += Time.deltaTime)
		{
			direction = target.position - lazerGun.position + Vector3.up;
			targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
			lazerGun.rotation = Quaternion.Euler(0, 0, targetAngle);

			yield return null;
		}
	}

	private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
	{
		yield return new WaitForSeconds(delay);
		animator.SetTrigger(trigger);
	}
}
