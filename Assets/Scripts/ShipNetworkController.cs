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
        if(isLocalPlayer)
        {
            GameObject camera = (GameObject)Instantiate(Resources.Load("Prefabs/Cameras/CameraPrefab"), null);

            camera.transform.rotation = ship.transform.rotation;
            camera.transform.position = ship.transform.position - ship.transform.forward * 8;
            camera.GetComponent<CamScript>().ship = ship.transform;
        }
    }

    [Command]
    public void CmdSendInputHandler(float steeringInput, float accelerationInput, float downwardInput)
    {
        steeringForce = steeringInput;
        accelerationForce = accelerationInput;
        downwardForce = downwardInput;
        ship.SteeringForce = steeringForce;
        ship.AccelerationForce = accelerationForce;
    }
    // Update is called once per frame
    void Update()
    {
        if (!ship)
            ship = GetComponent<ShipController>();
        else
        {
            if(isLocalPlayer)
            {
                CmdSendInputHandler(ship.SteeringForce, ship.AccelerationForce, ship.downwardSpeed);
            }
            else
            {
                ship.SteeringForce = steeringForce;
                ship.AccelerationForce = accelerationForce;
            }
        }
    }
}
