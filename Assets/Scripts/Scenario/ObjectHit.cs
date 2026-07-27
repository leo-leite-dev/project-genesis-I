using UnityEngine;

public class ObjectHit : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerStats playerStats))
            playerStats.TakeDamage(damage);
    }
}
