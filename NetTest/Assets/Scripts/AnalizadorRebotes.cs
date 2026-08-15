using Fusion;
using UnityEngine;
using System.Collections; // Necesario para las Corrutinas
using Fusion.Addons.Physics;

public class AnalizadorRebotes : NetworkBehaviour
{
    private int contadorRebotes = 0;
    private const int MAX_REBOTES_MEDICION = 5;

    private float[] discrepancias = new float[MAX_REBOTES_MEDICION];

    private void OnCollisionEnter(Collision collision)
    {
        // SOLO el Servidor tiene físicas reales, así que solo él entra aquí
        if (!HasStateAuthority) return;

        if (contadorRebotes >= MAX_REBOTES_MEDICION) return;

        // El servidor envía SU punto de impacto exacto a los clientes
        Rpc_IniciarRastreoRebote(contadorRebotes, collision.contacts[0].point);

        contadorRebotes++;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_IniciarRastreoRebote(int numRebote, Vector3 posicionImpactoServer)
    {
        // El anfitrión de AWS (Host) no aporta observaciones al análisis, solo el cliente
        if (HasStateAuthority) return;

        // Iniciamos el rastreo de la trayectoria visual del cliente
        StartCoroutine(RastrearTrayectoriaVisual(numRebote, posicionImpactoServer));
    }

    // Esta rutina ignora el tiempo y busca puramente el error espacial de la curva
    private IEnumerator RastrearTrayectoriaVisual(int numRebote, Vector3 posicionImpactoServer)
    {
        NetworkRigidbody3D netRb = GetComponent<NetworkRigidbody3D>();
        float distanciaMinima = float.MaxValue;

        // Rastrearemos la posición visual del cliente durante 0.5 segundos 
        // para darle tiempo de sobra a llegar al muro y rebotar visualmente
        float tiempoRastreo = 0.5f;
        float timer = 0f;

        while (timer < tiempoRastreo)
        {
            // Posición donde el jugador realmente ve la esfera
            Vector3 posicionVisual = netRb.InterpolationTarget != null ? netRb.InterpolationTarget.position : transform.position;

            // Calculamos la distancia actual respecto al rebote real del servidor
            float distanciaActual = Vector3.Distance(posicionImpactoServer, posicionVisual);

            // Nos quedamos siempre con el punto de mayor acercamiento (la esquina del rebote visual)
            if (distanciaActual < distanciaMinima)
            {
                distanciaMinima = distanciaActual;
            }

            timer += Time.deltaTime;
            yield return null; // Esperamos al siguiente frame visual
        }

        // Guardamos el error espacial puro de la trayectoria
        discrepancias[numRebote] = distanciaMinima;

        // Si ya hemos rastreado los 5 rebotes, enviamos los datos finales al Gestor CSV
        if (numRebote == MAX_REBOTES_MEDICION - 1)
        {
            if (VolcadorCSV.Instancia != null)
            {
                VolcadorCSV.Instancia.RegistrarLanzamiento(Runner.Tick, discrepancias);
            }
        }
    }
}