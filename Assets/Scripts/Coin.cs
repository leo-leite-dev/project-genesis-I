using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    private int value = 1;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerScore playerScore = other.GetComponent<PlayerScore>();

        if (playerScore != null)
        {
            playerScore.AddCoins(value);
            Destroy(gameObject);
        }
    }
}
