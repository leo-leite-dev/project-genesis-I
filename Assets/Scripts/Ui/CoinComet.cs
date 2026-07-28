using System;
using System.Collections;
using UnityEngine;

public class CoinComet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float flyDuration = 0.55f;

    [SerializeField]
    private float minArcStrength = 0.12f;

    [SerializeField]
    private float maxArcStrength = 0.35f;

    [SerializeField]
    [Range(0f, 1f)]
    private float straightChance = 0.3f;

    [Header("Scale")]
    [SerializeField]
    private float startScale = 1f;

    [SerializeField]
    private float endScale = 0.35f;

    public void Fly(Vector3 startPosition, RectTransform target, Camera camera, Action onArrived)
    {
        transform.position = startPosition;

        StartCoroutine(FlyRoutine(startPosition, target, camera, onArrived));
    }

    private IEnumerator FlyRoutine(
        Vector3 startPosition,
        RectTransform target,
        Camera camera,
        Action onArrived
    )
    {
        Vector3 originalScale = transform.localScale;

        transform.localScale = originalScale * startScale;

        Vector3 startScreenPosition = camera.WorldToScreenPoint(startPosition);

        float worldDepth = startScreenPosition.z;

        float arcStrength = UnityEngine.Random.Range(minArcStrength, maxArcStrength);

        float arcDirection = 0f;

        if (UnityEngine.Random.value > straightChance)
            arcDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;

        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / flyDuration);

            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 targetScreenPosition = target.position;

            float x = Mathf.Lerp(startScreenPosition.x, targetScreenPosition.x, easedT);

            float y = Mathf.Lerp(startScreenPosition.y, targetScreenPosition.y, easedT);

            float arc = Mathf.Sin(t * Mathf.PI) * arcStrength * 100f * arcDirection;

            y += arc;

            transform.position = camera.ScreenToWorldPoint(new Vector3(x, y, worldDepth));

            transform.localScale = Vector3.Lerp(
                originalScale * startScale,
                originalScale * endScale,
                easedT
            );

            yield return null;
        }

        Vector3 finalTargetScreenPosition = target.position;

        transform.position = camera.ScreenToWorldPoint(
            new Vector3(finalTargetScreenPosition.x, finalTargetScreenPosition.y, worldDepth)
        );

        transform.localScale = originalScale * endScale;

        onArrived?.Invoke();

        Destroy(gameObject);
    }
}
