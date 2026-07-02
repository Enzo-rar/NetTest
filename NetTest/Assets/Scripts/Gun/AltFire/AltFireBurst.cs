using UnityEngine;

public class AltFireBurst : AltFireBehavior
{
    public override void ExecuteAltFire(Transform cameraPoint)
    {
        Debug.Log("<color=orange>¡Disparo Secundario!</color> Ráfaga de 3 balas disparada.");
        // Aquí iría tu lógica de instanciar 3 proyectiles o hacer 3 raycasts
    }
}