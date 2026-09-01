using Fusion;

// Cualquier objeto que queramos recoger con la 'E' llevará esta interfaz
public interface IInteractable
{
    // Función que ejecutará el servidor cuando el jugador pulse la E mirándolo
    void Interact(PlayerWeaponController player);
}