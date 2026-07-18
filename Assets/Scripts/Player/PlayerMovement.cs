using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCollisionResolver))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerInputReader inputReader;
    private PlayerStats playerStats;
    private PlayerCollisionResolver collisionResolver;
    private Rigidbody rb;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        playerStats = GetComponent<PlayerStats>();
        collisionResolver = GetComponent<PlayerCollisionResolver>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector2 input = inputReader.MoveInput;

        Vector3 desiredDirection = new Vector3(input.x, 0f, input.y).normalized;

        if (desiredDirection == Vector3.zero)
            return;

        Vector3 allowedDirection = collisionResolver.ResolveDirection(desiredDirection);

        Vector3 movement = allowedDirection * playerStats.MoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }
}
