using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class InteractionSequenceActivator : MonoBehaviour, IActivatable
{
    [Header("Player")]
    [SerializeField]
    private PlayerMovement playerMovement;

    [Header("Cameras")]
    [SerializeField]
    private CinemachineCamera playerCamera;

    [SerializeField]
    private CinemachineCamera focusCamera;

    [SerializeField]
    private Transform focusPosition;

    [SerializeField]
    private Transform focusTarget;

    [Header("Camera Timing")]
    [SerializeField]
    private float blendDuration = 0.8f;

    [SerializeField]
    private float holdBeforeActivation = 0.4f;

    [SerializeField]
    private float holdAfterActivation = 1.5f;

    [SerializeField]
    private int focusPriority = 20;

    [Header("Activation Targets")]
    [SerializeField]
    private MonoBehaviour[] targets;

    [Header("Dialogue")]
    [SerializeField]
    private DialogueController dialogueController;

    [SerializeField]
    private string title;

    [TextArea(3, 8)]
    [SerializeField]
    private string message;

    private bool isRunning;
    private bool hasActivated;

    public void Activate()
    {
        if (isRunning)
            return;

        if (hasActivated)
            return;

        if (!ValidateReferences())
            return;

        hasActivated = true;

        StartCoroutine(RunSequence());
    }

    private bool ValidateReferences()
    {
        if (playerMovement == null)
        {
            Debug.LogError("InteractionSequence: PlayerMovement não configurado.");
            return false;
        }

        if (playerCamera == null)
        {
            Debug.LogError("InteractionSequence: PlayerCamera não configurada.");
            return false;
        }

        if (focusCamera == null)
        {
            Debug.LogError("InteractionSequence: FocusCamera não configurada.");
            return false;
        }

        if (focusPosition == null)
        {
            Debug.LogError("InteractionSequence: FocusPosition não configurado.");
            return false;
        }

        if (focusTarget == null)
        {
            Debug.LogError("InteractionSequence: FocusTarget não configurado.");
            return false;
        }

        return true;
    }

    private IEnumerator RunSequence()
    {
        isRunning = true;

        Debug.Log("InteractionSequence: iniciando.");

        playerMovement.SetMovementLocked(true);

        focusCamera.transform.SetPositionAndRotation(
            focusPosition.position,
            focusPosition.rotation
        );

        focusCamera.Target.TrackingTarget = focusTarget;

        SetCameraPriority(focusCamera, focusPriority);

        Debug.Log("FocusCamera ativada. Priority: " + focusCamera.Priority.Value);

        yield return new WaitForSeconds(blendDuration);

        if (holdBeforeActivation > 0f)
            yield return new WaitForSeconds(holdBeforeActivation);

        ActivateTargets();

        if (holdAfterActivation > 0f)
            yield return new WaitForSeconds(holdAfterActivation);

        int playerPriority = GetCameraPriority(playerCamera, 10);

        SetCameraPriority(focusCamera, playerPriority - 1);

        Debug.Log("InteractionSequence: retornando câmera.");

        yield return new WaitForSeconds(blendDuration);

        if (dialogueController != null)
            dialogueController.Show(title, message, playerMovement);
        else
            playerMovement.SetMovementLocked(false);

        isRunning = false;
    }

    private void ActivateTargets()
    {
        foreach (MonoBehaviour target in targets)
        {
            if (target == null)
                continue;

            if (target is IActivatable activatable)
                activatable.Activate();
        }
    }

    private void SetCameraPriority(CinemachineCamera camera, int priority)
    {
        if (camera == null)
            return;

        PrioritySettings settings = camera.Priority;

        settings.Enabled = true;
        settings.Value = priority;

        camera.Priority = settings;
    }

    private int GetCameraPriority(CinemachineCamera camera, int fallback)
    {
        if (camera == null)
            return fallback;

        if (!camera.Priority.Enabled)
            return fallback;

        return camera.Priority.Value;
    }
}
