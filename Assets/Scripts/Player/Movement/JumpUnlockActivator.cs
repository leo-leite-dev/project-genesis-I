using UnityEngine;

public class JumpUnlockActivator : MonoBehaviour, IActivatable
{
    [SerializeField]
    private PlayerJumpController jumpController;

    public void Activate()
    {
        if (jumpController == null)
            return;

        jumpController.UnlockJump();

        Debug.Log("Jump desbloqueado!");
    }
}
