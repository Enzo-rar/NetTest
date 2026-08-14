using System.Collections;
using UnityEngine;

public class LanzadorEsferas : MonoBehaviour
{
    [Header("Configuración del Lanzamiento")]
    public GameObject prefabEsfera;
    public float fuerzaLanzamiento = 25f;
    public float intervaloEntreLanzamientos = 5f;

    void Start()
    {
        // Inicia el bucle de lanzamientos repetidos
        StartCoroutine(RutinaLanzamiento());
    }

    IEnumerator RutinaLanzamiento()
    {
        while (true)
        {
            LanzarEsfera();
            yield return new WaitForSeconds(intervaloEntreLanzamientos);
        }
    }

    void LanzarEsfera()
    {
        // Instanciamos la esfera en la posición y rotación del lanzador (a 45 grados)
        GameObject nuevaEsfera = Instantiate(prefabEsfera, transform.position, transform.rotation);

        Rigidbody rb = nuevaEsfera.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Aplicamos la fuerza hacia adelante respecto a la rotación del lanzador (45º)
            rb.AddForce(transform.forward * fuerzaLanzamiento, ForceMode.Impulse);
        }

        // Destruir la esfera después de un tiempo para no saturar la memoria
        Destroy(nuevaEsfera, intervaloEntreLanzamientos - 0.5f);
    }
}