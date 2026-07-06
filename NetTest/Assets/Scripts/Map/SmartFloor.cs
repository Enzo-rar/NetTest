using UnityEngine;
using Unity.Netcode;

public class SmartFloor : MonoBehaviour
{
    [Tooltip("El Transform del jugador al que este suelo debe seguir.")]
    public Transform targetPlayer;

    private void LateUpdate()
    {
        if (targetPlayer == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    // MODO HOST: El Servidor busca al Cliente conectado para ponerle el suelo debajo
                    // y evitar que la gravedad del servidor lo tire al vacío.
                    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                    {
                        if (client.ClientId != NetworkManager.Singleton.LocalClientId) // Si NO es el propio Host
                        {
                            targetPlayer = client.PlayerObject.transform;
                            Debug.Log("<color=green>[SmartFloor]</color> Host: Suelo vinculado al corredor Cliente.");
                            break; // Ya lo hemos encontrado
                        }
                    }
                }
                else
                {
                    // MODO CLIENTE: El Cliente se busca a sí mismo (como ya teníamos)
                    var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    if (localPlayer != null)
                    {
                        targetPlayer = localPlayer.transform;
                        Debug.Log("<color=green>[SmartFloor]</color> Cliente: Suelo vinculado a mí mismo.");
                    }
                }
            }
        }

        // Seguimos al objetivo en X y Z
        if (targetPlayer != null)
        {
            transform.position = new Vector3(
                targetPlayer.position.x,
                transform.position.y,
                targetPlayer.position.z
            );
        }
    }
}