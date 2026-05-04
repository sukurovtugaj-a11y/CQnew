using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AngerEnemyINT : AngerEnemy
{
    public float attackCooldown;
    public float delayBeforeAttack;
    public float minRangeToAttack;
    public float moveSpeed;
    public float stopingDistance;
    public TextMeshProUGUI commantText;
    public Animator animator;
    public float clubAttackRange = 3f;

    private float lastAttackTime;
    private float sqrtMinRangeToAttack;
    private float sqrtStopingDistance;
    private float sqrtClubAttackRange;

    private void Start()
    {
        float detectionRadius = GetDetectionRadius();
        minRangeToAttack = detectionRadius - 3f;

        sqrtMinRangeToAttack = minRangeToAttack * minRangeToAttack;
        sqrtStopingDistance = stopingDistance * stopingDistance;
        sqrtClubAttackRange = clubAttackRange * clubAttackRange;
    }

    private float GetDetectionRadius()
    {
        foreach (var col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
            {
                if (col is CircleCollider2D circle)
                    return circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                if (col is BoxCollider2D box)
                    return box.size.magnitude / 2f * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            }
        }
        Debug.LogWarning("Trigger collider not found on " + gameObject.name + "! Using default 20f.");
        return 20f;
    }

	private void Update()
	{
		if (target == null) return;

		float sqrtRange = Vector3.SqrMagnitude(target.position - (transform.position + Vector3.up * 2));

		if (sqrtRange > sqrtStopingDistance)
		{
			float speedScale = 1;
			if (target.position.x < transform.position.x)
				speedScale = -1;

			transform.position += speedScale * moveSpeed * Time.deltaTime * Vector3.right;
		}

		if (sqrtRange > sqrtMinRangeToAttack)
			return;

		if (Time.time - lastAttackTime < attackCooldown)
			return;

		if (sqrtRange <= sqrtClubAttackRange)
		{
			lastAttackTime = Time.time;
			AttackClub();
		}
		else
		{
			lastAttackTime = Time.time;
			if (Random.Range(0, 100) > 50)
				AttackLazer();
			else
				AttackRocket();
		}
	}

    private void AttackClub()
    {
        if (target.position.x < transform.position.x)
        {
            commantText.text = "195";
            StartCoroutine(DelayedAnimatorTrigger("leftClub", delayBeforeAttack));
        }
        else
        {
            commantText.text = "100";
            StartCoroutine(DelayedAnimatorTrigger("rightClub", delayBeforeAttack));
        }
    }

    private void AttackLazer()
    {
        if (target.position.x < transform.position.x)
        {
            commantText.text = "5";
            StartCoroutine(DelayedAnimatorTrigger("leftLazer", delayBeforeAttack));
        }
        else
        {
            commantText.text = "10";
            StartCoroutine(DelayedAnimatorTrigger("rightLazer", delayBeforeAttack));
        }
    }

    private void AttackRocket()
    {
        if (target.position.x < transform.position.x)
        {
            commantText.text = "18";
            StartCoroutine(DelayedAnimatorTrigger("leftRocket", delayBeforeAttack));
        }
        else
        {
            commantText.text = "15";
            StartCoroutine(DelayedAnimatorTrigger("rightRocket", delayBeforeAttack));
        }
    }

    private IEnumerator DelayedAnimatorTrigger(string trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(trigger);
    }
}
