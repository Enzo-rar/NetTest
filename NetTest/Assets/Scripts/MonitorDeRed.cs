using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class MonitorDeRed : NetworkBehaviour
{
    // Variables para el volcado
    private string rutaCSV;
    private float temporizador = 0f;
    private float intervaloBloque = 5f; // 2.5 minutos

    // Listas para las muestras del bloque actual
    private List<uint> muestrasRTT = new List<uint>();
    private List<uint> muestrasJitter = new List<uint>();

    // Variable para calcular la variación (Jitter)
    private uint rttAnterior = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Bloqueo estricto: Usamos IsServerInitialized como pide Fish-Net 4.x
        if (base.IsServerInitialized) return;

        // Definimos la ruta en NetTest_Data
        rutaCSV = Application.dataPath + "/red.csv";

        if (!File.Exists(rutaCSV))
        {
            File.WriteAllText(rutaCSV, "Tiempo,RTT_Mediana_ms,Jitter_Mediana_ms\n");
        }

        // Comenzamos a tomar una muestra de la red cada 1 segundo
        InvokeRepeating(nameof(RecolectarMuestra), 1f, 1f);
    }

    private void RecolectarMuestra()
    {
        // Extraemos el RTT nativo que Fish-Net ya calcula por debajo
        uint rttActual = (uint)base.TimeManager.RoundTripTime;

        // El Jitter es la variación absoluta entre el ping actual y el anterior
        uint jitterActual = (uint)Mathf.Abs((int)rttActual - (int)rttAnterior);

        muestrasRTT.Add(rttActual);
        muestrasJitter.Add(jitterActual);

        rttAnterior = rttActual;
    }

    void Update()
    {
        // Bloqueo estricto: Usamos IsServerInitialized
        if (base.IsServerInitialized) return;

        temporizador += Time.deltaTime;
        if (temporizador >= intervaloBloque)
        {
            VolcarDatosAlCSV();
            temporizador = 0f;
        }
    }

    private void VolcarDatosAlCSV()
    {
        if (muestrasRTT.Count == 0) return;

        // Calculamos las medianas del bloque
        float rttMediana = CalcularMediana(muestrasRTT);
        float jitterMediana = CalcularMediana(muestrasJitter);

        // Formateamos con punto decimal
        string linea = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}",
            Time.time, rttMediana, jitterMediana);

        using (StreamWriter sw = File.AppendText(rutaCSV))
        {
            sw.WriteLine(linea);
        }

        Debug.Log($"[Monitor de Red] Bloque volcado a red.csv. RTT Mediana: {rttMediana}ms | Jitter Mediana: {jitterMediana}ms");

        // Limpiamos las listas para el siguiente bloque de 2.5 minutos
        muestrasRTT.Clear();
        muestrasJitter.Clear();
    }

    // --- Función matemática auxiliar ---
    private float CalcularMediana(List<uint> lista)
    {
        List<uint> ordenada = new List<uint>(lista);
        ordenada.Sort();

        int count = ordenada.Count;
        if (count == 0) return 0;

        if (count % 2 == 0)
        {
            return (ordenada[(count / 2) - 1] + ordenada[count / 2]) / 2f;
        }
        else
        {
            return ordenada[count / 2];
        }
    }
}