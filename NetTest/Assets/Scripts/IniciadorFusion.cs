using Fusion;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IniciadorFusion : MonoBehaviour
{
    private NetworkRunner _runner;

    async void Start()
    {
        Debug.Log("[Iniciador] Preparando NetworkRunner...");
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        SceneRef escenaActual = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        GameMode modoJuego = GameMode.Host;

        string[] argumentos = Environment.GetCommandLineArgs();
        foreach (string arg in argumentos)
        {
            if (arg.ToLower() == "-host") modoJuego = GameMode.Host;
            else if (arg.ToLower() == "-client") modoJuego = GameMode.Client;
        }

        Debug.Log($"[Iniciador] Intentando conectar a Photon como: {modoJuego}. Escena index: {SceneManager.GetActiveScene().buildIndex}");

        // Capturamos el resultado de la conexión
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = modoJuego,
            SessionName = "SalaTestTFG",
            Scene = escenaActual,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // Trazas para saber si hemos entrado a la sala o si ha petado
        if (result.Ok)
        {
            Debug.Log($"[Iniciador] ¡Conexión EXITOSA! Estamos dentro de la sala 'SalaTestTFG' como {modoJuego}.");
        }
        else
        {
            Debug.LogError($"[Iniciador] ERROR AL CONECTAR: {result.ErrorMessage}");
        }
    }
}