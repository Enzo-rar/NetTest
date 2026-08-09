using Fusion;
using UnityEngine;

public class DianaOscilanteNetworked : NetworkBehaviour
{
    public float velocidad = 2f;
    private float amplitud = 15f;
    private Vector3 posicionInicial;

    public override void Spawned()
    {
        posicionInicial = transform.position;
        Debug.Log($"[Diana] Spawned! Posición inicial: {posicionInicial}. StateAuthority: {HasStateAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        // Trazamos cada segundo (aprox cada 60 ticks) para no colapsar la consola
        if (Runner.Tick % 60 == 0)
        {
        }

        // Solo el servidor (Host) debe mover la diana. El NetworkTransform hará el resto.
        if (HasStateAuthority)
        {
            float desplazamiento = Mathf.Sin(Runner.SimulationTime * velocidad) * amplitud;
            transform.position = posicionInicial + new Vector3(desplazamiento, 0, 0);
        }
    }
}