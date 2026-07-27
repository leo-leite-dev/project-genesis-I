using System.Collections;
using UnityEngine;

public class InteractableActivator : MonoBehaviour
{
    [Header("Activation Targets")]
    [SerializeField]
    private MonoBehaviour[] targets;

    [Header("Activation")]
    [SerializeField]
    private float activationDelay = 0f;

    private bool isActivating;

    public void Activate()
    {
        if (isActivating)
            return;

        StartCoroutine(ActivateWithDelay());
    }

    private IEnumerator ActivateWithDelay()
    {
        isActivating = true;

        if (activationDelay > 0f)
            yield return new WaitForSeconds(activationDelay);

        foreach (MonoBehaviour target in targets)
        {
            if (target == null)
                continue;

            if (target is IActivatable activatable)
                activatable.Activate();
        }

        isActivating = false;
    }
}
