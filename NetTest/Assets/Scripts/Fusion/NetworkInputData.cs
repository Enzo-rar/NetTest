using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 Move;
    public Vector2 Look;
    public NetworkBool Sprint;
    public NetworkBool Jump;
    public NetworkBool Crouch;
    public NetworkBool Fire; // Opcional para el arma
    public NetworkBool Interact;
    public float Yaw;
}