using UnityEngine;
using Fusion;
using System.Linq;
using TMPro; // Necesario para la UI
using UnityEngine.SceneManagement;

// Cambiamos SimulationBehaviour por NetworkBehaviour
public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkPrefabRef playerPrefab;
    public Transform[] spawnSlots;
    public TMP_Text roomCodeText; // Referencia al texto del código

    // Spawned se ejecuta automáticamente en cuanto la escena de red termina de cargar
    public override void Spawned()
    {


        if (roomCodeText != null)
        {
            roomCodeText.text = "CÓDIGO: " + Runner.SessionInfo.Name;
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            int playerCount = Runner.ActivePlayers.Count();
            int slotIndex = (playerCount - 1) % spawnSlots.Length;

            Vector3 spawnPosition = spawnSlots[slotIndex].position;
            Quaternion spawnRotation = spawnSlots[slotIndex].rotation;

            Runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player, (runner, spawnedObj) =>
            {
                LobbyCapsule capsule = spawnedObj.GetComponent<LobbyCapsule>();
                if (capsule != null)
                {
                    capsule.SlotIndex = slotIndex;
                }
            });

            Debug.Log($"Jugador {player.PlayerId} conectado y asignado al slot {slotIndex}.");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            // Buscamos qué cápsula pertenecía al jugador que se acaba de ir
            foreach (var capsule in FindObjectsByType<LobbyCapsule>(FindObjectsSortMode.None))
            {
                if (capsule.Object.InputAuthority == player)
                {
                    // Despawn la elimina de la red para todos los clientes
                    Runner.Despawn(capsule.Object);
                    break;
                }
            }
            Debug.Log($"Jugador {player.PlayerId} ha abandonado la sala. Cápsula destruida.");
        }
    }

}