using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJumpController))]
[RequireComponent(typeof(PlayerDashController))]
public class PlayerMovementVFX : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerJumpController jumpController;
    private PlayerDashController dashController;

    [Header("References")]
    [SerializeField]
    private ParticleSystem runDust;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        jumpController = GetComponent<PlayerJumpController>();
        dashController = GetComponent<PlayerDashController>();

        StopRunDust(true);
    }

    private void Update()
    {
        UpdateRunDust();
    }

    private void UpdateRunDust()
    {
        if (runDust == null)
            return;

        bool isMoving = playerMovement.IsMoving || dashController.IsDashing;

        bool shouldPlay = isMoving && jumpController.IsGrounded && !playerMovement.IsMovementLocked;

        if (shouldPlay)
        {
            PlayRunDust();
            return;
        }

        StopRunDust(false);
    }

    private void PlayRunDust()
    {
        if (runDust == null)
            return;

        if (runDust.isPlaying)
            return;

        runDust.Play(true);
    }

    private void StopRunDust(bool clearParticles)
    {
        if (runDust == null)
            return;

        ParticleSystemStopBehavior stopBehavior = clearParticles
            ? ParticleSystemStopBehavior.StopEmittingAndClear
            : ParticleSystemStopBehavior.StopEmitting;

        if (!runDust.isPlaying && !clearParticles)
            return;

        runDust.Stop(true, stopBehavior);

        if (clearParticles)
            runDust.Clear(true);
    }
}
