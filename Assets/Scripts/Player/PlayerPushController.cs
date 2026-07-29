using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPushController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Alignment")]
    [SerializeField]
    private float alignmentSpeed = 4f;

    [SerializeField]
    private float alignmentRotationSpeed = 540f;

    [SerializeField]
    private float positionTolerance = 0.02f;

    [SerializeField]
    private float rotationTolerance = 1f;

    [Header("Push Input")]
    [SerializeField]
    [Range(0f, 1f)]
    private float releaseDirectionThreshold = 0.4f;

    private PushableObject currentPushable;
    private Transform currentPushPoint;

    private Vector3 pushingDirection;

    private bool isPushActive;
    private bool isAligned;

    public bool IsPushing => isPushActive && isAligned;

    public bool IsAligning => isPushActive && !isAligned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartPush(Vector3 direction, Collider obstacle)
    {
        if (isPushActive)
            return;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        if (obstacle == null)
            return;

        PushableObject pushable = obstacle.GetComponentInParent<PushableObject>();

        if (pushable == null)
            return;

        bool setupFound = pushable.TryGetPushSetup(
            rb.position,
            out Transform pushPoint,
            out Vector3 pushDirection
        );

        if (!setupFound)
            return;

        currentPushable = pushable;
        currentPushPoint = pushPoint;

        pushingDirection = pushDirection;
        pushingDirection.y = 0f;

        if (pushingDirection.sqrMagnitude <= 0.01f)
        {
            StopPush();
            return;
        }

        pushingDirection.Normalize();

        isPushActive = true;
        isAligned = false;
    }

    public bool UpdatePush(Vector3 desiredDirection, bool isGrounded, out Vector3 pushMovement)
    {
        pushMovement = Vector3.zero;

        if (!isPushActive)
            return false;

        if (!isGrounded)
        {
            StopPush();
            return false;
        }

        if (currentPushable == null || currentPushPoint == null)
        {
            StopPush();
            return false;
        }

        if (desiredDirection.sqrMagnitude <= 0.01f)
        {
            StopPush();
            return true;
        }

        Vector3 normalizedDirection = desiredDirection;

        normalizedDirection.y = 0f;

        if (normalizedDirection.sqrMagnitude <= 0.01f)
        {
            StopPush();
            return true;
        }

        normalizedDirection.Normalize();

        float directionAmount = Vector3.Dot(normalizedDirection, pushingDirection);

        if (directionAmount <= -releaseDirectionThreshold)
        {
            StopPush();
            return false;
        }

        if (directionAmount <= 0f)
            return true;

        if (!isAligned)
        {
            UpdateAlignment(out pushMovement);

            return true;
        }

        LockRotationToPushDirection();

        Vector3 pushPointBeforeMovement = currentPushPoint.position;

        Vector3 boxMovement = currentPushable.TryPush(currentPushPoint);

        Vector3 targetPosition = pushPointBeforeMovement + boxMovement;

        targetPosition.y = rb.position.y;

        pushMovement = targetPosition - rb.position;

        return true;
    }

    private void UpdateAlignment(out Vector3 alignmentMovement)
    {
        alignmentMovement = Vector3.zero;

        Vector3 targetPosition = currentPushPoint.position;

        targetPosition.y = rb.position.y;

        Vector3 positionOffset = targetPosition - rb.position;

        positionOffset.y = 0f;

        float distance = positionOffset.magnitude;

        Quaternion targetRotation = Quaternion.LookRotation(pushingDirection, Vector3.up);

        float angle = Quaternion.Angle(rb.rotation, targetRotation);

        if (distance > positionTolerance)
        {
            Vector3 nextPosition = Vector3.MoveTowards(
                rb.position,
                targetPosition,
                alignmentSpeed * Time.fixedDeltaTime
            );

            alignmentMovement = nextPosition - rb.position;
        }

        if (angle > rotationTolerance)
        {
            Quaternion nextRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                alignmentRotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(nextRotation);
        }

        bool positionAligned = distance <= positionTolerance;

        bool rotationAligned = angle <= rotationTolerance;

        if (!positionAligned || !rotationAligned)
            return;

        alignmentMovement = targetPosition - rb.position;

        rb.MoveRotation(targetRotation);

        isAligned = true;
    }

    private void LockRotationToPushDirection()
    {
        if (pushingDirection.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(pushingDirection, Vector3.up);

        rb.MoveRotation(targetRotation);
    }

    public void StopPush()
    {
        isPushActive = false;
        isAligned = false;

        pushingDirection = Vector3.zero;

        currentPushPoint = null;
        currentPushable = null;
    }
}
