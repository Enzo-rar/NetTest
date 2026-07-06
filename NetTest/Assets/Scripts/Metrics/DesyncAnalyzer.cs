using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq; // Necesario para calcular la Media (.Average)

public class DesyncAnalyzer : MonoBehaviour
{
    [Header("Configuración de Muestreo")]
    [Tooltip("Intervalo en segundos para tomar una muestra. 0.1s = 10 veces por segundo.")]
    public float intervaloMuestreo = 0.1f;

    [Tooltip("Cada cuántos MINUTOS se agrupan los datos para escribir la línea en el CSV.")]
    public float intervaloAgrupacionMinutos = 10f; // Por defecto, cada 10 minutos

    [Tooltip("Identificador del cliente para el CSV.")]
    public string clientID = "Client_NGO_1";

    private float sampleTimer = 0f;
    private float aggregateTimer = 0f;
    private List<float> distancias = new List<float>();

    void Update()
    {
        // Asegurarnos de que el Logger y la red están listos, y que somos un cliente
        if (CSVMetricsLogger.Instance == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) return;

        // 1. En NGO, le pedimos al motor de red que nos dé a nuestro jugador local automáticamente
        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayerObj == null) return;

        PlayerMovement pm = localPlayerObj.GetComponent<PlayerMovement>();
        if (pm == null) return;

        // --- TEMPORIZADORES ---
        float dt = Time.deltaTime;
        sampleTimer += dt;
        aggregateTimer += dt;

        // 2. TOMAR LA MUESTRA DE DESINCRONIZACIÓN
        if (sampleTimer >= intervaloMuestreo)
        {
            sampleTimer -= intervaloMuestreo;

            Vector3 posCliente = pm.transform.position; // Dónde creo que estoy
            Vector3 posServer = pm.PosicionRealServidor.Value; // Dónde dice el Server que estoy

            // Aplanamos el eje Y a 0 para que los saltos o agaches no ensucien la distancia 2D
            posCliente.y = 0;
            posServer.y = 0;

            float distancia = Vector3.Distance(posCliente, posServer);
            distancias.Add(distancia);
        }

        // 3. CALCULAR ESTADÍSTICAS Y GUARDAR EN CSV
        if (aggregateTimer >= (intervaloAgrupacionMinutos * 60f))
        {
            aggregateTimer -= (intervaloAgrupacionMinutos * 60f);
            CalcularYGuardarCSV();
        }
    }

    private void CalcularYGuardarCSV()
    {
        if (distancias.Count == 0) return;

        // Ordenamos la lista de menor a mayor
        distancias.Sort();

        float min = distancias[0];
        float max = distancias[distancias.Count - 1];
        float media = distancias.Average();

        // Calcular la mediana matemática
        float mediana = 0f;
        int mitad = distancias.Count / 2;
        if (distancias.Count % 2 == 0)
            mediana = (distancias[mitad - 1] + distancias[mitad]) / 2f;
        else
            mediana = distancias[mitad];

        // Se lo enviamos al Logger y vaciamos la lista para los próximos X minutos
        CSVMetricsLogger.Instance.LogBloqueEstadistico(Time.time, clientID, min, max, media, mediana, distancias.Count);

        Debug.Log($"<color=cyan>[DesyncAnalyzer]</color> Bloque guardado. Media: {media:F2}m. Muestras: {distancias.Count}");
        distancias.Clear();
    }
}