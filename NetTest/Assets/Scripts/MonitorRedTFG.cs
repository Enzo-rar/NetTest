using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using FishNet;

public class MonitorRedTFG : MonoBehaviour
{
    [Header("Configuración de Registro")]
    public float intervaloRegistro = 1f; // Registra datos cada 1 segundo
    private float temporizador = 0f;

    private StreamWriter writer;
    private string filePath;

    // Variables para el cálculo del Jitter
    private double rttAnterior = 0.0;
    private bool inicializado = false;

    void Update()
    {
        // 1. Esperamos a que el sistema de FishNet esté activo
        if (InstanceFinder.NetworkManager == null) return;
        if (!InstanceFinder.NetworkManager.IsClientStarted && !InstanceFinder.NetworkManager.IsServerStarted) return;

        // 2. Solo el cliente remoto debe aportar las métricas de red
        if (InstanceFinder.NetworkManager.IsServerStarted) return;

        // 3. Inicializamos el archivo CSV
        if (!inicializado)
        {
            CrearArchivoCSV();
            inicializado = true;
        }

        // 4. Bucle de registro por segundo
        temporizador += Time.unscaledDeltaTime;
        if (temporizador >= intervaloRegistro)
        {
            RegistrarMetricasDeRed();
            temporizador = 0f;
        }
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"CondicionesRed_FishNet_{timestamp}.csv");

        writer = new StreamWriter(filePath, false);

        string header = "Timestamp,RTT_ms,Jitter_ms,PacketLoss_pct,Tx_kbps,Rx_kbps";
        writer.WriteLine(header);
        writer.Flush();

        Debug.LogError($"[Monitor de Red] CSV de latencia creado en: {filePath}");
    }

    private void RegistrarMetricasDeRed()
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);

        // FishNet devuelve el RTT directamente en milisegundos a través del TimeManager
        double rttActualMs = (double)InstanceFinder.TimeManager.RoundTripTime;

        double jitterMs = rttAnterior == 0.0 ? 0.0 : Math.Abs(rttActualMs - rttAnterior);
        rttAnterior = rttActualMs;

        double packetLossPct = 0.0;
        double txKbps = 0.0;
        double rxKbps = 0.0;

        string sRtt = rttActualMs.ToString("F2", CultureInfo.InvariantCulture);
        string sJitter = jitterMs.ToString("F2", CultureInfo.InvariantCulture);
        string sLoss = packetLossPct.ToString("F2", CultureInfo.InvariantCulture);
        string sTx = txKbps.ToString("F2", CultureInfo.InvariantCulture);
        string sRx = rxKbps.ToString("F2", CultureInfo.InvariantCulture);

        string linea = $"{time},{sRtt},{sJitter},{sLoss},{sTx},{sRx}";

        writer.WriteLine(linea);
        writer.Flush();
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