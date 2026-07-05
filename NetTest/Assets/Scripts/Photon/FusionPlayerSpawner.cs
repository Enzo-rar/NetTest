using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Fusion.Sockets;

public class FusionPlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs Diferenciados")]
    public NetworkPrefabRef hostPrefab;
    public NetworkPrefabRef clientPrefab;

    [Header("Puntos de Aparición (Transform)")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    [Header("Referencias de la Escena")]
    public SmartFloor sueloInteligente;

    private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Solo el Server (Host) tiene autoridad para hacer Spawn
        if (runner.IsServer)
        {
            // ¿El jugador que acaba de entrar es el propio Host?
            if (player == runner.LocalPlayer)
            {
                Debug.Log($"<color=green>[Spawner]</color> Host conectado. Spawneando en plataforma superior...");
                NetworkObject hostObj = runner.Spawn(
                    hostPrefab,
                    hostSpawnPoint.position,
                    hostSpawnPoint.rotation,
                    player
                );
                spawnedCharacters.Add(player, hostObj);
            }
            // Si no es el Host, es un Cliente
            else
            {
                Debug.Log($"<color=cyan>[Spawner]</color> Cliente conectado. Spawneando en el suelo...");
                NetworkObject clientObj = runner.Spawn(
                    clientPrefab,
                    clientSpawnPoint.position,
                    clientSpawnPoint.rotation,
                    player
                );
                spawnedCharacters.Add(player, clientObj);

                // Enganchamos el suelo inteligente al cliente recién aparecido
                if (sueloInteligente != null)
                {
                    sueloInteligente.targetPlayer = clientObj.transform;
                    Debug.Log("<color=magenta>[Spawner]</color> Suelo inteligente vinculado al Cliente.");
                }
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            spawnedCharacters.Remove(player);
        }
    }

    // ========================================================================
    // MÉTODOS OBLIGATORIOS (vacíos)
    // ========================================================================
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason info) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}