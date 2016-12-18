using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipController : MonoBehaviour {
    enum LeftOrRight
    {
        Left,
        Right,
        None
    }
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
    private bool oldTurbo;
    [Tooltip("Cooldown of the turbo after release of the button")]
    public float cooldown;
    private float currentCooldown;
    private bool overheated;
    public float maxspeedBoost; //How much the maxspeed should be increased by when using turbo. 1 is normal speed 2 is twice as fast.
    public float speedBoost;
    public float rechargePerSecond;
    public float heatReductionPerSecond;

    [Header("Acceleration Values")]
    [Tooltip("Foward movement of the ship")]
    public float forwardAccelerationSpeed;
    private float currentForwardAccelerationSpeed;

    [Tooltip("The falloff acceleration if there is no accelerationForce")]
    public float noAccelerationDrag;
    public float maxForwardAccelerationSpeed;
    public float maxBackwardAccelerationSpeed;
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

    //Forces to help with Calculations
    private float normalForce;
    private float normalPitchForce;

    //Values that help with the model rotation so thtat it looks smoother
    public float shipCurrentBank;
    public float shipReturnBankSpeed;
    public float shipBankVelocity;
    public float shipBankSpeed;
    private LeftOrRight leftOrRight;
    [Header("Ship Yaw Handling")]
    public float shipBankReturnSpeed;
    public float shipSpeedBank;
    public float shipMaxBank;
    private float rollState = 0;

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

    //Respawn Variables
    float respawnTimer = 2.0f;
    public float currentRespawnTime = 2.0f;

    //Helpfull stuff
    private Rigidbody rb;
    private GameObject model;
    private bool shipIsColliding;
    private bool grounded;
    private float rubberbanding = 0.9f; 


    [Header("Audio")]
    public string engineAudioName;
    public string boostAudioName;
    VirtualAudioSource_NormalizedMultiSources engineAudioSource;
    VirtualAudioSource_NormalizedMultiSources boostAudioSource;

    // Use this for initialization
    void Start()
    {
        shipHoverSpeed = Mathf.PI / 2;

        rb = GetComponent<Rigidbody>();
        model = transform.FindChild("Model").gameObject;
        debuff = null;
        weapon = null;
        drain = false;
        //shielded = false;
        turbo = false;
        oldTurbo = false;
        overheated = false;
        energy = maxEnergy;
        newEnergy = 0;
        currentHeat = 0;
        currentRespawnTime = respawnTimer;
        leftOrRight = LeftOrRight.None;
        grounded = true;
        // Audio init
        AudioController audioController = GameObject.Find("AudioController").GetComponent<AudioController>();       // Audio controller probably should be made static..

        SpawnAudioSourceObject(audioController, engineAudioName);

        GameObject engineAudioObject = new GameObject("EngineAudioSource");
        engineAudioObject.transform.parent = transform;
        engineAudioObject.transform.localPosition = Vector3.zero;
        engineAudioObject.SetActive(false);
        engineAudioSource = engineAudioObject.AddComponent<VirtualAudioSource_NormalizedMultiSources>();
        engineAudioSource.mySource = GameObject.Find(engineAudioName + " (sound effect for: " + transform.name + ")").GetComponent<AudioSource>();
        engineAudioSource.playOnEnable = true;
        engineAudioSource.loopCoroutine = true;
        engineAudioObject.SetActive(true);

        SpawnAudioSourceObject(audioController, boostAudioName);

        GameObject boostAudioObject = new GameObject("BoostAudioSource");
        boostAudioObject.transform.parent = transform;
        boostAudioObject.transform.localPosition = Vector3.zero;
        boostAudioObject.SetActive(false);
        boostAudioSource = boostAudioObject.AddComponent<VirtualAudioSource_NormalizedMultiSources>();
        boostAudioSource.mySource = GameObject.Find(boostAudioName + " (sound effect for: " + transform.name + ")").GetComponent<AudioSource>();
        boostAudioSource.playOnEnable = false;
        boostAudioSource.loopCoroutine = false;
        boostAudioObject.SetActive(true);
    }

    // Need to spawn an object for each sound, needs to be seperate for each ship
    void SpawnAudioSourceObject(AudioController audioController, string audioName)
    {
        GameObject newSourceObject = new GameObject(audioName + " (sound effect for: " + transform.name + ")");
        AudioSource source = newSourceObject.AddComponent<AudioSource>();
        source.clip = audioController.GetAudioClip(audioName);
        source.playOnAwake = false;
        source.loop = false;
        source.dopplerLevel = 0;
        source.spatialBlend = 0.5f;
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
            if (leftOrRight != LeftOrRight.Left)
                shipBankSpeed = 0;
            leftOrRight = LeftOrRight.Left;
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
            if (leftOrRight != LeftOrRight.Right)
                shipBankSpeed = 0;
            leftOrRight = LeftOrRight.Right;
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
        // Collision with ship
        if (other.gameObject.tag == "Ship")
            shipIsColliding = true;

            // Collision with wall
        if (other.gameObject.tag == "Wall")
        {
            shipIsColliding = true;
            for (int i = 0; i < other.contacts.Length; ++i)
            {
                float dot = Vector3.Dot(transform.forward, -other.contacts[i].normal);
                Debug.DrawRay(other.contacts[i].point, other.contacts[i].normal, Color.black, 3);

                if (dot > 0)
                {
                    Vector3 shipVelocity = transform.InverseTransformDirection(rb.velocity);
                    //Vector3 wallDirection = transform.InverseTransformDirection(rb.velocity);

                    float pow = 3; // The curve that the bouncyness of the walls.
                    float knockback = Mathf.Pow(dot, pow);
                    knockback *= shipVelocity.z;
                    
                    shipVelocity.z *= (1 - dot);
                    
                    rb.velocity = transform.TransformDirection(shipVelocity);
                    currentForwardAccelerationSpeed *= (1 - dot);
                }
            }
        }
    }


    void OnCollisionStay(Collision other)
    {
        // Collision with ship
        if (other.gameObject.tag == "Ship")
            shipIsColliding = true;

        // Collision with wall
        if (other.collider.tag == "Wall")
        {
            shipIsColliding = true;
            float contactNum = other.contacts.Length;
            contactNum = Mathf.Clamp(contactNum, 0, 1);

            for (int i = 0; i < contactNum; i++)
            {
                rb.angularVelocity = new Vector3(0, 0, 0);
                Vector3 collisionNormal = transform.InverseTransformDirection((transform.position - other.contacts[i].point).normalized);
                transform.Rotate(Vector3.up * collisionNormal.x * (transform.InverseTransformDirection(rb.velocity).z) / 70);
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        shipIsColliding = false;
    }
    void AccelerationGroundBehavior()
    {
        float maxAccelMultiplier = 1 * rubberbanding;

        //Calculate new CurrentAccelerationForwardSpeed and clamp if out of range
        if(accelerationForce > 0)
        {
            currentForwardAccelerationSpeed += accelerationForce * forwardAccelerationSpeed * Time.deltaTime;
            if (currentForwardAccelerationSpeed > maxForwardAccelerationSpeed * maxAccelMultiplier)
                currentForwardAccelerationSpeed = maxForwardAccelerationSpeed * maxAccelMultiplier;
        }
        else if (accelerationForce < 0)
        {
            currentForwardAccelerationSpeed += accelerationForce * forwardAccelerationSpeed * Time.deltaTime;
            if (currentForwardAccelerationSpeed < -maxBackwardAccelerationSpeed)
                currentForwardAccelerationSpeed = -maxBackwardAccelerationSpeed;
        }
        else
        {
            if (currentForwardAccelerationSpeed > 0 + 150)
                currentForwardAccelerationSpeed -= noAccelerationDrag * Time.deltaTime;
            else if (currentForwardAccelerationSpeed < 0 - 150)
                currentForwardAccelerationSpeed += noAccelerationDrag * Time.deltaTime;
            else
                currentForwardAccelerationSpeed = 0;
        }

        //Add The force to the Ship
        rb.AddForce(transform.forward * currentForwardAccelerationSpeed * Time.deltaTime);
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
            currentForwardAccelerationSpeed += speedBoost * Time.deltaTime;

            maxForwardAccelerationSpeed *= maxspeedBoost;
        }
        HoverHandler(); // Set Position depending if there is a ground under the Ship
        if (grounded)
        {
            AccelerationGroundBehavior(); // Forward Movement
            SteeringGroundBehavior(); // Left and right Movement
            HoverBehavior(); // Only Visual effect of the model
        }
        else
        {
            //HandleGravity(); // Down the ship goes
        }

        if (oldTurbo && !turbo) // If the turbo button releases the cooldown should start
        {
            currentCooldown = cooldown;
        }
        oldTurbo = turbo;
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

    void HandleParticles()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem p in particles)
        {
            p.startLifetime = 0.5f + currentForwardAccelerationSpeed / maxForwardAccelerationSpeed * (turbo ? 1 : 2);
            ParticleSystem.EmissionModule em = p.emission;
            em.rate = (currentForwardAccelerationSpeed > 0 ? currentForwardAccelerationSpeed / maxForwardAccelerationSpeed * 1000 : 10) * (turbo ? 1 : 1.5f);
        }
    }

    // Handle sound effects
    void HandleSounds()
    {
        engineAudioSource.pitch = currentForwardAccelerationSpeed / maxForwardAccelerationSpeed + 1;

        if (turbo)
        {
            if (!boostAudioSource.isPlaying)
                boostAudioSource.Play();
        }
        else
        {
            if (boostAudioSource.isPlaying)
                boostAudioSource.Stop();
        }
    }
    void RespawnOnTrack()
    {
        RaceController rc = GameObject.Find("RaceController").GetComponent<RaceController>();
        float pos = rc.currentPositions[rc.GetRacePosition(this) - 1] % rc.waypoints.Length;
        Waypoint w1 = rc.waypoints[(int)pos];
        Waypoint w2 = rc.waypoints[(int)pos + 1];
        Vector3 RespawnPosition = (w1.transform.position + w2.transform.position) * 0.5f + new Vector3(0, hoverHeight, 0);
        rb.velocity = Vector3.zero;
        currentForwardAccelerationSpeed = 0;
        transform.position = RespawnPosition;
        transform.LookAt(w2.transform);
        HoverHandler();
    }
    void HandleRespawn()
    {
        if (grounded)
        {
            currentRespawnTime = respawnTimer;
        }
        else
        {
            currentRespawnTime -= Time.deltaTime;
            if(currentRespawnTime < 0)
            {
                RespawnOnTrack();
                currentRespawnTime = respawnTimer;
            }
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
                float cfas = currentForwardAccelerationSpeed;
                currentForwardAccelerationSpeed *= (1 - debuff.speedReduction);
                HandleShipPhysics();
                currentForwardAccelerationSpeed = cfas;
            }
            else
            {
                HandleShipPhysics();
            }
            HandleParticles();
        }

        if(overheated)
        if (overheated && (heatTimer -= Time.deltaTime) <= 0)
        {
            heatTimer = 0;
            overheated = false;
        }
        if (currentHeat != 0 && !turbo && !overheated)
        {

            if (currentCooldown < 0)
            {
                if ((currentHeat -= heatReductionPerSecond * Time.deltaTime) < 0)
                    currentHeat = 0;
            }
            else
            {
                currentCooldown -= Time.deltaTime;
            }
        }

        if (debuff != null && debuff.energyDrain)
            drain = true;

        Recharge();
        DrainEnergy();

        if (debuff != null && debuff.DebuffFinished())
        {
            if (debuff.shutDown)
            {
                currentForwardAccelerationSpeed = 0;
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

        HandleSounds();
        HandleRespawn();
    }
    public float SteeringForce { get { return steeringForce; } set { steeringForce = value; } }
    public float AccelerationForce { get { return accelerationForce; } set { accelerationForce = value; } }
    public float Energy { get { return energy; } set { energy = value; } }
    public float CurrentHeat { get { return currentHeat; } set { currentHeat = value; } }
    public bool Turbo { get { return turbo; } set { turbo = value; } }
    public bool Overheated { get { return overheated; } set { overheated = value; } }
    public bool Activate { get { return activate; } set { activate = value; } }
    public float CurrentForwardAccelerationForce { get { return currentForwardAccelerationSpeed; } }
    public bool Grounded { get { return grounded; } }
    public float RubberBanding { get { return rubberbanding; } set { rubberbanding = value; } }

}
