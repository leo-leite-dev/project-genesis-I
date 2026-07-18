using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private PlayerStatsData data;

    public float MoveSpeed { get; private set; }

    private void Awake()
    {
        MoveSpeed = data.baseMoveSpeed;
    }
}
