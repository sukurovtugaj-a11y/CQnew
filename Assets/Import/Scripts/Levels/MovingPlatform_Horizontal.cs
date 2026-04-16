using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingPlatform_Horizontal : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Включить перемещение платформы")]
    public bool movePlatform = true;

    public float moveSpeed = 2f;
    public float moveDistance = 3f;

    [Header("Настройки вращения")]
    [Tooltip("Включить вращение платформы (независимо от движения)")]
    public bool rotatePlatform = false;

    public float rotateSpeed = 50f;
    public RotationDirection rotationDirection = RotationDirection.Clockwise;

    [Header("Точки")]
    public Transform startPoint;
    public Transform endPoint;

    public enum RotationDirection { Clockwise, CounterClockwise }

    /// <summary>Текущая скорость платформы (вычисляется в FixedUpdate)</summary>
    public Vector2 Velocity { get; private set; }

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float moveTimer = 0f;
    private float rotateTimer = 0f;
    private Vector3 prevPosition;

    private void Start()
    {
        startPosition = startPoint != null ? startPoint.position : transform.position;
        endPosition = endPoint != null ? endPoint.position : startPosition + Vector3.right * moveDistance;
        prevPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (movePlatform)
        {
            moveTimer += Time.fixedDeltaTime * moveSpeed;
            float movement = Mathf.PingPong(moveTimer, moveDistance);
            float progress = movement / moveDistance;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
        }

        if (rotatePlatform)
        {
            rotateTimer += Time.fixedDeltaTime;
            float direction = rotationDirection == RotationDirection.Clockwise ? 1f : -1f;
            transform.Rotate(0, 0, rotateSpeed * Time.fixedDeltaTime * direction);
        }

        // Вычисляем velocity платформы
        Velocity = (Vector2)(transform.position - prevPosition) / Time.fixedDeltaTime;
        prevPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        if (movePlatform)
        {
            Gizmos.color = Color.green;
            Vector3 start = startPoint != null ? startPoint.position : transform.position;
            Vector3 end = startPoint != null ? endPoint.position : start + Vector3.right * moveDistance;
            Gizmos.DrawLine(start, end);
        }
    }
}
