using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

// Heredamos de NetworkBehaviour para poder acceder a los datos de la red
public class DesyncAnalyzer : NetworkBehaviour
{
    [Header("Configuración del Experimento")]
    [Tooltip("Cada cuántos minutos se hace el cálculo y se guarda en el CSV (ej: 10)")]
    public float minutosPorIntervalo = 10f;
    public float frecuenciaMuestreo = 0.1f;
    public string clientID = "Client_1";

    private List<float> distanciasMuestreadas = new List<float>();
    private float timerMuestreo = 0f;
    private float timerIntervalo = 0f;

    private PlayerMovement playerMovement;

    public override void Spawned()
    {
        // Solo el cliente real (tú) debe medir su propio lag y escribir en su CSV.
        // Apagamos este script en el PC del Host para que no genere CSVs basura.
        if (!Object.HasInputAuthority)
        {
            this.enabled = false;
            return;
        }

        playerMovement = GetComponent<PlayerMovement>();

        // Ajustamos la lógica de tu experimento total de 6 horas / 10 minutos
        CSVMetricsLogger.Instance.libreriaActual = "Photon_Fusion_2";
        // Si no tienes el CSVMetricsLogger en tu escena multijugador, ¡recuerda añadirlo en un GameObject vacío!
    }

    void Update()
    {
        if (playerMovement == null || CSVMetricsLogger.Instance == null) return;

        float dt = Time.deltaTime;
        timerMuestreo += dt;
        timerIntervalo += dt;

        // 1. Recoger muestras (Diferencia entre mi posición y la que me dicta el Host desde Brasil)
        if (timerMuestreo >= frecuenciaMuestreo)
        {
            timerMuestreo -= frecuenciaMuestreo;

            // La magia del Client-Side Prediction: medimos el error
            Vector2 clientPos2D = new Vector2(transform.position.x, transform.position.z);
            Vector2 hostPos2D = new Vector2(playerMovement.PosicionRealServidor.x, playerMovement.PosicionRealServidor.z);

            float dist = Vector2.Distance(clientPos2D, hostPos2D);
            distanciasMuestreadas.Add(dist);
        }

        // 2. Procesar, calcular y guardar cada 10 minutos
        float segundosPorIntervalo = minutosPorIntervalo * 60f;
        if (timerIntervalo >= segundosPorIntervalo)
        {
            timerIntervalo -= segundosPorIntervalo;
            CalcularYGuardarEstadisticas();
        }
    }

    private void CalcularYGuardarEstadisticas()
    {
        if (distanciasMuestreadas.Count == 0) return;

        float min = distanciasMuestreadas.Min();
        float max = distanciasMuestreadas.Max();
        float media = distanciasMuestreadas.Average();

        distanciasMuestreadas.Sort();
        int count = distanciasMuestreadas.Count;
        float mediana = (count % 2 == 0)
            ? (distanciasMuestreadas[count / 2 - 1] + distanciasMuestreadas[count / 2]) / 2f
            : distanciasMuestreadas[count / 2];

        CSVMetricsLogger.Instance.LogEstadisticasDesincronizacion(
            clientID, min, max, media, mediana, count
        );

        Debug.Log($"<color=magenta>[DesyncAnalyzer]</color> Bloque de {minutosPorIntervalo} mins guardado en CSV. Muestras: {count} | Media Desincronización: {media:F3}m");

        distanciasMuestreadas.Clear();
    }
}