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
    public string libreriaActual = "Unity_NGO"; // Ya te lo dejo en NGO
    public int latenciaSimuladaMs = 0;
    public float packetLossSimulado = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CrearArchivoCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"Metricas_{libreriaActual}_{timestamp}.csv");

        writer = new StreamWriter(filePath, false);

        // Cabecera exacta agrupada
        string header = "Timestamp_GameTime,Libreria,Latencia_ms,PacketLoss,ClientID,Min_Dist,Max_Dist,Media_Dist,Mediana_Dist,Total_Muestras";
        writer.WriteLine(header);
        writer.Flush();

        Debug.Log($"<color=cyan>[CSVLogger]</color> Archivo de métricas creado en: {filePath}");
    }

    public void LogBloqueEstadistico(float gameTime, string clientID, float min, float max, float media, float mediana, int muestras)
    {
        string time = gameTime.ToString("F3", CultureInfo.InvariantCulture);
        string sMin = min.ToString("F4", CultureInfo.InvariantCulture);
        string sMax = max.ToString("F4", CultureInfo.InvariantCulture);
        string sMedia = media.ToString("F4", CultureInfo.InvariantCulture);
        string sMediana = mediana.ToString("F4", CultureInfo.InvariantCulture);

        string linea = $"{time},{libreriaActual},{latenciaSimuladaMs},{packetLossSimulado},{clientID},{sMin},{sMax},{sMedia},{sMediana},{muestras}";

        if (writer != null)
        {
            writer.WriteLine(linea);
            writer.Flush(); // ¡CRÍTICO! Forzamos a Windows a guardarlo en disco al instante
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