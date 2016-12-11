﻿using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour {

    //Misc
    [Header("Misc")]
    public string shipName;
    public bool activate;
    public int id;


    //Ship energy variables
    [Header("Energy System")]
    public float energyEfficiency; //How effective the ship is at converting energy into boost or shield 1 for 100%, higher value for less efficiency.
    //public float shieldEfficiency; //How much energy the shield uses compared to the turbo 1 for equaly effective, higher value for less efficiency.
    public float overheatAfter; //How long time of active boost it will take for the ship to overheat
    public float overheatLockTime; //How long it will take before the overheat timer starts going down if overheated
    public float maxEnergy;
    private float energy; //For 100% energyEfficiency 1 energy = 1 second of turbo
    private float newEnergy;
    private float currentHeat;
    private float heatTimer;
    public bool shielded;
    private bool turbo;
    private bool overheated;
    public float maxspeedBoost; //How much the maxspeed should be increased by when using turbo. 1 is normal speed 2 is twice as fast.
    public float speedBoost;
    public float rechargePerSecond;
    public float heatReductionPerSecond;

    [Header("Acceleration Values")]
    [Tooltip("Foward movement of the ship")]
    public float forwardAccelerationSpeed;
    [HideInInspector]
    public float currentFowardAccelerationSpeed;
    [Tooltip("The falloff acceleration if there is no accelerationForce")]
    public float noAccelerationDrag;
    public float maxForwardAccelerationSpeed;
    public float flightMinimumAccelerationSpeed;
    public float brakingAcceleration = 5;
    
    //Ship Flight downwardSpeed
    public float downwardSpeed;

    //Ship Rotation Speed
    [Tooltip("Ship Rotation Speed. Best Values from 1-3")]
    public float rotationSpeed;

    //Ship hover height over ground
    [Tooltip("Hight of the ship over the ground. Best Values from 0.5-1")]
    public float hoverHeight;

    //Weapon and debuff variables
    public Debuff debuff;
    public Weapon.WeaponType? weapon;
    private Transform target;
    private bool drain;
    [Header("Gravity")]
    [Tooltip("IT'S GRAVITY FOR FUCK SAKE!")]
    //Ship gravity when not on the ground an not in flightmode
    public float gravity;

    //Variables for when hit by EMP
    private float fallVelocity;
    private float fallAccelration;
    public float fallOffset;
    private float modelOffset;

    //Ship Wanted Position and Rotation depending on the conditions
    private Quaternion wantedTrackRot;
    private Quaternion wantedTrackPitchRot;
    private Vector3 wantedTrackPos;

    //The Values that determin where the ship shall move that are changed in the Input or AI
    private float steeringForce; //Left or Right
    private float accelerationForce; //Forward or Backwards
    private float downwardForce; //Down or Up in FlightMode

    //Forces to help with Calculations
    private float normalForce;
    private float normalPitchForce;

    //Values that help with the model rotation so thtat it looks smoother
    private float shipCurrentBank;
    private float shipReturnBankSpeed;
    private float shipCurrentPitchBank;
    private float shipBankPitchSpeed;
    private float shipReturnBankPitchSpeed;
    private float shipBankVelocity;
    private float shipPitchBankVelocity;
    private float shipBankSpeed;
    [Header("Ship Yaw Handling")]
    public float shipBankReturnSpeed;
    public float shipSpeedBank;
    public float shipMaxBank;
    private float rollState;

    //Hover behavior var
    private float shipHoverAmount;
    private float shipHoverSpeed;
    private float shipHoverTime;
    private float shipCurrentHover;

    // Wobble behavior var NOTE: you have to set these values when you want to make the ship wobble
    private float shipWobbleAmount;
    private float shipWobbleSpeed;
    private float shipWobbleTime;
    private float shipCurrentWobble;
    //This is an example
    //shipHoverTime = 0;
    //shipWobbleAmount = 2f;
    //shipWobbleSpeed = 6;
    //shipWobbleTime = 0;

    //Helpfull stuff
    private Rigidbody rb;
    private GameObject model;
    private bool shipIsColliding;
    private bool flightMode;
    private bool grounded;
    // Use this for initialization
    void Start()
    {
        flightMode = false;
        shipHoverSpeed = Mathf.PI / 2;

        rb = GetComponent<Rigidbody>();
        model = transform.FindChild("Model").gameObject;
        debuff = null;
        weapon = null;
        drain = false;
        //shielded = false;
        turbo = false;
        overheated = false;
        energy = maxEnergy;
        newEnergy = 0;
        currentHeat = 0;
    }

    /// <summary>
    /// Fires the weapon the ship is currently carrying, if any.
    /// </summary>
    public void FireWeapon()
    {
        if (weapon == null)
            return;
        switch (weapon)
        {
            case Weapon.WeaponType.Missile:
                if (target)
                {
                    GameObject missile = Instantiate(Resources.Load("Prefabs/Missile"), transform.position + transform.forward * 10, transform.rotation) as GameObject;
                    missile.GetComponent<GuidedWeapon>().target = target;
                    target = null;
                    weapon = null;
                }
                return;
            case Weapon.WeaponType.Mine:
                Instantiate(Resources.Load("Prefabs/Mine"), transform.position - transform.forward * 10, transform.rotation);
                break;
            case Weapon.WeaponType.EMP:
                Instantiate(Resources.Load("Prefabs/EMP"), transform.position, transform.rotation);
                break;
            case Weapon.WeaponType.EnergyDrain:
                Instantiate(Resources.Load("Prefabs/EnergyDrain"), transform.position + transform.forward * 10, transform.rotation);
                break;
            case Weapon.WeaponType.DecreasedVision:
                if (target)
                {
                    GameObject decreasedVision = Instantiate(Resources.Load("Prefabs/DecreasedVision"), transform.position + transform.forward * 10, transform.rotation) as GameObject;
                    decreasedVision.GetComponent<GuidedWeapon>().target = target;
                    target = null;
                    weapon = null;
                }
                return;
        }
        if (target == null)
            weapon = null;
    }

    /// <summary>
    /// Changes the target of the ship to the nearest ship, will have to be changed to target the ship in front and allow for aiming for the ship in front of that and triggers.
    /// </summary>
    public void Target()
    {
        ShipController[] enemies = FindObjectsOfType<ShipController>();
        Transform sMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        
        foreach (ShipController s in enemies)
        {
            if (s != this)
            {
                float dist = Vector3.Distance(s.transform.position, currentPos);

                if (dist < minDist)
                {
                    sMin = s.transform;
                    minDist = dist;
                }
            }
        }

        target = sMin;
    }

    /// <summary>
    /// Drains the ship of energy over a period of time if the drain variable is true.
    /// </summary>
    private void DrainEnergy()
    {
        if (drain)
        {
            if (energy > 0)
            {
                energy -= Time.deltaTime * 10;
            }
            else
            {
                energy = 0;
                drain = false;
            }
        }
    }

    /// <summary>
    /// Gives more energy to the ship for recharging
    /// </summary>
    /// <param name="energyIncrease">The amount of energy given to the ship</param>
    public void Recharge(float energyIncrease)
    {
        newEnergy += energyIncrease;
    }

    /// <summary>
    /// Recharges the energy of the ship overtime provided there is energy to recharge with
    /// </summary>
    private void Recharge()
    {
        if (newEnergy > 0)
        {
            float recharge = rechargePerSecond * Time.deltaTime;

            if (recharge > newEnergy)
                recharge = newEnergy;
            newEnergy -= recharge;
            energy += recharge;
            if (energy > maxEnergy)
            {
                energy = maxEnergy;
                newEnergy = 0;
            }
        }
    }

    void HoverHandler()
    {
        grounded = false;
        Ray ray = new Ray(transform.position, -transform.up);
        Debug.DrawRay(transform.position, -transform.up, Color.black, 2.0f);

        // Raycast downwards to find the road and sets a Wanted Rot and Pos that the ship banks towards
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            grounded = true;
            float correctionPosDelay = 15f;
            wantedTrackPos = hit.point + hit.normal * hoverHeight;
            float distance = Vector3.Distance(wantedTrackPos, transform.position);
            float distanceToGround = Vector3.Distance(wantedTrackPos, hit.point);
            float pullStrength = 1 - (distanceToGround - distance) / distanceToGround;
            if(!flightMode)
            {
                transform.position = Vector3.Slerp(transform.position, wantedTrackPos, Time.deltaTime * correctionPosDelay + pullStrength);
            }
            else
            {
                float shipDistanceToGround = Vector3.Distance(hit.point, transform.position);
                if(shipDistanceToGround < distanceToGround)
                {
                    transform.position = Vector3.Slerp(transform.position, wantedTrackPos, Time.deltaTime * correctionPosDelay + pullStrength);
                }
            }

            float correctionRotDelay = 15;
            wantedTrackRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, hit.normal), hit.normal), Time.deltaTime * correctionRotDelay);
            transform.rotation = Quaternion.Slerp(transform.rotation, wantedTrackRot, Time.deltaTime * correctionRotDelay);

            Debug.DrawRay(hit.point, hit.normal, Color.red, 2.0f);
        }
    }
    void HoverBehavior()
    {
        // Basic hover what uses two Sinus curves to determin the height
        shipHoverTime += Time.deltaTime;
        shipHoverTime = shipHoverTime % (Mathf.PI * 2.0f);
        shipHoverSpeed += Time.deltaTime / 2;
        shipHoverSpeed = shipHoverSpeed % (Mathf.PI * 2.0f);
        shipHoverAmount -= Time.deltaTime / 4;
        shipHoverAmount = shipHoverAmount % 3;

        shipCurrentHover = Mathf.Sin(shipHoverTime) + Mathf.Sin(shipHoverSpeed);
        model.transform.localPosition = new Vector3(0, shipCurrentHover * 0.03f, 0);
    }
    void SteeringGroundBehavior()
    {
        // Wobble, has to be initialized with setting Wobble parameters
        shipWobbleTime += Time.deltaTime;
        shipWobbleSpeed -= Time.deltaTime / 2;
        shipWobbleSpeed = Mathf.Clamp(shipWobbleSpeed, 0, 10);
        shipWobbleAmount -= Time.deltaTime / 2;
        shipWobbleAmount = Mathf.Clamp(shipWobbleAmount, 0, 10);

        shipCurrentWobble = Mathf.Sin(shipWobbleTime * shipWobbleSpeed) * shipWobbleAmount;

        // Change Banking speed depending on current bank
        #region Banking
        if (steeringForce > 0)
        {
            shipReturnBankSpeed = 0;
            if (shipCurrentBank > 0 || shipCurrentBank == 0)
            {
                if (shipCurrentBank > 15)
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 10, Time.fixedDeltaTime * shipSpeedBank);
                }
                else
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 7, Time.fixedDeltaTime * shipSpeedBank);
                }
            }
            else
            {
                shipBankSpeed = Mathf.Lerp(shipBankSpeed, 2f, Time.fixedDeltaTime * shipSpeedBank);
            }
            shipBankVelocity = Mathf.Lerp(shipBankVelocity, shipBankSpeed, Time.deltaTime * 50);
        }

        if (steeringForce < 0)
        {
            shipReturnBankSpeed = 0;
            if (shipCurrentBank < 0 || shipCurrentBank == 0)
            {

                if (shipCurrentBank < -15)
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 10, Time.fixedDeltaTime * shipSpeedBank);
                }
                else
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 7, Time.fixedDeltaTime * shipSpeedBank);
                }
            }
            else
            {
                shipBankSpeed = Mathf.Lerp(shipBankSpeed, 2f, Time.fixedDeltaTime * shipSpeedBank);
            }
            shipBankVelocity = Mathf.Lerp(shipBankVelocity, shipBankSpeed, Time.deltaTime * 50);
        }

        if (steeringForce == 0)
        {
            shipBankSpeed = 0;
            if (shipCurrentBank < 0)
            {
                if (shipCurrentBank < -15)
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 10f, Time.fixedDeltaTime * shipBankReturnSpeed);
                }
                else
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 7f, Time.fixedDeltaTime * shipBankReturnSpeed * 1.5f);
                }
            }
            else
            {
                if (shipCurrentBank > 15)
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 10f, Time.fixedDeltaTime * shipBankReturnSpeed);
                }
                else
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 7f, Time.fixedDeltaTime * shipBankReturnSpeed * 1.5f);
                }
            }
            shipBankVelocity = Mathf.Lerp(shipBankVelocity, shipReturnBankSpeed, Time.deltaTime * 30);
            shipCurrentBank = Mathf.Lerp(shipCurrentBank, 0, Time.fixedDeltaTime * shipBankVelocity);
        }
        else
        {
            shipCurrentBank = Mathf.Lerp(shipCurrentBank, Mathf.Clamp(steeringForce * (shipMaxBank), -shipMaxBank, shipMaxBank), Time.deltaTime * shipBankVelocity);
        }
        #endregion

        //Quaternion wantedTrackPitchRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(model.transform.localEulerAngles.x, model.transform.localEulerAngles.y, -shipCurrentBank + rollState + shipCurrentWobble), Time.deltaTime * 15);
        //model.transform.localRotation = Quaternion.Slerp(transform.rotation, wantedTrackPitchRot, Time.deltaTime * 15);
        model.transform.localRotation = Quaternion.Euler(0, model.transform.localEulerAngles.y, -shipCurrentBank + rollState + shipCurrentWobble);

        normalForce = Mathf.Lerp(normalForce, steeringForce, Time.deltaTime * 3);
        transform.Rotate(Vector3.up * normalForce);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Ship")
            shipIsColliding = true;   
    }


    //void OnCollisionStay(Collision other)
    //{
    //    // Collision
    //    if (other.gameObject.tag == "Wall")
    //    {
    //        float contactNum = other.contacts.Length;
    //        contactNum = Mathf.Clamp(contactNum, 0, 1);

    //        for (int i = 0; i < contactNum; i++)
    //        {
    //            rb.angularVelocity = new Vector3(0, 0, 0);
    //            Vector3 collisionSpeed = rb.GetPointVelocity(other.contacts[i].point);
    //            Vector3 collisionNormal = transform.InverseTransformDirection((transform.position - other.contacts[i].point).normalized);

    //            if (shipAccel > shipAccelCap / 2)
    //            {
    //                shipAccel -= 1 / 100;
    //            }

    //            if (shipThrust > (shipAccelCap * (rb.drag * 2)))
    //            {
    //                shipThrust -= Mathf.Abs(collisionSpeed.x + collisionSpeed.z) / (Time.deltaTime * 200);
    //            }

    //            if (collisionNormal.x < 0.3f)
    //            {
    //                float tempVelZ = transform.InverseTransformDirection(rb.velocity).z;
    //                //rigidbody.velocity = new Vector3(0,0,0);

    //                //shipAccel = 0;
    //                //shipThrust = 0;
    //                //shipBoostTimer = 0;
    //                //shipBoostAmount = 0;

    //                //rigidbody.AddRelativeForce(new Vector3(0,0, collisionSpeed.z / 10), ForceMode.Impulse);
    //            }

    //            if (collisionNormal.x < 0.5f || collisionNormal.x > -0.5f)
    //            {
    //                if (collisionNormal.x < 0.1f || collisionNormal.x > -0.1f)
    //                {
    //                    transform.Rotate(Vector3.up * collisionNormal.x * (transform.InverseTransformDirection(rb.velocity).z) / 230);
    //                }
    //                else
    //                {
    //                    transform.Rotate(Vector3.up * collisionNormal.x * (transform.InverseTransformDirection(rb.velocity).z) / 250);
    //                }

    //            }

    //            if (collisionNormal.x < 0.1f && collisionNormal.x > -0.1f)
    //            {
    //                float tempvel = collisionSpeed.z * 20;
    //                if (tempvel < 0)
    //                {
    //                    tempvel = -tempvel;
    //                }
    //                tempvel = Mathf.Clamp(tempvel, 0, 100);

    //                //shipBoostTimer = 0;
    //                //shipThrust = 0;
    //                //shipThrust = 0;
    //                rb.velocity = new Vector3(0, 0, 0);

    //                if (tempvel > 15)
    //                {
    //                    rb.AddRelativeForce(new Vector3(0, 0, -tempvel), ForceMode.Impulse);
    //                }
    //            }

    //        }
    //    }
    //}

    void OnCollisionExit()
    {
        shipIsColliding = false;
    }
    void AccelerationGroundBehavior()
    {
        //Calculate new CurrentAccelerationForwardSpeed and clamp if out of range
        if(accelerationForce > 0 || accelerationForce < 0)
        {
            currentFowardAccelerationSpeed += accelerationForce * forwardAccelerationSpeed * Time.deltaTime;
            if (currentFowardAccelerationSpeed > maxForwardAccelerationSpeed)
                currentFowardAccelerationSpeed = maxForwardAccelerationSpeed;
        }
        else
        {
            if (currentFowardAccelerationSpeed > 0 + 150)
                currentFowardAccelerationSpeed -= noAccelerationDrag * Time.deltaTime;
            else if (currentFowardAccelerationSpeed < 0 - 150)
                currentFowardAccelerationSpeed += noAccelerationDrag * Time.deltaTime;
            else
                currentFowardAccelerationSpeed = 0;
        }

        //Add The force to the Ship
        rb.AddForce(transform.forward * currentFowardAccelerationSpeed * Time.deltaTime);
    }
    void AccelerationFlightBehavior()
    {
        //Calculate new CurrentAccelerationForwardSpeed and clamp if out of range
        if (accelerationForce > 0 || accelerationForce < 0)
        {
            currentFowardAccelerationSpeed += accelerationForce * forwardAccelerationSpeed * Time.deltaTime;
            if (currentFowardAccelerationSpeed > maxForwardAccelerationSpeed)
                currentFowardAccelerationSpeed = maxForwardAccelerationSpeed;
        }
        else
        {
            if (currentFowardAccelerationSpeed > flightMinimumAccelerationSpeed)
                currentFowardAccelerationSpeed -= noAccelerationDrag * Time.deltaTime;
            else
                currentFowardAccelerationSpeed = flightMinimumAccelerationSpeed;
        }

        //Add The force to the Ship
        rb.AddForce(transform.forward * currentFowardAccelerationSpeed * Time.deltaTime);
    }
    void FlightHandler()
    {
        float correctionRotDelay = 15;
        wantedTrackRot = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantedTrackRot, Time.deltaTime * correctionRotDelay);
    }
    void SteeringFlightRollBehavior()
    {
        // Wobble, has to be initialized with setting Wobble parameters
        shipWobbleTime += Time.deltaTime;
        shipWobbleSpeed -= Time.deltaTime / 2;
        shipWobbleSpeed = Mathf.Clamp(shipWobbleSpeed, 0, 10);
        shipWobbleAmount -= Time.deltaTime / 2;
        shipWobbleAmount = Mathf.Clamp(shipWobbleAmount, 0, 10);

        shipCurrentWobble = Mathf.Sin(shipWobbleTime * shipWobbleSpeed) * shipWobbleAmount;

        // Change Banking speed depending on current bank
        #region Banking
        if (steeringForce > 0)
        {
            shipReturnBankSpeed = 0;
            if (shipCurrentBank > 0 || shipCurrentBank == 0)
            {
                if (shipCurrentBank > 15)
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 5, Time.fixedDeltaTime * 5);
                }
                else
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 3, Time.fixedDeltaTime * 5);
                }
            }
            else
            {
                shipBankSpeed = Mathf.Lerp(shipBankSpeed, 2f, Time.fixedDeltaTime * 5);
            }
            shipBankVelocity = Mathf.Lerp(shipBankVelocity, shipBankSpeed, Time.deltaTime * 50);
        }

        if (steeringForce < 0)
        {
            shipReturnBankSpeed = 0;
            if (shipCurrentBank < 0 || shipCurrentBank == 0)
            {

                if (shipCurrentBank < -15)
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 5, Time.fixedDeltaTime * 5);
                }
                else
                {
                    shipBankSpeed = Mathf.Lerp(shipBankSpeed, 3, Time.fixedDeltaTime * 5);
                }
            }
            else
            {
                shipBankSpeed = Mathf.Lerp(shipBankSpeed, 2f, Time.fixedDeltaTime * 5);
            }
            shipBankVelocity = Mathf.Lerp(shipBankVelocity, shipBankSpeed, Time.deltaTime * 50);
        }

        if (steeringForce == 0)
        {
            shipBankSpeed = 0;
            if (shipCurrentBank < 0)
            {
                if (shipCurrentBank < -15)
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 10f, Time.fixedDeltaTime * 2);
                }
                else
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 7f, Time.fixedDeltaTime * 3);
                }
            }
            else
            {
                if (shipCurrentBank > 15)
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 10f, Time.fixedDeltaTime * 2);
                }
                else
                {
                    shipReturnBankSpeed = Mathf.Lerp(shipReturnBankSpeed, 7f, Time.fixedDeltaTime * 3);
                }
            }
            shipBankVelocity = Mathf.Lerp(shipBankVelocity, shipReturnBankSpeed, Time.deltaTime * 30);
            shipCurrentBank = Mathf.Lerp(shipCurrentBank, 0, Time.fixedDeltaTime * shipBankVelocity);
        }
        else
        {
            shipCurrentBank = Mathf.Lerp(shipCurrentBank, steeringForce * (shipMaxBank), Time.deltaTime * shipBankVelocity);
        }
        #endregion

        model.transform.localRotation = Quaternion.Euler(model.transform.localEulerAngles.x, model.transform.localEulerAngles.y, -shipCurrentBank + rollState + shipCurrentWobble);

        normalForce = Mathf.Lerp(normalForce, steeringForce, Time.deltaTime * 3);
        transform.Rotate(Vector3.up * normalForce);
    }
    void SteeringFlightPitchBehavior()
    {
        // Wobble, has to be initialized with setting Wobble parameters
        shipWobbleTime += Time.deltaTime;
        shipWobbleSpeed -= Time.deltaTime / 2;
        shipWobbleSpeed = Mathf.Clamp(shipWobbleSpeed, 0, 10);
        shipWobbleAmount -= Time.deltaTime / 2;
        shipWobbleAmount = Mathf.Clamp(shipWobbleAmount, 0, 10);

        shipCurrentWobble = Mathf.Sin(shipWobbleTime * shipWobbleSpeed) * shipWobbleAmount;

        // Change Banking speed depending on current bank
        #region Banking
        if (downwardForce > 0)
        {
            shipReturnBankPitchSpeed = 0;
            if (shipCurrentPitchBank > 0 || shipCurrentPitchBank == 0)
            {
                if (shipCurrentPitchBank > 15)
                {
                    shipBankPitchSpeed = Mathf.Lerp(shipBankPitchSpeed, 5, Time.fixedDeltaTime * 5);
                }
                else
                {
                    shipBankPitchSpeed = Mathf.Lerp(shipBankPitchSpeed, 3, Time.fixedDeltaTime * 5);
                }
            }
            else
            {
                shipBankPitchSpeed = Mathf.Lerp(shipBankPitchSpeed, 2f, Time.fixedDeltaTime * 5);
            }
            shipPitchBankVelocity = Mathf.Lerp(shipPitchBankVelocity, shipBankPitchSpeed, Time.deltaTime * 50);
        }

        if (downwardForce < 0)
        {
            shipReturnBankPitchSpeed = 0;
            if (shipCurrentPitchBank < 0 || shipCurrentPitchBank == 0)
            {

                if (shipCurrentPitchBank < -15)
                {
                    shipBankPitchSpeed = Mathf.Lerp(shipBankPitchSpeed, 5, Time.fixedDeltaTime * 5);
                }
                else
                {
                    shipBankPitchSpeed = Mathf.Lerp(shipBankPitchSpeed, 3, Time.fixedDeltaTime * 5);
                }
            }
            else
            {
                shipBankPitchSpeed = Mathf.Lerp(shipBankPitchSpeed, 2f, Time.fixedDeltaTime * 5);
            }
            shipPitchBankVelocity = Mathf.Lerp(shipPitchBankVelocity, shipBankPitchSpeed, Time.deltaTime * 50);
        }

        if (downwardForce == 0)
        {
            shipBankPitchSpeed = 0;
            if (shipCurrentPitchBank < 0)
            {
                if (shipCurrentPitchBank < -15)
                {
                    shipReturnBankPitchSpeed = Mathf.Lerp(shipReturnBankPitchSpeed, 10f, Time.fixedDeltaTime * 2);
                }
                else
                {
                    shipReturnBankPitchSpeed = Mathf.Lerp(shipReturnBankPitchSpeed, 7f, Time.fixedDeltaTime * 3);
                }
            }
            else
            {
                if (shipCurrentBank > 15)
                {
                    shipReturnBankPitchSpeed = Mathf.Lerp(shipReturnBankPitchSpeed, 10f, Time.fixedDeltaTime * 2);
                }
                else
                {
                    shipReturnBankPitchSpeed = Mathf.Lerp(shipReturnBankPitchSpeed, 7f, Time.fixedDeltaTime * 3);
                }
            }
            shipPitchBankVelocity = Mathf.Lerp(shipPitchBankVelocity, shipReturnBankPitchSpeed, Time.deltaTime * 30);
            shipCurrentPitchBank = Mathf.Lerp(shipCurrentPitchBank, 0, Time.fixedDeltaTime * shipPitchBankVelocity);
        }
        else
        {
            shipCurrentPitchBank = Mathf.Lerp(shipCurrentPitchBank, downwardForce * (shipMaxBank), Time.deltaTime * shipPitchBankVelocity);
        }
        #endregion

        model.transform.localRotation = Quaternion.Euler(-shipCurrentPitchBank + rollState + shipCurrentWobble, model.transform.localEulerAngles.y, model.transform.localEulerAngles.z);

        //normalPitchForce = Mathf.Lerp(normalPitchForce, downwardForce, Time.deltaTime * 3);
        rb.AddForce(transform.up * downwardForce * downwardSpeed * Time.deltaTime);
        //transform.Rotate(Vector3.up * normalPitchForce);
    }
    public void DisableFlightMode()
    {
        flightMode = false;
    }
    public void EnableFlightMode()
    {
        if(!flightMode)
        {
            shipPitchBankVelocity = 0;
            normalPitchForce = 0;
            shipCurrentPitchBank = 0;
            flightMode = true;
        }
    }
    void HandleGravity()
    {
        rb.AddForce(-transform.up * gravity *  Time.deltaTime);
    }

    /// <summary>
    /// Handles the physics of the ship
    /// </summary>
    private void HandleShipPhysics()
    {
        float turboMemory = maxForwardAccelerationSpeed; //Remembers the maxForwardAccelerationSpeed in case turbo is on

        if (turbo)
        {
            currentFowardAccelerationSpeed += speedBoost * Time.deltaTime;
            maxForwardAccelerationSpeed *= maxspeedBoost;
        }

        HoverHandler(); // Set Position depending if there is a ground under the Ship
        if (flightMode)
        {
            if (!grounded)
            {
                FlightHandler(); // Sets Ship rotation to Zero
            }
            AccelerationFlightBehavior(); // Forward Movement
            SteeringFlightPitchBehavior(); // Left and right Movement
            SteeringFlightRollBehavior(); // Down  and Up Movement
        }
        else
        {
            if (grounded)
            {
                AccelerationGroundBehavior(); // Forward Movement
                SteeringGroundBehavior(); // Left and right Movement
                HoverBehavior(); // Only Visual effect of the model
            }
            else
            {
                HandleGravity(); // Down the ship goes
            }
        }

        if (turbo)
        {
            maxForwardAccelerationSpeed = turboMemory;
            currentHeat += Time.deltaTime;
            if (currentHeat > overheatAfter)
            {
                currentHeat = overheatAfter;
                overheated = true;
                heatTimer = overheatLockTime;
            }
        }
    }

    /// <summary>
    /// Makes the ship fall down on the ground if hit by EMP
    /// </summary>
    private void ShipFalling()
    {
        Ray ray = new Ray(transform.position, -transform.up);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            float distanceToGround = Vector3.Distance(transform.position, hit.point);
            if (Vector3.Distance(transform.position, model.transform.position) + fallOffset < distanceToGround)
            {
                fallAccelration = -9.8f * Time.deltaTime;
                fallVelocity += fallAccelration * Time.deltaTime;
                model.transform.position -= new Vector3(0, -1, 0) * fallVelocity;
            }
            if (Vector3.Distance(transform.position, model.transform.position) + fallOffset > distanceToGround)
            {
                model.transform.position -= new Vector3(0, -1, 0) * (Vector3.Distance(transform.position, model.transform.position) + fallOffset - distanceToGround);
            }
            modelOffset = Vector3.Distance(transform.position, model.transform.position);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (debuff != null && debuff.shutDown)
        {
            ShipFalling();
        }
        else
        {
            if (debuff != null && debuff.speedReduction != 0)
            {
                float cfas = currentFowardAccelerationSpeed;
                currentFowardAccelerationSpeed *= (1 - debuff.speedReduction);
                HandleShipPhysics();
                currentFowardAccelerationSpeed = cfas;
            }
            else
            {
                HandleShipPhysics();
            }
        }

        if(overheated)
        if (overheated && (heatTimer -= Time.deltaTime) <= 0)
        {
            heatTimer = 0;
            overheated = false;
        }
        if (currentHeat != 0 && !turbo && !overheated)
        {
            if ((currentHeat -= heatReductionPerSecond * Time.deltaTime) < 0)
                currentHeat = 0;
        }

        if (debuff != null && debuff.energyDrain)
            drain = true;

        Recharge();
        DrainEnergy();

        if (debuff != null && debuff.DebuffFinished())
        {
            if (debuff.shutDown)
            {
                currentFowardAccelerationSpeed = 0;
                fallVelocity = 0;
            }
            debuff = null;
        }

        // Collision
        if (shipIsColliding)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }
    public float SteeringForce { get { return steeringForce; } set { steeringForce = value; } }
    public float AccelerationForce { get { return accelerationForce; } set { accelerationForce = value; } }
    public float DownwardForce { get { return downwardForce; } set { downwardForce = value; } }
    public float Energy { get { return energy; } set { energy = value; } }
    public float CurrentHeat { get { return currentHeat; } set { currentHeat = value; } }
    public bool Turbo { get { return turbo; } set { turbo = value; } }
    public bool Overheated { get { return overheated; } set { overheated = value; } }
    public bool FlightMode { get { return flightMode; } set { flightMode = value; } }
    public bool Activate { get { return activate; } set { activate = value; } }
}
