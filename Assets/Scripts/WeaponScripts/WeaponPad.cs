using UnityEngine;
using System.Collections;

/// <summary>
/// This class represents an object from which players can pick up a weapon
/// </summary>
public class WeaponPad : MonoBehaviour {
    private float timer;
    private bool active;

	// Use this for initialization
	void Start () {
        active = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (!active)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                GetComponentInChildren<MeshRenderer>().enabled = true;
                active = true;
            }
        }
	}

    /// <summary>
    /// Handles collision between a ship and the Weapon Pad
    /// </summary>
    /// <param name="other">The collider of the ship</param>
    void OnTriggerEnter(Collider other)
    {
        if (active)
        {
            ShipController ship = other.GetComponent<ShipController>();

            if (ship && ship.weapon == null)
            {
                ship.weapon = GetWeapon();
                active = false;
                timer = 10;
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }
    }

    /// <summary>
    /// Picks a weapon randomly
    /// </summary>
    /// <returns>The random weapon</returns>
    private Weapon.WeaponType GetWeapon()
    {
        int rnd = Random.Range(0, 99);
        if (rnd < 5)
            return Weapon.WeaponType.EMP;
        if (rnd < 35)
            return Weapon.WeaponType.Missile;
        if (rnd < 57)
            return Weapon.WeaponType.Mine;
        if (rnd < 80)
            return Weapon.WeaponType.EnergyDrain;
        return Weapon.WeaponType.DecreasedVision;
    }
}
