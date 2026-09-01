using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public class PlayerInputPoller : NetworkBehaviour, INetworkRunnerCallbacks
{
    public Transform orientation;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;
    private bool crouchInput;
    private bool jumpInput;
    private bool fireInput;

    // AQUÍ declaramos la nueva variable para que exista en el contexto
    private bool interactInput;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Runner.AddCallbacks(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner != null) runner.RemoveCallbacks(this);
    }

    void Update()
    {
        if (!HasInputAuthority) return;

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        lookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        sprintInput = Input.GetKey(KeyCode.LeftShift);
        crouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        jumpInput |= Input.GetKeyDown(KeyCode.Space);
        fireInput |= Input.GetMouseButton(0);

        // Capturamos la pulsación de la tecla E
        interactInput |= Input.GetKeyDown(KeyCode.E);
    }

    // El compilador exige esta función exacta para cumplir con INetworkRunnerCallbacks
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();

        data.Move = moveInput;
        data.Look = lookInput;
        data.Sprint = sprintInput;
        data.Crouch = crouchInput;
        data.Jump = jumpInput;
        data.Fire = fireInput;

        // Guardamos la pulsación en nuestro paquete de red
        data.Interact = interactInput;

        if (orientation == null)
        {
            orientation = transform.Find("Orientation");
        }

        if (orientation != null)
        {
            data.Yaw = orientation.eulerAngles.y;
        }

        input.Set(data);

        // Reseteamos todas las teclas de pulsación única
        jumpInput = false;
        fireInput = false;
        interactInput = false;
    }

    // --- Bloque de relleno para evitar el error CS0535 ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}