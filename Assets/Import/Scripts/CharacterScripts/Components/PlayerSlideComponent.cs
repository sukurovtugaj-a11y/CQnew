using System.Collections;
using UnityEngine;

public class PlayerSlideComponent
{
    private readonly SecMainCharacter owner;
    private readonly Rigidbody2D rb;

    public bool IsActive => owner.isSliding;

    public PlayerSlideComponent(SecMainCharacter owner, Rigidbody2D rb)
    {
        this.owner = owner;
        this.rb = rb;
    }

    public void HandleSlide()
    {
        owner.horizontalInput = 0;

        if (owner.slideType == "Slope")
        {
            float spd = owner.slideSpeed;
            rb.velocity = new Vector2(owner.slideDirection.x * spd, owner.slideDirection.y * spd);

            if (Input.GetKey(owner.keys.JumpButton)) { StopSlide(); return; }
            if (!IsSlidingOnSurface()) StopSlide();
            return;
        }

        if (owner.slideType == "Special")
        {
            float sDir = owner.transform.localScale.x < 0 ? 1f : -1f;
            rb.velocity = new Vector2(sDir * owner.zoneSlideSpeed, rb.velocity.y);
            if (owner.currentSlideZone == null) StopSlide();
            return;
        }

        owner.slideTimer -= Time.fixedDeltaTime;
        float dir = owner.transform.localScale.x < 0 ? 1f : -1f;
        rb.velocity = new Vector2(dir * owner.slideSpeed, rb.velocity.y);

        if (owner.slideType == "Normal")
        {
            if (Input.GetKey(owner.keys.JumpButton)) { StopSlide(); return; }
            if (owner.slideTimer <= 0)
            {
                if (owner.currentSlideZone != null)
                    SwitchToSpecialSlide();
                else
                    StopSlide();
            }
        }
    }

    public void StartSlide(string type, float duration, Vector2 direction, float speed = -1f, bool limitDir = false, bool sRight = true, bool sLeft = true, float angle = 0f)
    {
        if (owner.isSliding) return;
        owner.isSliding = true;
        owner.slideType = type;
        owner.slideTimer = duration;
        owner.slideDirection = direction;
        if (type == "Slope" && speed > 0) { owner.savedSlideSpeed = owner.slideSpeed; owner.slideSpeed = speed; }
        if (type == "Special") { owner.savedSlideSpeed = owner.slideSpeed; owner.slideSpeed = owner.zoneSlideSpeed; }
        if (type == "Normal") owner.savedSlideSpeed = owner.slideSpeed;
        if (type == "Slope")
        {
            Debug.Log($"[Slope] Angle: {angle}, Direction: {direction}, slideRight: {sRight}, slideLeft: {sLeft}");
            owner.controlLockTimer = 999f;
            owner.slopeLimitDir = limitDir;
            owner.slopeSlideRight = sRight;
            owner.slopeSlideLeft = sLeft;
            owner.slopeAngle = angle;

            owner.savedScaleX = owner.transform.localScale.x;
            var s = owner.transform.localScale;
            s.x = Mathf.Abs(s.x);
            owner.transform.localScale = s;
        }
        owner.currentState = SecMainCharacter.PlayerState.Sliding;
        owner.playerCollider.enabled = false;
        UpdateSlideSprite(true);
    }

    public void StopSlide()
    {
        if (!owner.isSliding) return;
        owner.isSliding = false;
        owner.controlLockTimer = owner.slideType == "Slope" ? 0.5f : 0f;

        if (owner.slideType == "Slope")
        {
            var s = owner.transform.localScale;
            s.x = owner.savedScaleX;
            owner.transform.localScale = s;

            var slideSR = owner.slideSprite != null ? owner.slideSprite.GetComponent<SpriteRenderer>() : null;
            if (slideSR != null) { slideSR.flipX = false; slideSR.flipY = false; }
        }

        owner.playerCollider.enabled = true;
        owner.currentSlideZone = null;
        UpdateSlideSprite(false);
        if (owner.slideSprite != null) owner.slideSprite.transform.localPosition = Vector3.zero;
        owner.slideSpeed = owner.savedSlideSpeed;
        if (IsGrounded()) owner.currentState = SecMainCharacter.PlayerState.Grounded;
    }

    public void EnterSlideZone(float speed, SlideZone zone)
    {
        owner.zoneSlideSpeed = speed;
        owner.currentSlideZone = zone;
    }

    public void SwitchToSpecialSlide()
    {
        if (!owner.isSliding || owner.slideType == "Special") return;
        owner.slideType = "Special";
        owner.savedSlideSpeed = owner.slideSpeed;
        owner.slideSpeed = owner.zoneSlideSpeed;
    }

    public void ExitSlideZone(SlideZone zone)
    {
        if (owner.currentSlideZone != zone) return;
        owner.currentSlideZone = null;
    }

    private bool IsGrounded() => rb.GetContacts(owner.groundFilter, owner.groundContacts) > 0;

    private bool IsSlidingOnSurface()
    {
        var hits = Physics2D.OverlapCircleAll(owner.transform.position, 0.3f, LayerMask.GetMask("Ground", "Platform"));
        foreach (var hit in hits)
        {
            if (hit.GetComponent<SlopeSurface>() != null) return true;
        }
        return false;
    }

    public IEnumerator HandleSlideInput()
    {
        if (!owner.isSliding && owner.CanSlide && IsGrounded() && Input.GetKeyDown(owner.keys.MoveDown))
        {
            StartSlide("Normal", owner.normalSlideDuration, Vector2.zero);
        }
        yield return null;
    }

    private void UpdateSlideSprite(bool sliding)
    {
        if (owner.slideSprite == null) return;
        owner.slideSprite.SetActive(sliding);
        if (owner.idleAnimation != null) owner.idleAnimation.SetActive(!sliding);
        if (owner.runAnimation != null) owner.runAnimation.SetActive(false);

        if (sliding)
        {
            float offset = owner.slideType == "Slope" ? owner.slideSpriteOffset * 1.8f : owner.slideSpriteOffset;
            owner.slideSprite.transform.localPosition = Vector3.down * offset;
            if (owner.slideType == "Slope")
            {
                bool wasFacingLeft = owner.savedScaleX > 0;
                owner.slideSprite.transform.localRotation = Quaternion.Euler(0, 0, wasFacingLeft ? -owner.slopeAngle : -owner.slopeAngle - 180f);

                var slideSR = owner.slideSprite.GetComponent<SpriteRenderer>();
                if (slideSR != null)
                {
                    slideSR.flipX = false;
                    slideSR.flipY = false;
                    if (owner.slopeSlideRight && !owner.slopeSlideLeft)
                    {
                        if (owner.savedScaleX > 0) slideSR.flipX = true;
                        else slideSR.flipY = true;
                    }
                    else if (owner.slopeSlideLeft && !owner.slopeSlideRight)
                    {
                        if (owner.savedScaleX <= 0) { slideSR.flipX = true; slideSR.flipY = true; }
                    }
                }
            }
        }
        else
        {
            owner.slideSprite.transform.localPosition = Vector3.zero;
            owner.slideSprite.transform.localRotation = Quaternion.identity;
        }
    }
}