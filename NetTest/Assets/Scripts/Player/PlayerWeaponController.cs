using UnityEngine;
using Fusion;

public class PlayerWeaponController : NetworkBehaviour
{
    [Header("Arma Actual")]
    public Weapon currentWeapon;
    public Transform cameraTransform;

    [Header("Interacción")]
    public float interactDistance = 3f;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            // 1. Detectar si pulsamos la tecla 'E'
            if (input.Interact)
            {
                // Disparamos un Raycast desde la cámara hacia adelante
                if (Runner.GetPhysicsScene().Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactDistance))
                {
                    // Comprobamos si el objeto impactado tiene la interfaz IInteractable
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact(this);
                    }
                }
            }

            // 2. Lógica del arma actual (si tenemos una)
            if (currentWeapon != null)
            {
                currentWeapon.HandleWeaponInputs(input, cameraTransform);
            }
        }
    }
}