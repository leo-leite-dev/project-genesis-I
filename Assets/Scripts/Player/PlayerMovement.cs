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

    [Header("Movement")]
    [SerializeField]
    private float acceleration = 20f;

    [SerializeField]
    private float deceleration = 25f;

    private Vector3 currentVelocity;

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

    public bool IsGrounded => isGrounded;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeedMultiplier = 1.8f;

    private bool isDashing;
    private bool hasDashBoost;

    private Vector3 dashDirection;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        playerStats = GetComponent<PlayerStats>();
        collisionResolver = GetComponent<PlayerCollisionResolver>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
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

        Vector3 desiredDirection = new Vector3(input.x, 0f, input.y).normalized;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(desiredDirection);

        Vector3 targetVelocity = allowedDirection * playerStats.MoveSpeed;

        float changeSpeed = desiredDirection == Vector3.zero ? deceleration : acceleration;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            changeSpeed * Time.fixedDeltaTime
        );

        Vector3 movement = currentVelocity * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void TryJump()
    {
        if (!inputReader.JumpPressed)
            return;

        inputReader.ConsumeJump();

        if (!canJump)
            return;

        if (!isGrounded)
            return;

        Jump();
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

        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        if (direction == Vector3.zero)
            return;

        StartDash(direction);
    }

    private void StartDash(Vector3 direction)
    {
        isDashing = true;
        hasDashBoost = false;

        dashDirection = direction.normalized;
    }

    private void UpdateDash()
    {
        float dashSpeed = playerStats.BaseMoveSpeed;

        if (hasDashBoost)
            dashSpeed *= dashSpeedMultiplier;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(dashDirection);

        Vector3 movement = allowedDirection * dashSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
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
