using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour {

    //Ship Forward Acceleration Variables
    public float forwardAccelerationSpeed;
    public float currentFowardAccelerationSpeed;
    public float noAccelerationDrag;
    public float maxForwardAccelerationSpeed;
    public float flightMinimumAccelerationSpeed;
    
    //Ship Flight downwardSpeed
    public float downwardSpeed;

    //Ship Rotation Speed
    public float rotationSpeed;
    
    //Ship hover height over ground
    public float hoverHeight;
    public Debuff debuff;
    public Weapon.WeaponType? weapon;
    public bool shielded;
    private Transform target;

    //Ship gravity when not on the ground an not in flightmode
    public float gravity;

    //Ship temp values
    private GameObject camera;
    public Vector3 cameraPosition;

    //Ship Wanted Position and Rotation depending on the conditions
    private Quaternion wantedTrackRot;
    private Quaternion wantedTrackPitchRot;
    private Vector3 wantedTrackPos;

    //The Values that determin where the ship shall move that are changed in the Input or AI
    public float steeringForce; //Left or Right
    public float accelerationForce; //Forward or Backwards
    public float downwardForce; //Down or Up in FlightMode

    //Forces to help with Calculations
    private float normalForce;
    private float normalPitchForce;

    //Values that help with the model rotation so thtat it looks smoother
    private float shipCurrentBank;
    private float shipBankSpeed;
    public float shipReturnBankSpeed;
    private float shipCurrentPitchBank;
    private float shipBankPitchSpeed;
    public float shipReturnBankPitchSpeed;
    private float shipBankVelocity;
    private float shipPitchBankVelocity;
    private float shipMaxBank = 40;
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
    private ShipNetworkController networkController;
    private Rigidbody rb;
    private GameObject model;
    private bool shipIsColliding;
    public bool flightMode;
    private bool grounded;

    // Use this for initialization
    void Start()
    {
        flightMode = false;
        shipHoverSpeed = Mathf.PI / 2;

        rb = GetComponent<Rigidbody>();
        model = transform.FindChild("Model").gameObject;
        networkController = GetComponent<ShipNetworkController>();
        if (networkController)
        {
            if (networkController.isLocalPlayer)
            {
                camera = GameObject.Find("Main Camera");
                camera.transform.SetParent(transform);
                camera.transform.position = transform.position + cameraPosition;
            }
        }
        else
        {
            camera = GameObject.Find("Main Camera");
            camera.transform.SetParent(transform);
            camera.transform.position = transform.position + cameraPosition;
        }
        debuff = null;
        shielded = false;
        weapon = null;
    }
    void InputHandler()
    {
        accelerationForce = 0;
        steeringForce = 0;
        downwardForce = 0;
        if (Input.GetKey(KeyCode.W))
        {
            accelerationForce = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            accelerationForce = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            steeringForce = -1 * rotationSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            steeringForce = 1 * rotationSpeed;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            FireWeapon();
        }
        if ((weapon == Weapon.WeaponType.Missile || weapon == Weapon.WeaponType.DecreasedVision) && Input.GetKey(KeyCode.Tab))
        {
            Target();
        }
        if(Input.GetKey(KeyCode.Space))
        {
            downwardForce = 1;
        }
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
        {
            downwardForce = -1;
        }
    }

    void FireWeapon()
    {
        if (weapon == null)
            return;
        switch (weapon)
        {//Hitta bra plats att spawna vapnen!!!
            case Weapon.WeaponType.Missile:
                if (target)
                {
                    GameObject missile = Instantiate(Resources.Load("Prefabs/Missile"), transform.position + transform.forward * 10, transform.rotation) as GameObject;
                    missile.GetComponent<GuidedWeapon>().target = target;
                    target = null;
                }
                break;
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
                }
                break;
        }
        if (target == null)
            weapon = null;
    }

    void Target()
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
            //Vector3.Slerp(transform.position, hit.point + hit.normal * hoverHeight, Time.deltaTime * correctionPosDelay);
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


            //Quaternion wantedTrackPitchRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(model.transform.localEulerAngles.x, model.transform.localEulerAngles.y, -shipCurrentBank + rollState + shipCurrentWobble), Time.deltaTime * correctionRotDelay);
            //model.transform.localRotation = Quaternion.Slerp(transform.rotation, wantedTrackPitchRot, Time.deltaTime * correctionRotDelay);

            Debug.DrawRay(hit.point, hit.normal, Color.red, 2.0f);
        }
    }
    void HoverBehavior()
    {
        // Basic hover what uses two Sinus curves to determin the height
        shipHoverTime += Time.deltaTime;
        shipHoverTime = shipHoverTime % (Mathf.PI * 2);
        shipHoverSpeed += Time.deltaTime / 2;
        shipHoverSpeed = shipHoverSpeed % (Mathf.PI * 2);// Mathf.Clamp(shipHoverSpeed, 0, 10);
        shipHoverAmount -= Time.deltaTime / 4;
        shipHoverAmount = shipHoverAmount % 3;// Mathf.Clamp(shipHoverAmount, 0, 10);

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
        wantedTrackRot = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);//Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, hit.normal), hit.normal), Time.deltaTime * correctionRotDelay);
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

        //if(!grounded)
        //{
        //    wantedTrackPitchRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(-shipCurrentPitchBank + rollState + shipCurrentWobble, model.transform.localEulerAngles.y, model.transform.localEulerAngles.z), Time.deltaTime * 15);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, wantedTrackPitchRot, Time.deltaTime * 15);
        //}
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
    // Update is called once per frame
    void FixedUpdate()
    {
        if(networkController) //Check if it has a networkController to be able to spawn without using networkManager
        {
            if(networkController.isLocalPlayer)
            {
                InputHandler();
                networkController.CmdSendInputHandler(steeringForce, accelerationForce, downwardForce); // Send The Information To the Server
            }
        }
        else
        {
            InputHandler();
        }
        HoverHandler(); // Set Position depending if there is a gound under the Ship
        if (flightMode)
        {
            if(!grounded)
            {
                FlightHandler(); // Sets Ship rotation to Zero
            }
            AccelerationFlightBehavior(); // Forward Movement
            SteeringFlightPitchBehavior(); // Left and right Movement
            SteeringFlightRollBehavior(); // Down  and Up Movement
        }
        else
        {
            if(grounded)
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

        if (debuff != null && debuff.DebuffFinished())
            debuff = null;

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
}
