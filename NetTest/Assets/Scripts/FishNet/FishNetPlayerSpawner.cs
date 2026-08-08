using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class FishNetPlayerSpawner : NetworkBehaviour
{
    [Header("Prefabs Diferenciados")]
    public NetworkObject hostPrefab;
    public NetworkObject clientPrefab;

    [Header("Puntos de Aparición")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    [Header("Referencias de la Escena")]
    [Tooltip("Arrastra aquí el objeto de la escena que tiene el script SmartFloor")]
    public SmartFloor sueloInteligente;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.SceneManager.OnClientLoadedStartScenes += SpawnPlayer;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnClientLoadedStartScenes -= SpawnPlayer;
        }
    }

    private void SpawnPlayer(NetworkConnection conn, bool asServer)
    {
        if (!asServer) return;

        NetworkObject prefabToSpawn;
        Transform spawnPoint;

        // Si es el Host (Server)
        if (conn.IsLocalClient)
        {
            prefabToSpawn = hostPrefab;
            spawnPoint = hostSpawnPoint;
            Debug.Log("<color=green>[Spawner]</color> Spawneando Host estático arriba.");
        }
        // Si es el Cliente remoto
        else
        {
            prefabToSpawn = clientPrefab;
            spawnPoint = clientSpawnPoint;
            Debug.Log("<color=cyan>[Spawner]</color> Spawneando Cliente dinámico en el suelo.");
        }

        NetworkObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        InstanceFinder.ServerManager.Spawn(spawnedObj, conn);

        // --- LA MAGIA DEL SUELO (ACTUALIZADA) ---
        if (!conn.IsLocalClient && sueloInteligente != null)
        {
            // Buscamos específicamente al hijo llamado "PlayerBody"
            Transform body = spawnedObj.transform.Find("PlayerBody");

            if (body != null)
            {
                sueloInteligente.targetPlayer = body;
                Debug.Log("<color=magenta>[Spawner]</color> Suelo de red vinculado al PlayerBody del Cliente.");
            }
            else
            {
                // Fallback por seguridad por si le cambias el nombre en el futuro
                sueloInteligente.targetPlayer = spawnedObj.transform;
                Debug.LogWarning("<color=yellow>[Spawner]</color> No se encontró 'PlayerBody', vinculando a la raíz del prefab.");
            }
        }
    }
}