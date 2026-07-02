using UnityEngine;
using System;

[Serializable]
public struct PlayerInputData
{
    public Vector2 Move;
    public Vector2 Look;
    public bool Jump;
    public bool Crouch;
    public bool Sprint;
    public bool Fire;
    public bool AltFire;
}

public interface IPlayerInputProvider
{
    PlayerInputData GetInput();
}
