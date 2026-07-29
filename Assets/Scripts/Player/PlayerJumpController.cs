using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerJumpController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputReader inputReader;

    [Header("Jump")]
    [SerializeField]
    private bool canJump = false;

    [SerializeField]
    private bool canDoubleJump = false;

    [SerializeField]
    private float jumpForce = 8f;

    [Header("Ground Detection")]
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

    public Collider GroundCollider { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputReader = GetComponent<PlayerInputReader>();
    }

    public void UpdateGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            GroundCollider = null;

            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!isGrounded)
        {
            GroundCollider = null;

            return;
        }

        GroundCollider = FindGroundCollider();

        hasUsedDoubleJump = false;
    }

    private Collider FindGroundCollider()
    {
        Collider[] colliders = Physics.OverlapSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (colliders.Length == 0)
            return null;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider currentCollider = colliders[i];

            if (currentCollider == null)
                continue;

            MovingPlatform platform = currentCollider.GetComponentInParent<MovingPlatform>();

            if (platform != null)
                return currentCollider;
        }

        return colliders[0];
    }

    public void TryJump()
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

    public void UnlockJump()
    {
        canJump = true;
    }

    public void UnlockDoubleJump()
    {
        canDoubleJump = true;
    }
}
