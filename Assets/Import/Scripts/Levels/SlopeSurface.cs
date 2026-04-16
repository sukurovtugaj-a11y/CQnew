using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlopeSurface : MonoBehaviour
{
    [Header("Скольжение")]
    public float slideSpeed = 5f;
    public bool limitDirection = true;
    public bool slideRight = true;
    public bool slideLeft = true;

    [Header("Отладка")]
    public bool showGizmos = true;

    public Vector2 SlideDirection { get; private set; }

    private void OnDrawGizmos()
    {
        if (showGizmos && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)SlideDirection * 2f);
            Gizmos.DrawSphere(transform.position + (Vector3)SlideDirection * 2f, 0.2f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var mc = collision.gameObject.GetComponent<SecMainCharacter>();
        if (mc != null && !mc.IsSliding && collision.contactCount > 0)
        {
            var normal = collision.contacts[0].normal;
            Vector2 alongSlope = new Vector2(normal.y, -normal.x).normalized;

            // Выбираем направление: вниз по склону (y < 0), с учётом slideRight/slideLeft
            Vector2 dir1 = alongSlope;
            Vector2 dir2 = -alongSlope;

            if (limitDirection)
            {
                bool onlyRight = slideRight && !slideLeft;
                bool onlyLeft = slideLeft && !slideRight;

                if (onlyRight)
                    SlideDirection = dir1.x > 0 ? dir1 : dir2;
                else if (onlyLeft)
                    SlideDirection = dir1.x < 0 ? dir1 : dir2;
                else
                    SlideDirection = dir1.y < 0 ? dir1 : dir2;
            }
            else
            {
                SlideDirection = dir1.y < 0 ? dir1 : dir2;
            }

            float angle = -Mathf.Atan2(-normal.x, normal.y) * Mathf.Rad2Deg + 180f;
            mc.StartSlide("Slope", 0, SlideDirection, slideSpeed, limitDirection, slideRight, slideLeft, angle);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var mc = collision.gameObject.GetComponent<SecMainCharacter>();
        if (mc != null && mc.IsSliding && mc.GetColliderEnabled()) mc.StopSlide();
    }

}
