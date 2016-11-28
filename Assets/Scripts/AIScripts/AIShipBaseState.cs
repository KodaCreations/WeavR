using UnityEngine;
using System.Collections;

public class AIShipBaseState : MonoBehaviour {

    public TheRabbit rabbit;
    private Vector3 target;
    private Vector3 lineOfSight;
    public float aiMovementSpeed;
    private Vector3 maxSeeAHead;
    private Vector3 vel;
    private Vector3 posDifference;

    int MaxDist = 20;
    int MinDist = 10;
    public Transform[] triggerPoints;
    public Transform[] weaponPoints;

    [HideInInspector]
    public int destTriggPoint = 0, destWeapPoint = 0;
    public float aiRotationSpeed;
    public float aiHoverHeight;

    public GameObject aiModel;

    Quaternion aiWantedTrackRot;
    Vector3 aiWantedTrackPos;

    bool aiShipIsColliding;

    float shipAccel;
    float shipAccelCap;


    public float steeringForce;

    public float accelerationForce;

    float aiNormalForce;

    public float aiShipCurrentBank;
    public float aiShipBankSpeed;
    public float aiShipReturnBankSpeed;
    float aiShipBankVelocity;
    float aiShipMaxBank = 40;
    float aiRollState;

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
        shipHoverSpeed = Mathf.PI / 2;
        shipHoverTime = 0;
        //shipWobbleAmount = 2f;
        //shipWobbleSpeed = 6;
        //shipWobbleTime = 0;
        rb = GetComponent<Rigidbody>();
        aiModel = transform.FindChild("ship").gameObject;
        //if(!isServer)
        //{
        //    rb.isKinematic = true;
        //}
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
            aiWantedTrackPos = hit.point + hit.normal * aiHoverHeight;
            float distance = Vector3.Distance(aiWantedTrackPos, transform.position);
            float distanceToGround = Vector3.Distance(aiWantedTrackPos, hit.point);
            float pullStrength = 1 - (distanceToGround - distance) / distanceToGround;
            transform.position = Vector3.Slerp(transform.position, aiWantedTrackPos, Time.deltaTime * correctionPosDelay + pullStrength);

