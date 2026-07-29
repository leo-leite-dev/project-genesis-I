using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, IActivatable
{
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField]
    private Vector3 moveOffset = new Vector3(0f, 3f, 0f);

    [SerializeField]
    private float moveSpeed = 2f;

    [Header("Activation")]
    [SerializeField]
    private bool startActive = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool isActive;
    private bool movingToTarget = true;

    public Vector3 Position => rb.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startPosition = rb.position;
        targetPosition = startPosition + moveOffset;

        isActive = startActive;
    }

    private void FixedUpdate()
    {
        if (!isActive)
            return;

        Vector3 destination = movingToTarget ? targetPosition : startPosition;

        Vector3 nextPosition = Vector3.MoveTowards(
            rb.position,
            destination,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        if (Vector3.Distance(nextPosition, destination) <= 0.001f)
        {
            rb.MovePosition(destination);

            movingToTarget = !movingToTarget;
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
