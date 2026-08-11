using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using System.IO;
using System.Globalization; // Necesario para asegurar el punto en los decimales
using FishNet.Managing.Timing;
using FishNet.Component.ColliderRollback;

public class TiradorAutomatico : NetworkBehaviour
{
    public Transform puntoDeDisparo;
    public float cadenciaDeDisparo = 0.5f;
    private float tiempoUltimoDisparo = -1f;

    // Variables para estructurar el volcado de datos
    private string rutaCSV;
    private float temporizadorVolcado = 0f;
    private float intervaloVolcado = 150f; // 2.5 minutos

    // Listas para almacenar los datos crudos del bloque actual
    private List<float> dTransitoBloque = new List<float>();
    private List<float> eValidacionBloque = new List<float>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        rutaCSV = Application.dataPath + "/Metricas_Caso2_Reducido.csv";
        if (!File.Exists(rutaCSV))
        {
            // Cabecera exacta solicitada
            File.WriteAllText(rutaCSV, "Tiempo,Min_D,Max_D,Media_D,Mediana_D,Min_E,Max_E,Media_E,Mediana_E\n");
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (puntoDeDisparo != null)
            {
                Debug.DrawRay(puntoDeDisparo.position, puntoDeDisparo.forward * 30f, Color.red);
            }

            if (Time.time >= tiempoUltimoDisparo + cadenciaDeDisparo)
            {
                RaycastHit hit;
                if (Physics.Raycast(puntoDeDisparo.position, puntoDeDisparo.forward, out hit))
                {
                    if (hit.collider.CompareTag("Diana"))
                    {
                        // Restamos 5 ticks artificiales para simular latencia y ver bien la diferencia
                        uint td = TimeManager.Tick;
                        Vector3 pDisparo = hit.collider.transform.position;

                        // Ahora enviamos también desde dónde disparamos y hacia dónde
                        DispararServerRpc(td, pDisparo, puntoDeDisparo.position, puntoDeDisparo.forward);
                        tiempoUltimoDisparo = Time.time;
                    }
                }
            }
        }

        if (IsServerInitialized)
        {
            temporizadorVolcado += Time.deltaTime;
            if (temporizadorVolcado >= intervaloVolcado)
            {
                VolcarDatosAlCSV();
                temporizadorVolcado = 0f;
            }
        }
    }

    [ServerRpc]
    void DispararServerRpc(uint td, Vector3 pDisparo, Vector3 origenRayo, Vector3 dirRayo)
    {
        GameObject diana = GameObject.FindGameObjectWithTag("Diana");
        if (diana == null) return;

        // 1. Capturamos P_Recepcion (Presente visual)
        Vector3 pRecepcion = diana.transform.position;

        // 2. Rebobinamos las cajas de colisión físicas
        PreciseTick tickPreciso = new PreciseTick(td, 0);
        base.RollbackManager.Rollback(tickPreciso, RollbackPhysicsType.Physics);

        // CRUCIAL: Obligamos a Unity a sincronizar el motor de físicas con los colliders movidos
        Physics.SyncTransforms();

        Vector3 pValidacion = pRecepcion; // Por defecto
        bool impactoValidado = false;

        // 3. LA VALIDACIÓN DEL SERVIDOR: Tiramos el rayo en el pasado
        if (Physics.Raycast(origenRayo, dirRayo, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Diana"))
            {
                // Extraemos la posición directamente del collider rebobinado al que hemos acertado
                pValidacion = hit.collider.transform.position;
                impactoValidado = true;
            }
        }

        // 4. Restauramos el tiempo al presente
        base.RollbackManager.Return();

        // Calculamos las métricas
        float dTransito = Vector3.Distance(pRecepcion, pDisparo);
        float eValidacion = Vector3.Distance(pValidacion, pDisparo);

        dTransitoBloque.Add(dTransito);
        eValidacionBloque.Add(eValidacion);

        if (dTransito == eValidacion || !impactoValidado)
        {
            Debug.LogWarning($"[ALERTA] Impacto: {impactoValidado} | D_Transito: {dTransito} | E_Validacion: {eValidacion}");
        }
        else
        {
            Debug.Log($"[ÉXITO] Servidor validó el tiro en el pasado. D_Transito: {dTransito} -> E_Validacion: {eValidacion}");
        }
    }

    private void VolcarDatosAlCSV()
    {
        if (dTransitoBloque.Count == 0 || eValidacionBloque.Count == 0) return;

        // Cálculos para D_transito
        float minD = Mathf.Min(dTransitoBloque.ToArray());
        float maxD = Mathf.Max(dTransitoBloque.ToArray());
        float mediaD = CalcularMedia(dTransitoBloque);
        float medianaD = CalcularMediana(dTransitoBloque);

        // Cálculos para E_validacion
        float minE = Mathf.Min(eValidacionBloque.ToArray());
        float maxE = Mathf.Max(eValidacionBloque.ToArray());
        float mediaE = CalcularMedia(eValidacionBloque);
        float medianaE = CalcularMediana(eValidacionBloque);

        // Formateamos la línea asegurando que usamos punto para los decimales
        string linea = string.Format(CultureInfo.InvariantCulture,
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
            Time.time, minD, maxD, mediaD, medianaD, minE, maxE, mediaE, medianaE);

        using (StreamWriter sw = File.AppendText(rutaCSV))
        {
            sw.WriteLine(linea);
        }

        Debug.Log($"[Servidor] Bloque de 2.5 min volcado: {dTransitoBloque.Count} muestras procesadas.");

        // Limpiamos las listas para el siguiente bloque
        dTransitoBloque.Clear();
        eValidacionBloque.Clear();
    }

    // --- Funciones auxiliares matemáticas ---

    private float CalcularMedia(List<float> lista)
    {
        float suma = 0f;
        foreach (float valor in lista)
        {
            suma += valor;
        }
        return suma / lista.Count;
    }

    private float CalcularMediana(List<float> lista)
    {
        // Ordenamos una copia de la lista para no alterar la original
        List<float> ordenada = new List<float>(lista);
        ordenada.Sort();

        int count = ordenada.Count;
        if (count % 2 == 0)
        {
            // Si es par, la mediana es la media de los dos valores centrales
            return (ordenada[(count / 2) - 1] + ordenada[count / 2]) / 2f;
        }
        else
        {
            // Si es impar, es el valor central
            return ordenada[count / 2];
        }
    }
}