using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("��������� ������")]
    [Tooltip("���� ������� �����")]
    public float bounceForce = 15f;

    [Tooltip("��������� �������������� �������� (0 = ������ �����, 1 = ���������� �������)")]
    [Range(0f, 1f)]
    public float horizontalRetention = 0.5f;

    [Tooltip("���������� ������ (�����������)")]
    public GameObject bounceEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MonoBehaviour player = collision.GetComponent<SecMainCharacter>();
        if (player == null) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (rb.velocity.y >= 0) return;

        Vector2 newVelocity = new Vector2(rb.velocity.x * horizontalRetention, bounceForce);
        rb.velocity = newVelocity;

        if (bounceEffect != null)
            Instantiate(bounceEffect, transform.position, Quaternion.identity);

        Debug.Log($"[?????] ??????! ????: {bounceForce}");
    }
}