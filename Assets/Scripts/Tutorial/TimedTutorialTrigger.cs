using System.Collections;
using UnityEngine;

public class TimedTutorialTrigger : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField]
    private string playerTag = "Player";

    [SerializeField]
    private float delayBeforeShow = 3f;

    [Header("Tutorial")]
    [TextArea]
    [SerializeField]
    private string message;

    private TutorialBubble tutorialBubble;
    private Coroutine showRoutine;

    private bool playerInside;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrou na área: " + other.name);

        if (!other.CompareTag(playerTag))
            return;

        Debug.Log("É o Player!");

        tutorialBubble = other.GetComponentInParent<TutorialBubble>();

        if (tutorialBubble == null)
        {
            Debug.LogWarning("TutorialBubble não encontrado no Player!");
            return;
        }

        Debug.Log("TutorialBubble encontrado. Iniciando timer.");

        playerInside = true;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowAfterDelay());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (tutorialBubble != null)
            tutorialBubble.Hide();

        tutorialBubble = null;
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        Debug.Log("Terminou os 3 segundos.");

        if (playerInside && tutorialBubble != null)
        {
            Debug.Log("Mostrando tutorial.");
            tutorialBubble.Show(message);
        }

        showRoutine = null;
    }
}
