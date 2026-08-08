using FishNet;
using UnityEngine;
using System.Collections;

public class FishNetConnectionManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("<color=orange>[Traza]</color> 1. FishNetConnectionManager: Awake ejecutado. El script está activo en la escena.");
    }

    private void Start()
    {
        Debug.Log("<color=orange>[Traza]</color> 2. FishNetConnectionManager: Start ejecutado. Iniciando cuenta atrás de 0.5s...");
        StartCoroutine(EsperarYConectar());
    }

    private IEnumerator EsperarYConectar()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("<color=orange>[Traza]</color> 3. FishNetConnectionManager: Tiempo finalizado. Buscando CommandReader...");
        ConectarSegunArgumentos();
    }

    private void ConectarSegunArgumentos()
    {
        if (CommandReader.Instance == null)
        {
            Debug.LogError("<color=red>[Traza ERROR]</color> No se encontró CommandReader.Instance. ¿Está el objeto en la escena?");
            return;
        }

        CommandReader.StartupMode mode = CommandReader.Instance.currentMode;
        Debug.Log($"<color=orange>[Traza]</color> 4. FishNetConnectionManager: Modo detectado -> {mode}");

        if (mode == CommandReader.StartupMode.Host)
        {
            Debug.Log("<color=blue>[FishNet]</color> Iniciando como HOST (Servidor + Cliente local)...");
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        }
        else if (mode == CommandReader.StartupMode.Client)
        {
            Debug.Log($"<color=blue>[FishNet]</color> Iniciando como CLIENTE hacia {CommandReader.Instance.targetIP}...");
            InstanceFinder.ClientManager.StartConnection(CommandReader.Instance.targetIP);
        }
        else
        {
            Debug.LogWarning("<color=yellow>[FishNet]</color> Modo Manual detectado. No se conectará a la red automáticamente.");
        }
    }
}