using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin")]
    [SerializeField]
    private int value = 1;

    [Header("Audio")]
    [SerializeField]
    private AudioClip collectSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float collectSoundVolume = 1f;

    [Header("Collect Animation")]
    [SerializeField]
    private float riseHeight = 1f;

    [SerializeField]
    private float riseDuration = 0.35f;

    [SerializeField]
    private float collectRotationSpeedY = 720f;

    [SerializeField]
    private float collectScale = 1.2f;

    [SerializeField]
    private float cometDelay = 0.08f;

    [Header("Collect Effects")]
    [SerializeField]
    private ParticleSystem collectEffect;

    [SerializeField]
    private float collectEffectHeightOffset = 0.3f;

    private bool collected;

    private Collider coinCollider;
    private ObjectRotator objectRotator;

    private void Awake()
    {
        coinCollider = GetComponent<Collider>();
        objectRotator = GetComponent<ObjectRotator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerScore playerScore = other.GetComponent<PlayerScore>();

        if (playerScore == null)
            return;

        collected = true;

        if (coinCollider != null)
            coinCollider.enabled = false;

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);

        StartCoroutine(CollectSequence(playerScore));
    }

    private IEnumerator CollectSequence(PlayerScore playerScore)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.up * riseHeight;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * collectScale;

        if (objectRotator != null)
            objectRotator.RotationSpeedY = collectRotationSpeedY;

        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / riseDuration);

            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPosition, endPosition, easedT);

            transform.localScale = Vector3.Lerp(startScale, endScale, easedT);

            yield return null;
        }

        transform.position = endPosition;
        transform.localScale = endScale;

        Vector3 transitionPosition = transform.position;

        HideCoin();

        SpawnCollectEffect(transitionPosition);

        if (cometDelay > 0f)
            yield return new WaitForSeconds(cometDelay);

        CollectCoinsAnim collectAnim = FindFirstObjectByType<CollectCoinsAnim>();

        if (collectAnim != null)
        {
            collectAnim.PlayAnim(value, transitionPosition);
        }

        playerScore.AddCoins(value);

        Destroy(gameObject);
    }

    private void HideCoin()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
            renderer.enabled = false;
    }

    private void SpawnCollectEffect(Vector3 position)
    {
        if (collectEffect == null)
            return;

        Vector3 effectPosition = position + Vector3.up * collectEffectHeightOffset;

        ParticleSystem effect = Instantiate(collectEffect, effectPosition, Quaternion.identity);

        effect.Play();

        ParticleSystem.MainModule main = effect.main;

        float destroyDelay = main.duration + main.startLifetime.constantMax;

        Destroy(effect.gameObject, destroyDelay);
    }
}
