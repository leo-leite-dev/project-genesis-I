using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ObjectHit))]
public class MineDropper : MonoBehaviour
{
    private Transform player;
    public Transform Player => player;

    private Rigidbody rb;

    [SerializeField]
    private float warningDelay = 0.35f;

    [Header("Explosão")]
    [SerializeField]
    private GameObject explosionPrefab;

    [SerializeField]
    private string groundTag = "Inflatable";

    [SerializeField]
    private string playerTag = "Player";

    [SerializeField]
    private float explosionLifetime = 2f;

    private bool hasDropped;
    private bool hasExploded;

    private void Awake()
    {
        SetupRigidbody();
        FindPlayer();
    }

    private void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = true;
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    public void ActivateDrop()
    {
        if (hasDropped)
            return;

        StartCoroutine(Drop());
    }

    private IEnumerator Drop()
    {
        hasDropped = true;

        yield return new WaitForSeconds(warningDelay);

        rb.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded)
            return;

        if (
            collision.gameObject.CompareTag(groundTag) || collision.gameObject.CompareTag(playerTag)
        )
            Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(explosion, explosionLifetime);
        }

        Destroy(gameObject);
    }
}
