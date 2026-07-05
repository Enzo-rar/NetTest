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
    public string libreriaActual = "Photon_Fusion_2";
    public int latenciaSimuladaMs = 0; // Si usas AWS en Brasil, aquí puedes poner 0 porque el lag será real
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

        // Cabecera ajustada para las estadísticas de desincronización por intervalos
        string header = "Timestamp_GameTime,Libreria,Latencia_ms,PacketLoss,ClientID,Min_Dist,Max_Dist,Media_Dist,Mediana_Dist,Total_Muestras";
        writer.WriteLine(header);
        writer.Flush();

        Debug.Log($"<color=cyan>[CSVLogger]</color> Archivo de métricas creado en: {filePath}");
    }

    /// <summary>
    /// Guarda los bloques estadísticos de desincronización (ej: cada 10 minutos).
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

    private void EscribirLinea(string linea)
    {
        if (writer != null)
        {
            writer.WriteLine(linea);
            // Obligamos a guardar en disco al momento por si el juego crashea en la hora 5
            writer.Flush();
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