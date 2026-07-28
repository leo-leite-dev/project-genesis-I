using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Push")]
    [SerializeField]
    private float pushSpeed = 3f;

    [SerializeField]
    private Transform frontPushPoint;

    [SerializeField]
    private Transform backPushPoint;

    [SerializeField]
    private float pushPointRadius = 0.6f;

    private Vector3 startPosition;
    private Vector3 pushAxis;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        startPosition = rb.position;

        pushAxis = transform.forward;
        pushAxis.y = 0f;
        pushAxis.Normalize();
    }

    private void FixedUpdate()
    {
        LockToPushAxis();
    }

    public void TryPush(Transform player)
    {
        if (player == null)
            return;

        if (frontPushPoint == null || backPushPoint == null)
            return;

        float frontDistance = Vector3.Distance(player.position, frontPushPoint.position);

        float backDistance = Vector3.Distance(player.position, backPushPoint.position);

        if (frontDistance <= pushPointRadius)
        {
            Push(-pushAxis);
            return;
        }

        if (backDistance <= pushPointRadius)
            Push(pushAxis);
    }

    private void Push(Vector3 direction)
    {
        Vector3 movement = direction * pushSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
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
