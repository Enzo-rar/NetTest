using Fusion;
using Fusion.Addons.Physics; // <-- Vital para usar NetworkRigidbody3D
using UnityEngine;

public class NetworkCameraSetup : NetworkBehaviour
{
    [Header("Componentes a desactivar en los clones")]
    public Camera playerCamera;
    public AudioListener audioListener;

    public override void Spawned()
    {
        // 1. EL PARCHE DE FÍSICAS (Solución al teletransporte al origen)
        // Obligamos al Rigidbody local a acatar las coordenadas de red iniciales
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = transform.position;
            rb.rotation = transform.rotation;
        }

        // Le decimos a Fusion 2 que aplique este cambio radical en su simulación
        NetworkRigidbody3D nrb = GetComponent<NetworkRigidbody3D>();
        if (nrb != null)
        {
            nrb.Teleport(transform.position, transform.rotation);
        }

        // 2. EL SECUESTRO DE CÁMARAS
        // Si no soy el dueño de este avatar, le apago los sentidos
        if (!Object.HasInputAuthority)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }
    }
}