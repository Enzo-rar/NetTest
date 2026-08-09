using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

// Añadimos INetworkRunnerCallbacks para enterarnos de quién se conecta
public class NetworkStarter : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [Header("Configuración en Editor")]
    [Tooltip("Elige cómo se iniciará el juego al darle al Play en el Editor")]
    public GameMode modoEnEditor = GameMode.Host;

    async void Start()
    {
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();

        _runner.AddCallbacks(this);

        // Por defecto será Cliente para las builds ejecutables...
        GameMode mode = GameMode.Client;

        // ... PERO si estamos dentro del editor de Unity, usamos lo que hayas puesto en el Inspector
        if (Application.isEditor)
        {
            mode = modoEnEditor;
        }

        // Leemos los argumentos de la consola (esto sobrescribirá lo anterior si lanzas un .bat)
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-mode" && i + 1 < args.Length)
            {
                if (args[i + 1].ToLower() == "host") mode = GameMode.Host;
                else if (args[i + 1].ToLower() == "client") mode = GameMode.Client;
            }
        }

        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);

        Debug.Log($"[NetworkStarter] Intentando iniciar Fusion en modo: {mode}");

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = sceneInfo,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log($"[NetworkStarter] Fusion iniciado exitosamente como: {mode}");
    }

    // --- CALLBACKS DE FUSION ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[NetworkStarter] Jugador conectado. ID: {player.PlayerId}");

        // Si somos el servidor, le damos autoridad de entrada al cliente sobre el tirador
        if (runner.IsServer)
        {
            var tirador = FindFirstObjectByType<TiradorNetworked>();
            if (tirador != null)
            {
                tirador.Object.AssignInputAuthority(player);
                Debug.Log($"[NetworkStarter] Autoridad del Tirador asignada al jugador {player.PlayerId}");
            }
            else
            {
                Debug.LogError("[NetworkStarter] ¡No se encontró el TiradorNetworked en la escena para asignarle autoridad!");
            }
        }
    }

    // Métodos obligatorios de la interfaz (los dejamos vacíos por ahora)
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}