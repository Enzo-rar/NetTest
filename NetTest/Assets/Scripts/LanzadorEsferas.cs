using FishNet.Object;
using UnityEngine;

public class LanzadorEsferas : NetworkBehaviour
{
    [Header("Configuración del Lanzamiento")]
    public GameObject prefabEsfera;
    public float fuerzaLanzamiento = 25f;
    public float intervaloEntreLanzamientos = 5f;

    private float temporizadorLanzamiento;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[Lanzador] Iniciando temporizador de lanzamientos...");
        temporizadorLanzamiento = intervaloEntreLanzamientos;
    }

    private void Update()
    {
        if (!base.IsServerInitialized) return;

        temporizadorLanzamiento -= Time.deltaTime;
        if (temporizadorLanzamiento <= 0f)
        {
            LanzarEsfera();
            temporizadorLanzamiento = intervaloEntreLanzamientos;
        }
    }

    void LanzarEsfera()
    {
        if (prefabEsfera == null) return;

        // Instanciamos el objeto en la escena de Unity
        GameObject nuevaEsfera = Instantiate(prefabEsfera, transform.position, transform.rotation);

        // ¡VITAL EN FISHNET! Registramos el objeto en la red
        base.ServerManager.Spawn(nuevaEsfera);

        Rigidbody rb = nuevaEsfera.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * fuerzaLanzamiento, ForceMode.Impulse);
        }
    }
}