using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerAnimation : MonoBehaviour
{
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
    }

    private void UpdateAnimation()
    {
        if (playerMovement.IsMovementLocked || playerMovement.IsKnockbacked)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsPushing", false);

            wasDashing = false;

            return;
        }

        bool isPushing = playerMovement.IsPushing;

        float speed = inputReader.MoveInput.magnitude;

        if (!playerMovement.IsGrounded || isPushing)
            speed = 0f;

        animator.SetFloat("Speed", speed);

        animator.SetBool("IsSlowed", playerStats.IsSlowed);

        animator.SetBool("IsPushing", isPushing);

        if (playerMovement.IsDashing && !wasDashing)
            animator.SetTrigger("Dash");

        wasDashing = playerMovement.IsDashing;
    }

    public void PlayHit()
    {
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
    }
}
