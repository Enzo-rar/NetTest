using UnityEngine;
using Fusion;

public class WeaponSpawner : NetworkBehaviour, IInteractable
{
    [Header("Configuración del Pilar")]
    public float cooldownSeconds = 8f;

    // Variable sincronizada en red que actúa como temporizador
    [Networked] private TickTimer cooldownTimer { get; set; }

    // Esta función la llama el Raycast del jugador desde PlayerWeaponController
    public void Interact(PlayerWeaponController player)
    {
        // Solo el servidor tiene autoridad para decidir si el pilar suelta el arma
        if (!Runner.IsServer) return;

        // Si el temporizador sigue corriendo, ignoramos la interacción
        if (!cooldownTimer.ExpiredOrNotRunning(Runner)) return;

        Debug.Log($"El pilar le ha entregado un arma al jugador {player.Object.InputAuthority}");

        // TODO: En el siguiente paso, instaciaremos físicamente el arma en las manos del jugador aquí

        // Reiniciamos el cooldown de 8 segundos para todos los jugadores
        cooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldownSeconds);
    }
}