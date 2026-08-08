using System;
using System.IO;
using System.Globalization;
using UnityEngine;

public class CSVMetricsLogger : MonoBehaviour
{
    public static CSVMetricsLogger Instance;

    private StreamWriter writer;
    private string filePath;

    [Header("Configuración del Experimento")]
    public string libreriaActual = "FishNet_4.7.2R";
    public int latenciaSimuladaMs = 0;
    public float packetLossSimulado = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CrearArchivoCSV();
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"Metricas_{libreriaActual}_{timestamp}.csv");
        writer = new StreamWriter(filePath, false);

        string header = "Timestamp_GameTime,Libreria,Latencia_ms,PacketLoss,ClientID,Min_Dist,Max_Dist,Media_Dist,Mediana_Dist,Total_Muestras";
        writer.WriteLine(header);
        writer.Flush();

        Debug.Log($"<color=cyan>[CSVLogger]</color> Archivo de métricas creado en: {filePath}");
    }

    /// <summary>
    /// Guarda los bloques estadísticos de desincronización
    /// </summary>
    public void LogEstadisticasDesincronizacion(string clientID, float min, float max, float media, float mediana, int totalMuestras)
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);

        string sMin = min.ToString("F4", CultureInfo.InvariantCulture);
        string sMax = max.ToString("F4", CultureInfo.InvariantCulture);
        string sMedia = media.ToString("F4", CultureInfo.InvariantCulture);
        string sMediana = mediana.ToString("F4", CultureInfo.InvariantCulture);

        string linea = $"{time},{libreriaActual},{latenciaSimuladaMs},{packetLossSimulado},{clientID},{sMin},{sMax},{sMedia},{sMediana},{totalMuestras}";

        EscribirLinea(linea);
    }

    // Para la prueba de la tecla P que tienes en PlayerMovement
    public void LogDesincronizacionMovimiento(int tick, string player, Vector3 pos1, Vector3 pos2)
    {
        Debug.Log($"<color=yellow>[Prueba Manual CSV]</color> Tick: {tick} - {player} | P1: {pos1} - P2: {pos2}");
        // Aquí podrías guardar el dato bruto si en un futuro te hace falta
    }

    private void EscribirLinea(string linea)
    {
        if (writer != null)
        {
            writer.WriteLine(linea);
            writer.Flush(); // Flush inmediato por seguridad
        }
    }

    private void OnDestroy()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }
    }
}