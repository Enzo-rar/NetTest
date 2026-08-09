using UnityEngine;

public class TiradorAutomatico : MonoBehaviour
{
    public Transform puntoDeDisparo;

    [Tooltip("Tiempo mínimo entre disparos en segundos (Cadencia)")]
    public float cadenciaDeDisparo = 0.5f;
    private float tiempoUltimoDisparo = -1f;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(puntoDeDisparo.position, puntoDeDisparo.forward, out hit))
        {
            // Verificamos si es la diana y si ya ha pasado el tiempo de cooldown
            if (hit.collider.CompareTag("Diana") && Time.time >= tiempoUltimoDisparo + cadenciaDeDisparo)
            {
                Disparar(hit.transform.position);
                tiempoUltimoDisparo = Time.time;
            }
        }
    }

    void Disparar(Vector3 posicionDianaPercibida)
    {
        Debug.Log("Disparo efectuado. Posición percibida: " + posicionDianaPercibida);

        // Aquí registraremos el Pdisparo, el td (tick de disparo) y el Impacto de Referencia (que en este cliente es true).
        // Posteriormente, esto se enviará por red.
    }
}