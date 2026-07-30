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

    [SerializeField]
    [Range(0.8f, 1f)]
    private float capsuleRadiusMultiplier = 0.95f;

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
        ResetCollisionState();

        Vector3 horizontalDirection = GetHorizontalDirection(desiredDirection);

        if (horizontalDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        if (!TryGetObstacleHit(horizontalDirection, out RaycastHit hit))
            return horizontalDirection;

        SetCurrentObstacle(hit.collider);

        UpdatePushState(horizontalDirection, hit.normal);

        if (IsPushingAgainstObstacle)
            return Vector3.zero;

        return GetSlidingDirection(horizontalDirection, hit.normal);
    }

    private Vector3 GetHorizontalDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        return direction.normalized;
    }

    private bool TryGetObstacleHit(Vector3 direction, out RaycastHit hit)
    {
        GetCapsulePoints(out Vector3 bottom, out Vector3 top);

        float radius = GetWorldRadius() * capsuleRadiusMultiplier;

        return Physics.CapsuleCast(
            bottom,
            top,
            radius,
            direction,
            out hit,
            collisionCheckDistance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private void SetCurrentObstacle(Collider obstacle)
    {
        CurrentObstacle = obstacle;
    }

    private void UpdatePushState(Vector3 desiredDirection, Vector3 obstacleNormal)
    {
        float pushingAmount = Vector3.Dot(desiredDirection, -obstacleNormal);

        IsPushingAgainstObstacle = pushingAmount >= pushingThreshold;
    }

    private Vector3 GetSlidingDirection(Vector3 desiredDirection, Vector3 obstacleNormal)
    {
        Vector3 slidingDirection = Vector3.ProjectOnPlane(desiredDirection, obstacleNormal);

        slidingDirection.y = 0f;

        if (slidingDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        return slidingDirection.normalized;
    }

    private void ResetCollisionState()
    {
        IsPushingAgainstObstacle = false;
        CurrentObstacle = null;
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
