using UnityEngine;

public class SideLauncher : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("���� ������� �����")]
    public float launchForce = 15f;

    [Tooltip("��������� ������������ �������� (0 = ������ �����, 1 = ���������� �������)")]
    [Range(0f, 1f)]
    public float verticalRetention = 0.5f;

    [Tooltip("�������� ����� ��������� ������������� (���)")]
    public float cooldown = 0.5f;

    [Tooltip("���������� ������ (�����������)")]
    public GameObject launchEffect;

    // ���������� ���������� ��� ��������
    private float lastTriggerTime = -999f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - lastTriggerTime < cooldown) return;

        MonoBehaviour player = collision.GetComponent<SecMainCharacter>();
        if (player == null) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.velocity = new Vector2(-launchForce, rb.velocity.y * verticalRetention);

        lastTriggerTime = Time.time;

        if (launchEffect != null)
            Instantiate(launchEffect, transform.position, Quaternion.identity);

        Debug.Log($"[SideLauncher] ???????? ?????! ????: {launchForce}");
    }
}