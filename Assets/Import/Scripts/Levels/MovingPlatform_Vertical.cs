using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MovingPlatform_Vertical : MonoBehaviour
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

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float moveTimer = 0f;
    private float rotateTimer = 0f;

    public Vector2 PlatformVelocity { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true; // Вращаем вручную через transform
    }

    private void Start()
    {
        startPosition = startPoint != null ? startPoint.position : transform.position;
        endPosition = endPoint != null ? endPoint.position : startPosition + Vector3.up * moveDistance;
    }

    private void FixedUpdate()
    {
        // === ДВИЖЕНИЕ (независимый блок, только если включено) ===
        if (movePlatform)
        {
            Vector3 prevPos = transform.position;

            moveTimer += Time.fixedDeltaTime * moveSpeed;
            float movement = Mathf.PingPong(moveTimer, moveDistance);
            float progress = movement / moveDistance;

            rb.MovePosition(Vector3.Lerp(startPosition, endPosition, progress));

            PlatformVelocity = (Vector2)(transform.position - prevPos) / Time.fixedDeltaTime;
        }
        else
        {
            PlatformVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        // === ВРАЩЕНИЕ (независимый блок, работает всегда в Update) ===
        if (rotatePlatform)
        {
            rotateTimer += Time.deltaTime;
            float direction = rotationDirection == RotationDirection.Clockwise ? 1f : -1f;
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime * direction);
        }
    }

    private void OnDrawGizmos()
    {
        if (movePlatform)
        {
            Gizmos.color = Color.green;
            Vector3 start = startPoint != null ? startPoint.position : transform.position;
            Vector3 end = startPoint != null ? endPoint.position : start + Vector3.up * moveDistance;
            Gizmos.DrawLine(start, end);
        }
    }
}