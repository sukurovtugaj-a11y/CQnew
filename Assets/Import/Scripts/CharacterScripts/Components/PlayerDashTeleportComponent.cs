using System.Collections;
using UnityEngine;

public class PlayerDashTeleportComponent
{
    private readonly SecMainCharacter owner;
    private readonly Rigidbody2D rb;

    public bool IsDashing { get; private set; }
    public bool CanDash => owner.canDash && !owner.isSliding && !IsDashing;

    public PlayerDashTeleportComponent(SecMainCharacter owner, Rigidbody2D rb)
    {
        this.owner = owner;
        this.rb = rb;
    }

    public void TryDashOrTeleport()
    {
        if (!CanDash) return;

        var hits = Physics2D.OverlapCircleAll(owner.transform.position, owner.teleportRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("TeleportObj") && IsPlayerOnSide(hit) && IsPlayerFacingObject(hit.bounds))
            {
                Teleport(hit.bounds);
                return;
            }
        }
        owner.StartCoroutine(DoDash());
    }

    private bool IsPlayerFacingObject(Bounds objBounds)
    {
        bool facingRight = owner.transform.localScale.x < 0;
        float objCenterX = objBounds.center.x;
        return facingRight ? objCenterX > owner.transform.position.x : objCenterX < owner.transform.position.x;
    }

    private bool IsPlayerOnSide(Collider2D obj)
    {
        Bounds b = obj.bounds;
        return (owner.transform.position.x < b.min.x || owner.transform.position.x > b.max.x) &&
               (owner.transform.position.y >= b.min.y && owner.transform.position.y <= b.max.y);
    }

    private void Teleport(Bounds objBounds)
    {
        float newX = owner.transform.position.x < objBounds.min.x
            ? objBounds.max.x + owner.teleportOffset
            : objBounds.min.x - owner.teleportOffset;
        owner.transform.position = new Vector3(newX, owner.transform.position.y, owner.transform.position.z);
    }

    private IEnumerator DoDash()
    {
        IsDashing = true;
        owner.canDash = false;
        owner.isDashing = true;

        float dir = owner.transform.localScale.x < 0 ? 1f : -1f;
        Vector2 startPos = rb.position;
        Vector2 direction = new Vector2(dir, 0);

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, owner.dashDistance,
            LayerMask.GetMask("Ground", "Platform"));

        float actualDistance = owner.dashDistance;
        if (hit.collider != null && hit.distance > 0.1f)
        {
            actualDistance = hit.distance - 0.1f;
            if (actualDistance < 0.1f) actualDistance = 0.1f;
        }

        Vector2 targetPos = startPos + direction * actualDistance;
        float elapsed = 0f;
        float savedYVel = rb.velocity.y;

        while (elapsed < owner.dashDuration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsed / owner.dashDuration));
            rb.velocity = new Vector2(0, savedYVel);
            yield return null;
        }

        rb.MovePosition(targetPos);
        IsDashing = false;
        owner.isDashing = false;

        yield return new WaitForSeconds(owner.dashCooldown);
        owner.canDash = true;
    }
}
