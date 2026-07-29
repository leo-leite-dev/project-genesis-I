using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCollisionResolver : MonoBehaviour
{
    private CapsuleCollider capsuleCollider;

    [Header("Collision")]
    [SerializeField]
    private LayerMask obstacleLayers;

    [SerializeField]
    private float collisionCheckDistance = 0.1f;

    [Header("Sliding")]
    [SerializeField]
    [Range(0f, 1f)]
    private float minSlideSpeedMultiplier = 0.35f;

    [SerializeField]
    [Range(0f, 1f)]
    private float maxSlideSpeedMultiplier = 0.9f;

    [Header("Pushing")]
    [SerializeField]
    [Range(0f, 1f)]
    private float pushingThreshold = 0.5f;

    public bool IsPushingAgainstObstacle { get; private set; }

    public Collider CurrentObstacle { get; private set; }

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public Vector3 ResolveDirection(Vector3 desiredDirection)
    {
        IsPushingAgainstObstacle = false;
        CurrentObstacle = null;

        if (desiredDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        Vector3 normalizedDirection = desiredDirection.normalized;

        GetCapsulePoints(out Vector3 bottom, out Vector3 top);

        float radius = GetWorldRadius() * 0.95f;

        bool hitSomething = Physics.CapsuleCast(
            bottom,
            top,
            radius,
            normalizedDirection,
            out RaycastHit hit,
            collisionCheckDistance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );

        if (!hitSomething)
            return desiredDirection;

        CurrentObstacle = hit.collider;

        float impactAmount = Mathf.Clamp01(Vector3.Dot(normalizedDirection, -hit.normal));

        PushableObject pushable = hit.collider.GetComponentInParent<PushableObject>();

        bool isPushable = pushable != null;

        if (isPushable && impactAmount >= pushingThreshold)
        {
            IsPushingAgainstObstacle = true;

            return Vector3.zero;
        }

        Vector3 slidingDirection = Vector3.ProjectOnPlane(desiredDirection, hit.normal);

        slidingDirection.y = 0f;

        if (slidingDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        float slideSpeedMultiplier = Mathf.Lerp(
            maxSlideSpeedMultiplier,
            minSlideSpeedMultiplier,
            impactAmount
        );

        slidingDirection *= slideSpeedMultiplier;

        return slidingDirection;
    }

    private void GetCapsulePoints(out Vector3 bottom, out Vector3 top)
    {
        Vector3 center = transform.TransformPoint(capsuleCollider.center);

        float radius = GetWorldRadius();

        float height = Mathf.Max(capsuleCollider.height * transform.lossyScale.y, radius * 2f);

        float halfDistance = (height * 0.5f) - radius;

        bottom = center + Vector3.down * halfDistance;

        top = center + Vector3.up * halfDistance;
    }

    private float GetWorldRadius()
    {
        float horizontalScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);

        return capsuleCollider.radius * horizontalScale;
    }
}
