using UnityEditor;
using UnityEngine;

public class PlayableAreaOptimizer : MonoBehaviour
{
    [Header("Área jogável")]
    [SerializeField]
    private BoxCollider playableArea;

    [Header("Objetos que serão analisados")]
    [SerializeField]
    private Transform environmentRoot;

    [Header("Margem visual além da área jogável")]
    [Min(0f)]
    [SerializeField]
    private float visualMargin = 20f;

    [Header("Configurações")]
    [SerializeField]
    private bool includeInactiveObjects = true;

    public void DisableOutsideArea()
    {
        if (playableArea == null || environmentRoot == null)
        {
            Debug.LogError("Defina o BoxCollider da área jogável e a raiz do cenário.");
            return;
        }

        Bounds areaBounds = playableArea.bounds;
        areaBounds.Expand(visualMargin * 2f);

        Transform[] objects = environmentRoot.GetComponentsInChildren<Transform>(
            includeInactiveObjects
        );

        int enabledCount = 0;
        int disabledCount = 0;

        foreach (Transform currentTransform in objects)
        {
            GameObject currentObject = currentTransform.gameObject;

            if (currentTransform == environmentRoot)
                continue;

            if (currentObject == playableArea.gameObject)
                continue;

            Renderer[] renderers = currentObject.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
                continue;

            Bounds objectBounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                objectBounds.Encapsulate(renderers[i].bounds);
            }

            bool isInside = areaBounds.Intersects(objectBounds);

            Undo.RecordObject(currentObject, "Optimize playable area");

            currentObject.SetActive(isInside);

            if (isInside)
                enabledCount++;
            else
                disabledCount++;
        }

        Debug.Log(
            $"Otimização concluída. "
                + $"{enabledCount} objetos mantidos e "
                + $"{disabledCount} objetos desativados."
        );
    }

    public void EnableEverything()
    {
        if (environmentRoot == null)
        {
            Debug.LogError("Defina a raiz do cenário.");
            return;
        }

        Transform[] objects = environmentRoot.GetComponentsInChildren<Transform>(true);

        int enabledCount = 0;

        foreach (Transform currentTransform in objects)
        {
            if (currentTransform == environmentRoot)
                continue;

            GameObject currentObject = currentTransform.gameObject;

            Undo.RecordObject(currentObject, "Restore environment");

            currentObject.SetActive(true);
            enabledCount++;
        }

        Debug.Log($"{enabledCount} objetos foram reativados.");
    }
}

[CustomEditor(typeof(PlayableAreaOptimizer))]
public class PlayableAreaOptimizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayableAreaOptimizer optimizer = (PlayableAreaOptimizer)target;

        GUILayout.Space(12);

        if (GUILayout.Button("Desativar objetos fora da área"))
            optimizer.DisableOutsideArea();

        if (GUILayout.Button("Restaurar todos os objetos"))
            optimizer.EnableEverything();
    }
}
