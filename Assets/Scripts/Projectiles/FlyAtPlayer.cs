using UnityEngine;

public class FlyAtPlayer : MonoBehaviour
{
    [SerializeField]
    float speed = 1.0f;

    [SerializeField]
    Transform playerTransform;

    Vector3 playerPositon;

    void Start()
    {
        playerPositon = playerTransform.transform.position;
    }

    void Update()
    {
        MoveToPlayer();
        DestroyWhenReached();
    }

    void MoveToPlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            playerPositon,
            Time.deltaTime * speed
        );
    }

    void DestroyWhenReached()
    {
        if (transform.position == playerPositon)
            Destroy(gameObject);
    }
}
