using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Debug")]
    [SerializeField]
    private bool debugPush = true;

    [Header("Push Movement")]
    [SerializeField]
    private float maxPushSpeed = 2.5f;

    [SerializeField]
    private float pushAcceleration = 3f;

    [SerializeField]
    private float pushDeceleration = 6f;

    [SerializeField]
    private float initialResistanceTime = 0.15f;

    [Header("Push Detection")]
    [SerializeField]
    private Transform frontPushPoint;

    [SerializeField]
    private Transform backPushPoint;

    [SerializeField]
    private float pushPointRadius = 0.6f;

    [Header("Movement Constraints")]
    [SerializeField]
    private LayerMask blockingLayers;

    [SerializeField]
    private float collisionSkin = 0.02f;

    private Vector3 startPosition;
    private Vector3 pushAxis;

    private Vector3 previousPhysicsPosition;

    private float currentPushSpeed;
    private float pushResistanceTimer;

    private bool wasPushedThisFrame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        startPosition = rb.position;
        previousPhysicsPosition = rb.position;

        pushAxis = transform.forward;
        pushAxis.y = 0f;

        if (pushAxis.sqrMagnitude > 0.01f)
            pushAxis.Normalize();

        if (debugPush)
        {
            Debug.Log(
                $"[BOX AWAKE] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Position={rb.position} | "
                    + $"PushAxis={pushAxis} | "
                    + $"Velocity={rb.linearVelocity}"
            );
        }
    }

    private void FixedUpdate()
    {
        Vector3 actualDelta = rb.position - previousPhysicsPosition;

        if (debugPush)
        {
            Debug.Log(
                $"[BOX ACTUAL FIXED DELTA] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Position={rb.position} | "
                    + $"Previous={previousPhysicsPosition} | "
                    + $"ActualDelta={actualDelta} | "
                    + $"Magnitude={actualDelta.magnitude:F4} | "
                    + $"Velocity={rb.linearVelocity} | "
                    + $"WasPushed={wasPushedThisFrame} | "
                    + $"PushSpeed={currentPushSpeed:F4} | "
                    + $"ResistanceTimer={pushResistanceTimer:F4}"
            );
        }

        previousPhysicsPosition = rb.position;

        if (!wasPushedThisFrame)
        {
            currentPushSpeed = Mathf.MoveTowards(
                currentPushSpeed,
                0f,
                pushDeceleration * Time.fixedDeltaTime
            );

            pushResistanceTimer = 0f;
        }

        wasPushedThisFrame = false;

        LockToPushAxis();
    }

    public bool TryGetPushSetup(
        Vector3 playerPosition,
        out Transform pushPoint,
        out Vector3 pushDirection
    )
    {
        pushPoint = null;
        pushDirection = Vector3.zero;

        if (frontPushPoint == null || backPushPoint == null)
        {
            if (debugPush)
            {
                Debug.LogWarning(
                    $"[BOX PUSH SETUP FAILED] "
                        + $"fixedTime={Time.fixedTime:F3} | "
                        + $"FrontNull={frontPushPoint == null} | "
                        + $"BackNull={backPushPoint == null}"
                );
            }

            return false;
        }

        Vector3 frontOffset = playerPosition - frontPushPoint.position;

        Vector3 backOffset = playerPosition - backPushPoint.position;

        frontOffset.y = 0f;
        backOffset.y = 0f;

        float frontDistance = frontOffset.magnitude;

        float backDistance = backOffset.magnitude;

        if (debugPush)
        {
            Debug.Log(
                $"[BOX PUSH SETUP] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Player={playerPosition} | "
                    + $"Box={rb.position} | "
                    + $"FrontPoint={frontPushPoint.position} | "
                    + $"FrontDistance={frontDistance:F4} | "
                    + $"BackPoint={backPushPoint.position} | "
                    + $"BackDistance={backDistance:F4} | "
                    + $"Radius={pushPointRadius:F4}"
            );
        }

        if (frontDistance > pushPointRadius && backDistance > pushPointRadius)
        {
            if (debugPush)
            {
                Debug.LogWarning(
                    $"[BOX PUSH SETUP OUT OF RANGE] "
                        + $"fixedTime={Time.fixedTime:F3} | "
                        + $"FrontDistance={frontDistance:F4} | "
                        + $"BackDistance={backDistance:F4}"
                );
            }

            return false;
        }

        if (frontDistance <= backDistance)
        {
            pushPoint = frontPushPoint;
            pushDirection = pushAxis;

            if (debugPush)
            {
                Debug.Log(
                    $"[BOX PUSH SETUP FRONT] "
                        + $"fixedTime={Time.fixedTime:F3} | "
                        + $"Direction={pushDirection}"
                );
            }

            return true;
        }

        pushPoint = backPushPoint;
        pushDirection = -pushAxis;

        if (debugPush)
        {
            Debug.Log(
                $"[BOX PUSH SETUP BACK] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Direction={pushDirection}"
            );
        }

        return true;
    }

    public Vector3 TryPush(Transform pushPoint)
    {
        if (pushPoint == null)
            return Vector3.zero;

        if (pushPoint == frontPushPoint)
            return Push(pushAxis);

        if (pushPoint == backPushPoint)
            return Push(-pushAxis);

        if (debugPush)
        {
            Debug.LogWarning(
                $"[BOX TRY PUSH INVALID POINT] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Point={pushPoint.name}"
            );
        }

        return Vector3.zero;
    }

    private Vector3 Push(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        direction.Normalize();

        if (debugPush)
        {
            Debug.Log(
                $"[BOX PUSH ENTER] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Position={rb.position} | "
                    + $"Direction={direction} | "
                    + $"Velocity={rb.linearVelocity} | "
                    + $"CurrentSpeed={currentPushSpeed:F4} | "
                    + $"ResistanceTimer={pushResistanceTimer:F4}"
            );
        }

        wasPushedThisFrame = true;

        ClearHorizontalVelocity();

        if (pushResistanceTimer < initialResistanceTime)
        {
            pushResistanceTimer += Time.fixedDeltaTime;

            currentPushSpeed = 0f;

            if (debugPush)
            {
                Debug.Log(
                    $"[BOX RESISTING] "
                        + $"fixedTime={Time.fixedTime:F3} | "
                        + $"Position={rb.position} | "
                        + $"Velocity={rb.linearVelocity} | "
                        + $"Timer={pushResistanceTimer:F4} | "
                        + $"Required={initialResistanceTime:F4}"
                );
            }

            return Vector3.zero;
        }

        currentPushSpeed = Mathf.MoveTowards(
            currentPushSpeed,
            maxPushSpeed,
            pushAcceleration * Time.fixedDeltaTime
        );

        float distance = currentPushSpeed * Time.fixedDeltaTime;

        if (debugPush)
        {
            Debug.Log(
                $"[BOX PUSH SPEED] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"CurrentSpeed={currentPushSpeed:F4} | "
                    + $"Distance={distance:F4}"
            );
        }

        if (distance <= 0f)
            return Vector3.zero;

        if (
            blockingLayers.value != 0
            && rb.SweepTest(
                direction,
                out RaycastHit hit,
                distance + collisionSkin,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            int hitLayerMask = 1 << hit.collider.gameObject.layer;

            if ((blockingLayers.value & hitLayerMask) != 0)
            {
                if (debugPush)
                {
                    Debug.LogWarning(
                        $"[BOX PUSH BLOCKED] "
                            + $"fixedTime={Time.fixedTime:F3} | "
                            + $"Hit={hit.collider.name} | "
                            + $"Distance={hit.distance:F4} | "
                            + $"Layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}"
                    );
                }

                currentPushSpeed = 0f;

                return Vector3.zero;
            }
        }

        Vector3 movement = direction * distance;

        Vector3 beforePosition = rb.position;

        Vector3 targetPosition = beforePosition + movement;

        if (debugPush)
        {
            Debug.Log(
                $"[BOX MOVE COMMAND] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Before={beforePosition} | "
                    + $"Target={targetPosition} | "
                    + $"Movement={movement} | "
                    + $"Magnitude={movement.magnitude:F4} | "
                    + $"VelocityBefore={rb.linearVelocity}"
            );
        }

        rb.MovePosition(targetPosition);

        return movement;
    }

    private void LockToPushAxis()
    {
        Vector3 beforePosition = rb.position;

        Vector3 offset = rb.position - startPosition;

        float distance = Vector3.Dot(offset, pushAxis);

        Vector3 lockedPosition = startPosition + pushAxis * distance;

        lockedPosition.y = rb.position.y;

        Vector3 correction = lockedPosition - beforePosition;

        if (debugPush && correction.sqrMagnitude > 0.000001f)
        {
            Debug.Log(
                $"[BOX AXIS CORRECTION] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Before={beforePosition} | "
                    + $"Locked={lockedPosition} | "
                    + $"Correction={correction} | "
                    + $"Magnitude={correction.magnitude:F4}"
            );
        }

        rb.position = lockedPosition;

        ClearHorizontalVelocity();
    }

    private void ClearHorizontalVelocity()
    {
        Vector3 velocityBefore = rb.linearVelocity;

        Vector3 velocity = velocityBefore;

        velocity.x = 0f;
        velocity.z = 0f;

        rb.linearVelocity = velocity;

        if (
            debugPush
            && (Mathf.Abs(velocityBefore.x) > 0.001f || Mathf.Abs(velocityBefore.z) > 0.001f)
        )
        {
            Debug.Log(
                $"[BOX CLEAR HORIZONTAL VELOCITY] "
                    + $"fixedTime={Time.fixedTime:F3} | "
                    + $"Before={velocityBefore} | "
                    + $"After={rb.linearVelocity}"
            );
        }
    }
}
