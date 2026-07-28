using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField]
    private GameObject promptObject;

    public void Show()
    {
        if (promptObject != null)
            promptObject.SetActive(true);
    }

    public void Hide()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }
}
