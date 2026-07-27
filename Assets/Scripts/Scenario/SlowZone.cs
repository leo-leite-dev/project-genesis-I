using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)]
    private float slowMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.ApplySlow(slowMultiplier);
            Debug.Log("Entrou");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.RemoveSlow();
            Debug.Log("Saiu");
        }
    }
}
