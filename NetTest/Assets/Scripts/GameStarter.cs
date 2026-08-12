using UnityEngine;
using FishNet;
using FishNet.Example;
using System;

public class GameStarter : MonoBehaviour
{
    void Start()
    {
        // Retrasamos medio segundo para dar tiempo a que Fish-Net se inicialice internamente
        Invoke(nameof(ProcesarArgumentos), 0.5f);
    }

    private void ProcesarArgumentos()
    {
        if (InstanceFinder.NetworkManager == null) return;

        // 1. Buscamos el Canvas de Fish-Net que me has pasado en la foto y lo apagamos
        // para que no estorbe ni consuma recursos, especialmente en modo Headless.
        NetworkHudCanvases hud = FindFirstObjectByType<NetworkHudCanvases>();
        if (hud != null)
        {
            hud.gameObject.SetActive(false);
        }

        // 2. Leemos los argumentos del .bat
        string[] args = Environment.GetCommandLineArgs();
        bool esCliente = false;
        string ip = "localhost"; // IP por defecto para pruebas locales

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower() == "-client")
            {
                esCliente = true;
            }
            else if (args[i].ToLower() == "-ip" && i + 1 < args.Length)
            {
                ip = args[i + 1];
            }
        }

        // 3. Ejecutamos la lógica de conexión
        if (esCliente)
        {
            Debug.Log($"[GameStarter] Iniciando como CLIENTE REMOTO. Conectando a IP: {ip}");
            // Le pasamos la IP del .bat al sistema de transporte (Tugboat)
            InstanceFinder.TransportManager.Transport.SetClientAddress(ip);
            // Iniciamos el cliente (equivale al OnClick_Client)
            InstanceFinder.ClientManager.StartConnection();
        }
        else
        {
            // Topología Listen Server: Arrancamos servidor y cliente local a la vez
            Debug.Log("[GameStarter] Iniciando como HOST (Listen Server).");
            // Equivale al OnClick_Server (que en ese HUD arranca ambos)
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        }
    }
}