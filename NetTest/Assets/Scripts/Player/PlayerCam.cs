using UnityEngine;
using Fusion; // Necesario para comprobar la autoridad

public class PlayerCam : MonoBehaviour
{
    public float sensX, sensY;
    public Transform orientation;

    // Necesitamos saber si somos los dueños para mover la cámara
    public NetworkObject networkObject;

    float xRotation, yRotation;

    void Start()
    {
        sensX = PlayerPrefs.GetFloat("SensX", 20f);
        sensY = PlayerPrefs.GetFloat("SensY", 20f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Solo rotamos la cámara si la cápsula nos pertenece a nosotros
        if (networkObject == null || !networkObject.HasInputAuthority) return;

        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Multiplicador extra (ej. 10f) para que los valores 1-100 del slider tengan sentido
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * 10f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * 10f;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}