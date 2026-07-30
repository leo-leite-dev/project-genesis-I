using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCollisionResolver))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    private PlayerInputReader inputReader;
    private PlayerStats playerStats;
    private PlayerCollisionResolver collisionResolver;

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

    [Header("State")]
    private bool isMovementLocked;

    private Vector3 currentVelocity;

    public bool IsMoving => currentVelocity.sqrMagnitude > 0.01f;

    public bool IsMovementLocked => isMovementLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputReader = GetComponent<PlayerInputReader>();
        playerStats = GetComponent<PlayerStats>();
        collisionResolver = GetComponent<PlayerCollisionResolver>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    public Vector3 GetInputDirection()
    {
        return GetCameraRelativeDirection(inputReader.MoveInput);
    }

    public void Move(Vector3 desiredDirection)
    {
        bool hasInput = desiredDirection.sqrMagnitude > 0.01f;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(desiredDirection);

        bool isBlocked = hasInput && allowedDirection.sqrMagnitude <= 0.01f;

        if (isBlocked)
        {
            StopHorizontalMovement();

            Rotate(desiredDirection);

            return;
        }

        Vector3 targetVelocity = allowedDirection * playerStats.MoveSpeed;

        float changeSpeed = hasInput ? acceleration : deceleration;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            changeSpeed * Time.fixedDeltaTime
        );

        if (!hasInput)
            ClearHorizontalRigidbodyVelocity();

        Vector3 movement = currentVelocity * Time.fixedDeltaTime;

        ApplyMovement(movement);

        Rotate(desiredDirection);
    }

    public void ApplyMovement(Vector3 movement)
    {
        if (movement.sqrMagnitude <= 0.0001f)
            return;

        rb.MovePosition(rb.position + movement);
    }

    public void StopHorizontalMovement()
    {
        currentVelocity = Vector3.zero;

        ClearHorizontalRigidbodyVelocity();
    }

    public void ClearHorizontalRigidbodyVelocity()
    {
        Vector3 velocity = rb.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity = velocity;
    }

    public void Rotate(Vector3 direction)
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

    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;

        if (locked)
            StopHorizontalMovement();
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
}
