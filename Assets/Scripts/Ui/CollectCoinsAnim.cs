using UnityEngine;

public class CollectCoinsAnim : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CoinComet cometPrefab;

    [SerializeField]
    private RectTransform coinTarget;

    [SerializeField]
    private PlayerHUD playerHUD;

    [Header("Audio")]
    [SerializeField]
    private AudioClip coinArrivedSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float coinArrivedVolume = 1f;

    [Header("Runtime")]
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void PlayAnim(int amount, Vector3 worldPosition)
    {
        if (amount <= 0)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || cometPrefab == null || coinTarget == null)
            return;

        CoinComet comet = Instantiate(cometPrefab, worldPosition, Quaternion.identity);

        comet.Fly(
            worldPosition,
            coinTarget,
            mainCamera,
            () =>
            {
                PlayCoinArrivedSound();

                if (playerHUD != null)
                    playerHUD.CoinArrived(amount);
            }
        );
    }

    private void PlayCoinArrivedSound()
    {
        if (coinArrivedSound == null)
            return;

        AudioSource.PlayClipAtPoint(
            coinArrivedSound,
            mainCamera.transform.position,
            coinArrivedVolume
        );
    }
}
