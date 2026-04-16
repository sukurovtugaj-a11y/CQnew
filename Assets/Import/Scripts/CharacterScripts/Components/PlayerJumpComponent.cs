using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpComponent
{
    private readonly SecMainCharacter owner;
    private readonly Rigidbody2D rb;
    private readonly PlayerMovementComponent movement;

    public PlayerJumpComponent(SecMainCharacter owner, Rigidbody2D rb, PlayerMovementComponent movement)
    {
        this.owner = owner;
        this.rb = rb;
        this.movement = movement;
    }

    public void UpdateInput()
    {
        if (owner.controlLockTimer > 0f) return;
        if (Input.GetKeyDown(owner.keys.JumpButton))
            owner.jumpBufferCounter = owner.jumpBufferTime;
    }

    public void HandleJumpRequests()
    {
        if (owner.jumpBufferCounter <= 0) return;
        bool isGrounded = movement.IsGrounded();
        bool canGroundJump = (isGrounded || owner.coyoteTimeCounter > 0);
        bool canAirJump = owner.currentState == SecMainCharacter.PlayerState.Airborne && owner.extraJumpsLeft > 0;

        if (canGroundJump) { ExecuteGroundJump(); owner.jumpBufferCounter = 0; }
        else if (canAirJump) { ExecuteAirJump(); owner.jumpBufferCounter = 0; }
    }

    public void AddJumpAbility(JumpAbilityData ability) { owner.activeAbilities.Add(ability); RecalculateJumpParameters(); }
    public void RemoveJumpAbility(JumpAbilityData ability) { owner.activeAbilities.Remove(ability); RecalculateJumpParameters(); }

    public void RecalculateJumpParameters()
    {
        int originalMaxExtraJumps = owner.maxExtraJumps;
        owner.maxExtraJumps = 0;
        owner.jumpHeight = owner.baseJumpHeight;
        foreach (var ability in owner.activeAbilities)
        {
            owner.maxExtraJumps += ability.additionalJumps;
            owner.jumpHeight = Mathf.Max(owner.jumpHeight, owner.baseJumpHeight * ability.heightMultiplier);
        }
        if (owner.activeAbilities.Count == 0) owner.maxExtraJumps = originalMaxExtraJumps;
        float gravity = Physics2D.gravity.y * rb.gravityScale;
        owner.jumpSpeed = Mathf.Sqrt(-2f * gravity * owner.jumpHeight);
        if (movement.IsGrounded()) owner.extraJumpsLeft = owner.maxExtraJumps;
    }

    private void ExecuteGroundJump()
    {
        movement.DetachFromPlatform();
        rb.velocity = new Vector2(rb.velocity.x, owner.jumpSpeed);
        owner.currentState = SecMainCharacter.PlayerState.Airborne;
        owner.extraJumpsLeft = owner.maxExtraJumps;
        owner.coyoteTimeCounter = 0;
    }

    private void ExecuteAirJump()
    {
        movement.DetachFromPlatform();
        rb.velocity = new Vector2(rb.velocity.x, owner.jumpSpeed);
        owner.extraJumpsLeft--;
        owner.coyoteTimeCounter = 0;
    }
}
