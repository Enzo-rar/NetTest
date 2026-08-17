using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class AnalizadorRebotes : NetworkBehaviour
{
    private int contadorRebotes = 0;
    private const int MAX_REBOTES_MEDICION = 5;
    private float[] discrepancias = new float[MAX_REBOTES_MEDICION];

    private void OnCollisionEnter(Collision collision)
    {
        // SOLO el Servidor (Host) tiene físicas reales en NGO con NetworkRigidbody
        if (!IsServer) return;

        if (contadorRebotes >= MAX_REBOTES_MEDICION) return;

        // El servidor envía el RPC a todos. 
        RastrearReboteClientRpc(contadorRebotes, collision.contacts[0].point);

        contadorRebotes++;
    }

    [ClientRpc]
    public void RastrearReboteClientRpc(int numRebote, Vector3 posicionImpactoServer)
    {
        // El Host no aporta observaciones al análisis, solo los clientes puros
        if (IsServer) return;

        StartCoroutine(RastrearTrayectoriaVisual(numRebote, posicionImpactoServer));
    }

    private IEnumerator RastrearTrayectoriaVisual(int numRebote, Vector3 posicionImpactoServer)
    {
        float distanciaMinima = float.MaxValue;
        float tiempoRastreo = 0.5f;
        float timer = 0f;

        while (timer < tiempoRastreo)
        {
            // En NGO, NetworkTransform suaviza la posición en transform.position del cliente
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
                // Usamos el Tick del servidor para tener una referencia
                VolcadorCSV.Instancia.RegistrarLanzamiento(NetworkManager.ServerTime.Tick, discrepancias);
            }
        }
    }
}