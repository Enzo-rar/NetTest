using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NGOConnectionManager : MonoBehaviour
{
    // Esta será la IP "por defecto" si lo abres desde el editor de Unity sin el .bat
    private const string IP_POR_DEFECTO = "18.231.36.49";
    private const ushort PUERTO = 7777;

    void Start()
    {
        Invoke(nameof(IniciarConexion), 0.5f);
    }

    void IniciarConexion()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // 1. Obtenemos la IP desde los argumentos (si existe), si no, usamos la por defecto
        string ipDestino = IP_POR_DEFECTO;
        if (CommandReader.Instance != null && CommandReader.Instance.targetIP != "127.0.0.1")
        {
            ipDestino = CommandReader.Instance.targetIP;
        }

        // 2. Configuramos el transporte con la IP dinámica
        transport.SetConnectionData(ipDestino, PUERTO, "0.0.0.0");

        if (CommandReader.Instance != null)
        {
            switch (CommandReader.Instance.currentMode)
            {
                case CommandReader.StartupMode.DedicatedServer:
                    NetworkManager.Singleton.StartServer();
                    Debug.Log($"[NGO] ARRANCANDO COMO SERVIDOR DEDICADO EN PUERTO {PUERTO}");
                    break;

                case CommandReader.StartupMode.Host:
                    // En modo host local, pisamos la IP para forzar localhost
                    transport.SetConnectionData("127.0.0.1", PUERTO, "0.0.0.0");
                    NetworkManager.Singleton.StartHost();
                    Debug.Log($"[NGO] ARRANCANDO COMO HOST EN LOCALHOST");
                    break;

                case CommandReader.StartupMode.Client:
                    NetworkManager.Singleton.StartClient();
                    Debug.Log($"[NGO] ARRANCANDO COMO CLIENTE HACIA {ipDestino}:{PUERTO}");
                    break;
            }
        }
    }
}