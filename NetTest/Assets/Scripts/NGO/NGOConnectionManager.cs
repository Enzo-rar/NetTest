using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NGOConnectionManager : MonoBehaviour
{
    void Start()
    {
        // Esto es lo único que hará el script: cuando empiece, configura y conecta.
        Invoke(nameof(ConfigurarYConectar), 0.5f);
    }

    void ConfigurarYConectar()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var reader = CommandReader.Instance;

        // Inyección directa de IP y Puerto
        transport.ConnectionData.Address = reader.targetIP;
        transport.ConnectionData.Port = 7777;

        if (reader.currentMode == CommandReader.StartupMode.Host)
        {
            transport.ConnectionData.Address = "0.0.0.0";
            NetworkManager.Singleton.StartHost();
            Debug.Log($"[NGO] Iniciado como HOST en 0.0.0.0:7777");
        }
        else if (reader.currentMode == CommandReader.StartupMode.Client)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log($"[NGO] Iniciado como CLIENTE hacia {reader.targetIP}:7777");
        }
    }
}