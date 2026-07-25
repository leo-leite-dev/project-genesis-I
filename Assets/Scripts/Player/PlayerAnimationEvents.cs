using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    public void StartDashBoost()
    {
        playerMovement.StartDashBoost();
    }

    public void EndDashBoost()
    {
        playerMovement.EndDashBoost();
    }

    public void EndDash()
    {
        playerMovement.EndDash();
    }
}
