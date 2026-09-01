using UnityEngine;
using Fusion;

public class LobbyCapsule : NetworkBehaviour
{
    // Usamos OnChangedRender (novedad de Fusion 2) para que cuando el servidor 
    // cambie este valor, todos los clientes actualicen el color automáticamente
    [Networked, OnChangedRender(nameof(UpdateColorVisuals))]
    public int SlotIndex { get; set; }

    [Tooltip("Orden: 0(Rojo 1), 1(Rojo 2), 2(Azul 1), 3(Azul 2)")]
    public Material[] teamMaterials;

    private MeshRenderer meshRenderer;

    public override void Spawned()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateColorVisuals(); // Forzamos el color inicial al aparecer
    }

    // Esta función se ejecuta automáticamente en los clientes cuando cambia el SlotIndex
    public void UpdateColorVisuals()
    {
        if (meshRenderer != null && teamMaterials != null && teamMaterials.Length > SlotIndex)
        {
            meshRenderer.material = teamMaterials[SlotIndex];
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSlotChange(int requestedSlot)
    {
        LobbyManager lobby = FindFirstObjectByType<LobbyManager>();
        if (lobby == null) return;

        int currentSlot = this.SlotIndex;
        LobbyCapsule occupant = null;

        // Buscamos si alguien ya ocupa el hueco solicitado
        foreach (var capsule in FindObjectsByType<LobbyCapsule>(FindObjectsSortMode.None))
        {
            if (capsule.SlotIndex == requestedSlot)
            {
                occupant = capsule;
                break;
            }
        }

        // Si hay alguien, lo movemos al hueco que nosotros estamos dejando libre
        if (occupant != null)
        {
            occupant.SlotIndex = currentSlot;
            NetworkTransform occupantTransform = occupant.GetComponent<NetworkTransform>();
            if (occupantTransform != null)
            {
                occupantTransform.Teleport(lobby.spawnSlots[currentSlot].position, lobby.spawnSlots[currentSlot].rotation);
            }
        }

        // Finalmente, nos movemos nosotros al hueco solicitado
        this.SlotIndex = requestedSlot;
        NetworkTransform myTransform = GetComponent<NetworkTransform>();
        if (myTransform != null)
        {
            myTransform.Teleport(lobby.spawnSlots[requestedSlot].position, lobby.spawnSlots[requestedSlot].rotation);
        }
    }
}