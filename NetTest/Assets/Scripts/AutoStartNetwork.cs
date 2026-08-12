using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class AutoStartNetwork : MonoBehaviour
{
    void Start()
    {
        if (Application.isEditor)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Iniciado como Host (Editor)");
            return;
        }

        string[] args = System.Environment.GetCommandLineArgs();
        bool isClient = false;
        string ipAddress = "127.0.0.1";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-client") isClient = true;

            if (args[i] == "-serverIP" && i + 1 < args.Length)
            {
                ipAddress = args[i + 1];
            }
        }

        if (isClient)
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (isClient)
            {
                // AÑADIMOS "0.0.0.0" COMO TERCER PARÁMETRO
                transport.SetConnectionData(ipAddress, (ushort)7777, "0.0.0.0");
                NetworkManager.Singleton.StartClient();
                Debug.Log($"Iniciado como Cliente Remoto. Conectando a IP: {ipAddress}");
            }
        }
        else
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Iniciado como Host");
        }
    }
}