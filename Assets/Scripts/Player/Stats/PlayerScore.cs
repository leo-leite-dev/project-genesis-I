using System;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int Coins { get; private set; }

    [SerializeField]
    private int nextLifeAt = 100;

    private PlayerStats playerStats;

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        Debug.Log($"[PlayerScore] AddCoins chamado | amount = {amount} | antes = {Coins}");

        Coins += amount;

        Debug.Log($"[PlayerScore] depois = {Coins}");

        while (Coins >= nextLifeAt)
        {
            playerStats.AddLife(1);
            nextLifeAt += 100;
        }

        OnCoinsChanged?.Invoke(Coins);

        Debug.Log(
            $"Vidas: {playerStats.CurrentLife} | "
                + $"Corações: {playerStats.CurrentHearts} | "
                + $"Moedas: {Coins}"
        );
    }
}
