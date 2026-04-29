using UnityEngine;

public class PlayerMovementComponent
{
    private readonly SecMainCharacter owner;
    private readonly Rigidbody2D rb;
    private MovingPlatform_Horizontal currentHPlatform;
    private Vector3 lastPlatformPos;

    public PlayerMovementComponent(SecMainCharacter owner, Rigidbody2D rb)
    {
        this.owner = owner;
        this.rb = rb;
        SetupGroundFilter();
    }

    // === INPUT ===
    public float HorizontalInput => owner.horizontalInput;

    public void UpdateInput()
    {
        if (owner.controlLockTimer > 0f)
        {
            owner.horizontalInput = 0;
            owner.isBoosting = false;
            return;
        }

        owner.horizontalInput = Input.GetKey(owner.keys.MoveFront) ? 1 : Input.GetKey(owner.keys.MoveBack) ? -1 : 0;

        if (owner.horizontalInput != 0)
        {
            Vector3 scale = owner.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (owner.horizontalInput > 0 ? -1 : 1);
            owner.transform.localScale = scale;
        }

        owner.isBoosting = Input.GetKey(owner.keys.boost);

        if (!owner.firstMoveMade && (owner.horizontalInput != 0 || Input.GetKey(owner.keys.JumpButton)))
        {
            owner.firstMoveMade = true;
            if (GameProgressManager.Instance != null) GameProgressManager.Instance.OnFirstMove();
        }
    }

    // === FIXEDUPDATE ===
    public void FixedUpdate()
    {
        bool isGrounded = IsGrounded();
        UpdateTimers(isGrounded);
        UpdateState(isGrounded);
        ApplyMovement();

        if (owner.currentPlatform == null && isGrounded && Mathf.Abs(owner.horizontalInput) > 0.01f)
            rb.AddForce(Vector2.down * owner.groundStickForce, ForceMode2D.Force);

        UpdateAnimations();
        CheckMovingPlatform(isGrounded);
    }

    // === GROUND CHECK ===
    public bool IsGrounded() => rb.GetContacts(owner.groundFilter, owner.groundContacts) > 0;

    // === PLATFORMS ===
    private void AttachToHorizontal(MovingPlatform_Horizontal platform)
    {
        owner.currentPlatform = platform;
        owner.isParentMode = true;
        currentHPlatform = platform;
        lastPlatformPos = platform.transform.position;
    }

    private void AttachToVertical(MovingPlatform_Vertical platform)
    {
        owner.currentPlatform = platform;
        owner.isParentMode = false;
        currentHPlatform = null;
    }

    public void DetachFromPlatform()
    {
        if (owner.currentPlatform != null)
        {
            if (owner.isParentMode) owner.transform.SetParent(null);
            owner.currentPlatform = null;
            owner.isParentMode = false;
            currentHPlatform = null;
        }
    }

    // === PRIVATE ===
    private void SetupGroundFilter()
    {
        owner.groundFilter = new ContactFilter2D();
        int groundLayer = LayerMask.NameToLayer("Ground");
        int platformLayer = LayerMask.NameToLayer("Platform");
        int mask = (platformLayer != -1) ? (1 << groundLayer) | (1 << platformLayer) : (1 << groundLayer);
        owner.groundFilter.SetLayerMask(mask);
        owner.groundFilter.useLayerMask = true;
        owner.groundFilter.useNormalAngle = true;
        owner.groundFilter.minNormalAngle = 45f;
        owner.groundFilter.maxNormalAngle = 135f;
    }

    private void UpdateTimers(bool isGrounded)
    {
        owner.coyoteTimeCounter = isGrounded ? owner.coyoteTime : owner.coyoteTimeCounter - Time.fixedDeltaTime;
        if (owner.jumpBufferCounter > 0) owner.jumpBufferCounter -= Time.deltaTime;
    }

    private void UpdateState(bool isGrounded)
    {
        if (owner.currentState == SecMainCharacter.PlayerState.Grounded && !isGrounded)
            owner.currentState = SecMainCharacter.PlayerState.Airborne;
        if (owner.currentState == SecMainCharacter.PlayerState.Airborne && isGrounded)
        {
            owner.currentState = SecMainCharacter.PlayerState.Grounded;
            owner.extraJumpsLeft = owner.maxExtraJumps;
        }
    }

    private void ApplyMovement()
    {
        if (owner.isDashing) return;

        float speed = owner.moveSpeed * (owner.isBoosting ? 2f : 1f);
        float control = 1f;

        if (owner.currentState == SecMainCharacter.PlayerState.Airborne) { speed *= owner.airControl; control = owner.airControl; }

        if (owner.isParentMode && currentHPlatform != null)
        {
            Vector2 platVel = currentHPlatform.Velocity;
            rb.velocity = platVel + new Vector2(owner.horizontalInput * speed, rb.velocity.y);
        }
        else
        {
            Vector2 targetVelocity = new Vector2(owner.horizontalInput * speed, rb.velocity.y);
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, control);
        }
    }

    private void CheckMovingPlatform(bool isGrounded)
    {
        if (!isGrounded) { DetachFromPlatform(); return; }

        if (owner.currentPlatform != null)
        {
            if (!IsActuallyOnTop(owner.currentPlatform.GetComponent<Collider2D>()))
                DetachFromPlatform();
            else if (!owner.isParentMode && owner.currentPlatform is MovingPlatform_Vertical vert)
                rb.velocity += vert.PlatformVelocity;
            return;
        }

        for (int i = 0; i < owner.groundContacts.Length; i++)
        {
            if (owner.groundContacts[i] == null) continue;

            var horiz = owner.groundContacts[i].GetComponent<MovingPlatform_Horizontal>();
            if (horiz != null && IsActuallyOnTop(owner.groundContacts[i])) { AttachToHorizontal(horiz); return; }

            var vert = owner.groundContacts[i].GetComponent<MovingPlatform_Vertical>();
            if (vert != null && IsActuallyOnTop(owner.groundContacts[i])) { AttachToVertical(vert); return; }
        }
    }

    private bool IsActuallyOnTop(Collider2D platCollider)
    {
        float playerFeetY = owner.transform.position.y - owner.playerCollider.bounds.extents.y;
        float platformTopY = platCollider.bounds.max.y;
        return playerFeetY <= platformTopY + 0.15f && playerFeetY >= platformTopY - 0.1f;
    }

    private void UpdateAnimations()
    {
        if (owner.MenuPanel.activeSelf || Time.timeScale <= 0) return;
        bool isMoving = Mathf.Abs(owner.horizontalInput) > 0.1f;

        if (owner.runAnimation != null)
        {
            var anim = owner.runAnimation.GetComponent<Animator>();
            if (anim != null)
                anim.speed = isMoving ? (owner.isBoosting ? 1.5f : 1.1f) : 1f;
        }

        if (owner.idleAnimation != null && owner.runAnimation != null && owner.slideSprite != null && !owner.isSliding)
        {
            owner.idleAnimation.SetActive(!isMoving);
            owner.runAnimation.SetActive(isMoving);
        }
    }
}
