using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using Fusion;

public class MonitorRedTFG : MonoBehaviour
{
    [Header("Configuración de Registro")]
    public float intervaloRegistro = 1f; // Registra datos cada 1 segundo
    private float temporizador = 0f;

    private StreamWriter writer;
    private string filePath;

    // Variables para el cálculo del Jitter
    private double rttAnterior = 0.0;

    private NetworkRunner _runner;
    private bool inicializado = false;

    void Update()
    {
        // 1. Buscamos el Runner si no lo tenemos
        if (_runner == null)
        {
            _runner = FindFirstObjectByType<NetworkRunner>();
            return;
        }

        // 2. Esperamos a que la red esté conectada
        if (!_runner.IsRunning) return;

        // 3. Según tu TFG, solo el cliente remoto debe aportar las métricas de red para el análisis
        if (_runner.IsServer) return;

        // 4. Inicializamos el archivo CSV solo una vez para el cliente
        if (!inicializado)
        {
            CrearArchivoCSV();
            inicializado = true;
        }

        // 5. Bucle de tiempo para registrar cada segundo
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
        filePath = Path.Combine(Application.dataPath, $"CondicionesRed_Fusion_{timestamp}.csv");

        writer = new StreamWriter(filePath, false);

        // Cabecera basada en los requisitos de monitorización de tu TFG
        string header = "Timestamp,RTT_ms,Jitter_ms,PacketLoss_pct,Tx_kbps,Rx_kbps";
        writer.WriteLine(header);
        writer.Flush();

        // Usamos LogError para que lo veas fácilmente en rojo en la consola del .bat
        Debug.LogError($"[Monitor de Red] CSV de latencia creado en: {filePath}");
    }

    private void RegistrarMetricasDeRed()
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);

        // 1. RTT (Latencia) en milisegundos
        // Multiplicamos por 1000 porque Fusion devuelve el RTT en segundos
        double rttActualMs = _runner.GetPlayerRtt(_runner.LocalPlayer) * 1000.0;

        // 2. Jitter (Variación del RTT en milisegundos)
        double jitterMs = rttAnterior == 0.0 ? 0.0 : Math.Abs(rttActualMs - rttAnterior);
        rttAnterior = rttActualMs;

        // Estos valores se inicializan a 0.0 para mantener la estructura de la tabla 6.1 de tu memoria
        // Si más adelante integras herramientas para pérdida de paquetes y ancho de banda, las enlazas aquí
        double packetLossPct = 0.0;
        double txKbps = 0.0;
        double rxKbps = 0.0;

        // Formateo estricto a cultura invariante (puntos decimales)
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