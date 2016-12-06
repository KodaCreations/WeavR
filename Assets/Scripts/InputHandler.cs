using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

    ShipController ship;
	// Use this for initialization
	void Start ()
    {
        ship = gameObject.GetComponent<ShipController>();
	}

    void InputController()
    {
        ship.AccelerationForce = 0;
        ship.SteeringForce = 0;
        ship.DownwardForce = 0;
        ship.shielded = false;
        ship.Turbo = false;
        if (!ship.Activate)
            return;

        if (Input.GetKey(KeyCode.W))
        {
            ship.AccelerationForce = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            ship.AccelerationForce = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            ship.SteeringForce = -1 * ship.rotationSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            ship.SteeringForce = 1 * ship.rotationSpeed;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            ship.FireWeapon();
        }
        if ((ship.weapon == Weapon.WeaponType.Missile || ship.weapon == Weapon.WeaponType.DecreasedVision) && Input.GetKey(KeyCode.Tab))
        {
            ship.Target();
        }
        if (Input.GetKey(KeyCode.Space))
        {
            ship.DownwardForce = 1;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
        {
            ship.DownwardForce = -1;
        }
        if (Input.GetKey(KeyCode.Q) && ship.Energy > 0)
        {
            ship.shielded = true;
            ship.Energy -= Time.deltaTime * ship.energyEfficiency;
        }
        if (Input.GetKey(KeyCode.E) && ship.Energy > 0)
        {
            ship.Turbo = true;
            ship.Energy -= Time.deltaTime * ship.energyEfficiency * ship.shieldEfficiency;
        }
    }
	// Update is called once per frame
	void Update ()
    {
        if (ship)
            InputController();
	}
}
