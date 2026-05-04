using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AngerEnemyCHAR : AngerEnemy
{
	public LineRenderer line;
	public float attackCooldown;
	public float delayBeforeAttack;
	public float minRangeToAttack;
	public float moveSpeed;
	public Animator animator;
	public float stopingDistance;
	public float chargeDamage;
	public float chargeStopingDistance;
	public GameObject explosionVFX;
	public Gun gun;
	public Vector3[] pointsO;
	public Vector3[] pointsB;

	private Vector3[] pointsI = new Vector3[10];
	private float lastAttackTime;
	private float sqrtMinRangeToAttack;
	private float sqrtStopingDistance;
	private bool onCharge = false;

	private void Start()
	{
		sqrtMinRangeToAttack = minRangeToAttack * minRangeToAttack;
		sqrtStopingDistance = stopingDistance * stopingDistance;

		line.GetPositions(pointsI);
	}

	private void Update()
	{
		if (onCharge)
			return;

		//if (target == null)
		//	return;

		float sqrtRange = Vector3.SqrMagnitude(target.position - transform.position);

		if (sqrtRange > sqrtStopingDistance)
		{
			float speedScale = 1;
			if (target.position.x < transform.position.x)
				speedScale = -1;

			transform.position += speedScale * moveSpeed * Time.deltaTime * Vector3.right;
		}

		if (Time.time - lastAttackTime < attackCooldown)
			return;

		lastAttackTime = Time.time;

		if (sqrtRange > sqrtMinRangeToAttack)
			StartCoroutine(AttackCharge());
		else
		{
			if(transform.position.x < target.position.x)
				StartCoroutine(AttackGun());
			else
				StartCoroutine(AttackFly());
		}
	}

	private IEnumerator AttackCharge()
	{
		onCharge = true;
		yield return StartCoroutine(GoToForm(pointsO));


		float animSpeed = 1;
		if (target.position.x > transform.position.x)
			animSpeed = -1;

		animator.SetFloat("rotation speed", animSpeed);
		animator.SetBool("rotation", true);
		yield return StartCoroutine(ChargeToTarget());  
		animator.SetBool("rotation", false);
		target.GetComponent<HP>().TakeDamage(chargeDamage, this.gameObject);

		yield return StartCoroutine(GoToForm(pointsI));
		onCharge = false;
	}

	private IEnumerator AttackGun()
	{
		onCharge = true;
		yield return StartCoroutine(GoToForm(pointsB));

		yield return new WaitForSeconds(0.5f);
		gun.Shoot(target.position + Vector3.up);

		yield return StartCoroutine(GoToForm(pointsI));
		onCharge = false;
		lastAttackTime = Time.time;
	}

	private IEnumerator AttackFly()
	{
		onCharge = true;
		animator.SetTrigger("ZH");
		yield return new WaitForSeconds(0.5f);

		yield return StartCoroutine(ChargeToTarget());
		target.GetComponent<HP>().TakeDamage(chargeDamage, this.gameObject);

		yield return StartCoroutine(GoToForm(pointsI));
		animator.SetTrigger("ZH reverce");
		onCharge = false;
		lastAttackTime = Time.time;
	}

	private IEnumerator ChargeToTarget()
	{
		Vector3 moveVector = Vector3.right;
		if (target.position.x < transform.position.x)
			moveVector = Vector3.left;

		Vector3 a = transform.position;
		Vector3 b;

		for (float t = 0; t < 1; t += (Time.deltaTime * 2 + Time.deltaTime * 2 * t))
		{
			b = target.position - moveVector * chargeStopingDistance;
			b.y = a.y;
			transform.position = Vector3.Lerp(a, b, t);

			yield return null;
		}

		transform.position = target.position - moveVector * chargeStopingDistance;
		lastAttackTime = Time.time;
	}

	private IEnumerator GoToForm(Vector3[] targetForm)
	{
		int pointsCount = line.positionCount;
		Vector3[] pointsA = new Vector3[pointsCount];
		line.GetPositions(pointsA);
		Vector3[] pointsB = new Vector3[pointsCount];

		for (float t = 0; t < 1; t += Time.deltaTime * 2)
		{
			for (int i = 0; i < pointsCount; i++)
				pointsB[i] = Vector3.Lerp(pointsA[i], targetForm[i], t);

			line.SetPositions(pointsB);

			yield return null;
		}

		line.SetPositions(targetForm);
	}
}
