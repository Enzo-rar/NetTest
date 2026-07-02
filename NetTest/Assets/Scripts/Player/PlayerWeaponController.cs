using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Arma Actual")]
    public Weapon currentWeapon;

    // Necesitamos la cámara para saber hacia dónde apunta exactamente el centro de la pantalla
    public Transform cameraTransform;

    private IPlayerInputProvider inputProvider;

    void Start()
    {
        inputProvider = GetComponentInParent<IPlayerInputProvider>();
    }

    void Update()
    {
        if (inputProvider == null || currentWeapon == null) return;

        PlayerInputData input = inputProvider.GetInput();

        // Le pasamos la información al arma para que ella decida si disparar o no
        currentWeapon.HandleWeaponInputs(input, cameraTransform);
    }
}