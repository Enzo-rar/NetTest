using FishNet.Object;
using UnityEngine;
using System.Collections;

public class AnalizadorRebotes : NetworkBehaviour
{
    private int contadorRebotes = 0;
    private const int MAX_REBOTES_MEDICION = 5;
    private float[] discrepancias = new float[MAX_REBOTES_MEDICION];

    private void OnCollisionEnter(Collision collision)
    {
        // SOLO el Servidor gestiona las colisiones reales
        if (!base.IsServerInitialized) return;

        if (contadorRebotes >= MAX_REBOTES_MEDICION) return;

        // El servidor envía SU punto de impacto a los clientes mediante un RPC
        ObserversRpc_IniciarRastreoRebote(contadorRebotes, collision.contacts[0].point);

        contadorRebotes++;
    }

    [ObserversRpc]
    public void ObserversRpc_IniciarRastreoRebote(int numRebote, Vector3 posicionImpactoServer)
    {
        // El Host (AWS en tu caso) no aporta datos visuales al CSV, solo el cliente remoto
        if (base.IsServerInitialized) return;

        StartCoroutine(RastrearTrayectoriaVisual(numRebote, posicionImpactoServer));
    }

    private IEnumerator RastrearTrayectoriaVisual(int numRebote, Vector3 posicionImpactoServer)
    {
        float distanciaMinima = float.MaxValue;
        float tiempoRastreo = 0.5f;
        float timer = 0f;

        while (timer < tiempoRastreo)
        {
            // En FishNet con NetworkTransform, transform.position ya representa la posición visual interpolada
            Vector3 posicionVisual = transform.position;
            float distanciaActual = Vector3.Distance(posicionImpactoServer, posicionVisual);

            if (distanciaActual < distanciaMinima)
            {
                distanciaMinima = distanciaActual;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        discrepancias[numRebote] = distanciaMinima;

        if (numRebote == MAX_REBOTES_MEDICION - 1)
        {
            if (VolcadorCSV.Instancia != null)
            {
                // Usamos el Tick de FishNet
                VolcadorCSV.Instancia.RegistrarLanzamiento(base.TimeManager.Tick, discrepancias);
            }
        }
    }
}