using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerDashController : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeedMultiplier = 1.8f;

    private bool isDashing;
    private bool hasDashBoost;

    private Vector3 dashDirection;

    public bool IsDashing => isDashing;

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
    }

    public void StartDash(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        isDashing = true;
        hasDashBoost = false;

        dashDirection = direction.normalized;
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
        hasDashBoost = false;
        isDashing = false;

        dashDirection = Vector3.zero;
    }
}
