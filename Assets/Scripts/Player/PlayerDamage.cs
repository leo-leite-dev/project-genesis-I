using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimation))]
public class PlayerDamage : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private PlayerAnimation playerAnimation;

    [Header("Invincibility")]
    [SerializeField]
    private float invincibilityDuration = 0.8f;

    [Header("Knockback")]
    [SerializeField]
    private float knockbackForce = 3f;

    [SerializeField]
    private float knockbackUpForce = 1.2f;

    [SerializeField]
    private float knockbackDuration = 0.18f;

    [Header("Hit Flash")]
    [SerializeField]
    private Color flashColor = Color.white;

    [SerializeField]
    private int flashCount = 3;

    [SerializeField]
    private float flashInterval = 0.08f;

    private Renderer[] renderers;
    private MaterialPropertyBlock[] originalPropertyBlocks;

    private Coroutine damageRoutine;

    private bool isInvincible;

    public bool IsInvincible => isInvincible;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();

        playerMovement = GetComponent<PlayerMovement>();

        playerAnimation = GetComponent<PlayerAnimation>();

        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void TakeDamage(int amount, Vector3 damageSourcePosition)
    {
        if (amount <= 0)
            return;

        if (isInvincible)
            return;

        isInvincible = true;

        playerStats.TakeDamage(amount);

        playerAnimation.PlayHit();

        ApplyKnockback(damageSourcePosition);

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamageRoutine());
    }

    private void ApplyKnockback(Vector3 damageSourcePosition)
    {
        Vector3 direction = transform.position - damageSourcePosition;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            direction = -transform.forward;

        playerMovement.ApplyKnockback(
            direction,
            knockbackForce,
            knockbackUpForce,
            knockbackDuration
        );
    }

    private IEnumerator DamageRoutine()
    {
        CaptureOriginalPropertyBlocks();

        float flashTime = 0f;

        for (int i = 0; i < flashCount; i++)
        {
            SetFlash(true);

            yield return new WaitForSeconds(flashInterval);

            flashTime += flashInterval;

            SetFlash(false);

            yield return new WaitForSeconds(flashInterval);

            flashTime += flashInterval;
        }

        RestoreOriginalPropertyBlocks();

        float remainingInvincibility = invincibilityDuration - flashTime;

        if (remainingInvincibility > 0f)
            yield return new WaitForSeconds(remainingInvincibility);

        isInvincible = false;
        damageRoutine = null;
    }

    private void CaptureOriginalPropertyBlocks()
    {
        originalPropertyBlocks = new MaterialPropertyBlock[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            MaterialPropertyBlock block = new MaterialPropertyBlock();

            renderers[i].GetPropertyBlock(block);

            originalPropertyBlocks[i] = block;
        }
    }

    private void SetFlash(bool active)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer currentRenderer = renderers[i];

            if (currentRenderer == null)
                continue;

            if (!active)
            {
                if (originalPropertyBlocks != null && i < originalPropertyBlocks.Length)
                    currentRenderer.SetPropertyBlock(originalPropertyBlocks[i]);

                continue;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();

            currentRenderer.GetPropertyBlock(block);

            block.SetColor(BaseColorId, flashColor);

            block.SetColor(ColorId, flashColor);

            currentRenderer.SetPropertyBlock(block);
        }
    }

    private void RestoreOriginalPropertyBlocks()
    {
        if (originalPropertyBlocks == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            renderers[i].SetPropertyBlock(originalPropertyBlocks[i]);
        }
    }

    private void OnDisable()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);

            damageRoutine = null;
        }

        RestoreOriginalPropertyBlocks();

        isInvincible = false;
    }
}
