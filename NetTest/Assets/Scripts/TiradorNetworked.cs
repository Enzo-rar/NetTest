using Fusion;
using UnityEngine;

public class TiradorNetworked : NetworkBehaviour
{
    public Transform puntoDeDisparo;
    public float cadenciaDeDisparo = 0.5f;
    private float tiempoUltimoDisparo = -1f;

    public override void Spawned()
    {
        Debug.Log($"[Tirador] Spawned! InputAuth: {HasInputAuthority} | StateAuth: {HasStateAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        // Traza para saber si el cliente está bloqueado
        if (!HasInputAuthority)
        {
            if (Runner.Tick % 120 == 0)
                Debug.Log("[Tirador] No tengo InputAuthority. No puedo enviar comandos de disparo.");
            return;
        }

        // Lanzamos un rayo rojo visible en la escena del editor para depurar hacia dónde apunta
        Debug.DrawRay(puntoDeDisparo.position, puntoDeDisparo.forward * 50f, Color.red);

        if (Runner.SimulationTime >= tiempoUltimoDisparo + cadenciaDeDisparo)
        {
            RaycastHit hit;
            if (Physics.Raycast(puntoDeDisparo.position, puntoDeDisparo.forward, out hit))
            {
                // Verificamos si detectamos la Hitbox
                if (hit.collider.GetComponent<Hitbox>() != null)
                {
                    Debug.Log($"[Tirador] ¡Diana detectada en tick {Runner.Tick}! Enviando RPC al servidor...");
                    Rpc_RegistrarDisparo(Runner.Tick, hit.transform.position);
                    tiempoUltimoDisparo = Runner.SimulationTime;
                }
                else
                {
                    // Traza por si el rayo choca con otra cosa (como el suelo o el collider base)
                    if (Runner.Tick % 60 == 0)
                        Debug.Log($"[Tirador] Rayo chocó contra: {hit.collider.name}, pero no tiene componente Hitbox.");
                }
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_RegistrarDisparo(int tickDisparo, Vector3 posicionPercibida)
    {
        Debug.Log($"[RPC - Servidor] Recibido disparo del cliente originado en el tick {tickDisparo}. Posición: {posicionPercibida}");

        // API ACTUALIZADA: Usamos FindFirstObjectByType
        var diana = FindFirstObjectByType<DianaOscilanteNetworked>();
        if (diana == null) return;

        Vector3 posicionRecepcion = diana.transform.position;
        float dTransito = Vector3.Distance(posicionRecepcion, posicionPercibida);

        var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
        float eValidacion = 0f;

        // Lanzamos el rayo compensado en el servidor
        if (Runner.LagCompensation.Raycast(puntoDeDisparo.position, puntoDeDisparo.forward, 100f, Object.InputAuthority, out var lagHit, -1, hitOptions))
        {
            // Usamos lagHit.Point para evitar el NullReferenceException
            eValidacion = Vector3.Distance(lagHit.Point, posicionPercibida);
            Debug.Log($"[RPC - Servidor] Rebobinado completado. D_transito: {dTransito} | E_validacion: {eValidacion}");
        }
        else
        {
            Debug.LogWarning("[RPC - Servidor] El Raycast compensado NO impactó con nada en el historial.");
        }

        // API ACTUALIZADA: Usamos FindFirstObjectByType
        var metricManager = FindFirstObjectByType<MetricManagerFusion>();
        if (metricManager != null)
        {
            metricManager.AgregarDatos(dTransito, eValidacion);
        }
        else
        {
            Debug.LogError("[RPC - Servidor] Falta el objeto MetricManagerFusion en la escena para guardar los datos.");
        }
    }
}