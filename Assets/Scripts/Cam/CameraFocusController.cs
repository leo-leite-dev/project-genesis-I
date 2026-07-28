using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField]
    private CinemachineCamera playerCamera;

    [SerializeField]
    private CinemachineCamera focusCamera;

    [Header("Priority")]
    [SerializeField]
    private int focusPriority = 20;

    [Header("Timing")]
    [SerializeField]
    private float blendDuration = 0.8f;

    private Coroutine focusRoutine;

    public void Focus(Transform focusPosition, Transform focusTarget, float holdDuration)
    {
        if (focusPosition == null || focusTarget == null || focusCamera == null)
            return;

        if (focusRoutine != null)
            StopCoroutine(focusRoutine);

        focusRoutine = StartCoroutine(FocusRoutine(focusPosition, focusTarget, holdDuration));
    }

    private IEnumerator FocusRoutine(
        Transform focusPosition,
        Transform focusTarget,
        float holdDuration
    )
    {
        focusCamera.transform.SetPositionAndRotation(
            focusPosition.position,
            focusPosition.rotation
        );

        focusCamera.Target.TrackingTarget = focusTarget;

        focusCamera.Priority = focusPriority;

        yield return new WaitForSeconds(blendDuration);

        yield return new WaitForSeconds(holdDuration);

        ReturnToPlayerCamera();

        yield return new WaitForSeconds(blendDuration);

        focusRoutine = null;
    }

    private void ReturnToPlayerCamera()
    {
        if (focusCamera == null)
            return;

        int playerPriority = playerCamera != null ? playerCamera.Priority : 10;

        focusCamera.Priority = playerPriority - 1;
    }
}
