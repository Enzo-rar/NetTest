using UnityEngine;
using FishNet;
using FishNet.Connection;

public class CustomPlayerSpawner : MonoBehaviour
{
    [Tooltip("Prefab que se instanciará para el Host (AWS/Local).")]
    public GameObject hostPrefab;

    [Tooltip("Prefab que se instanciará para el Tirador (Cliente Remoto).")]
    public GameObject clientePrefab;

    void Start()
    {
        // Nos suscribimos al evento global a través de InstanceFinder
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }
    }

    void OnDestroy()
    {
        // Limpiamos el evento al destruir el objeto para evitar fugas de memoria
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }
    }

    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        // Solo queremos que el servidor ejecute la lógica de instanciación
        if (!asServer) return;

        GameObject prefabASpawnear;
        Vector3 posicionSpawn;
        Quaternion rotacionSpawn = Quaternion.identity;

        // Comprobamos si la conexión es la del anfitrión (Host) o la del cliente remoto
        if (conn.IsLocalClient)
        {
            // Es el Host. Lo colocamos detrás del cliente remoto.
            prefabASpawnear = hostPrefab;
            posicionSpawn = new Vector3(0, 1f, -5f); // 5 unidades por detrás
            Debug.Log("[Spawner] Instanciando al Host.");
        }
        else
        {
            // Es el Cliente Remoto (Tirador). Lo colocamos en su posición exacta (0, 1, 0)
            prefabASpawnear = clientePrefab;
            posicionSpawn = new Vector3(0, 1f, 0);
            Debug.Log("[Spawner] Instanciando al Cliente Remoto (Tirador).");
        }

        // Instanciamos el objeto físico en el servidor
        GameObject jugador = Instantiate(prefabASpawnear, posicionSpawn, rotacionSpawn);

        // Le damos autoridad sobre el objeto a la conexión que acaba de entrar y lo propagamos
        InstanceFinder.ServerManager.Spawn(jugador, conn);
    }
}