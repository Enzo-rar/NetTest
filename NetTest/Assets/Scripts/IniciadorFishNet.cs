using FishNet;
using UnityEngine;
using System;

public class IniciadorFishNet : MonoBehaviour
{
    void Start()
    {
        string[] argumentos = Environment.GetCommandLineArgs();
        bool startHost = false;
        bool startClient = false;

        // Ponemos localhost por defecto por si alguna vez lo abres sin el .bat
        string serverAddress = "localhost";

        for (int i = 0; i < argumentos.Length; i++)
        {
            if (argumentos[i].ToLower() == "-host")
            {
                startHost = true;
            }
            else if (argumentos[i].ToLower() == "-client")
            {
                startClient = true;
            }
            // Capturamos la etiqueta -address y cogemos el siguiente argumento como IP
            else if (argumentos[i].ToLower() == "-address" && i + 1 < argumentos.Length)
            {
                serverAddress = argumentos[i + 1];
            }
        }

        // --- AÑADIDO PARA EL EDITOR ---
#if UNITY_EDITOR
        startHost = true;
#endif
        // ------------------------------

        if (startHost)
        {
            // El Host normalmente no necesita IP porque abre los puertos en la propia máquina
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
            Debug.Log("[Iniciador] Conectado como Host.");
        }
        else if (startClient)
        {
            // Le inyectamos la IP al Transporte de FishNet ANTES de conectar
            InstanceFinder.TransportManager.Transport.SetClientAddress(serverAddress);
            InstanceFinder.ClientManager.StartConnection();

            Debug.Log($"[Iniciador] Conectado como Cliente al servidor: {serverAddress}");
        }
    }
}