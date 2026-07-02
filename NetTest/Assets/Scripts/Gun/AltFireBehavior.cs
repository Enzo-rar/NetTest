using UnityEngine;


public abstract class AltFireBehavior : MonoBehaviour
{
    // Referencia al arma principal para gastar munición extra si fuera necesario
    protected Weapon weapon;

    public virtual void Initialize(Weapon mainWeapon)
    {
        weapon = mainWeapon;
    }

    // El método que se llamará al hacer click derecho
    public abstract void ExecuteAltFire(Transform cameraPoint);
}