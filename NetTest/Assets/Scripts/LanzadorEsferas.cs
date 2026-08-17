using Unity.Netcode;
using UnityEngine;

public class LanzadorEsferasNGO : NetworkBehaviour
{
    [Header("Configuración del Lanzamiento")]
    public GameObject prefabEsfera;
    public float fuerzaLanzamiento = 25f;
    public float intervaloEntreLanzamientos = 5f;

    private float timer = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("[Lanzador] Iniciando temporizador de lanzamientos en NGO...");
            timer = intervaloEntreLanzamientos;
        }
    }

    private void FixedUpdate()
    {
        // Solo el servidor tiene autoridad para spawnear
        if (!IsSpawned || !IsServer) return;

        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
        {
            LanzarEsfera();
            timer = intervaloEntreLanzamientos;
        }
    }

    void LanzarEsfera()
    {
        if (prefabEsfera == null) return;

        // Instanciamos el objeto localmente en el server
        GameObject nuevaEsfera = Instantiate(prefabEsfera, transform.position, transform.rotation);

        // Lo spawneamos por red
        NetworkObject netObj = nuevaEsfera.GetComponent<NetworkObject>();
        netObj.Spawn(true); // true = destroyWithScene

        Rigidbody rb = nuevaEsfera.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * fuerzaLanzamiento, ForceMode.Impulse);
        }
    }
}