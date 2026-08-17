using Unity.Netcode;
using UnityEngine;

public class AutoDestructor : NetworkBehaviour
{
    public float tiempoDeVida = 4.5f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(DestruirPorRed), tiempoDeVida);
        }
    }

    private void DestruirPorRed()
    {
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}