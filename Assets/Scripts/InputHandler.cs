using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

    public KeyCode forwardKey;
    public KeyCode backwardKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode brakeKey;
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
    public void SetKeys(KeyCode forwardKey, KeyCode backwardKey, KeyCode leftKey, KeyCode rightKey, KeyCode brakeKey, KeyCode turboKey)
    {
        this.forwardKey = forwardKey;
        this.backwardKey = backwardKey;
        this.leftKey = leftKey;
        this.rightKey = rightKey;
        this.brakeKey = brakeKey;
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
            if (Input.GetKey(brakeKey))
            {
                if (ship.currentFowardAccelerationSpeed > 1)
                {
                    ship.AccelerationForce = -ship.brakingAcceleration;
                }
                else if (ship.currentFowardAccelerationSpeed < 1)
                    ship.AccelerationForce = ship.brakingAcceleration;
                else
                    ship.AccelerationForce = 0;
            }
            if (Input.GetKey(turboKey) && ship.Energy > 0)
            {
                ship.Turbo = true;
                ship.Energy -= Time.deltaTime * ship.energyEfficiency; //*ship.shieldEfficiency;
            }
            else
            {
                ship.Turbo = false;
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
        }
        else
        {
            // Get input
            float horizontalInput = Input.GetAxis("Horizontal" + gamepadNumber);
            float throttleInput = Input.GetAxis("Throttle" + gamepadNumber);
            float reverseInput = Input.GetAxis("Reverse" + gamepadNumber);

            float combinedInput = throttleInput + reverseInput;

            // Forward and backward
            if (combinedInput != 0)
            {
                ship.AccelerationForce = combinedInput;
            }

            // Braking
            if (Input.GetKey("joystick " + (gamepadNumber + 1) + " button 2"))
            {
                if (ship.currentFowardAccelerationSpeed > 1)
                {
                    ship.AccelerationForce = -ship.brakingAcceleration;
                }
                else if (ship.currentFowardAccelerationSpeed < 1)
                    ship.AccelerationForce = ship.brakingAcceleration;
                else
                    ship.AccelerationForce = 0;
            }
            if (horizontalInput != 0)
            {
                ship.SteeringForce =  horizontalInput * ship.rotationSpeed;
            }

            // Boost
            if (Input.GetKey("joystick " + (gamepadNumber + 1) + " button 1") && ship.Energy > 0)
            {
                ship.Turbo = true;
                ship.Energy -= Time.deltaTime * ship.energyEfficiency;// * ship.shieldEfficiency;
            }
        }
    }

    void Update ()
    {
        if (ship)
            HandleInput();
	}
}
