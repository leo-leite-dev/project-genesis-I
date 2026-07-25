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

        if (desiredDirection == Vector3.zero)
            return;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(desiredDirection);

        Vector3 movement = allowedDirection * playerStats.MoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
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
