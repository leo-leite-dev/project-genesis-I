using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJumpController))]
[RequireComponent(typeof(PlayerDashController))]
[RequireComponent(typeof(PlayerPushController))]
[RequireComponent(typeof(PlayerKnockbackController))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    private PlayerInputReader inputReader;
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;

    private PlayerJumpController jumpController;
    private PlayerDashController dashController;
    private PlayerPushController pushController;
    private PlayerKnockbackController knockbackController;

    private bool wasDashing;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        inputReader = GetComponent<PlayerInputReader>();
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();

        jumpController = GetComponent<PlayerJumpController>();
        dashController = GetComponent<PlayerDashController>();
        pushController = GetComponent<PlayerPushController>();
        knockbackController = GetComponent<PlayerKnockbackController>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (playerMovement.IsMovementLocked || knockbackController.IsKnockbacked)
        {
            StopMovementAnimation();

            wasDashing = false;

            return;
        }

        bool isPushing = pushController.IsPushing;

        float speed = GetMovementAnimationSpeed(isPushing);

        animator.SetFloat("Speed", speed);

        animator.SetBool("IsSlowed", playerStats.IsSlowed);

        animator.SetBool("IsPushing", isPushing);

        UpdateDashAnimation();
    }

    private float GetMovementAnimationSpeed(bool isPushing)
    {
        if (!jumpController.IsGrounded)
            return 0f;

        if (isPushing)
            return 0f;

        return inputReader.MoveInput.magnitude;
    }

    private void UpdateDashAnimation()
    {
        bool isDashing = dashController.IsDashing;

        if (isDashing && !wasDashing)
            animator.SetTrigger("Dash");

        wasDashing = isDashing;
    }

    private void StopMovementAnimation()
    {
        animator.SetFloat("Speed", 0f);

        animator.SetBool("IsPushing", false);
    }

    public void PlayHit()
    {
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
    }
}
