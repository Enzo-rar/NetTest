using UnityEngine;
using Unity.Netcode;

// ¡EL CAMBIO ESTÁ AQUÍ! Cambiamos NetworkBehaviour por MonoBehaviour
public class NGOSpawner : MonoBehaviour
{
    [Header("Prefabs de Red (Deben estar en la lista Network Prefabs)")]
    public GameObject hostPrefab;
    public GameObject clientPrefab;

    [Header("Puntos de Aparición")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AlConectarseCliente;
        }
    }

    public void OnDestroy() // Quita el "override" si lo tenías
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseCliente;
        }
    }

    private void AlConectarseCliente(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        GameObject prefabAElegir;
        Vector3 posicion;
        Quaternion rotacion;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            prefabAElegir = hostPrefab;
            posicion = hostSpawnPoint != null ? hostSpawnPoint.position : Vector3.zero;
            rotacion = hostSpawnPoint != null ? hostSpawnPoint.rotation : Quaternion.identity;
            Debug.Log($"<color=yellow>[Spawner]</color> Host instanciado.");
        }
        else
        {
            prefabAElegir = clientPrefab;
            posicion = clientSpawnPoint != null ? clientSpawnPoint.position : Vector3.zero;
            rotacion = clientSpawnPoint != null ? clientSpawnPoint.rotation : Quaternion.identity;
            Debug.Log($"<color=cyan>[Spawner]</color> Cliente {clientId} instanciado en la salida.");
        }

        GameObject instancia = Instantiate(prefabAElegir, posicion, rotacion);

        NetworkObject netObj = instancia.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnAsPlayerObject(clientId);
        }
    }
}