using System.Collections;
using UnityEngine;

public class ObjectFloating : MonoBehaviour
{
    [Header("Height")]
    [SerializeField]
    private float minY = -0.5f;

    [SerializeField]
    private float warningY = -0.26f;

    [SerializeField]
    private float maxY = 0.05f;

    [Header("Warning")]
    [SerializeField]
    private float warningUpSpeed = 0.25f;

    [SerializeField]
    private float warningPauseDuration = 0.4f;

    [Header("Attack")]
    [SerializeField]
    private float attackUpSpeed = 3f;

    [SerializeField]
    private float topPauseDuration = 0.25f;

    [Header("Return")]
    [SerializeField]
    private float downSpeed = 0.6f;

    [Header("Damage")]
    [SerializeField]
    private Collider damageCollider;

    private Coroutine floatingCoroutine;

    private void Awake()
    {
        SetDamageCollider(false);
    }

    private void OnEnable()
    {
        SetY(minY);
        SetDamageCollider(false);

        floatingCoroutine = StartCoroutine(FloatRoutine());
    }

    private void OnDisable()
    {
        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
            floatingCoroutine = null;
        }

        SetDamageCollider(false);
    }

    private IEnumerator FloatRoutine()
    {
        while (true)
        {
            SetDamageCollider(false);

            yield return MoveToY(warningY, warningUpSpeed);

            yield return new WaitForSeconds(warningPauseDuration);

            yield return MoveToY(maxY, attackUpSpeed);

            SetDamageCollider(true);

            yield return new WaitForSeconds(topPauseDuration);

            yield return MoveDown();
        }
    }

    private IEnumerator MoveDown()
    {
        while (transform.localPosition.y > minY)
        {
            Vector3 position = transform.localPosition;

            position.y = Mathf.MoveTowards(position.y, minY, downSpeed * Time.deltaTime);

            transform.localPosition = position;

            if (position.y <= warningY)
                SetDamageCollider(false);

            yield return null;
        }

        SetY(minY);
        SetDamageCollider(false);
    }

    private IEnumerator MoveToY(float targetY, float speed)
    {
        while (Mathf.Abs(transform.localPosition.y - targetY) > 0.0001f)
        {
            Vector3 position = transform.localPosition;

            position.y = Mathf.MoveTowards(position.y, targetY, speed * Time.deltaTime);

            transform.localPosition = position;

            yield return null;
        }

        SetY(targetY);
    }

    private void SetY(float y)
    {
        Vector3 position = transform.localPosition;
        position.y = y;

        transform.localPosition = position;
    }

    private void SetDamageCollider(bool active)
    {
        if (damageCollider != null)
            damageCollider.enabled = active;
    }
}
