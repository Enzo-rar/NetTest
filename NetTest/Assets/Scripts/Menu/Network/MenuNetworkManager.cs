using UnityEngine;
using Fusion;
using TMPro;
using System.Threading.Tasks;

public class MenuNetworkManager : MonoBehaviour
{
    public NetworkRunner runner;
    public TMP_InputField nameInputField;
    public TMP_InputField codeInputField;

    public async void CreateSession()
    {
        // Aseguramos que haya un NetworkRunner en el objeto
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        // Guardamos el nombre localmente para usarlo luego en la Lobby
        string playerName = string.IsNullOrEmpty(nameInputField.text) ? "Jugador" : nameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);

        string roomCode = GenerateRoomCode();

        // Inicializamos la partida como Host (Servidor Anfitrión)
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomCode,
            Scene = SceneRef.FromIndex(1),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log("Partida creada. Código para compartir: " + roomCode);
    }

    public async void JoinSession()
    {
        string inputCode = codeInputField.text.ToUpper().Trim();

        if (string.IsNullOrEmpty(inputCode) || inputCode.Length != 4)
        {
            Debug.LogWarning("El código debe tener exactamente 4 letras.");
            return;
        }

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        string playerName = string.IsNullOrEmpty(nameInputField.text) ? "Jugador" : nameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);

        // Inicializamos la partida como Cliente
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = inputCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] code = new char[4];

        for (int i = 0; i < 4; i++)
        {
            code[i] = chars[Random.Range(0, chars.Length)];
        }

        return new string(code);
    }
}