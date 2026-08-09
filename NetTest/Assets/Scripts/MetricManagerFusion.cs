using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Globalization; // Añadido para forzar el punto en los decimales

public class MetricManagerFusion : MonoBehaviour
{
    private string filePath;
    private float tiempoInicio;
    private float intervaloBloque = 150f; // 2.5 minutos
    private float tiempoLimite = 1800f;   // 30 minutos
    private float proximoGuardado;
    private bool experimentoActivo = true;

    private List<float> listaDTransito = new List<float>();
    private List<float> listaEValidacion = new List<float>();

    void Awake()
    {
        // Evita que Fusion o Unity destruyan este objeto al sincronizar escenas
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "Resultados_PruebaRapida.csv");

        try
        {
            File.WriteAllText(filePath, "Tiempo,Min_D,Max_D,Media_D,Mediana_D,Min_E,Max_E,Media_E,Mediana_E\n");
            Debug.Log($"[MetricManager] Archivo creado correctamente en: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"[MetricManager] ERROR FATAL: No se puede crear el archivo. ¿Está abierto en otro programa? Detalle: {e.Message}");
        }

        tiempoInicio = Time.time;
        proximoGuardado = tiempoInicio + intervaloBloque;
    }

    void Update()
    {
        if (!experimentoActivo) return;

        float tiempoActual = Time.time;

        if (tiempoActual >= proximoGuardado)
        {
            EscribirBloque();
            proximoGuardado += intervaloBloque;
        }

        if (tiempoActual >= tiempoInicio + tiempoLimite)
        {
            Debug.Log("[MetricManager] Experimento de 30 minutos finalizado.");
            experimentoActivo = false;
        }
    }

    public void AgregarDatos(float dTransito, float eValidacion)
    {
        if (!experimentoActivo) return;
        listaDTransito.Add(dTransito);
        listaEValidacion.Add(eValidacion);
    }

    private void EscribirBloque()
    {
        Debug.Log($"[MetricManager] Intentando guardar bloque... Disparos registrados: {listaDTransito.Count}");

        if (listaDTransito.Count == 0)
        {
            Debug.LogWarning("[MetricManager] No se escribirá nada en el CSV porque no hubo disparos en los últimos 2.5 minutos.");
            return;
        }

        float minD = listaDTransito.Min();
        float maxD = listaDTransito.Max();
        float mediaD = listaDTransito.Average();
        float medianaD = CalcularMediana(listaDTransito);

        float minE = listaEValidacion.Min();
        float maxE = listaEValidacion.Max();
        float mediaE = listaEValidacion.Average();
        float medianaE = CalcularMediana(listaEValidacion);

        // Utilizamos CultureInfo.InvariantCulture para forzar los puntos en lugar de las comas en los decimales
        string linea = string.Format(CultureInfo.InvariantCulture,
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
            Time.time - tiempoInicio, minD, maxD, mediaD, medianaD, minE, maxE, mediaE, medianaE);

        try
        {
            File.AppendAllText(filePath, linea);
            Debug.Log("[MetricManager] Bloque guardado con éxito en el CSV.");

            // Limpiamos solo si el guardado fue exitoso
            listaDTransito.Clear();
            listaEValidacion.Clear();
        }
        catch (IOException e)
        {
            Debug.LogError($"[MetricManager] ERROR AL GUARDAR. Cierra el archivo si lo tienes abierto. Los datos se mantendrán para el siguiente intento. Detalle: {e.Message}");
        }
    }

    private float CalcularMediana(List<float> lista)
    {
        var ordenada = lista.OrderBy(n => n).ToList();
        int count = ordenada.Count;
        if (count % 2 == 0)
            return (ordenada[count / 2 - 1] + ordenada[count / 2]) / 2.0f;
        else
            return ordenada[count / 2];
    }
}