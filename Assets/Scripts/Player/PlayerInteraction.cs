using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerInteraction : MonoBehaviour
{
    private PlayerInputReader inputReader;

    [Header("Interaction")]
    [SerializeField]
    private Transform interactionPoint;

    [SerializeField]
    private float interactionRadius = 2f;

    [SerializeField]
    private LayerMask interactableLayer;

    [Header("Dialogue")]
    [SerializeField]
    private DialogueController dialogueController;

    private InteractionPrompt currentPrompt;
    private InteractableActivator currentActivator;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        if (HandleDialogue())
            return;

        DetectInteractable();
        TryInteract();
    }

    private bool HandleDialogue()
    {
        if (dialogueController == null)
            return false;

        if (!dialogueController.IsOpen)
            return false;

        HideCurrentPrompt();

        if (!inputReader.InteractPressed)
            return true;

        inputReader.ConsumeInteract();

        dialogueController.Hide();

        return true;
    }

    private void DetectInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(
            interactionPoint.position,
            interactionRadius,
            interactableLayer
        );

        InteractionPrompt newPrompt = null;
        InteractableActivator newActivator = null;

        foreach (Collider collider in colliders)
        {
            newPrompt = collider.GetComponentInParent<InteractionPrompt>();

            newActivator = collider.GetComponentInParent<InteractableActivator>();

            if (newPrompt != null || newActivator != null)
                break;
        }

        if (newPrompt != currentPrompt)
        {
            if (currentPrompt != null)
                currentPrompt.Hide();

            currentPrompt = newPrompt;

            if (currentPrompt != null)
                currentPrompt.Show();
        }

        currentActivator = newActivator;
    }

    private void TryInteract()
    {
        if (!inputReader.InteractPressed)
            return;

        inputReader.ConsumeInteract();

        if (currentActivator == null)
            return;

        currentActivator.Activate();
    }

    private void HideCurrentPrompt()
    {
        if (currentPrompt != null)
            currentPrompt.Hide();

        currentPrompt = null;
        currentActivator = null;
    }
}
