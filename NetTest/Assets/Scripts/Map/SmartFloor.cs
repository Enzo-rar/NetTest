using UnityEngine;

public class SmartFloor : MonoBehaviour
{
    [Tooltip("El Transform del jugador (Cliente) al que este suelo debe seguir.")]
    public Transform targetPlayer;

    private void LateUpdate()
    {
        if (targetPlayer != null)
        {
            // El suelo sigue al jugador en X y Z, pero mantiene su propia altura (Y) original
            transform.position = new Vector3(
                targetPlayer.position.x,
                transform.position.y,
                targetPlayer.position.z
            );
        }
    }
}