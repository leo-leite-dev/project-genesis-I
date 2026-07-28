using UnityEngine;

public class MovingPlatform : MonoBehaviour, IActivatable
{
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

    private void Awake()
    {
        startPosition = transform.position;
        targetPosition = startPosition + moveOffset;

        isActive = startActive;
    }

    private void Update()
    {
        if (!isActive)
            return;

        Vector3 destination = movingToTarget ? targetPosition : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, destination) <= 0.01f)
        {
            transform.position = destination;
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
