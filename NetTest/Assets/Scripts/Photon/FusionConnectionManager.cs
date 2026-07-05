using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Addons.Physics;

public class FusionConnectionManager : MonoBehaviour
{
    private NetworkRunner _runner;

    private void Start()
    {
        // Le damos un pequeñísimo margen (0.5s) para asegurarnos de que CommandReader 
        // haya tenido tiempo de leer los comandos del .bat antes de iniciar la red.
        Invoke(nameof(ConectarSegunArgumentos), 0.5f);
    }

    private async void ConectarSegunArgumentos()
    {
        if (CommandReader.Instance == null)
        {
            Debug.LogError("<color=red>[Fusion]</color> No se encontró CommandReader.");
            return;
        }

        CommandReader.StartupMode mode = CommandReader.Instance.currentMode;

        if (mode == CommandReader.StartupMode.Host)
        {
            Debug.Log("<color=blue>[Fusion]</color> Iniciando como HOST...");
            await IniciarSesion(GameMode.Host);
        }
        else if (mode == CommandReader.StartupMode.Client)
        {
            Debug.Log("<color=blue>[Fusion]</color> Iniciando como CLIENTE...");
            await IniciarSesion(GameMode.Client);
        }
        else
        {
            Debug.Log("<color=yellow>[Fusion]</color> Modo Manual. Puedes poner botones en UI para probar en el Editor.");
        }
    }

    private async Task IniciarSesion(GameMode gameMode)
    {
        // 1. Configuramos el NetworkRunner (El núcleo de Fusion)
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();

        if (gameObject.GetComponent<RunnerSimulatePhysics3D>() == null)
        {
            gameObject.AddComponent<RunnerSimulatePhysics3D>();
        }

        _runner.AddCallbacks(GetComponent<FusionPlayerSpawner>());

        // 2. Fusion necesita saber cómo manejar las escenas de Unity
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        // 3. Preparamos los argumentos de la partida
        var startGameArgs = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "ExperimentoTFG", // Nombre fijo para que Bot y Host siempre se encuentren
            SceneManager = sceneManager
        };

        // 4. ¡Arrancamos Fusion!
        StartGameResult result = await _runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            Debug.Log($"<color=green>[Fusion]</color> ¡Conexión exitosa! Modo: {gameMode} | Sala: ExperimentoTFG");
        }
        else
        {
            Debug.LogError($"<color=red>[Fusion]</color> Error al conectar: {result.ShutdownReason}");
        }
    }
}