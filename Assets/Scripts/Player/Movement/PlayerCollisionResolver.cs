using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCollisionResolver : MonoBehaviour
{
    [SerializeField]
    private LayerMask obstacleLayers;

    [SerializeField]
    private float collisionCheckDistance = 0.1f;

    private CapsuleCollider capsuleCollider;

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public Vector3 ResolveDirection(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        GetCapsulePoints(out Vector3 bottom, out Vector3 top);

        float radius = GetWorldRadius() * 0.95f;

        bool hitSomething = Physics.CapsuleCast(
            bottom,
            top,
            radius,
            desiredDirection,
            out RaycastHit hit,
            collisionCheckDistance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );

        if (!hitSomething)
            return desiredDirection;

        Vector3 slidingDirection = Vector3.ProjectOnPlane(desiredDirection, hit.normal);

        if (slidingDirection.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        return slidingDirection.normalized;
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
