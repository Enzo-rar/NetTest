using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using FishNet.Object;

public class FishNetNetworkMetricsLogger : NetworkBehaviour
{
    [Header("Configuración de Registro")]
    public float intervaloRegistro = 1f;
    private float temporizador = 0f;

    private StreamWriter writer;
    private string filePath;

    private long rttAnterior = -1;
    private bool isLoggerActive = false; // Bandera de seguridad

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // Validación segura de FishNet v4 (igual que en tu DesyncAnalyzer)
        if (base.Owner.IsValid && base.Owner.IsLocalClient)
        {
            isLoggerActive = true;
            CrearArchivoCSV();
        }
        else
        {
            this.enabled = false;
        }
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"red_fishnet_{timestamp}.csv");
        writer = new StreamWriter(filePath, false);

        string header = "Timestamp,ClientID,RTT_ms,Jitter_ms";
        writer.WriteLine(header);
        writer.Flush();

        Debug.Log($"<color=green>[FishNet Logger]</color> CSV de red creado en: {filePath}");
    }

    void Update()
    {
        if (!isLoggerActive || writer == null) return;

        temporizador += Time.deltaTime;
        if (temporizador >= intervaloRegistro)
        {
            RegistrarMetricasDeRed();
            temporizador = 0f;
        }
    }

    private void RegistrarMetricasDeRed()
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);
        string clientId = base.Owner.ClientId.ToString();

        long rttActualMs = base.TimeManager.RoundTripTime;
        long jitterMs = rttAnterior == -1 ? 0 : Math.Abs(rttActualMs - rttAnterior);
        rttAnterior = rttActualMs;

        string sRtt = rttActualMs.ToString(CultureInfo.InvariantCulture);
        string sJitter = jitterMs.ToString(CultureInfo.InvariantCulture);

        string linea = $"{time},{clientId},{sRtt},{sJitter}";

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