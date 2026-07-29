using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
    private Rigidbody rb;

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

    private float currentPushSpeed;
    private float pushResistanceTimer;

    private bool wasPushedThisFrame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        startPosition = rb.position;

        pushAxis = transform.forward;
        pushAxis.y = 0f;

        if (pushAxis.sqrMagnitude > 0.01f)
            pushAxis.Normalize();

        Debug.Log($"[PUSH SETUP] Awake | PushAxis={pushAxis}");
    }

    private void FixedUpdate()
    {
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

        if (frontPushPoint == null)
        {
            Debug.LogWarning("[PUSH SETUP] FrontPushPoint está NULL.");

            return false;
        }

        if (backPushPoint == null)
        {
            Debug.LogWarning("[PUSH SETUP] BackPushPoint está NULL.");

            return false;
        }

        Vector3 frontOffset = playerPosition - frontPushPoint.position;

        Vector3 backOffset = playerPosition - backPushPoint.position;

        frontOffset.y = 0f;
        backOffset.y = 0f;

        float frontDistance = frontOffset.magnitude;

        float backDistance = backOffset.magnitude;

        Debug.Log(
            $"[PUSH SETUP] Player={playerPosition} | "
                + $"Front={frontPushPoint.position} | "
                + $"FrontDistance={frontDistance:F3} | "
                + $"Back={backPushPoint.position} | "
                + $"BackDistance={backDistance:F3} | "
                + $"Radius={pushPointRadius:F3}"
        );

        if (frontDistance > pushPointRadius && backDistance > pushPointRadius)
        {
            Debug.LogWarning(
                $"[PUSH SETUP] FALHOU | "
                    + $"Front={frontDistance:F3} | "
                    + $"Back={backDistance:F3} | "
                    + $"Radius={pushPointRadius:F3}"
            );

            return false;
        }

        if (frontDistance <= backDistance)
        {
            pushPoint = frontPushPoint;
            pushDirection = pushAxis;

            Debug.Log($"[PUSH SETUP] FRONT escolhido | " + $"Direction={pushDirection}");

            return true;
        }

        pushPoint = backPushPoint;
        pushDirection = -pushAxis;

        Debug.Log($"[PUSH SETUP] BACK escolhido | " + $"Direction={pushDirection}");

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

        return Vector3.zero;
    }

    private Vector3 Push(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        direction.Normalize();

        wasPushedThisFrame = true;

        if (pushResistanceTimer < initialResistanceTime)
        {
            pushResistanceTimer += Time.fixedDeltaTime;

            currentPushSpeed = 0f;

            return Vector3.zero;
        }

        currentPushSpeed = Mathf.MoveTowards(
            currentPushSpeed,
            maxPushSpeed,
            pushAcceleration * Time.fixedDeltaTime
        );

        float distance = currentPushSpeed * Time.fixedDeltaTime;

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
                currentPushSpeed = 0f;

                return Vector3.zero;
            }
        }

        Vector3 movement = direction * distance;

        rb.MovePosition(rb.position + movement);

        return movement;
    }

    private void LockToPushAxis()
    {
        Vector3 offset = rb.position - startPosition;

        float distance = Vector3.Dot(offset, pushAxis);

        Vector3 lockedPosition = startPosition + pushAxis * distance;

        lockedPosition.y = rb.position.y;

        rb.position = lockedPosition;

        Vector3 velocity = rb.linearVelocity;

        float velocityAlongAxis = Vector3.Dot(velocity, pushAxis);

        Vector3 lockedVelocity = pushAxis * velocityAlongAxis;

        lockedVelocity.y = velocity.y;

        rb.linearVelocity = lockedVelocity;
    }
}
