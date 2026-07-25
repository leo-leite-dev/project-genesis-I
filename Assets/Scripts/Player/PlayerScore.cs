using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int Coins { get; private set; }

    [SerializeField]
    private int nextLifeAt = 100;
    private PlayerStats playerStats;

    public void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;

        if (Coins >= nextLifeAt)
        {
            playerStats.AddLife(1);
            nextLifeAt += 100;
        }

        Debug.Log($"Vida: {playerStats.CurrentLife} => Moedas: {Coins}");
    }
}
