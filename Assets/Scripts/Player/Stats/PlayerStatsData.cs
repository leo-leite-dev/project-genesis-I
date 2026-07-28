using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Player/Player Stats Data")]
public class PlayerStatsData : ScriptableObject
{
    [Header("Movement")]
    public float baseMoveSpeed = 5f;

    [Header("Health")]
    public int baseHearts = 3;

    [Header("Lives")]
    public int baseLife = 5;
}
