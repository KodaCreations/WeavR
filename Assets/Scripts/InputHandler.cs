using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

    public KeyCode forwardKey;
    public KeyCode backwardKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode turboKey;

    ShipController ship;

    [HideInInspector]
    public bool usingGamepad;
    int gamepadNumber;

	void Start ()
    {
        ship = gameObject.GetComponent<ShipController>();
	}

    // Set new input keys
    public void SetKeys(KeyCode forwardKey, KeyCode backwardKey, KeyCode leftKey, KeyCode rightKey, KeyCode turboKey)
    {
        this.forwardKey = forwardKey;
        this.backwardKey = backwardKey;
        this.leftKey = leftKey;
        this.rightKey = rightKey;
        this.turboKey = turboKey;
    }

    // Use gamepad instead of keyboard, needs to know which index of controllers to use
    public void UseGamepad(int gamepadNumber)
    {
        this.gamepadNumber = gamepadNumber;
    }

    void HandleInput()
    {
        ship.AccelerationForce = 0;
        ship.SteeringForce = 0;
        ship.DownwardForce = 0;
        ship.shielded = false;
        ship.Turbo = false;
        if (!ship.Activate)
            return;

        if (!usingGamepad)
        {
            if (Input.GetKey(forwardKey))
            {
                ship.AccelerationForce = 1;
            }
            if (Input.GetKey(backwardKey))
            {
                ship.AccelerationForce = -1;
            }
            if (Input.GetKey(leftKey))
            {
                ship.SteeringForce = -1 * ship.rotationSpeed;
            }
            if (Input.GetKey(rightKey))
            {
                ship.SteeringForce = 1 * ship.rotationSpeed;
            }
            //if (Input.GetKey(KeyCode.Z))
            //{
            //    ship.FireWeapon();
            //}
            //if ((ship.weapon == Weapon.WeaponType.Missile || ship.weapon == Weapon.WeaponType.DecreasedVision) && Input.GetKey(KeyCode.Tab))
            //{
            //    ship.Target();
            //}
            //if (Input.GetKey(KeyCode.Space))
            //{
            //    ship.DownwardForce = 1;
            //}
            //if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
            //{
            //    ship.DownwardForce = -1;
            //}
            //if (Input.GetKey(KeyCode.Q) && ship.Energy > 0)
            //{
            //    ship.shielded = true;
            //    ship.Energy -= Time.deltaTime * ship.energyEfficiency;
            //}
            if (Input.GetKey(turboKey) && ship.Energy > 0)
            {
                ship.Turbo = true;
                ship.Energy -= Time.deltaTime * ship.energyEfficiency * ship.shieldEfficiency;
            }
        }
        else
        {
            // Get input
            float horizontalInput = Input.GetAxis("Horizontal" + gamepadNumber);
            float throttleInput = Input.GetAxis("Throttle" + gamepadNumber);

            Debug.Log(throttleInput);

            // Check if inside deadzone
            //if (Mathf.Abs(horizontalInput) < gamePadDeadzone)
            //    horizontalInput = 0;
            //if (Mathf.Abs(throttleInput) < gamePadDeadzone)
            //    throttleInput = 0;

            // Apply forces
            if (throttleInput != 0)
            {
                ship.AccelerationForce = throttleInput;
            }

            //if (Input.GetKey("joystick " + gamepadNumber + " button 1"))
            //{
            //    ship.AccelerationForce = -1;
            //}
            if (horizontalInput != 0)
            {
                ship.SteeringForce =  horizontalInput * ship.rotationSpeed;
            }
            //if (Input.GetKey("joystick " + gamepadNumber + " button 4") && ship.Energy > 0)
            //{
            //    ship.Turbo = true;
            //    ship.Energy -= Time.deltaTime * ship.energyEfficiency * ship.shieldEfficiency;
            //}
        }
    }

    void Update ()
    {
        if (ship)
            HandleInput();
	}
}
