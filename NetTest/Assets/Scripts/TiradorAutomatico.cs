using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

public class TiradorAutomatico : NetworkBehaviour
{
    public Transform puntoDeDisparo;
    public float cadenciaDeDisparo = 0.5f;
    private float tiempoUltimoDisparo = -1f;

    private float tiempoExperimento = 0f;
    private float intervaloRecopilacion = 150f; // 2.5 minutos
    private int bloquesCompletados = 0;
    private const int BLOQUES_MAXIMOS = 12; // 30 minutos

    private List<float> distanciasTransito = new List<float>();
    private List<float> erroresValidacion = new List<float>();

    private string csvFilePath;

    void Awake()
    {
        csvFilePath = Path.Combine(Application.dataPath, "Experimento_Impactos.csv");
    }

    void Update()
    {
        if (!IsSpawned) return;

        // 1. Lógica de volcado (Exclusiva del Host)
        if (IsServer && bloquesCompletados < BLOQUES_MAXIMOS)
        {
            tiempoExperimento += Time.deltaTime;

            if (tiempoExperimento >= intervaloRecopilacion)
            {
                bloquesCompletados++;
                float marcaTiempo = bloquesCompletados * intervaloRecopilacion;

                VolcarDatosBloque(marcaTiempo);
                tiempoExperimento -= intervaloRecopilacion;

                if (bloquesCompletados >= BLOQUES_MAXIMOS)
                    Debug.Log("Experimento de 30 minutos finalizado.");
            }
        }

        // 2. Lógica de disparo (Exclusiva del Cliente Remoto)
        if (IsClient && IsOwner && bloquesCompletados < BLOQUES_MAXIMOS)
        {
            // Dibujamos un rayo rojo en la vista "Scene" para comprobar hacia dónde estamos apuntando
            Debug.DrawRay(puntoDeDisparo.position, puntoDeDisparo.forward * 50f, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(puntoDeDisparo.position, puntoDeDisparo.forward, out hit))
            {
                if (hit.collider.CompareTag("Diana") && Time.time >= tiempoUltimoDisparo + cadenciaDeDisparo)
                {
                    Debug.Log($"[CLIENTE] Disparo efectuado. Diana detectada en: {hit.transform.position}");

                    DispararServerRpc(hit.transform.position, NetworkManager.ServerTime.Tick);
                    tiempoUltimoDisparo = Time.time;
                }
            }
        }
    }

    // Añadimos ServerRpcParams para identificar al remitente
    [ServerRpc]
    void DispararServerRpc(Vector3 posicionDianaPercibida, int tickDisparo, ServerRpcParams rpcParams = default)
    {
        // Si el que dispara es el propio Host (Listen Server local client), lo ignoramos.
        // Esto cumple con el TFG: "sus resultados no se incorporan al CSV experimental"
        if (rpcParams.Receive.SenderClientId == NetworkManager.Singleton.LocalClientId)
        {
            // Descomenta esto si quieres ver en consola cuándo dispara el Host ciego
            // Debug.Log("[HOST] Disparo local omitido para el CSV.");
            return;
        }

        GameObject diana = GameObject.FindGameObjectWithTag("Diana");
        if (diana != null)
        {
            Vector3 pRecepcion = diana.transform.position;
            float dTransito = Vector3.Distance(pRecepcion, posicionDianaPercibida);

            // Este log ahora solo imprimirá los disparos del cliente remoto
            Debug.Log($"[HOST] Disparo REMOTO recibido | Tick: {tickDisparo} | P_Cliente: {posicionDianaPercibida} | P_Host: {pRecepcion} | Distancia: {dTransito}");

            float eValidacion = dTransito;

            distanciasTransito.Add(dTransito);
            erroresValidacion.Add(eValidacion);
        }
        else
        {
            Debug.LogWarning("[HOST] Disparo recibido, pero no se encontró ningún objeto con el Tag 'Diana' en la escena.");
        }
    }

    private void VolcarDatosBloque(float marcaTiempo)
    {
        bool isNewFile = !File.Exists(csvFilePath);
        using (StreamWriter writer = new StreamWriter(csvFilePath, true, Encoding.UTF8))
        {
            if (isNewFile)
            {
                writer.WriteLine("Tiempo,Min_D,Max_D,Media_D,Mediana_D,Min_E,Max_E,Media_E,Mediana_E");
            }

            if (distanciasTransito.Count > 0)
            {
                float minD = distanciasTransito.Min();
                float maxD = distanciasTransito.Max();
                float mediaD = distanciasTransito.Average();
                float medianaD = CalcularMediana(distanciasTransito);

                float minE = erroresValidacion.Min();
                float maxE = erroresValidacion.Max();
                float mediaE = erroresValidacion.Average();
                float medianaE = CalcularMediana(erroresValidacion);

                writer.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    marcaTiempo, minD, maxD, mediaD, medianaD, minE, maxE, mediaE, medianaE));
            }
            else
            {
                Debug.LogWarning($"[HOST] Bloque {bloquesCompletados} volcado sin datos. El cliente no registró ningún disparo.");
                writer.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},0,0,0,0,0,0,0,0", marcaTiempo));
            }
        }

        Debug.Log($"Bloque {bloquesCompletados}/{BLOQUES_MAXIMOS} volcado a los {marcaTiempo}s.");
        distanciasTransito.Clear();
        erroresValidacion.Clear();
    }

    private float CalcularMediana(List<float> lista)
    {
        if (lista == null || lista.Count == 0) return 0f;

        float[] arreglo = lista.ToArray();
        System.Array.Sort(arreglo);

        int mitad = arreglo.Length / 2;
        if (arreglo.Length % 2 == 0)
        {
            return (arreglo[mitad - 1] + arreglo[mitad]) / 2f;
        }
        else
        {
            return arreglo[mitad];
        }
    }
}