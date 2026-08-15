using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using Fusion;

public class FusionNetworkMetricsLogger : NetworkBehaviour
{
    [Header("Configuraci?n de Registro")]
    public float intervaloRegistro = 1f; // Registra datos cada 1 segundo
    private float temporizador = 0f;

    private StreamWriter writer;
    private string filePath;

    // Variables para el c?lculo del Jitter
    private double rttAnterior = 0.0;

    public override void Spawned()
    {
        // Solo el jugador local registrar? sus m?tricas en el CSV
        if (!Object.HasInputAuthority && !Runner.IsServer) return;

        CrearArchivoCSV();
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"red_fusion_{timestamp}.csv");
        writer = new StreamWriter(filePath, false);

        // Cabecera ajustada al punto 6.1 de tu TFG
        string header = "Timestamp,ClientID,RTT_ms,Jitter_ms,PacketLoss_pct,Tx_kbps,Rx_kbps";
        writer.WriteLine(header);
        writer.Flush();

        Debug.Log($"<color=green>[Red Fusion Logger]</color> CSV de red creado en: {filePath}");
    }

    public override void FixedUpdateNetwork()
    {
        if (writer == null) return;

        temporizador += Runner.DeltaTime;
        if (temporizador >= intervaloRegistro)
        {
            RegistrarMetricasDeRed();
            temporizador = 0f;
        }
    }

    private void RegistrarMetricasDeRed()
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);
        string clientId = Runner.LocalPlayer.PlayerId.ToString();

        // 1. RTT (Latencia)
        // El cliente local PUEEDE obtener su propio RTT. Dar? 0 si juegas en la misma red local.
        double rttActualMs = Runner.GetPlayerRtt(Runner.LocalPlayer) * 1000.0;

        // 2. Jitter (Variaci?n del RTT)
        double jitterMs = rttAnterior == 0.0 ? 0.0 : Math.Abs(rttActualMs - rttAnterior);
        rttAnterior = rttActualMs;

        // 3. P?rdida de paquetes y Ancho de banda
        // Para no requerir dependencias externas o paquetes de UI complejos, 
        // usamos la p?rdida simulada del GameManager.
        double packetLossPct = CSVMetricsLogger.Instance != null ? CSVMetricsLogger.Instance.packetLossSimulado : 0.0;

        // El ancho de banda puro (Tx/Rx) es dif?cil de aislar por c?digo limpio sin el Advanced Stats package.
        // Lo dejamos preparado con 0.0 para que el CSV tenga la estructura correcta para tu tabla.
        double txKbps = 0.0;
        double rxKbps = 0.0;

        // Formateo estricto para evitar problemas de comas/puntos en el CSV
        string sRtt = rttActualMs.ToString("F2", CultureInfo.InvariantCulture);
        string sJitter = jitterMs.ToString("F2", CultureInfo.InvariantCulture);
        string sLoss = packetLossPct.ToString("F2", CultureInfo.InvariantCulture);
        string sTx = txKbps.ToString("F2", CultureInfo.InvariantCulture);
        string sRx = rxKbps.ToString("F2", CultureInfo.InvariantCulture);

        string linea = $"{time},{clientId},{sRtt},{sJitter},{sLoss},{sTx},{sRx}";

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