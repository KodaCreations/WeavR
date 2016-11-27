using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ShipNetworkController : NetworkBehaviour
{
    [SyncVar]
    public float steeringForce;

    [SyncVar]
    public float accelerationForce;

    [SyncVar]
    public float downwardForce;

    // Use this for initialization
    ShipController ship;
    void Start()
    {
        ship = GetComponent<ShipController>();
    }

    [Command]
    public void CmdSendInputHandler(float steeringInput, float accelerationInput, float downwardInput)
    {
        steeringForce = steeringInput;
        accelerationForce = accelerationInput;
        downwardForce = downwardInput;
        ship.steeringForce = steeringForce;
        ship.accelerationForce = accelerationForce;
        ship.downwardForce = downwardForce;
    }
    // Update is called once per frame
    void Update()
    {
        if (!ship)
            ship = GetComponent<ShipController>();
        else
        {
            ship.steeringForce = steeringForce;
            ship.accelerationForce = accelerationForce;
            ship.downwardForce = downwardForce;
        }
    }
}
