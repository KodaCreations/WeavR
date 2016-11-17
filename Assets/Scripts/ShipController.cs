using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ShipController : NetworkBehaviour {

    public float movementSpeed;
    public float rotationSpeed;
    public float downwardSpeed;
    public float hoverHeight;

    public GameObject model;
    public GameObject camera;

    Quaternion wantedTrackRot;
    Vector3 wantedTrackPos;

    bool shipIsColliding;

    float shipAccel;
    float shipAccelCap;

    bool flightMode;

    [SyncVar]
    public float steeringForce;
    [SyncVar]
    public float accelerationForce;
    [SyncVar]
    public float downwardForce;

    float normalForce;
    float normalPitchForce;

    public float shipCurrentBank;
    public float shipBankSpeed;
    public float shipReturnBankSpeed;
    public float shipCurrentPitchBank;
    public float shipBankPitchSpeed;
    public float shipReturnBankPitchSpeed;
    float shipBankVelocity;
    float shipPitchBankVelocity;
    float shipMaxBank = 40;
    float rollState;

    float shipThrust;
    float shipThrustCap;

    //Hover behavior var
    float shipHoverAmount;
    float shipHoverSpeed;
    float shipHoverTime;
    float shipCurrentHover;

    // Wobble behavior var
    float shipWobbleAmount;
    float shipWobbleSpeed;
    float shipWobbleTime;
    float shipCurrentWobble;

    Rigidbody rb;
    // Use this for initialization
    void Start()
    {
        flightMode = true;
        shipHoverSpeed = Mathf.PI / 2;
        shipHoverTime = 0;
        //shipWobbleAmount = 2f;
        //shipWobbleSpeed = 6;
        //shipWobbleTime = 0;
        rb = GetComponent<Rigidbody>();
        model = transform.FindChild("Model").gameObject;
        if (isLocalPlayer)
        {
            camera = GameObject.Find("Main Camera");
            camera.transform.SetParent(transform);
        }
        //if(!isServer)
        //{
        //    rb.isKinematic = true;
        //}
    }
    void InputHandler()
    {
        accelerationForce = 0;
        steeringForce = 0;
        downwardForce = 0;
        if (Input.GetKey(KeyCode.W))
        {
            accelerationForce = 1 * movementSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            accelerationForce = -1 * movementSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            steeringForce = -1 * rotationSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            steeringForce = 1 * rotationSpeed;
        }
        if(Input.GetKey(KeyCode.Space))
        {
            downwardForce = 1 * downwardSpeed;
        }
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
        {
            downwardForce = -1 * downwardSpeed;
        }
    }
    void HoverHandler()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        Debug.DrawRay(transform.position, -transform.up, Color.black, 2.0f);

        // Raycast downwards to find the road and sets a Wanted Rot and Pos that the ship banks towards
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            float correctionPosDelay = 15f;
            //Vector3.Slerp(transform.position, hit.point + hit.normal * hoverHeight, Time.deltaTime * correctionPosDelay);
            wantedTrackPos = hit.point + hit.normal * hoverHeight;
            float distance = Vector3.Distance(wantedTrackPos, transform.position);
            float distanceToGround = Vector3.Distance(wantedTrackPos, hit.point);
            float pullStrength = 1 - (distanceToGround - distance) / distanceToGround;
            transform.position = Vector3.Slerp(transform.position, wantedTrackPos, Time.deltaTime * correctionPosDelay + pullStrength);

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

        model.transform.localRotation = Quaternion.Euler(model.transform.localEulerAngles.x, model.transform.localEulerAngles.y, -shipCurrentBank + rollState + shipCurrentWobble);

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
    [Command]
    void CmdSendInputHandler(float steeringInput, float accelerationInput)
    {
        steeringForce = steeringInput;
        accelerationForce = accelerationInput;
    }
    void AccelerationGroundBehavior()
    {
        rb.AddForce(transform.forward * accelerationForce * Time.deltaTime);
    }
    void AccelerationFlightBehavior()
    {
        rb.AddForce(transform.forward * accelerationForce * Time.deltaTime);
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

        model.transform.localRotation = Quaternion.Euler(-shipCurrentPitchBank + rollState + shipCurrentWobble, model.transform.localEulerAngles.y, model.transform.localEulerAngles.z);

        //normalPitchForce = Mathf.Lerp(normalPitchForce, downwardForce, Time.deltaTime * 3);
        rb.AddForce(transform.up * downwardForce * 5000 * Time.deltaTime);
        //transform.Rotate(Vector3.up * normalPitchForce);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            InputHandler();
            CmdSendInputHandler(steeringForce, accelerationForce);
        }

        if(flightMode)
        {
            FlightHandler();
            AccelerationFlightBehavior();
            SteeringFlightRollBehavior();
            SteeringFlightPitchBehavior();
        }
        else
        {
            HoverHandler();
            SteeringGroundBehavior();
            AccelerationGroundBehavior();
            HoverBehavior();
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
}
