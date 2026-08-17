using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using FishNet; // Espacio de nombres necesario para FishNet

public class VolcadorCSV : MonoBehaviour
{
    public static VolcadorCSV Instancia;

    private string rutaCSV;
    private List<float>[] divergenciasPorRebote;

    private float temporizador = 0f;
    private const float DURACION_BLOQUE = 150f;
    private int bloqueActual = 0;

    private bool inicializado = false;

    void Awake()
    {
        Instancia = this;
        divergenciasPorRebote = new List<float>[5];
        for (int i = 0; i < 5; i++) divergenciasPorRebote[i] = new List<float>();
    }

    void Update()
    {
        // 1. Comprobar que FishNet está inicializado
        if (InstanceFinder.NetworkManager == null) return;

        // 2. Esperar a que la red esté conectada (como Servidor o como Cliente)
        if (!InstanceFinder.NetworkManager.IsClientStarted && !InstanceFinder.NetworkManager.IsServerStarted) return;

        // 3. Según tu TFG, solo el cliente remoto aporta observaciones al análisis
        if (InstanceFinder.NetworkManager.IsServerStarted) return;

        // 4. Inicializar archivo si somos un cliente remoto
        if (!inicializado)
        {
            rutaCSV = Path.Combine(Application.dataPath, "Resultados_FishNet_Experimento3.csv");
            if (!File.Exists(rutaCSV))
            {
                string cabecera = "Bloque";
                for (int i = 1; i <= 5; i++)
                {
                    cabecera += $",R{i}_Min,R{i}_Max,R{i}_Media,R{i}_Mediana";
                }
                cabecera += "\n";
                File.WriteAllText(rutaCSV, cabecera);
            }
            Debug.LogError($"[VolcadorCSV] CLIENTE INICIALIZADO. Guardando en: {rutaCSV}");
            inicializado = true;
        }

        temporizador += Time.deltaTime;

        if (temporizador >= DURACION_BLOQUE)
        {
            temporizador -= DURACION_BLOQUE;
            VolcarDatosFisicamente();
            bloqueActual++;
        }
    }

    // FishNet usa uint para el Tick (a diferencia de Fusion que usa int o struct)
    public void RegistrarLanzamiento(uint tick, float[] discrepancias)
    {
        for (int i = 0; i < 5; i++)
        {
            divergenciasPorRebote[i].Add(discrepancias[i]);
        }
    }

    private void VolcarDatosFisicamente()
    {
        int totalLanzamientos = divergenciasPorRebote[0].Count;
        if (totalLanzamientos == 0) return;

        string lineaDato = $"{bloqueActual}";

        for (int i = 0; i < 5; i++)
        {
            List<float> datos = divergenciasPorRebote[i];

            float min = datos.Min();
            float max = datos.Max();
            float media = datos.Average();
            float mediana = CalcularMediana(datos);

            // Obligamos a usar InvariantCulture (.)
            lineaDato += string.Format(CultureInfo.InvariantCulture, ",{0:F6},{1:F6},{2:F6},{3:F6}", min, max, media, mediana);

            datos.Clear();
        }

        lineaDato += "\n";
        File.AppendAllText(rutaCSV, lineaDato);
        Debug.LogError($"[VolcadorCSV] ESTADÍSTICAS DEL BLOQUE {bloqueActual} VOLCADAS.");
    }

    private float CalcularMediana(List<float> datos)
    {
        if (datos == null || datos.Count == 0) return 0f;
        List<float> ordenados = datos.OrderBy(x => x).ToList();
        int count = ordenados.Count;
        int mid = count / 2;

        if (count % 2 == 0) return (ordenados[mid - 1] + ordenados[mid]) / 2.0f;
        else return ordenados[mid];
    }

    void OnApplicationQuit()
    {
        // Solo volcamos el último bloque si somos cliente remoto
        if (InstanceFinder.NetworkManager != null && !InstanceFinder.NetworkManager.IsServerStarted)
        {
            VolcarDatosFisicamente();
        }
    }
}