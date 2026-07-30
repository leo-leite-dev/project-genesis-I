using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerKnockbackController : MonoBehaviour
{
    private Rigidbody rb;

    private bool isKnockbacked;
    private float knockbackTimer;

    public bool IsKnockbacked => isKnockbacked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyKnockback(
        Vector3 direction,
        float horizontalForce,
        float upwardForce,
        float duration
    )
    {
        direction = GetValidDirection(direction);

        horizontalForce = Mathf.Max(0f, horizontalForce);
        upwardForce = Mathf.Max(0f, upwardForce);
        duration = Mathf.Max(0f, duration);

        StartKnockback(direction, horizontalForce, upwardForce, duration);
    }

    public void UpdateKnockback()
    {
        if (!isKnockbacked)
            return;

        knockbackTimer -= Time.fixedDeltaTime;

        if (knockbackTimer > 0f)
            return;

        EndKnockback();
    }

    public void EndKnockback()
    {
        if (!isKnockbacked)
            return;

        isKnockbacked = false;
        knockbackTimer = 0f;

        ClearHorizontalVelocity();
    }

    private void StartKnockback(
        Vector3 direction,
        float horizontalForce,
        float upwardForce,
        float duration
    )
    {
        isKnockbacked = true;
        knockbackTimer = duration;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = direction.x * horizontalForce;

        velocity.z = direction.z * horizontalForce;

        if (upwardForce > 0f)
            velocity.y = upwardForce;

        rb.linearVelocity = velocity;

        if (knockbackTimer <= 0f)
            EndKnockback();
    }

    private Vector3 GetValidDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            direction = -transform.forward;

        return direction.normalized;
    }

    private void ClearHorizontalVelocity()
    {
        Vector3 velocity = rb.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity = velocity;
    }
}
