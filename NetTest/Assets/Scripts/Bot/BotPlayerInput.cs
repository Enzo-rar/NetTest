using UnityEngine;

public class BotPlayerInput : MonoBehaviour, IPlayerInputProvider
{
    private PlayerInputData currentInput;
    private float timer = 0f;

    [Header("Ajustes del Cuadrado")]
    [Tooltip("Ajusta este valor en el inspector hasta que el bot gire exactamente 90 grados.")]
    public float fuerzaDeGiro = 80f;

    private void Update()
    {
        timer += Time.deltaTime;
        currentInput = new PlayerInputData();

        float cycle = timer % 6f;


        if (cycle < 1.5f)
        {
            currentInput.Move = new Vector2(0, 1);
            currentInput.Sprint = true;
        }


        else if (cycle >= 1.5f && cycle < 2.0f)
        {
            currentInput.Move = new Vector2(0, 1);
            currentInput.Crouch = true;
        }

        else if (cycle >= 2.0f && cycle <= 2.1f)
        {
            currentInput.Move = new Vector2(0, 1); 
            currentInput.Jump = true;
        }

       

        else if (cycle >= 5.0f && cycle <= 5.8f)
        {
            /
            currentInput.Look = new Vector2(fuerzaDeGiro, 0);
        }


    }

    public PlayerInputData GetInput()
    {
        return currentInput;
    }
}