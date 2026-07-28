using UnityEngine;

public class JumpUnlockActivator : MonoBehaviour, IActivatable
{
    [SerializeField]
    private PlayerMovement playerMovement;

    public void Activate()
    {
        if (playerMovement == null)
            return;

        playerMovement.UnlockJump();

        Debug.Log("Jump desbloqueado!");
    }
}