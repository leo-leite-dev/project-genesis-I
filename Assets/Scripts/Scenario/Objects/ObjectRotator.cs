using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeedX = 0f;

    [SerializeField]
    private float rotationSpeedY = 90f;

    [SerializeField]
    private float rotationSpeedZ = 0f;

    public float RotationSpeedY
    {
        get => rotationSpeedY;
        set => rotationSpeedY = value;
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.Rotate(
            rotationSpeedX * Time.deltaTime,
            rotationSpeedY * Time.deltaTime,
            rotationSpeedZ * Time.deltaTime
        );
    }
}