            float correctionRotDelay = 15;
            aiWantedTrackRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, hit.normal), hit.normal), Time.deltaTime * correctionRotDelay);
            transform.rotation = Quaternion.Slerp(transform.rotation, aiWantedTrackRot, Time.deltaTime * correctionRotDelay);

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
        aiModel.transform.localPosition = new Vector3(0, shipCurrentHover * 0.03f, 0);
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
            aiShipReturnBankSpeed = 0;
            if (aiShipCurrentBank > 0 || aiShipCurrentBank == 0)
            {
                if (aiShipCurrentBank > 15)
                {
                    aiShipBankSpeed = Mathf.Lerp(aiShipBankSpeed, 5, Time.fixedDeltaTime * 5);
                }
                else
                {
                    aiShipBankSpeed = Mathf.Lerp(aiShipBankSpeed, 3, Time.fixedDeltaTime * 5);
                }
            }
            else
            {
                aiShipBankSpeed = Mathf.Lerp(aiShipBankSpeed, 2f, Time.fixedDeltaTime * 5);
            }
            aiShipBankVelocity = Mathf.Lerp(aiShipBankVelocity, aiShipBankSpeed, Time.deltaTime * 50);
        }

        if (steeringForce < 0)
        {
            aiShipReturnBankSpeed = 0;
            if (aiShipCurrentBank < 0 || aiShipCurrentBank == 0)
            {

                if (aiShipCurrentBank < -15)
                {
                    aiShipBankSpeed = Mathf.Lerp(aiShipBankSpeed, 5, Time.fixedDeltaTime * 5);
                }
                else
                {
                    aiShipBankSpeed = Mathf.Lerp(aiShipBankSpeed, 3, Time.fixedDeltaTime * 5);
                }
            }
            else
            {
                aiShipBankSpeed = Mathf.Lerp(aiShipBankSpeed, 2f, Time.fixedDeltaTime * 5);
            }
            aiShipBankVelocity = Mathf.Lerp(aiShipBankVelocity, aiShipBankSpeed, Time.deltaTime * 50);
        }

        if (steeringForce == 0)
        {
            aiShipBankSpeed = 0;
            if (aiShipCurrentBank < 0)
            {
                if (aiShipCurrentBank < -15)
                {
                    aiShipReturnBankSpeed = Mathf.Lerp(aiShipReturnBankSpeed, 10f, Time.fixedDeltaTime * 2);
                }
                else
                {
                    aiShipReturnBankSpeed = Mathf.Lerp(aiShipReturnBankSpeed, 7f, Time.fixedDeltaTime * 3);
                }
            }
            else
            {
                if (aiShipCurrentBank > 15)
                {
                    aiShipReturnBankSpeed = Mathf.Lerp(aiShipReturnBankSpeed, 10f, Time.fixedDeltaTime * 2);
                }
                else
                {
                    aiShipReturnBankSpeed = Mathf.Lerp(aiShipReturnBankSpeed, 7f, Time.fixedDeltaTime * 3);
                }
            }
            aiShipBankVelocity = Mathf.Lerp(aiShipBankVelocity, aiShipReturnBankSpeed, Time.deltaTime * 30);
            aiShipCurrentBank = Mathf.Lerp(aiShipCurrentBank, 0, Time.fixedDeltaTime * aiShipBankVelocity);
        }
        else
        {
            aiShipCurrentBank = Mathf.Lerp(aiShipCurrentBank, steeringForce * (aiShipMaxBank), Time.deltaTime * aiShipBankVelocity);
        }
        #endregion

        aiModel.transform.localRotation = Quaternion.Euler(aiModel.transform.localEulerAngles.x, aiModel.transform.localEulerAngles.y, -aiShipCurrentBank + aiRollState + shipCurrentWobble);

        aiNormalForce = Mathf.Lerp(aiNormalForce, steeringForce, Time.deltaTime * 3);
        transform.Rotate(Vector3.up * aiNormalForce);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Ship")
            aiShipIsColliding = true;
    }


    void OnCollisionExit()
    {
        aiShipIsColliding = false;
    }

    void CmdSendInputHandler(float steeringInput, float accelerationInput)
    {
        steeringForce = steeringInput;
        accelerationForce = accelerationInput;
    }
    void AccelerationGroundBehavior()
    {
        rb.AddForce(transform.forward * accelerationForce * Time.deltaTime);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //if (isLocalPlayer)
        //{
        //    InputHandler();
        //    CmdSendInputHandler(steeringForce, accelerationForce);
        //}

        HoverHandler();
        SteeringGroundBehavior();
        AccelerationGroundBehavior();
        HoverBehavior();
        Chase();
        // Collision
        if (aiShipIsColliding)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0)
        {
            return 1.0f;
        }
        else if (dir < 0.0)
        {
            return -1.0f;
        }
        else {
            return 0.0f;
        }
    }

    void Chase()
    {

        accelerationForce = 0;
        steeringForce = 0;

        //Debug.Log(accelerationForce + "");
        Vector3 targetDir = rabbit.transform.position - transform.position;
        targetDir.Normalize();
        float dir = AngleDir(transform.forward, -targetDir, transform.up);



        if (Vector3.Distance(rabbit.transform.position, transform.position) > 10)
        {
            accelerationForce = 1 * aiMovementSpeed;
        }

        if (dir > 0.0f)
        {
            steeringForce = -1 * aiRotationSpeed;
        }
        else if (dir < 0.0f)
        {
            steeringForce = 1 * aiRotationSpeed;
        }




        //transform.LookAt(rabbit.transform);
        ////transform.TransformDisrection(rabbit.transform.position);

        //if (Vector3.Distance(transform.position, rabbit.transform.position) >= MinDist && Vector3.Distance(transform.position, rabbit.transform.position) < MaxDist)
        //{
        //    transform.position += transform.forward * aiMovementSpeed * Time.deltaTime;

        //}

        //if (Vector3.Distance(transform.position, rabbit.transform.position) > MaxDist)
        //{
        //    transform.position += transform.forward * (aiMovementSpeed * 1.5f) * Time.deltaTime;
        //    //accelerationForce += 1 * aiMovementSpeed * Time.deltaTime;
        //}

        //if (Vector3.Distance(transform.position, rabbit.transform.position) < MinDist)
        //{
        //    transform.position += transform.forward * (aiMovementSpeed / 2) * Time.deltaTime;
        //    //accelerationForce += 1 * (aiMovementSpeed / 2) * Time.deltaTime;
        //}


    }
    void InputHandler()
    {
        //transform.LookAt(rabbit.transform);
        //accelerationForce = 0;
        //steeringForce = 0;

        //Debug.Log(accelerationForce + "");

        //accelerationForce = 1 * aiMovementSpeed;
        //transform.position = target * accelerationForce;


        //if (Input.GetKey(KeyCode.S))
        //{
        //    accelerationForce = -1 * movementSpeed;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    steeringForce = -1 * rotationSpeed;
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    steeringForce = 1 * rotationSpeed;
        //}
    }
}
