using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerDashController dashController;

    private void Awake()
    {
        dashController = GetComponentInParent<PlayerDashController>();
    }

    public void StartDashBoost()
    {
        if (dashController == null)
            return;

        dashController.StartDashBoost();
    }

    public void EndDashBoost()
    {
        if (dashController == null)
            return;

        dashController.EndDashBoost();
    }

    public void EndDash()
    {
        if (dashController == null)
            return;

        dashController.EndDash();
    }
}
