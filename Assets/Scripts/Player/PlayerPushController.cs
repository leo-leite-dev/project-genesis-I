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
    private Vector3 pushFollowOffset;

    private bool isPushActive;
    private bool isAligned;

    public bool IsPushing => isPushActive && isAligned;

    public bool IsAligning => isPushActive && !isAligned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public bool TryGetMovement(
        Vector3 desiredDirection,
        bool isGrounded,
        bool isPushingAgainstObstacle,
        Collider obstacle,
        out Vector3 pushMovement
    )
    {
        pushMovement = Vector3.zero;

        if (!isGrounded)
        {
            StopPush();
            return false;
        }

        if (!isPushActive)
        {
            if (!CanStartPush(isPushingAgainstObstacle, obstacle))
                return false;

            if (!TryStartPush(desiredDirection, obstacle))
                return false;
        }

        return UpdatePush(desiredDirection, out pushMovement);
    }

    public void StopPush()
    {
        isPushActive = false;
        isAligned = false;

        currentPushable = null;
        currentPushPoint = null;

        pushingDirection = Vector3.zero;
        pushFollowOffset = Vector3.zero;
    }

    private bool CanStartPush(bool isPushingAgainstObstacle, Collider obstacle)
    {
        if (!isPushingAgainstObstacle)
            return false;

        if (obstacle == null)
            return false;

        return obstacle.GetComponentInParent<PushableObject>() != null;
    }

    private bool TryStartPush(Vector3 desiredDirection, Collider obstacle)
    {
        if (isPushActive)
            return true;

        if (desiredDirection.sqrMagnitude <= 0.01f)
            return false;

        if (obstacle == null)
            return false;

        PushableObject pushable = obstacle.GetComponentInParent<PushableObject>();

        if (pushable == null)
            return false;

        if (
            !pushable.TryGetPushSetup(
                rb.position,
                out Transform pushPoint,
                out Vector3 pushDirection
            )
        )
        {
            return false;
        }

        if (pushPoint == null)
            return false;

        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude <= 0.01f)
            return false;

        currentPushable = pushable;
        currentPushPoint = pushPoint;

        pushingDirection = pushDirection.normalized;
        pushFollowOffset = Vector3.zero;

        isPushActive = true;
        isAligned = false;

        return true;
    }

    private bool UpdatePush(Vector3 desiredDirection, out Vector3 pushMovement)
    {
        pushMovement = Vector3.zero;

        if (!HasValidPushTarget())
        {
            StopPush();
            return false;
        }

        if (!TryGetPushInputAmount(desiredDirection, out float directionAmount))
        {
            StopPush();
            return true;
        }

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

        UpdateActivePush(out pushMovement);

        return true;
    }

    private bool HasValidPushTarget()
    {
        return isPushActive && currentPushable != null && currentPushPoint != null;
    }

    private bool TryGetPushInputAmount(Vector3 desiredDirection, out float directionAmount)
    {
        directionAmount = 0f;

        if (desiredDirection.sqrMagnitude <= 0.01f)
            return false;

        Vector3 normalizedDirection = desiredDirection;

        normalizedDirection.y = 0f;

        if (normalizedDirection.sqrMagnitude <= 0.01f)
            return false;

        normalizedDirection.Normalize();

        directionAmount = Vector3.Dot(normalizedDirection, pushingDirection);

        return true;
    }

    private void UpdateActivePush(out Vector3 pushMovement)
    {
        LockRotationToPushDirection();

        Vector3 boxMovement = currentPushable.TryPush(currentPushPoint);

        Vector3 followCorrection = GetPushFollowCorrection();

        pushMovement = boxMovement + followCorrection;
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

        CompleteAlignment(targetRotation);
    }

    private void CompleteAlignment(Quaternion targetRotation)
    {
        rb.MoveRotation(targetRotation);

        pushFollowOffset = rb.position - currentPushPoint.position;

        pushFollowOffset.y = 0f;

        isAligned = true;
    }

    private Vector3 GetPushFollowCorrection()
    {
        if (currentPushPoint == null)
            return Vector3.zero;

        Vector3 targetPosition = currentPushPoint.position + pushFollowOffset;

        targetPosition.y = rb.position.y;

        Vector3 offset = targetPosition - rb.position;

        offset.y = 0f;

        if (offset.sqrMagnitude <= positionTolerance * positionTolerance)
            return Vector3.zero;

        return Vector3.MoveTowards(Vector3.zero, offset, alignmentSpeed * Time.fixedDeltaTime);
    }

    private void LockRotationToPushDirection()
    {
        if (pushingDirection.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(pushingDirection, Vector3.up);

        rb.MoveRotation(targetRotation);
    }
}
