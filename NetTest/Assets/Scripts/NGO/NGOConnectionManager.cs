using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;

public class NGOConnectionManager : MonoBehaviour
{
    void Start()
    {
        // Si estamos en el Editor de Unity, arrancamos como Host por defecto para probar rápido
#if UNITY_EDITOR
        Debug.Log("<color=yellow>[NGO]</color> Modo Editor: Arrancando Host en local.");
        NetworkManager.Singleton.StartHost();
        return;
#endif

        // Si es una Build, leemos el .bat
        ConectarDesdeArgumentos();
    }

    private void ConectarDesdeArgumentos()
    {
        string[] args = Environment.GetCommandLineArgs();
        bool isHost = false;
        bool isClient = false;
        string ipDestino = "127.0.0.1";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-mode" && i + 1 < args.Length)
            {
                if (args[i + 1] == "host") isHost = true;
                if (args[i + 1] == "client") isClient = true;
            }
            if (args[i] == "-ip" && i + 1 < args.Length)
            {
                ipDestino = args[i + 1];
            }
        }

        if (isHost)
        {
            Debug.Log("<color=yellow>[NGO]</color> Arrancando HOST");
            NetworkManager.Singleton.StartHost();
        }
        else if (isClient)
        {
            Debug.Log($"<color=cyan>[NGO]</color> Arrancando CLIENTE hacia {ipDestino}");

            // Le inyectamos la IP al UnityTransport antes de conectar
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = ipDestino;
            }
            NetworkManager.Singleton.StartClient();
        }
    }
}
