using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class NGONetworkMetricsLogger : NetworkBehaviour
{
    [Header("Configuración de Registro")]
    public float intervaloRegistro = 1f;
    private float temporizador = 0f;

    private StreamWriter writer;
    private string filePath;

    private float rttAnterior = -1f;
    private bool isLoggerActive = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner || !IsClient)
        {
            enabled = false;
            return;
        }

        isLoggerActive = true;
        CrearArchivoCSV();
    }

    private void CrearArchivoCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"red_ngo_{timestamp}.csv");
        writer = new StreamWriter(filePath, false);

        string header = "Timestamp,ClientID,RTT_ms,Jitter_ms";
        writer.WriteLine(header);
        writer.Flush();
    }

    void Update()
    {
        if (!isLoggerActive || writer == null) return;

        temporizador += Time.deltaTime;

        // Cada segundo, en lugar de preguntar a UTP, lanzamos nuestro propio paquete de Ping a AWS
        if (temporizador >= intervaloRegistro)
        {
            // Time.realtimeSinceStartup es el reloj más preciso de Unity (ignora la escala de tiempo)
            PingServerRpc(Time.realtimeSinceStartup);
            temporizador = 0f;
        }
    }

    // --- MAGIA DEL PING/PONG ---

    [ServerRpc]
    private void PingServerRpc(float timeSent, ServerRpcParams serverParams = default)
    {
        // El servidor recibe el Ping de AWS.
        // Preparamos la respuesta para mandársela ÚNICAMENTE al cliente que nos hizo el Ping
        ClientRpcParams clientParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverParams.Receive.SenderClientId }
            }
        };

        // AWS devuelve el "Pong" con la misma marca de tiempo
        PongClientRpc(timeSent, clientParams);
    }

    [ClientRpc]
    private void PongClientRpc(float timeSent, ClientRpcParams clientParams = default)
    {
        // Este código solo lo ejecuta el cliente original cuando AWS le responde
        if (!IsOwner || writer == null) return;

        // RTT: Tiempo actual menos el tiempo en el que mandamos el Ping, multiplicado por 1000 para ms
        float rttActualMs = (Time.realtimeSinceStartup - timeSent) * 1000f;

        // Jitter: Diferencia absoluta con el RTT anterior
        float jitterMs = rttAnterior < 0f ? 0f : Mathf.Abs(rttActualMs - rttAnterior);
        rttAnterior = rttActualMs;

        // Guardamos los datos en el CSV instantáneamente
        string timeStr = Time.time.ToString("F3", CultureInfo.InvariantCulture);
        string clientIdStr = NetworkManager.Singleton.LocalClientId.ToString();
        string sRtt = rttActualMs.ToString("F2", CultureInfo.InvariantCulture);
        string sJitter = jitterMs.ToString("F2", CultureInfo.InvariantCulture);

        string linea = $"{timeStr},{clientIdStr},{sRtt},{sJitter}";
        writer.WriteLine(linea);
        writer.Flush();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }
    }
}