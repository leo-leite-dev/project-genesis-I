using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private PlayerStatsData data;

    public float MoveSpeed { get; private set; }
    public float BaseMoveSpeed => data.baseMoveSpeed;
    public int CurrentLife { get; private set; }
    public bool IsSlowed { get; private set; }

    private void Awake()
    {
        MoveSpeed = data.baseMoveSpeed;
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
        CurrentLife += amount;

        Debug.Log($"Vida adicionada! Vida atual: {CurrentLife}");
    }

    public void TakeDamage(int amount)
    {
        CurrentLife -= amount;

        Debug.Log($"Voce levo um dano de: {amount}! Vida atual: {CurrentLife}");
    }
}
