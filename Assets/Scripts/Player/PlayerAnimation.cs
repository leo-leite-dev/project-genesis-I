using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    private Transform visual;

    [SerializeField]
    private float rotationSpeed = 10f;

    private Animator animator;
    private PlayerInputReader inputReader;
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;

    private bool wasDashing;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        inputReader = GetComponent<PlayerInputReader>();
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        UpdateAnimation();
        RotateVisual();
    }

    private void UpdateAnimation()
    {
        float speed = inputReader.MoveInput.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsSlowed", playerStats.IsSlowed);

        if (playerMovement.IsDashing && !wasDashing)
            animator.SetTrigger("Dash");

        wasDashing = playerMovement.IsDashing;
    }

    private void RotateVisual()
    {
        if (playerMovement.IsDashing)
            return;

        Vector2 input = inputReader.MoveInput;

        Vector3 direction = new Vector3(input.x, 0f, input.y);

        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        visual.rotation = Quaternion.Slerp(
            visual.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
