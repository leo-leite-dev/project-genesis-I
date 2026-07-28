using UnityEngine;

public class CameraFocusActivator : MonoBehaviour, IActivatable
{
    [Header("Camera")]
    [SerializeField]
    private CameraFocusController cameraFocusController;

    [SerializeField]
    private Transform focusPosition;

    [SerializeField]
    private Transform focusTarget;

    [SerializeField]
    private float focusDuration = 2f;

    public void Activate()
    {
        if (cameraFocusController == null)
            return;

        cameraFocusController.Focus(focusPosition, focusTarget, focusDuration);
    }
}
