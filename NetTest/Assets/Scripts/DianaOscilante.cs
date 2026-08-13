using UnityEngine;
using Unity.Netcode;

// Ahora hereda de NetworkBehaviour
public class DianaOscilante : NetworkBehaviour
{
    [Tooltip("Velocidad de oscilación de la diana.")]
    public float velocidad = 2f;

    [Tooltip("Amplitud desde el centro.")]
    public float amplitud = 20f; // Ajustado a tus pruebas

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Solo el Servidor (AWS) tiene derecho a mover la diana.
        // El cliente solo verá el reflejo del movimiento gracias al NetworkTransform.
        if (IsServer)
        {
            float desplazamiento = Mathf.Sin(Time.time * velocidad) * amplitud;
            transform.position = posicionInicial + new Vector3(desplazamiento, 0, 0);
        }
    }
}