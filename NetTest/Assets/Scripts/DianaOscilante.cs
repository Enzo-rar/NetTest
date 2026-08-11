using UnityEngine;
using FishNet.Object;

public class DianaOscilante : NetworkBehaviour
{
    [Tooltip("Velocidad de oscilación de la diana.")]
    public float velocidad = 2f;

    [Tooltip("Amplitud desde el centro.")]
    public float amplitud = 20f;

    private Vector3 posicionInicial;

    public override void OnStartServer()
    {
        base.OnStartServer();
        posicionInicial = transform.position;

        // Nos suscribimos al evento de Tick de Fish-Net
        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (base.TimeManager != null)
        {
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }
    }

    private void TimeManager_OnTick()
    {
        // Calculamos el tiempo de red exacto multiplicando el Tick actual por la duración de cada Tick
        float tiempoRed = (float)base.TimeManager.Tick * (float)base.TimeManager.TickDelta;

        float desplazamiento = Mathf.Sin(tiempoRed * velocidad) * amplitud;
        transform.position = posicionInicial + new Vector3(desplazamiento, 0, 0);
    }
}