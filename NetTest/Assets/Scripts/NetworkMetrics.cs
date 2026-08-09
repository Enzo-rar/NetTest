using System.IO;
using UnityEngine;
using Fusion;
using System.Globalization;

public class NetworkMetrics : NetworkBehaviour
{
    private string filePath;
    private float proximoGuardado;
    private double rttAnterior = 0.0;
    private bool archivoIniciado = false;

    public override void Spawned()
    {
        // Solo queremos que el cliente remoto mida su propia conexión al servidor
        if (!Runner.IsClient) return;

        // Utilizamos Application.dataPath para que vaya a la carpeta NetTest_Data en la build
        filePath = Path.Combine(Application.dataPath, "red.csv");

        try
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "Tiempo,RTT_ms,Jitter_ms\n");
            }
            archivoIniciado = true;
            Debug.Log($"[NetworkMetrics] Archivo red.csv listo en: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"[NetworkMetrics] Error al crear red.csv: {e.Message}");
        }

        proximoGuardado = Time.time + 1f; // Cada 1 segundo
    }

    public override void FixedUpdateNetwork()
    {
        if (!archivoIniciado || !Runner.IsClient) return;

        if (Time.time >= proximoGuardado)
        {
            RegistrarMetricas();
            proximoGuardado = Time.time + 1f;
        }
    }

    private void RegistrarMetricas()
    {
        // Fusion devuelve el RTT en segundos. Lo multiplicamos por 1000 para tenerlo en milisegundos (ms)
        double rttActual = Runner.GetPlayerRtt(Runner.LocalPlayer) * 1000.0;

        // El Jitter es la variación entre el RTT actual y el RTT de la muestra anterior[cite: 1]
        double jitter = rttAnterior == 0.0 ? 0.0 : System.Math.Abs(rttActual - rttAnterior);
        rttAnterior = rttActual;

        // Formateamos con InvariantCulture para asegurar el punto en los decimales
        string linea = string.Format(CultureInfo.InvariantCulture, "{0:F2},{1:F2},{2:F2}\n",
            Time.time, rttActual, jitter);

        try
        {
            File.AppendAllText(filePath, linea);
        }
        catch (IOException e)
        {
            Debug.LogWarning($"[NetworkMetrics] No se pudo escribir en red.csv (¿archivo abierto?): {e.Message}");
        }
    }
}