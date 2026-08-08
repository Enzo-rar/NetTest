using FishNet.Object;
using UnityEngine;

public class ClientFloorLinker : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Si este clon no es mi jugador local (es decir, es un jugador remoto), me detengo
        if (!base.IsOwner) return;

        GameObject objetoSuelo = GameObject.Find("SmartFloor"); // Pon el nombre exacto de tu objeto
        if (objetoSuelo != null)
        {
            SmartFloor suelo = objetoSuelo.GetComponent<SmartFloor>();
            suelo.targetPlayer = this.transform;
            Debug.Log("<color=green>[ClientFloorLinker]</color> ¡Suelo vinculado por Nombre!");
        }
    }
}