using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AngerEnemyDOUBLE : AngerEnemy
{
	public float timeToAim;
	public float attackCooldown;
	public float delayBeforeAttack;
	public float minRangeToAttack;
	public float moveSpeed;
	public float stopingDistance;
	public float fireAttackMinRange = 12;
	public float chargeStopingDistance;
	public TextMeshProUGUI commantText;
	public Animator animator;
	public HP hp;
	public GameObject floatPrefab;
	public Transform floatPrefabSpawnPoint;
	public GameObject explosionVFX;
	public Transform lazerGun;

	private float lowHPLevel = 0;
	private float lastAttackTime;
	private float sqrtMinRangeToAttack;
	private float sqrtStopingDistance;
	private float sqrtFireAttackMinRange;

	private void Start()
	{
		sqrtMinRangeToAttack = minRangeToAttack * minRangeToAttack;
		sqrtStopingDistance = stopingDistance * stopingDistance;
		sqrtFireAttackMinRange = fireAttackMinRange * fireAttackMinRange;

		lowHPLevel = hp.maxHP * 0.3f;
		hp.onChangeHp.AddListener(LowHPCheck);
	}

	public void LowHPCheck(float currentHP)
	{
		if(currentHP <= lowHPLevel)
		{
			Instantiate(floatPrefab, floatPrefabSpawnPoint.position, Quaternion.identity);
			Instantiate(explosionVFX, transform.position, Quaternion.identity);

			Destroy(this.gameObject);
		}
	}

	private void Update()
	{
		float sqrtRange = Vector3.SqrMagnitude(target.position - (transform.position + Vector3.up * 2));

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
		else if (sqrtRange < sqrtFireAttackMinRange)
			AttackFire();
		else
			StartCoroutine(AttackLazer());
	}
	private IEnumerator AttackCharge()
	{
		Vector3 moveVector = Vector3.right;
		if (target.position.x < transform.position.x)
		{
			commantText.text = "8.7";
			moveVector = Vector3.left;
		}
		else
			commantText.text = "14.95";

		Vector3 a = transform.position;
		Vector3 b;

		for (float t = 0; t < 1; t += (Time.deltaTime * 2 + Time.deltaTime * 2 * t))
		{
			b = target.position - moveVector * chargeStopingDistance;
			b.y = a.y;
			transform.position = Vector3.Lerp(a,b,t);

			yield return null;
		}

		transform.position = target.position - moveVector * chargeStopingDistance;
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
	}

	private void AttackFire()
	{
		commantText.text = "1";
		StartCoroutine(DelayedAnimatorTrigger("fire", delayBeforeAttack));
	}

	private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
	{
		yield return new WaitForSeconds(delay);
		animator.SetTrigger(trigger);
	}
}
