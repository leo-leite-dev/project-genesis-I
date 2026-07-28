using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCollisionResolver))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerInputReader inputReader;
    private PlayerStats playerStats;
    private PlayerCollisionResolver collisionResolver;
    private Rigidbody rb;

    [Header("References")]
    [SerializeField]
    private Transform cameraTransform;

    [Header("Movement State")]
    private bool isMovementLocked;

    public bool IsMovementLocked => isMovementLocked;

    [Header("Movement")]
    [SerializeField]
    private float acceleration = 20f;

    [SerializeField]
    private float deceleration = 25f;

    [SerializeField]
    private float rotationSpeed = 12f;

    private Vector3 currentVelocity;

    public bool IsMoving => currentVelocity.sqrMagnitude > 0.01f;

    [Header("Jump")]
    [SerializeField]
    private bool canJump = false;

    [SerializeField]
    private bool canDoubleJump = false;

    [SerializeField]
    private float jumpForce = 8f;

    [SerializeField]
    private Transform groundCheck;

    [SerializeField]
    private float groundCheckRadius = 0.2f;

    [SerializeField]
    private LayerMask groundLayer;

    private bool isGrounded;
    private bool hasUsedDoubleJump;

    public bool IsGrounded => isGrounded;
    public bool CanJump => canJump;
    public bool CanDoubleJump => canDoubleJump;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeedMultiplier = 1.8f;

    private bool isDashing;
    private bool hasDashBoost;

    private Vector3 dashDirection;

    public bool IsDashing => isDashing;

    [Header("Knockback")]
    private bool isKnockbacked;
    private float knockbackTimer;

    public bool IsKnockbacked => isKnockbacked;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        playerStats = GetComponent<PlayerStats>();
        collisionResolver = GetComponent<PlayerCollisionResolver>();
        rb = GetComponent<Rigidbody>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (isGrounded)
            hasUsedDoubleJump = false;

        if (isKnockbacked)
        {
            UpdateKnockback();
            return;
        }

        if (isMovementLocked)
            return;

        TryJump();

        if (isDashing)
        {
            UpdateDash();
            return;
        }

        TryStartDash();

        if (isDashing)
            return;

        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector2 input = inputReader.MoveInput;

        Vector3 desiredDirection = GetCameraRelativeDirection(input);

        Vector3 allowedDirection = collisionResolver.ResolveDirection(desiredDirection);

        bool hasInput = desiredDirection.sqrMagnitude > 0.01f;

        bool isBlocked = hasInput && allowedDirection.sqrMagnitude <= 0.01f;

        if (isBlocked)
        {
            currentVelocity = Vector3.zero;

            Vector3 velocity = rb.linearVelocity;

            velocity.x = 0f;
            velocity.z = 0f;

            rb.linearVelocity = velocity;

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

        rb.MovePosition(rb.position + movement);

        RotatePlayer(desiredDirection);
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        if (cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y).normalized;
        }

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

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void TryJump()
    {
        if (!inputReader.JumpPressed)
            return;

        inputReader.ConsumeJump();

        if (!canJump)
            return;

        if (isGrounded)
        {
            Jump();
            return;
        }

        if (canDoubleJump && !hasUsedDoubleJump)
        {
            hasUsedDoubleJump = true;

            Jump();
        }
    }

    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;

        velocity.y = 0f;

        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void TryStartDash()
    {
        if (!inputReader.DashPressed)
            return;

        inputReader.ConsumeDash();

        Vector2 input = inputReader.MoveInput;

        Vector3 direction = GetCameraRelativeDirection(input);

        if (direction.sqrMagnitude <= 0.01f)
            return;

        StartDash(direction);
    }

    private void StartDash(Vector3 direction)
    {
        isDashing = true;
        hasDashBoost = false;

        dashDirection = direction.normalized;

        RotatePlayer(dashDirection);
    }

    private void UpdateDash()
    {
        float dashSpeed = playerStats.BaseMoveSpeed;

        if (hasDashBoost)
            dashSpeed *= dashSpeedMultiplier;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(dashDirection);

        Vector3 movement = allowedDirection * dashSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        RotatePlayer(dashDirection);
    }

    public void ApplyKnockback(
        Vector3 direction,
        float horizontalForce,
        float upwardForce,
        float duration
    )
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            direction = -transform.forward;

        direction.Normalize();

        currentVelocity = Vector3.zero;

        isDashing = false;
        hasDashBoost = false;

        isKnockbacked = true;
        knockbackTimer = duration;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = direction.x * horizontalForce;

        velocity.z = direction.z * horizontalForce;

        if (upwardForce > 0f)
            velocity.y = upwardForce;

        rb.linearVelocity = velocity;
    }

    private void UpdateKnockback()
    {
        knockbackTimer -= Time.fixedDeltaTime;

        if (knockbackTimer > 0f)
            return;

        EndKnockback();
    }

    private void EndKnockback()
    {
        isKnockbacked = false;
        knockbackTimer = 0f;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity = velocity;

        currentVelocity = Vector3.zero;
    }

    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;

        if (!locked)
            return;

        currentVelocity = Vector3.zero;

        hasDashBoost = false;
        isDashing = false;

        if (isKnockbacked)
            EndKnockback();

        Vector3 velocity = rb.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity = velocity;
    }

    public void UnlockJump()
    {
        canJump = true;
    }

    public void UnlockDoubleJump()
    {
        canDoubleJump = true;
    }

    public void StartDashBoost()
    {
        hasDashBoost = true;
    }

    public void EndDashBoost()
    {
        hasDashBoost = false;
    }

    public void EndDash()
    {
        hasDashBoost = false;
        isDashing = false;
    }
}
