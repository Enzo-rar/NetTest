using FishNet.Object;
using UnityEngine;

public class AutoDestructor : NetworkBehaviour
{
    private float tiempoDeVida = 4.5f;

    private void Update()
    {
        if (!base.IsServerInitialized) return;

        tiempoDeVida -= Time.deltaTime;
        if (tiempoDeVida <= 0f)
        {
            // Despawn remueve el objeto de la red y lo destruye de forma segura
            base.ServerManager.Despawn(gameObject);
        }
    }
}