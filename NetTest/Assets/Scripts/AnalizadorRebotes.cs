using UnityEngine;

public class AnalizadorRebotes : MonoBehaviour
{
    private int contadorRebotes = 0;
    private const int MAX_REBOTES_MEDICION = 5;

    // Aquí almacenarás las referencias de red en tus ramas (ej. NetworkObject)
    // public bool isServer; // Ejemplo genérico

    private void OnCollisionEnter(Collision collision)
    {
        if (contadorRebotes >= MAX_REBOTES_MEDICION)
            return; // Solo nos interesan los primeros 5 rebotes

        contadorRebotes++;

        RegistrarDiscrepanciaEspacial(collision.contacts[0].point, collision.gameObject.name);
    }

    private void RegistrarDiscrepanciaEspacial(Vector3 puntoImpactoLocal, string objetoGolpeado)
    {
        // BASE PARA LA TOMA DE DATOS (A rellenar en cada rama de red)

        // 1. Obtener posición autoritativa (Servidor)
        // Vector3 posicionServidor = ObtenerPosicionAutoritativaDelServidor(); 

        // 2. Obtener posición representada (Cliente remoto)
        Vector3 posicionCliente = transform.position;

        // 3. Calcular la diferencia espacial
        // float discrepancia = Vector3.Distance(posicionServidor, posicionCliente);

        // LOG temporal para la rama base
        Debug.Log($"[Rebote {contadorRebotes}] Impacto en {objetoGolpeado}. " +
                  $"Posición Cliente: {posicionCliente}. " +
                  $"(Añadir lógica de red para calcular la divergencia).");
    }
}