using UnityEngine;
using System.Collections;

public class OPollMover : MonoBehaviour
{
    public Transform leftPoll;
    public Transform rightPoll;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public float delayBeforeMove = 1f;
    public float delayBeforeReturn = 2f;
    public float returnSpeed = 2f;

    private bool isMoving;
    private bool isReturning;
    private float t;
    private Vector3 leftStart;
    private Vector3 rightStart;

    public void StartMoving()
    {
        if (leftPoll != null) leftStart = leftPoll.position;
        if (rightPoll != null) rightStart = rightPoll.position;
        StartCoroutine(DelayedMove());
    }

    private IEnumerator DelayedMove()
    {
        yield return new WaitForSeconds(delayBeforeMove);
        isMoving = true;
        t = 0f;
    }

    private IEnumerator DelayedReturn()
    {
        yield return new WaitForSeconds(delayBeforeReturn);
        isReturning = true;
        isMoving = false;
        t = 0f;
    }

    void Update()
    {
        if (isReturning)
        {
            t += Time.deltaTime * returnSpeed;
            if (t > 1f) { isReturning = false; return; }

            float progress = t;
            if (leftPoll != null)
                leftPoll.position = Vector3.Lerp(leftPoll.position, leftStart, progress);
            if (rightPoll != null)
                rightPoll.position = Vector3.Lerp(rightPoll.position, rightStart, progress);
        }
        else if (isMoving)
        {
            t += Time.deltaTime * moveSpeed;
            if (t > 1f)
            {
                isMoving = false;
                StartCoroutine(DelayedReturn());
                return;
            }

            if (leftPoll != null)
                leftPoll.position += Vector3.left * (moveDistance * Time.deltaTime * moveSpeed);
            if (rightPoll != null)
                rightPoll.position += Vector3.right * (moveDistance * Time.deltaTime * moveSpeed);
        }
    }
}