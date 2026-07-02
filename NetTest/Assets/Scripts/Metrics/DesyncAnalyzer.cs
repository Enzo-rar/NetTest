using UnityEngine;

public class DesyncAnalyzer : MonoBehaviour
{
    [Header("Objetivos a Medir")]
    [Tooltip("El jugador que se mueve de forma automatizada (Visión del Cliente)")]
    public Transform clientPlayer;

    [Tooltip("El jugador en la plataforma superior (Visión del Host sobre el Cliente)")]
    public Transform hostReplica;

    [Header("Configuración de Muestreo")]
    [Tooltip("Intervalo en segundos para guardar el dato. 0.1s = 10 veces por segundo.")]
    public float intervaloMuestreo = 0.1f;

    [Tooltip("Identificador del cliente para el CSV.")]
    public string clientID = "Client_1";

    private float timer = 0f;
    private int currentTick = 0;

    void Update()
    {
        // Nos aseguramos de que todo esté asignado y el CSV esté listo
        if (clientPlayer == null || hostReplica == null || CSVMetricsLogger.Instance == null) return;

        timer += Time.deltaTime;

        // Si se cumple el intervalo, tomamos la muestra
        if (timer >= intervaloMuestreo)
        {
            timer -= intervaloMuestreo; // Restamos en lugar de igualar a 0 para no perder precisión
            currentTick++;

            // Llamamos a la función exacta que ya tenías preparada en tu CSVMetricsLogger
            CSVMetricsLogger.Instance.LogDesincronizacionMovimiento(
                currentTick,
                clientID,
                hostReplica.position,
                clientPlayer.position
            );
        }
    }
}