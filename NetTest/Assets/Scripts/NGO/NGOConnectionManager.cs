using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NGOConnectionManager : MonoBehaviour
{
    [Header("Configuración de Red")]
    public ushort puertoBase = 7777;

    private void Start()
    {
        Invoke(nameof(IniciarConexion), 0.5f);
    }

    public void IniciarConexion()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // --- TRAZAS DE CONEXIÓN (NUEVO) ---
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log($"<color=green>[NGO TRAZA]</color> ¡ÉXITO! Cliente con ID {clientId} se ha conectado correctamente.");
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            Debug.Log($"<color=red>[NGO TRAZA]</color> DESCONEXIÓN. El cliente con ID {clientId} se ha desconectado o no ha podido entrar.");
        };
        // ---------------------------------

        if (CommandReader.Instance.currentMode == CommandReader.StartupMode.DedicatedServer)
        {
            transport.SetConnectionData("127.0.0.1", puertoBase, "0.0.0.0");
            NetworkManager.Singleton.StartServer();
            Debug.Log($"[NGO] ARRANCANDO COMO SERVIDOR DEDICADO EN PUERTO {puertoBase} (ESCUCHANDO EN 0.0.0.0)");
        }
        else if (CommandReader.Instance.currentMode == CommandReader.StartupMode.Client)
        {
            string targetIP = CommandReader.Instance.targetIP;
            transport.SetConnectionData(targetIP, puertoBase);
            NetworkManager.Singleton.StartClient();
            Debug.Log($"[NGO] ARRANCANDO COMO CLIENTE. CONECTANDO A IP: {targetIP}:{puertoBase}");
        }
        else if (CommandReader.Instance.currentMode == CommandReader.StartupMode.Host)
        {
            transport.SetConnectionData("127.0.0.1", puertoBase, "0.0.0.0");
            NetworkManager.Singleton.StartHost();
            Debug.Log("[NGO] ARRANCANDO COMO HOST LOCAL");
        }
    }
}