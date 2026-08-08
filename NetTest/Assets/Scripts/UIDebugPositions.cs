using UnityEngine;
using TMPro;

public class UIDebugPositions : MonoBehaviour
{
    [Header("Arrastra aquí los 3 textos de tu Canvas")]
    public TMP_Text textoX;
    public TMP_Text textoY;
    public TMP_Text textoZ;

    // Referencia al cuerpo del jugador que queremos rastrear
    private Transform playerToTrack;

    void Update()
    {
        // Si aún no tenemos al jugador, lo buscamos por su Tag único
        if (playerToTrack == null)
        {
            // Buscamos específicamente al objeto etiquetado como "ClientPlayer"
            GameObject clientObj = GameObject.FindGameObjectWithTag("ClientPlayer");

            if (clientObj != null)
            {
                // Buscamos al hijo PlayerBody para mayor precisión
                playerToTrack = clientObj.transform.Find("PlayerBody");

                // Fallback por si acaso no tiene el hijo
                if (playerToTrack == null)
                {
                    playerToTrack = clientObj.transform;
                }
            }

            // Si sigue sin haber jugador (no ha spawneado), no hacemos nada este frame
            return;
        }

        // Si ya tenemos al jugador, actualizamos los textos
        textoX.text = $"X: {playerToTrack.position.x:F3}";
        textoY.text = $"Y: {playerToTrack.position.y:F3}";
        textoZ.text = $"Z: {playerToTrack.position.z:F3}";
    }
}