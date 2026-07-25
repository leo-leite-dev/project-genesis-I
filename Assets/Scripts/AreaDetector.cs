// using UnityEngine;

// public class AreaDetector : MonoBehaviour
// {
//     [SerializeField]
//     private MineDropper mineDropper;

//     private void OnTriggerEnter(Collider other)
//     {
//         Debug.Log($"Algo entrou na área: {other.name}");

//         if (mineDropper == null)
//         {
//             Debug.LogError("MineDropper não foi configurado no Inspector.");
//             return;
//         }

//         if (other.transform != mineDropper.Player)
//         {
//             Debug.Log($"O objeto {other.name} não é o Player esperado.");
//             return;
//         }

//         Debug.Log("Player detectado. Ativando a queda da mina.");

//         mineDropper.ActivateDrop();
//     }
// }