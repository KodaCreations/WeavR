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

    [SyncVar]
    public int modelID;
    [SyncVar]
    public float energyEfficiency; //How effective the ship is at converting energy into boost or shield 1 for 100%, higher value for less efficiency.
    [SyncVar]
    public float overheatAfter; //How long time of active boost it will take for the ship to overheat
    [SyncVar]
    public float overheatLockTime; //How long it will take before the overheat timer starts going down if overheated
    [SyncVar]
    public float maxEnergy;
    [SyncVar]
    public bool shielded;
    [SyncVar]
    public float maxspeedBoost; //How much the maxspeed should be increased by when using turbo. 1 is normal speed 2 is twice as fast.
    [SyncVar]
    public float speedBoost;
    [SyncVar]
    public float rechargePerSecond;
    [SyncVar]
    public float heatReductionPerSecond;
    [SyncVar]
    public float forwardAccelerationSpeed;
    [SyncVar]
    public float noAccelerationDrag;
    [SyncVar]
    public float maxForwardAccelerationSpeed;
    [SyncVar]
    public float maxBackwardAccelerationSpeed;
    [SyncVar]
    public float brakingAcceleration;
    [SyncVar]
    public float downwardSpeed;
    [SyncVar]
    public float rotationSpeed;
    [SyncVar]
    public float hoverHeight;
    [SyncVar]
    public float gravity;
    [SyncVar]
    public float fallOffset;
    [SyncVar]
    public float shipBankReturnSpeed;
    [SyncVar]
    public float shipSpeedBank;
    [SyncVar]
    public float shipMaxBank;
    [SyncVar]
    public string engineAudioName;
    [SyncVar]
    public string boostAudioName;


    // Use this for initialization
    ShipController ship;
    void Start()
    {
        ship = GetComponent<ShipController>();
        Brain brain = GameObject.Find("Brain").GetComponent<Brain>();
        ShipController shipPrefab = brain.availableShips[modelID].GetComponent<ShipController>();
        GameObject model = (GameObject)Instantiate(brain.availableShipsMeshes[modelID], transform);
        model.name = "Model";
        model.transform.localRotation = Quaternion.identity;
        if(isLocalPlayer)
        {
            GameObject camera = (GameObject)Instantiate(Resources.Load("Prefabs/Cameras/CameraPrefab"), null);

            camera.transform.rotation = ship.transform.rotation;
            camera.transform.position = ship.transform.position - ship.transform.forward * 8;
            camera.GetComponent<CamScript>().ship = ship.transform;
            camera.GetComponent<CamScript>().StartNetworkIntro();
        }
        ship.energyEfficiency = energyEfficiency;
        ship.overheatAfter = overheatAfter;
        ship.overheatLockTime = overheatLockTime;
        ship.maxEnergy = maxEnergy;
        ship.maxspeedBoost = maxspeedBoost;
        ship.speedBoost = speedBoost;
        ship.rechargePerSecond = rechargePerSecond;
        ship.heatReductionPerSecond = heatReductionPerSecond;

        ship.forwardAccelerationSpeed = forwardAccelerationSpeed;
        ship.noAccelerationDrag = noAccelerationDrag;
        ship.maxForwardAccelerationSpeed = maxForwardAccelerationSpeed;
        ship.maxBackwardAccelerationSpeed = maxBackwardAccelerationSpeed;
        ship.brakingAcceleration = brakingAcceleration;
        ship.downwardSpeed = downwardSpeed;
        ship.rotationSpeed = rotationSpeed;
        ship.hoverHeight = hoverHeight;
        ship.gravity = gravity;
        ship.fallOffset = fallOffset;
        ship.shipBankReturnSpeed = shipBankReturnSpeed;
        ship.shipSpeedBank = shipSpeedBank;
        ship.shipMaxBank = shipMaxBank;
        ship.engineAudioName = engineAudioName;
        ship.boostAudioName = boostAudioName;
        //model.transform.SetParent(transform);

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
