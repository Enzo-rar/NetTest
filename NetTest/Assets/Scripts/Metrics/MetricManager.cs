using System.IO;
using System.Text;
using UnityEngine;

public class MetricManager : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        // Crea el archivo CSV en la carpeta persistente del proyecto
        filePath = Path.Combine(Application.persistentDataPath, "Resultados_Experimento_SkillIssue.csv");
        CrearArchivoCSV();
    }

    void CrearArchivoCSV()
    {
        // Si no existe, lo creamos y añadimos la cabecera
        if (!File.Exists(filePath))
        {
            StringBuilder sb = new StringBuilder();

            // --- COLUMNAS BASE (Condiciones de red y estructura - Apartado 5.1) ---
            sb.Append("Ejecucion,Biblioteca,CasoPrueba,MarcaTemporal,Bloque20Min,TickID,");
            sb.Append("RTT,Jitter,PaquetesEnviados,PaquetesRecibidos,PerdidaCalculada,BytesTx,BytesRx,");

            // --- COLUMNAS CASO DE PRUEBA 2 (Resolución de Impactos - Apartado 5.3) ---
            // t_d y t_r
            sb.Append("t_d,t_r,");
            // Tres posiciones
            sb.Append("Pdisparo_X,Pdisparo_Y,Pdisparo_Z,");
            sb.Append("Precepcion_X,Precepcion_Y,Precepcion_Z,");
            sb.Append("Pvalidacion_X,Pvalidacion_Y,Pvalidacion_Z,");
            // Ambas distancias
            sb.Append("Dtransito,Evalidacion,");
            // Resultados de impacto
            sb.Append("ImpactoReferencia,ImpactoAutoritativo");

            File.WriteAllText(filePath, sb.ToString() + "\n");
            Debug.Log("Archivo CSV creado en: " + filePath);
        }
    }

    // Método que llamaremos desde las librerías de red para ir añadiendo filas
    public void RegistrarFilaCSV(string datosFila)
    {
        File.AppendAllText(filePath, datosFila + "\n");
    }
}