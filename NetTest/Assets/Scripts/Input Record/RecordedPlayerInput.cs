using UnityEngine;

using System.IO;

public class RecordedPlayerInput : MonoBehaviour, IPlayerInputProvider
{
    [Header("Configuración del Experimento (24h)")]
    [Tooltip("Horas totales que durará el experimento.")]
    public float horasTotales = 24f;
    [Tooltip("Prefijo de los archivos. Ej: si es 'Movimiento_', buscará Movimiento_1, Movimiento_2...")]
    public string prefijoArchivos = "Movimiento_";
    [Tooltip("Cantidad total de archivos a dividir en el tiempo total.")]
    public int cantidadArchivos = 10;

    // Control de tiempos
    private float tiempoPorArchivo;
    private float tiempoInicioExperimento;
    private int indiceArchivoActual = -1;

    // Control de reproducción del archivo actual
    private RecordedInputSession session;
    private bool isPlaying = false;
    private float tiempoInicioBucle;
    private int currentEntryIndex = 0;
    private PlayerInputData currentInput;

    void Start()
    {
        // Calculamos cuántos segundos debe durar cada archivo (Ej: 24h / 10 = 8640 segundos)
        tiempoPorArchivo = (horasTotales * 3600f) / cantidadArchivos;
        tiempoInicioExperimento = Time.time;

        Debug.Log($"<color=cyan>[Experimento]</color> Iniciando prueba de {horasTotales}h. Cada archivo durará {tiempoPorArchivo} segundos.");

        ComprobarFaseDelExperimento();
    }

    void Update()
    {
        if (!isPlaying) return;

        // 1. Comprobamos si nos toca cambiar al siguiente archivo (o si ya terminaron las 24h)
        ComprobarFaseDelExperimento();

        if (session == null || session.entries.Count == 0) return;

        // 2. Reproducimos el archivo actual en bucle
        float tiempoBucleTranscurrido = Time.time - tiempoInicioBucle;

        // Buscamos el frame correspondiente al tiempo actual del bucle
        while (currentEntryIndex < session.entries.Count - 1 && tiempoBucleTranscurrido >= session.entries[currentEntryIndex + 1].timeStamp)
        {
            currentEntryIndex++;
        }

        currentInput = session.entries[currentEntryIndex].inputData;

        // 3. Si hemos llegado al final de este archivo ANTES de que pasen sus 2.4 horas, lo reiniciamos
        if (currentEntryIndex >= session.entries.Count - 1 && tiempoBucleTranscurrido >= session.entries[session.entries.Count - 1].timeStamp)
        {
            tiempoInicioBucle = Time.time;
            currentEntryIndex = 0;
        }
    }

    private void ComprobarFaseDelExperimento()
    {
        float tiempoTotalTranscurrido = Time.time - tiempoInicioExperimento;

        // Calculamos qué archivo debería estar sonando matemáticamente ahora mismo
        int indiceEsperado = Mathf.FloorToInt(tiempoTotalTranscurrido / tiempoPorArchivo);

        // Si el índice supera la cantidad de archivos, el experimento de 24h ha terminado
        if (indiceEsperado >= cantidadArchivos)
        {
            if (isPlaying)
            {
                Debug.Log("<color=red>[Experimento]</color> ¡Tiempo total alcanzado! Finalizando inputs.");
                isPlaying = false;
                currentInput = new PlayerInputData(); // Dejamos al personaje quieto

                // Opcional: Quita las barras dobles de abajo para que Unity/El servidor se cierre solo al acabar
                // Application.Quit(); 
            }
            return;
        }

        // Si el índice esperado es mayor que el actual, toca cargar el siguiente archivo
        if (indiceEsperado > indiceArchivoActual || indiceArchivoActual == -1)
        {
            indiceArchivoActual = indiceEsperado;

            // Sumamos 1 porque tus archivos empiezan en 1 (Movimiento_1) y no en 0
            CargarArchivoEspecifico(indiceArchivoActual + 1);
        }
    }

    private void CargarArchivoEspecifico(int numeroArchivo)
    {
        string fileName = $"{prefijoArchivos}{numeroArchivo}.json";
        string filePath = Path.Combine(Application.dataPath, "Records", fileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            session = JsonUtility.FromJson<RecordedInputSession>(json);

            if (session != null && session.entries.Count > 0)
            {
                Debug.Log($"<color=green>[Experimento]</color> Fase {numeroArchivo}/{cantidadArchivos}. Cargando: <b>{fileName}</b>");
                tiempoInicioBucle = Time.time;
                currentEntryIndex = 0;
                isPlaying = true;
            }
        }
        else
        {
            Debug.LogError($"[Experimento] ERROR FATAL: No se encontró <b>{fileName}</b>. Asegúrate de que existe en la carpeta Records.");
            session = null;
            currentInput = new PlayerInputData(); // Quieto por seguridad
        }
    }

    public PlayerInputData GetInput()
    {
        return currentInput;
    }
}