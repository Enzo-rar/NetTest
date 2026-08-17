using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class MonitorRed : NetworkBehaviour
{
    [Header("Configuración de Registro")]
    public float intervaloRegistro = 1f; // Cada cuánto escribe en el CSV
    public float intervaloPing = 0.2f;   // Cada cuánto calcula el RTT (200ms)

    private float temporizadorRegistro = 0f;
    private float temporizadorPing = 0f;

    private StreamWriter writer;
    private string filePath;

    // Variables de latencia
    private double rttActualMs = 0.0;
    private double rttAnterior = 0.0;
    private double jitterMs = 0.0;

    private bool inicializado = false;

    void Update()
    {
        // Si no estamos conectados o somos el servidor, no hacemos nada
        if (!IsSpawned || IsServer) return;

        if (!inicializado)
        {
            CrearArchivoCSV();
            inicializado = true;
        }

        // 1. Bucle del Ping para actualizar el RTT frecuentemente
        temporizadorPing += Time.unscaledDeltaTime;
        if (temporizadorPing >= intervaloPing)
        {
            EnviarPingRpc(Time.realtimeSinceStartup);
            temporizadorPing = 0f;
        }

        // 2. Bucle de Registro para volcar los datos al CSV
        temporizadorRegistro += Time.unscaledDeltaTime;
        if (temporizadorRegistro >= intervaloRegistro)
        {
            RegistrarMetricasDeRed();
            temporizadorRegistro = 0f;
        }
    }

    // NUEVA SINTAXIS NGO 2.13+: El cliente manda esto al servidor
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnviarPingRpc(float clientSendTime, RpcParams rpcParams = default)
    {
        // Extraemos la ID del cliente que acaba de enviar este ping
        ulong idCliente = rpcParams.Receive.SenderClientId;

        // Usamos RpcTarget.Single para dirigir la respuesta EXCLUSIVAMENTE a ese cliente.
        // RpcTargetUse.Temp evita crear basura en memoria (garbage collection).
        ResponderPingRpc(clientSendTime, RpcTarget.Single(idCliente, RpcTargetUse.Temp));
    }

    // NUEVA SINTAXIS NGO 2.13+: El servidor manda esto de vuelta al cliente especificado
    [Rpc(SendTo.SpecifiedInParams)]
    private void ResponderPingRpc(float clientSendTime, RpcParams rpcParams = default)
    {
        // RTT = Tiempo actual - Tiempo en el que enviamos el ping (pasado a milisegundos)
        rttActualMs = (Time.realtimeSinceStartup - clientSendTime) * 1000.0;

        // Jitter = Diferencia absoluta entre el RTT actual y el anterior
        jitterMs = rttAnterior == 0.0 ? 0.0 : Math.Abs(rttActualMs - rttAnterior);
        rttAnterior = rttActualMs;
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"CondicionesRed_NGO_{timestamp}.csv");

        writer = new StreamWriter(filePath, false);
        string header = "Timestamp,RTT_ms,Jitter_ms,PacketLoss_pct,Tx_kbps,Rx_kbps";
        writer.WriteLine(header);
        writer.Flush();

        Debug.LogError($"[Monitor NGO] CSV de latencia creado en: {filePath}");
    }

    private void RegistrarMetricasDeRed()
    {
        string time = Time.time.ToString("F3", CultureInfo.InvariantCulture);

        // Variables fijas a 0 porque no las necesitamos
        double packetLossPct = 0.0;
        double txKbps = 0.0;
        double rxKbps = 0.0;

        // Formateo estricto
        string sRtt = rttActualMs.ToString("F2", CultureInfo.InvariantCulture);
        string sJitter = jitterMs.ToString("F2", CultureInfo.InvariantCulture);
        string sLoss = packetLossPct.ToString("F2", CultureInfo.InvariantCulture);
        string sTx = txKbps.ToString("F2", CultureInfo.InvariantCulture);
        string sRx = rxKbps.ToString("F2", CultureInfo.InvariantCulture);

        string linea = $"{time},{sRtt},{sJitter},{sLoss},{sTx},{sRx}";
        writer.WriteLine(linea);
        writer.Flush();
    }

    public override void OnDestroy()
    {
        base.OnDestroy(); // Importante en NGO
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }
    }
}