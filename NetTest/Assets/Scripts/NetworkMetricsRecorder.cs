using UnityEngine;
using Unity.Netcode;
using System.IO;
using System.Text;

public class NetworkMetricsRecorder : NetworkBehaviour
{
    private float tiempoUltimoPing = 0f;
    private float intervaloPing = 1f; // 1 segundo

    private float rttAnterior = 0f;
    private string csvFilePath;

    void Start()
    {
        csvFilePath = Path.Combine(Application.dataPath, "Experimento_Red_RTT_Jitter.csv");

        if (IsClient && IsOwner)
        {
            if (!File.Exists(csvFilePath))
                File.WriteAllText(csvFilePath, "Marca_Temporal,RTT_ms,Jitter_ms\n");
        }
    }

    void Update()
    {
        if (IsClient && IsOwner && !IsServer)
        {
            if (Time.time >= tiempoUltimoPing + intervaloPing)
            {
                PingServerRpc(Time.realtimeSinceStartup);
                tiempoUltimoPing = Time.time;
            }
        }
    }

    [ServerRpc]
    void PingServerRpc(float clientSendTime)
    {
        PongClientRpc(clientSendTime);
    }

    [ClientRpc]
    void PongClientRpc(float clientSendTime)
    {
        if (!IsOwner) return;

        float rttActual = (Time.realtimeSinceStartup - clientSendTime) * 1000f;
        float jitter = rttAnterior > 0 ? Mathf.Abs(rttActual - rttAnterior) : 0f;
        rttAnterior = rttActual;

        using (StreamWriter writer = new StreamWriter(csvFilePath, true, Encoding.UTF8))
        {
            // Forzamos el uso de puntos para los decimales
            writer.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1},{2}", Time.realtimeSinceStartup, rttActual, jitter));
        }
    }
}