using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public GameObject startButton;

    void Start()
    {
        // Solo el anfitrión debe ver el botón de Iniciar Partida[cite: 1]
        if (NetworkRunner.Instances.Count > 0 && startButton != null)
        {
            startButton.SetActive(NetworkRunner.Instances[0].IsServer);
        }
    }

    public void StartGame()
    {
        if (NetworkRunner.Instances.Count == 0 || !NetworkRunner.Instances[0].IsServer) return;

        int team1Count = 0;
        int team2Count = 0;

        foreach (var capsule in FindObjectsByType<LobbyCapsule>(FindObjectsSortMode.None))
        {
            if (capsule.SlotIndex == 0 || capsule.SlotIndex == 1) team1Count++;
            if (capsule.SlotIndex == 2 || capsule.SlotIndex == 3) team2Count++;
        }

        // Comprobamos si la configuración es válida (1v1 o 2v2)[cite: 1]
        // Comprobamos si la configuración es válida (1v1 o 2v2)
        if ((team1Count == 1 && team2Count == 1) || (team1Count == 2 && team2Count == 2))
        {
            // En Fusion 2 usamos LoadScene pasándole el índice de la escena
            NetworkRunner.Instances[0].LoadScene(SceneRef.FromIndex(2));
        }
        else
        {
            Debug.LogWarning("Configuración inválida. Solo se permiten duelos 1v1 o 2v2.");
        }
    }

    public void RequestChangeSlot(int slotNumber)
    {
        foreach (var capsule in FindObjectsByType<LobbyCapsule>(FindObjectsSortMode.None))
        {
            if (capsule.HasInputAuthority)
            {
                capsule.RPC_RequestSlotChange(slotNumber);
                break;
            }
        }
    }

    public void LeaveSession()
    {
        if (NetworkRunner.Instances.Count > 0)
        {
            NetworkRunner.Instances[0].Shutdown();
        }

        // El jugador que pulsa el botón vuelve inmediatamente al título
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        // Vigila constantemente si el motor de red sigue existiendo.
        // Si el Host cierra la partida, el Runner se destruye y la cuenta baja a 0.
        if (NetworkRunner.Instances.Count == 0)
        {
            SceneManager.LoadScene(0);
        }
    }
}