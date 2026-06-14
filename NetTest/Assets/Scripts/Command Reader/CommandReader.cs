using UnityEngine;
using System;

public class CommandReader : MonoBehaviour
{
    public static CommandReader Instance;

    public enum StartupMode { Manual, Host, Client, DedicatedServer }

    [Header("Estado de Arranque")]
    public StartupMode currentMode = StartupMode.Manual;
    public string targetIP = "127.0.0.1"; // Localhost por defecto

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LeerArgumentos();
    }

    private void LeerArgumentos()
    {
        
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
           
            if (args[i] == "-mode" && i + 1 < args.Length)
            {
                string modeArg = args[i + 1].ToLower();
                if (modeArg == "host") currentMode = StartupMode.Host;
                else if (modeArg == "client") currentMode = StartupMode.Client;
                else if (modeArg == "server") currentMode = StartupMode.DedicatedServer;
            }
            
            else if (args[i] == "-ip" && i + 1 < args.Length)
            {
                targetIP = args[i + 1];
            }
            
            else if (args[i] == "-headless")
            {
                ActivarModoHeadless();
            }
        }

        Debug.Log($"<color=yellow>[CommandReader]</color> Arrancando en modo: <b>{currentMode}</b> | IP: {targetIP}");
    }

    private void ActivarModoHeadless()
    {
        Debug.Log("<color=red>[CommandReader]</color> Modo Headless detectado. Apagando sistemas no esenciales...");

       
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;


        AudioListener.pause = true;
        AudioListener.volume = 0f;

        // Unity tiene un argumento nativo llamado "-batchmode" que desactiva el renderizado gráfico.
        // Usaremos "-batchmode" nativo en AWS, pero este "-headless" sirve para apagar cosas 
        // lógicas del juego que "-batchmode" no apaga (como el audio o UI específicas).
    }
}