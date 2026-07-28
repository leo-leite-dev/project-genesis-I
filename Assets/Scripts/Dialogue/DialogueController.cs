using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text messageText;

    private PlayerMovement playerMovement;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void Show(string title, string message, PlayerMovement player)
    {
        if (dialoguePanel == null)
            return;

        playerMovement = player;

        if (titleText != null)
            titleText.text = title;

        if (messageText != null)
            messageText.text = message;

        isOpen = true;

        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        dialoguePanel.SetActive(true);
    }

    public void Hide()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(false);
            playerMovement = null;
        }
    }
}
