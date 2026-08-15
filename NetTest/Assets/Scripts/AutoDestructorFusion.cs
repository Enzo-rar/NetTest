using Fusion;
using UnityEngine;

public class AutoDestructorFusion : NetworkBehaviour
{
    [Networked] private TickTimer tiempoDeVida { get; set; }

    public override void Spawned()
    {
        // Solo el servidor (Host) decide cuándo se destruyen las cosas
        if (HasStateAuthority)
        {
            // Le damos 4.5 segundos de vida (justo antes de que salga la siguiente)
            tiempoDeVida = TickTimer.CreateFromSeconds(Runner, 4.5f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (tiempoDeVida.Expired(Runner))
        {
            Runner.Despawn(Object); // Se destruye a sí misma correctamente a través de la red
        }
    }
}