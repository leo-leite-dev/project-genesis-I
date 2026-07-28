using UnityEngine;

public class AbilityTutorialActivator : MonoBehaviour, IActivatable
{
    [Header("Tutorial")]
    [SerializeField]
    private TutorialBubble tutorialBubble;

    [TextArea]
    [SerializeField]
    private string message;

    public void Activate()
    {
        if (tutorialBubble == null)
            return;

        tutorialBubble.Show(message);
    }
}
