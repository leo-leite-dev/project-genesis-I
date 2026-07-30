using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCollisionResolver))]
[RequireComponent(typeof(PlayerJumpController))]
[RequireComponent(typeof(PlayerPushController))]
[RequireComponent(typeof(PlayerDashController))]
[RequireComponent(typeof(PlayerKnockbackController))]
public class PlayerLocomotionController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerCollisionResolver collisionResolver;

    private PlayerJumpController jumpController;
    private PlayerPushController pushController;
    private PlayerDashController dashController;
    private PlayerKnockbackController knockbackController;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        collisionResolver = GetComponent<PlayerCollisionResolver>();

        jumpController = GetComponent<PlayerJumpController>();
        pushController = GetComponent<PlayerPushController>();
        dashController = GetComponent<PlayerDashController>();
        knockbackController = GetComponent<PlayerKnockbackController>();
    }

    private void FixedUpdate()
    {
        jumpController.UpdateGrounded();

        if (HandleKnockback())
            return;

        if (HandleMovementLock())
            return;

        jumpController.TryJump();

        if (!jumpController.IsGrounded)
            pushController.StopPush();

        if (HandleActiveDash())
            return;

        TryStartDash();

        if (dashController.IsDashing)
            return;

        if (HandlePush())
            return;

        playerMovement.Move(playerMovement.GetInputDirection());
    }

    private bool HandleKnockback()
    {
        if (!knockbackController.IsKnockbacked)
            return false;

        pushController.StopPush();
        dashController.EndDash();

        knockbackController.UpdateKnockback();

        return true;
    }

    private bool HandleMovementLock()
    {
        if (!playerMovement.IsMovementLocked)
            return false;

        StopActiveMovementStates();

        return true;
    }

    private bool HandleActiveDash()
    {
        if (!dashController.IsDashing)
            return false;

        pushController.StopPush();

        Vector3 dashDirection = dashController.DashDirection;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(dashDirection);

        Vector3 dashMovement = dashController.GetMovement(allowedDirection, Time.fixedDeltaTime);

        playerMovement.ApplyMovement(dashMovement);

        playerMovement.Rotate(dashDirection);

        return true;
    }

    private void TryStartDash()
    {
        Vector3 direction = playerMovement.GetInputDirection();

        bool startedDash = dashController.TryStartDash(direction);

        if (!startedDash)
            return;

        pushController.StopPush();

        playerMovement.StopHorizontalMovement();

        playerMovement.Rotate(direction);
    }

    private bool HandlePush()
    {
        Vector3 desiredDirection = playerMovement.GetInputDirection();

        collisionResolver.ResolveDirection(desiredDirection);

        bool isHandlingPush = pushController.TryGetMovement(
            desiredDirection,
            jumpController.IsGrounded,
            collisionResolver.IsPushingAgainstObstacle,
            collisionResolver.CurrentObstacle,
            out Vector3 pushMovement
        );

        if (!isHandlingPush)
            return false;

        playerMovement.StopHorizontalMovement();

        playerMovement.ApplyMovement(pushMovement);

        return true;
    }

    private void StopActiveMovementStates()
    {
        playerMovement.StopHorizontalMovement();

        pushController.StopPush();
        dashController.EndDash();
        knockbackController.EndKnockback();
    }
}
