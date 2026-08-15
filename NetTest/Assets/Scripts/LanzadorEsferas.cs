using Fusion;
using UnityEngine;

public class LanzadorEsferas : NetworkBehaviour
{
    [Header("Configuración del Lanzamiento")]
    public NetworkPrefabRef prefabEsfera;
    public float fuerzaLanzamiento = 25f;
    public float intervaloEntreLanzamientos = 5f;

    [Networked] private TickTimer temporizadorLanzamiento { get; set; }

    public override void Spawned()
    {
        Debug.Log($"[Lanzador] Spawned() ejecutado. ¿Tengo autoridad (Host)?: {HasStateAuthority}");

        if (HasStateAuthority)
        {
            Debug.Log("[Lanzador] Iniciando temporizador de lanzamientos...");
            temporizadorLanzamiento = TickTimer.CreateFromSeconds(Runner, intervaloEntreLanzamientos);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        // Comprobamos si el temporizador ha expirado
        if (temporizadorLanzamiento.Expired(Runner))
        {
            //Debug.Log($"[Lanzador] Temporizador expirado en el Tick {Runner.Tick}. ¡Lanzando esfera!");
            LanzarEsfera();

            // Reiniciamos
            temporizadorLanzamiento = TickTimer.CreateFromSeconds(Runner, intervaloEntreLanzamientos);
        }
    }

    void LanzarEsfera()
    {
        if (prefabEsfera == NetworkPrefabRef.Empty)
        {
            Debug.LogError("[Lanzador] ERROR: El prefabEsfera no está asignado en el Inspector.");
            return;
        }

        NetworkObject nuevaEsfera = Runner.Spawn(prefabEsfera, transform.position, transform.rotation, Object.InputAuthority);

        if (nuevaEsfera != null)
        {
            //Debug.Log($"[Lanzador] Esfera instanciada correctamente con ID {nuevaEsfera.Id}");
            Rigidbody rb = nuevaEsfera.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * fuerzaLanzamiento, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("[Lanzador] La esfera no tiene Rigidbody clásico. (Si usas NetworkRigidbody3D, asegúrate de que el GameObject también tenga el Rigidbody de Unity).");
            }

            //Runner.Despawn(nuevaEsfera);
            // OJO: Runner.Despawn la destruye INMEDIATAMENTE.
            // Para que la esfera viaje y rebote antes de destruirse, ¡debemos quitar esta línea o retrasarla!
        }
    }
}