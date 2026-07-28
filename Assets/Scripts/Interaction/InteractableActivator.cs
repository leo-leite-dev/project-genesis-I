using System.Collections;
using UnityEngine;

public class InteractableActivator : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField]
    private GameObject target;

    [SerializeField]
    private float activationDelay = 0f;

    private bool isActivating;
    private bool hasActivated;

    public void Activate()
    {
        if (isActivating || hasActivated)
            return;

        if (target == null)
        {
            Debug.LogError("InteractableActivator: Target não configurado.", this);

            return;
        }

        IActivatable activatable = FindActivatable(target);

        if (activatable == null)
        {
            Debug.LogError(
                $"{target.name} não possui nenhum componente que implemente IActivatable.",
                this
            );

            return;
        }

        StartCoroutine(ActivateWithDelay(activatable));
    }

    private IEnumerator ActivateWithDelay(IActivatable activatable)
    {
        isActivating = true;
        hasActivated = true;

        if (activationDelay > 0f)
        {
            yield return new WaitForSeconds(activationDelay);
        }

        activatable.Activate();

        isActivating = false;
    }

    private IActivatable FindActivatable(GameObject targetObject)
    {
        MonoBehaviour[] components = targetObject.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is IActivatable activatable)
                return activatable;
        }

        return null;
    }
}
