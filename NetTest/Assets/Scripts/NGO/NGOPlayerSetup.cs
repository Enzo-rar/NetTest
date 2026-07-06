using UnityEngine;
using Unity.Netcode;

public class NGOPlayerSetup : NetworkBehaviour
{
    private Rigidbody rb;
    private RecordedPlayerInput scriptDeInputs;
    private SmartFloor targetSuelo; // Guardamos la referencia para saber si ya lo hemos vinculado

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        scriptDeInputs = GetComponent<RecordedPlayerInput>();

        if (!IsOwner)
        {
            if (scriptDeInputs != null) scriptDeInputs.enabled = false;
        }

        if (IsServer)
        {
            if (rb != null) rb.isKinematic = false; 
        }
        else
        {
            if (rb != null) rb.isKinematic = true;  
        }
    }

}