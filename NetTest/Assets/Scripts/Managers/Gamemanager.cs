using UnityEngine;
using Fusion;

public class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkPrefabRef realPlayerPrefab;
    private int spawnIndex = 0;

    public override void Spawned()
    {
        // Solo el anfitrión tiene autoridad para generar objetos en el mundo
        if (Runner.IsServer)
        {
            foreach (PlayerRef player in Runner.ActivePlayers)
            {
                SpawnPlayer(player);
            }
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        // Por si un jugador termina de cargar la escena un poco más tarde
        if (Runner.IsServer)
        {
            SpawnPlayer(player);
        }
    }

    private void SpawnPlayer(PlayerRef player)
    {
        string[] spawnNames = { "T11", "T12", "T21", "T22" };
        if (spawnIndex >= spawnNames.Length) return;

        // Buscamos el objeto vacío por su nombre exacto en la jerarquía
        GameObject spawnObj = GameObject.Find(spawnNames[spawnIndex]);

        Vector3 spawnPos = spawnObj != null ? spawnObj.transform.position : Vector3.up * 2;
        Quaternion spawnRot = spawnObj != null ? spawnObj.transform.rotation : Quaternion.identity;

        // Generamos tu prefab complejo y le damos la autoridad de input al jugador
        Runner.Spawn(realPlayerPrefab, spawnPos, spawnRot, player);
        spawnIndex++;
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            // Limpiamos la cápsula real si alguien se sale, igual que en el lobby
            foreach (var playerObj in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
            {
                if (playerObj.Object.InputAuthority == player)
                {
                    Runner.Despawn(playerObj.Object);
                    break;
                }
            }
        }
    }
}