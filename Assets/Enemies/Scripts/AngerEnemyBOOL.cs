using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngerEnemyBOOL : AngerEnemy
{
	public Animator anim;
	public float runSpeed;
	public float collisionDamage;
	public float waitBeforeRunTime;
	public float bounceTime;
	public float huntTime;
	public float rangeToLooseTarget;
	public IdleEnemy idleBehavior;

	private bool run = false;
	private float sqrtRangeToLooseTarget;
	private float huntStartTime;

	private void Start()
	{
		sqrtRangeToLooseTarget = rangeToLooseTarget * rangeToLooseTarget;
	}

	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
	}

	public override void TurnOn(Transform target)
	{
		base.TurnOn(target);

		anim.SetBool("state", true);
		StartCoroutine(WaitAndRun(waitBeforeRunTime));
		huntStartTime = Time.time;
	}

	private IEnumerator LooseTarget()
	{
		run = false;
		anim.SetBool("state", false);
		yield return new WaitForSeconds(waitBeforeRunTime);
		idleBehavior.MakePeace();
		idleBehavior.enabled = true;
		this.enabled = false;
	}

	private IEnumerator WaitAndRun(float time)
	{
		run = false;
		yield return new WaitForSeconds(time);
		run = true;
	}

	private void Update()
	{
		if (!run)
			return;

		if (Vector3.SqrMagnitude(target.position - transform.position) > sqrtRangeToLooseTarget ||
				Time.time - huntStartTime > huntTime)
		{
			StartCoroutine(LooseTarget());
			return;
		}

		float speedScale = 1;
		if (target.position.x < transform.position.x)
			speedScale = -1;

		transform.position += speedScale * runSpeed * Time.deltaTime * Vector3.right;
	}

	private IEnumerator BounceOut(float time, bool isRight)
	{
		float scale = 1;
		if(!isRight)
			scale = -1;

		run = false;
		for(float t = 0; t < 1; t += Time.deltaTime / time)
		{
			transform.position += scale * runSpeed * (1.25f - t) * Time.deltaTime * Vector3.right;
			yield return null;
		}

		run = true;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!run)
			return;

		if(collision.gameObject.TryGetComponent<HP>(out HP victim))
		{
			victim.TakeDamage(collisionDamage, this.gameObject);
			StartCoroutine(BounceOut(bounceTime, transform.position.x > target.position.x));
		}
	}
}