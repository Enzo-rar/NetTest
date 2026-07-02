using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class InputRecorder : MonoBehaviour
{
    [Header("Configuración del Archivo")]
    [Tooltip("Nombre del archivo (sin extensión) con el que se guardará el registro.")]
    public string fileName = "registro_movimiento_1";

    [Header("Controles Manuales (Teclado)")]
    public KeyCode teclaIniciar = KeyCode.F5;
    public KeyCode teclaDetener = KeyCode.F6;

    private IPlayerInputProvider inputProvider;
    private bool isRecording = false;
    private List<RecordedInputEntry> recordedEntries = new List<RecordedInputEntry>();
    private float startTime;

    void Start()
    {
        // Buscamos el proveedor de inputs actual (ej: tu LocalPlayerInput)
        inputProvider = GetComponent<IPlayerInputProvider>();
        if (inputProvider == null)
        {
            inputProvider = GetComponentInParent<IPlayerInputProvider>();
        }

        if (inputProvider == null)
        {
            Debug.LogError("<color=red>[InputRecorder]</color> No se encontró ningún componente que implemente IPlayerInputProvider.");
        }
    }

    void Update()
    {
        // Control mediante teclado en el Editor
        if (Input.GetKeyDown(teclaIniciar)) StartRecording();
        if (Input.GetKeyDown(teclaDetener)) StopRecording();

        // Si estamos grabando, capturamos el input de este frame
        if (isRecording && inputProvider != null)
        {
            RecordedInputEntry entry = new RecordedInputEntry
            {
                timeStamp = Time.time - startTime,
                inputData = inputProvider.GetInput()
            };
            recordedEntries.Add(entry);
        }
    }

    public void StartRecording()
    {
        if (isRecording) return;

        recordedEntries.Clear();
        startTime = Time.time;
        isRecording = true;
        Debug.Log("<color=green>[InputRecorder]</color> ¡Grabación INICIADA! Juega libremente para registrar tus movimientos.");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        Debug.Log("<color=orange>[InputRecorder]</color> Grabación DETENIDA. Procesando guardado...");
        SaveToFile();
    }

    private void SaveToFile()
    {
        // Definimos la ruta dentro de Assets/Records
        string folderPath = Path.Combine(Application.dataPath, "Records");

        // Si la carpeta 'Records' no existe, la creamos automáticamente
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName + ".json");

        // Preparamos el contenedor y lo convertimos a texto estructurado (JSON)
        RecordedInputSession session = new RecordedInputSession { entries = recordedEntries };
        string json = JsonUtility.ToJson(session, true); // 'true' para que sea legible al abrirlo

        // Escribimos el archivo en el disco
        File.WriteAllText(filePath, json);
        Debug.Log($"<color=cyan>[InputRecorder]</color> Archivo guardado con éxito en: <b>{filePath}</b>");
    }
}   