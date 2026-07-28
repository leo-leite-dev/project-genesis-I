using TMPro;
using UnityEngine;

public class TutorialBubble : MonoBehaviour
{
    [Header("Bubble")]
    [SerializeField]
    private GameObject bubbleObject;

    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private float visibleDuration = 4f;

    private float hideTimer;

    private void Update()
    {
        if (bubbleObject == null || !bubbleObject.activeSelf)
            return;

        if (visibleDuration <= 0f)
            return;

        hideTimer -= Time.deltaTime;

        if (hideTimer <= 0f)
            Hide();
    }

    public void Show(string message)
    {
        if (bubbleObject == null)
            return;

        if (messageText != null)
            messageText.text = message;

        hideTimer = visibleDuration;

        bubbleObject.SetActive(true);
    }

    public void Hide()
    {
        if (bubbleObject != null)
            bubbleObject.SetActive(false);
    }
}
