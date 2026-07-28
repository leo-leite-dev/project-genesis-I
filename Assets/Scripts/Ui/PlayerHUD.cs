using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private PlayerScore playerScore;

    [Header("Coins")]
    [SerializeField]
    private TMP_Text coinText;

    [SerializeField]
    private RectTransform coinIcon;

    [Header("Lives")]
    [SerializeField]
    private TMP_Text lifeText;

    [Header("Hearts")]
    [SerializeField]
    private Image[] heartImages;

    [SerializeField]
    private Sprite fullHeartSprite;

    [SerializeField]
    private Sprite emptyHeartSprite;

    [Header("Coin Feedback")]
    [SerializeField]
    private float coinPulseScale = 1.2f;

    [SerializeField]
    private float coinPulseDuration = 0.12f;

    private int displayedCoins;
    private int targetCoins;

    private Coroutine coinPulseRoutine;

    private void OnEnable()
    {
        if (playerScore != null)
            playerScore.OnCoinsChanged += OnCoinsChanged;

        if (playerStats != null)
        {
            playerStats.OnLifeChanged += UpdateLives;
            playerStats.OnHeartsChanged += UpdateHearts;
        }
    }

    private void Start()
    {
        if (playerScore != null)
        {
            displayedCoins = playerScore.Coins;
            targetCoins = playerScore.Coins;

            UpdateCoinText();
        }

        if (playerStats != null)
        {
            UpdateLives(playerStats.CurrentLife);
            UpdateHearts(playerStats.CurrentHearts);
        }
    }

    private void OnDisable()
    {
        if (playerScore != null)
            playerScore.OnCoinsChanged -= OnCoinsChanged;

        if (playerStats != null)
        {
            playerStats.OnLifeChanged -= UpdateLives;
            playerStats.OnHeartsChanged -= UpdateHearts;
        }
    }

    private void OnCoinsChanged(int amount)
    {
        targetCoins = amount;
    }

    public void CoinArrived(int amount)
    {
        displayedCoins += amount;

        if (displayedCoins > targetCoins)
            displayedCoins = targetCoins;

        UpdateCoinText();

        if (coinIcon != null)
        {
            if (coinPulseRoutine != null)
                StopCoroutine(coinPulseRoutine);

            coinPulseRoutine = StartCoroutine(PulseCoinIcon());
        }
    }

    private void UpdateCoinText()
    {
        coinText.text = $"X {displayedCoins}";
    }

    private void UpdateLives(int amount)
    {
        lifeText.text = $"{amount}";
    }

    private void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < heartImages.Length; i++)
            heartImages[i].sprite = i < currentHearts ? fullHeartSprite : emptyHeartSprite;
    }

    private IEnumerator PulseCoinIcon()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 enlargedScale = originalScale * coinPulseScale;

        float halfDuration = coinPulseDuration * 0.5f;

        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / halfDuration);

            coinIcon.localScale = Vector3.Lerp(originalScale, enlargedScale, t);

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / halfDuration);

            coinIcon.localScale = Vector3.Lerp(enlargedScale, originalScale, t);

            yield return null;
        }

        coinIcon.localScale = originalScale;

        coinPulseRoutine = null;
    }
}
