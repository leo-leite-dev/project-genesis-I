using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerJumpController))]
public class PlayerPlatformController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerJumpController jumpController;

    [Header("Platform Detection")]
    [SerializeField]
    [Range(0f, 1f)]
    private float groundNormalThreshold = 0.5f;

    private MovingPlatform currentPlatform;

    public bool IsOnMovingPlatform => currentPlatform != null;

    public MovingPlatform CurrentPlatform => currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpController = GetComponent<PlayerJumpController>();
    }

    private void FixedUpdate()
    {
        UpdatePlatformMovement();
    }

    private void UpdatePlatformMovement()
    {
        if (!CanInheritPlatformVelocity())
            return;

        ApplyVerticalPlatformVelocity();
    }

    private bool CanInheritPlatformVelocity()
    {
        if (currentPlatform == null)
            return false;

        if (!jumpController.IsGrounded)
            return false;

        if (jumpController.IsJumping)
            return false;

        return true;
    }

    private void ApplyVerticalPlatformVelocity()
    {
        Vector3 platformVelocity = currentPlatform.Velocity;

        Vector3 velocity = rb.linearVelocity;

        velocity.y = platformVelocity.y;

        rb.linearVelocity = velocity;
    }

    private void EnterPlatform(MovingPlatform platform)
    {
        if (platform == null)
            return;

        currentPlatform = platform;
    }

    private void LeavePlatform(MovingPlatform platform)
    {
        if (currentPlatform == null)
            return;

        if (platform != currentPlatform)
            return;

        currentPlatform = null;
    }

    private bool IsValidGroundContact(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (contact.normal.y >= groundNormalThreshold)
                return true;
        }

        return false;
    }

    private void OnCollisionStay(Collision collision)
    {
        MovingPlatform platform = collision.collider.GetComponentInParent<MovingPlatform>();

        if (platform == null)
            return;

        if (!IsValidGroundContact(collision))
            return;

        EnterPlatform(platform);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (currentPlatform == null)
            return;

        MovingPlatform platform = collision.collider.GetComponentInParent<MovingPlatform>();

        if (platform == null)
            return;

        LeavePlatform(platform);
    }
}
