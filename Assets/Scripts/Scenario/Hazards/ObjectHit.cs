using UnityEngine;

public class ObjectHit : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerDamage playerDamage = collision.collider.GetComponentInParent<PlayerDamage>();

        if (playerDamage == null)
            return;

        playerDamage.TakeDamage(damage, transform.position);
    }
}
