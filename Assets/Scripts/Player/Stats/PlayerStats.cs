using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private PlayerStatsData data;

    public float MoveSpeed { get; private set; }
    public float BaseMoveSpeed => data.baseMoveSpeed;

    public int CurrentHearts { get; private set; }
    public int CurrentLife { get; private set; }

    public bool IsSlowed { get; private set; }

    public event Action<int> OnHeartsChanged;
    public event Action<int> OnLifeChanged;

    private void Awake()
    {
        MoveSpeed = data.baseMoveSpeed;

        CurrentHearts = data.baseHearts;
        CurrentLife = data.baseLife;
    }

    public void ApplySlow(float multiplier)
    {
        MoveSpeed = data.baseMoveSpeed * multiplier;
        IsSlowed = true;
    }

    public void RemoveSlow()
    {
        MoveSpeed = data.baseMoveSpeed;
        IsSlowed = false;
    }

    public void AddLife(int amount)
    {
        if (amount <= 0)
            return;

        CurrentLife += amount;

        OnLifeChanged?.Invoke(CurrentLife);

        Debug.Log($"Vida adicionada! Vidas atuais: {CurrentLife}");
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        CurrentHearts -= amount;

        if (CurrentHearts <= 0)
        {
            CurrentHearts = 0;

            OnHeartsChanged?.Invoke(CurrentHearts);

            LoseLife();

            return;
        }

        OnHeartsChanged?.Invoke(CurrentHearts);

        Debug.Log($"Dano recebido: {amount}! Corações: {CurrentHearts}");
    }

    private void LoseLife()
    {
        CurrentLife--;

        if (CurrentLife < 0)
            CurrentLife = 0;

        OnLifeChanged?.Invoke(CurrentLife);

        if (CurrentLife <= 0)
        {
            Debug.Log("Game Over!");
            return;
        }

        CurrentHearts = data.baseHearts;

        OnHeartsChanged?.Invoke(CurrentHearts);

        Debug.Log($"Vida perdida! Vidas restantes: {CurrentLife}");
    }
}
