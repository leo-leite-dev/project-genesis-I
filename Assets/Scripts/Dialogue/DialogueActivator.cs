using UnityEngine;

public class DialogueActivator : MonoBehaviour, IActivatable
{
    [Header("References")]
    [SerializeField]
    private DialogueController dialogueController;

    [SerializeField]
    private PlayerMovement playerMovement;

    [Header("Dialogue")]
    [SerializeField]
    private string title;

    [TextArea(3, 8)]
    [SerializeField]
    private string message;

    public void Activate()
    {
        if (dialogueController == null)
            return;

        dialogueController.Show(title, message, playerMovement);
    }
}
