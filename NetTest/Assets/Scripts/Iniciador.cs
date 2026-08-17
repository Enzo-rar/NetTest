using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class IniciadorNGO : MonoBehaviour
{
    void Start()
    {
        if (Application.isEditor)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("[IniciadorNGO] Iniciado como Host (Editor)");
            return;
        }

        string[] args = System.Environment.GetCommandLineArgs();
        bool isClient = false;
        string ipAddress = "127.0.0.1";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower() == "-client") isClient = true;

            if (args[i].ToLower() == "-direccionip" && i + 1 < args.Length)
            {
                ipAddress = args[i + 1];
            }
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (isClient)
        {
            // El cliente remoto se conecta a la IP de AWS. No necesita ServerListenAddress.
            transport.SetConnectionData(ipAddress, 7777);
            NetworkManager.Singleton.StartClient();
            Debug.Log($"[IniciadorNGO] Iniciado como Cliente Remoto. Conectando a IP: {ipAddress}:7777");
        }
        else
        {
            // EL FIX ESTÁ AQUÍ: 
            // 127.0.0.1 -> Dónde se conecta el cliente local del Host.
            // 0.0.0.0   -> Dónde escucha el servidor autoritativo.
            transport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");
            NetworkManager.Singleton.StartHost();
            Debug.Log("[IniciadorNGO] Iniciado como Host en AWS. Escuchando en 0.0.0.0:7777");
        }
    }
}