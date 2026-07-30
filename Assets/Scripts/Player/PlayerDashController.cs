using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerDashController : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerInputReader inputReader;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeedMultiplier = 1.8f;

    private bool isDashing;
    private bool hasDashBoost;

    private Vector3 dashDirection;

    public bool IsDashing => isDashing;

    public bool HasDashBoost => hasDashBoost;

    public Vector3 DashDirection => dashDirection;

    public float CurrentSpeed
    {
        get
        {
            float speed = playerStats.BaseMoveSpeed;

            if (hasDashBoost)
                speed *= dashSpeedMultiplier;

            return speed;
        }
    }

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        inputReader = GetComponent<PlayerInputReader>();
    }

    public bool TryStartDash(Vector3 direction)
    {
        if (!inputReader.DashPressed)
            return false;

        inputReader.ConsumeDash();

        if (isDashing)
            return false;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return false;

        StartDash(direction);

        return true;
    }

    public Vector3 GetMovement(Vector3 allowedDirection, float deltaTime)
    {
        if (!isDashing)
            return Vector3.zero;

        allowedDirection.y = 0f;

        if (allowedDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        return allowedDirection.normalized * CurrentSpeed * deltaTime;
    }

    public void StartDashBoost()
    {
        if (!isDashing)
            return;

        hasDashBoost = true;
    }

    public void EndDashBoost()
    {
        hasDashBoost = false;
    }

    public void EndDash()
    {
        isDashing = false;
        hasDashBoost = false;

        dashDirection = Vector3.zero;
    }

    private void StartDash(Vector3 direction)
    {
        isDashing = true;
        hasDashBoost = false;

        dashDirection = direction.normalized;
    }
}
