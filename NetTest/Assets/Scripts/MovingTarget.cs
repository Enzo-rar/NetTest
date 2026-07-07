using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [Tooltip("Distancia en metros que recorrerá hacia los lados")]
    public float amplitud = 10f;

    [Tooltip("Velocidad a la que se mueve")]
    public float velocidad = 3f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Movimiento de péndulo infinito en el eje X
        float nuevaX = posicionInicial.x + Mathf.Sin(Time.time * velocidad) * amplitud;
        transform.position = new Vector3(nuevaX, transform.position.y, transform.position.z);
    }
}