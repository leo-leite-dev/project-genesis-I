using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerMovementVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ParticleSystem runDust;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (runDust != null)
        {
            runDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            runDust.Clear(true);
        }
    }

    private void Update()
    {
        UpdateRunDust();
    }

    private void UpdateRunDust()
    {
        if (runDust == null)
            return;

        bool shouldPlay =
            (playerMovement.IsMoving || playerMovement.IsDashing)
            && playerMovement.IsGrounded
            && !playerMovement.IsMovementLocked;

        if (shouldPlay)
        {
            if (!runDust.isPlaying)
                runDust.Play(true);
        }
        else
        {
            if (runDust.isPlaying)
                runDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
