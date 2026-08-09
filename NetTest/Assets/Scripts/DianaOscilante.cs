using UnityEngine;

public class DianaOscilante : MonoBehaviour
{
    [Tooltip("Velocidad de oscilación de la diana.")]
    public float velocidad = 2f;

    [Tooltip("Amplitud desde el centro. 15 significa un recorrido total de 30 unidades.")]
    private float amplitud = 15f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Movimiento oscilatorio usando la función Seno para simular el trayecto de -15 a 15 en X
        float desplazamiento = Mathf.Sin(Time.time * velocidad) * amplitud;
        transform.position = posicionInicial + new Vector3(desplazamiento, 0, 0);
    }
}