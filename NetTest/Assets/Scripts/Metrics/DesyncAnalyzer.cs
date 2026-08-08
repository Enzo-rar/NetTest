using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;

public class DesyncAnalyzer : NetworkBehaviour
{
    [Header("Configuración del Experimento")]
    [Tooltip("Cada cuántos minutos se hace el cálculo y se guarda en el CSV")]
    public float minutosPorIntervalo = 5f;
    public float frecuenciaMuestreo = 0.1f;
    public string clientID = "Client_1";

    private List<float> distanciasMuestreadas = new List<float>();
    private float timerMuestreo = 0f;
    private float timerIntervalo = 0f;
    private float chivatoTimer = 0f; // Temporizador para el debug

    // FISHNET V4: SyncVar genérica
    private readonly SyncVar<Vector3> posicionAutoritativaServidor = new SyncVar<Vector3>();

    // Referencia al cuerpo real que se mueve
    private Transform playerBody;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // ELIMINADO el this.enabled = false para evitar apagados prematuros por latencia de asignación

        if (base.IsOwner && CSVMetricsLogger.Instance != null)
        {
            CSVMetricsLogger.Instance.libreriaActual = "FishNet_4.7.2R";
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        playerBody = transform.Find("PlayerBody");
        if (playerBody == null)
        {
            playerBody = transform;
        }

        if (base.Owner.IsValid && base.Owner.IsLocalClient)
        {
            SmartFloor suelo = Object.FindFirstObjectByType<SmartFloor>();
            if (suelo != null)
            {
                suelo.targetPlayer = playerBody;
            }
        }

        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
            base.TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void TimeManager_OnTick()
    {
        if (base.IsServerInitialized)
        {
            posicionAutoritativaServidor.Value = playerBody.position;
        }
    }

    void Update()
    {
        if (!base.IsOwner || CSVMetricsLogger.Instance == null) return;

        float dt = Time.deltaTime;
        timerMuestreo += dt;
        timerIntervalo += dt;
        chivatoTimer += dt;

        // Calculamos las posiciones antes del chivato para poder imprimirlas
        Vector2 clientPos2D = new Vector2(playerBody.position.x, playerBody.position.z);
        Vector2 hostPos2D = new Vector2(posicionAutoritativaServidor.Value.x, posicionAutoritativaServidor.Value.z);
        float distActual = Vector2.Distance(clientPos2D, hostPos2D);

        // --- CHIVATO PARA DEBUGGEAR POSICIONES ---
        if (chivatoTimer >= 5f) // Cada 5 segundos te dirá dónde cree cada uno que está
        {
            chivatoTimer -= 5f;
            Debug.Log($"<color=orange>[Debug Posiciones]</color> Cliente: ({clientPos2D.x:F2}, {clientPos2D.y:F2}) | Host dice que el Cliente está en: ({hostPos2D.x:F2}, {hostPos2D.y:F2}) | Error: {distActual:F2}m");
        }
        // -----------------------------------------

        if (timerMuestreo >= frecuenciaMuestreo)
        {
            timerMuestreo -= frecuenciaMuestreo;
            distanciasMuestreadas.Add(distActual);
        }

        float segundosPorIntervalo = minutosPorIntervalo * 60f;
        if (timerIntervalo >= segundosPorIntervalo)
        {
            timerIntervalo -= segundosPorIntervalo;
            CalcularYGuardarEstadisticas();
        }
    }

    private void CalcularYGuardarEstadisticas()
    {
        if (distanciasMuestreadas.Count == 0) return;

        float min = distanciasMuestreadas.Min();
        float max = distanciasMuestreadas.Max();
        float media = distanciasMuestreadas.Average();

        distanciasMuestreadas.Sort();
        int count = distanciasMuestreadas.Count;
        float mediana = (count % 2 == 0)
            ? (distanciasMuestreadas[count / 2 - 1] + distanciasMuestreadas[count / 2]) / 2f
            : distanciasMuestreadas[count / 2];

        CSVMetricsLogger.Instance.LogEstadisticasDesincronizacion(
            clientID, min, max, media, mediana, count
        );

        Debug.Log($"<color=magenta>[DesyncAnalyzer]</color> Bloque de {minutosPorIntervalo} mins guardado en CSV. Muestras: {count} | Media Desincronización: {media:F3}m");

        distanciasMuestreadas.Clear();
    }
}