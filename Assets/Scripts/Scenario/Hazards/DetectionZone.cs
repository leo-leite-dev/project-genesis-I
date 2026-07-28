using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [SerializeField]
    private MineDropper mineDropper;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        mineDropper.ActivateDrop();
    }
}
