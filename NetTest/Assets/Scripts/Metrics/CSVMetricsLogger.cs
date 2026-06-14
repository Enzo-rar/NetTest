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
    public string libreriaActual = "Base_Offline";
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


        string header = "Timestamp,Libreria,Tick,Latencia_ms,PacketLoss,ClientID,Host_X,Host_Y,Host_Z,Client_X,Client_Y,Client_Z,Distancia_Desincronizacion,Client_Hit_Registrado,Host_Hit_Registrado";
        writer.WriteLine(header);
        writer.Flush();

        Debug.Log($"<color=cyan>[CSVLogger]</color> Archivo de métricas creado (Modo 4 Jugadores) en: {filePath}");
    }

    /// <summary>
    /// Registra la diferencia de posición. Ahora requiere saber qué cliente estamos evaluando.
    /// </summary>
    public void LogDesincronizacionMovimiento(int tickActual, string clientID, Vector3 posHost, Vector3 posClient)
    {
        float distancia = Vector3.Distance(posHost, posClient);

        string hostX = posHost.x.ToString("F3", CultureInfo.InvariantCulture);
        string hostY = posHost.y.ToString("F3", CultureInfo.InvariantCulture);
        string hostZ = posHost.z.ToString("F3", CultureInfo.InvariantCulture);

        string clientX = posClient.x.ToString("F3", CultureInfo.InvariantCulture);
        string clientY = posClient.y.ToString("F3", CultureInfo.InvariantCulture);
        string clientZ = posClient.z.ToString("F3", CultureInfo.InvariantCulture);

        string dist = distancia.ToString("F3", CultureInfo.InvariantCulture);
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);

       
        string linea = $"{time},{libreriaActual},{tickActual},{latenciaSimuladaMs},{packetLossSimulado},{clientID},{hostX},{hostY},{hostZ},{clientX},{clientY},{clientZ},{dist},,";
        EscribirLinea(linea);
    }

    /// <summary>
    /// Registra los disparos, indicando qué cliente apretó el gatillo.
    /// </summary>
    public void LogHit(int tickActual, string shooterClientID, bool clienteAcerto, bool hostValido)
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);

        
        string linea = $"{time},{libreriaActual},{tickActual},{latenciaSimuladaMs},{packetLossSimulado},{shooterClientID},,,,,,,,{(clienteAcerto ? 1 : 0)},{(hostValido ? 1 : 0)}";
        EscribirLinea(linea);
    }

    private void EscribirLinea(string linea)
    {
        if (writer != null)
        {
            writer.WriteLine(linea);
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