using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX, sensY;
    public Transform orientation;

    float xRotation, yRotation;

    private IPlayerInputProvider inputProvider;

    void Start()
    {

        if (CommandReader.Instance != null &&
       (CommandReader.Instance.currentMode == CommandReader.StartupMode.DedicatedServer ||
       (CommandReader.Instance.currentMode == CommandReader.StartupMode.Host && Application.isBatchMode)))
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null) cam.enabled = false;

            AudioListener audioLis = GetComponent<AudioListener>();
            if (audioLis != null) audioLis.enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputProvider = GetComponentInParent<IPlayerInputProvider>();

    }

    void Update()
    {
        if (inputProvider == null) return;

        PlayerInputData input = inputProvider.GetInput();
        Vector2 lookInput = input.Look;

        float mouseX = lookInput.x * Time.deltaTime * sensX;
        float mouseY = lookInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}