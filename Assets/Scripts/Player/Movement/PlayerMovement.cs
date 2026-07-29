using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCollisionResolver))]
[RequireComponent(typeof(PlayerPushController))]
[RequireComponent(typeof(PlayerJumpController))]
[RequireComponent(typeof(PlayerDashController))]
[RequireComponent(typeof(PlayerKnockbackController))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    private PlayerInputReader inputReader;
    private PlayerStats playerStats;
    private PlayerCollisionResolver collisionResolver;
    private PlayerPushController pushController;
    private PlayerJumpController jumpController;
    private PlayerDashController dashController;
    private PlayerKnockbackController knockbackController;

    [Header("References")]
    [SerializeField]
    private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField]
    private float acceleration = 20f;

    [SerializeField]
    private float deceleration = 25f;

    [SerializeField]
    private float rotationSpeed = 12f;

    [Header("Moving Platform")]
    [SerializeField]
    [Range(0f, 1f)]
    private float platformGroundNormalThreshold = 0.5f;

    [Header("State")]
    private bool isMovementLocked;

    private Vector3 currentVelocity;

    private MovingPlatform currentPlatform;
    private Vector3 lastPlatformPosition;

    public bool IsMoving => currentVelocity.sqrMagnitude > 0.01f;

    public bool IsMovementLocked => isMovementLocked;

    public bool IsGrounded => jumpController != null && jumpController.IsGrounded;

    public bool CanJump => jumpController != null && jumpController.CanJump;

    public bool CanDoubleJump => jumpController != null && jumpController.CanDoubleJump;

    public bool IsDashing => dashController != null && dashController.IsDashing;

    public bool IsKnockbacked => knockbackController != null && knockbackController.IsKnockbacked;

    public bool IsPushing => pushController != null && pushController.IsPushing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        inputReader = GetComponent<PlayerInputReader>();

        playerStats = GetComponent<PlayerStats>();

        collisionResolver = GetComponent<PlayerCollisionResolver>();

        pushController = GetComponent<PlayerPushController>();

        jumpController = GetComponent<PlayerJumpController>();

        dashController = GetComponent<PlayerDashController>();

        knockbackController = GetComponent<PlayerKnockbackController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        UpdateMovingPlatform();

        jumpController.UpdateGrounded();

        if (knockbackController.IsKnockbacked)
        {
            LeaveMovingPlatform();

            pushController.StopPush();
            dashController.EndDash();

            knockbackController.UpdateKnockback();

            return;
        }

        if (isMovementLocked)
        {
            StopAllMovementStates();

            return;
        }

        jumpController.TryJump();

        if (!jumpController.IsGrounded)
        {
            pushController.StopPush();

            LeaveMovingPlatform();
        }

        if (dashController.IsDashing)
        {
            pushController.StopPush();

            UpdateDashMovement();

            return;
        }

        TryStartDash();

        if (dashController.IsDashing)
            return;

        MovePlayer();
    }

    private void UpdateMovingPlatform()
    {
        if (currentPlatform == null)
            return;

        Vector3 platformPosition = currentPlatform.Position;

        Vector3 platformMovement = platformPosition - lastPlatformPosition;

        lastPlatformPosition = platformPosition;

        if (platformMovement.sqrMagnitude <= 0.0000001f)
            return;

        rb.MovePosition(rb.position + platformMovement);
    }

    private void MovePlayer()
    {
        Vector2 input = inputReader.MoveInput;

        Vector3 desiredDirection = GetCameraRelativeDirection(input);

        if (
            pushController.UpdatePush(
                desiredDirection,
                jumpController.IsGrounded,
                out Vector3 pushMovement
            )
        )
        {
            StopHorizontalMovement();

            ApplyMovement(pushMovement);

            return;
        }

        Vector3 allowedDirection = collisionResolver.ResolveDirection(desiredDirection);

        bool hasInput = desiredDirection.sqrMagnitude > 0.01f;

        if (collisionResolver.IsPushingAgainstObstacle)
        {
            Collider obstacle = collisionResolver.CurrentObstacle;

            pushController.StartPush(desiredDirection, obstacle);

            StopHorizontalMovement();

            if (
                pushController.UpdatePush(
                    desiredDirection,
                    jumpController.IsGrounded,
                    out pushMovement
                )
            )
                ApplyMovement(pushMovement);

            return;
        }

        bool isBlocked = hasInput && allowedDirection.sqrMagnitude <= 0.01f;

        if (isBlocked)
        {
            StopHorizontalMovement();

            RotatePlayer(desiredDirection);

            return;
        }

        Vector3 targetVelocity = allowedDirection * playerStats.MoveSpeed;

        float changeSpeed = hasInput ? acceleration : deceleration;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            changeSpeed * Time.fixedDeltaTime
        );

        Vector3 movement = currentVelocity * Time.fixedDeltaTime;

        ApplyMovement(movement);

        RotatePlayer(desiredDirection);
    }

    private void TryStartDash()
    {
        if (!inputReader.DashPressed)
            return;

        inputReader.ConsumeDash();

        Vector3 direction = GetCameraRelativeDirection(inputReader.MoveInput);

        if (direction.sqrMagnitude <= 0.01f)
            return;

        pushController.StopPush();

        dashController.StartDash(direction);

        RotatePlayer(direction);
    }

    private void UpdateDashMovement()
    {
        Vector3 dashDirection = dashController.DashDirection;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(dashDirection);

        Vector3 movement = allowedDirection * dashController.CurrentSpeed * Time.fixedDeltaTime;

        ApplyMovement(movement);

        RotatePlayer(dashDirection);
    }

    private void ApplyMovement(Vector3 movement)
    {
        if (movement.sqrMagnitude <= 0.0001f)
            return;

        rb.MovePosition(rb.position + movement);
    }

    private void StopHorizontalMovement()
    {
        currentVelocity = Vector3.zero;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity = velocity;
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        if (cameraTransform == null)
            return new Vector3(input.x, 0f, input.y).normalized;

        Vector3 cameraForward = cameraTransform.forward;

        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 direction = cameraForward * input.y + cameraRight * input.x;

        return direction.normalized;
    }

    private void RotatePlayer(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        Quaternion smoothRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(smoothRotation);
    }

    private void EnterMovingPlatform(MovingPlatform platform)
    {
        if (platform == null)
            return;

        if (currentPlatform == platform)
            return;

        currentPlatform = platform;

        lastPlatformPosition = platform.Position;
    }

    private void LeaveMovingPlatform()
    {
        currentPlatform = null;

        lastPlatformPosition = Vector3.zero;
    }

    private void StopAllMovementStates()
    {
        StopHorizontalMovement();

        pushController.StopPush();
        dashController.EndDash();
        knockbackController.EndKnockback();
    }

    private void OnCollisionStay(Collision collision)
    {
        MovingPlatform platform = collision.collider.GetComponentInParent<MovingPlatform>();

        if (platform == null)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (contact.normal.y < platformGroundNormalThreshold)
                continue;

            EnterMovingPlatform(platform);

            return;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (currentPlatform == null)
            return;

        MovingPlatform platform = collision.collider.GetComponentInParent<MovingPlatform>();

        if (platform != null && platform == currentPlatform)
            LeaveMovingPlatform();
    }

    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;

        if (locked)
            StopAllMovementStates();
    }
}
