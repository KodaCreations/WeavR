using UnityEngine;
using System.Collections;

/// <summary>
/// This class represents a weapon
/// </summary>
public class Weapon : MonoBehaviour {
    public WeaponType weapon;
    public Debuff debuff;
    public float speed;

    public enum WeaponType
    {
        Missile,
        Mine,
        EMP,
        EnergyDrain,
        DecreasedVision
    }

	// Use this for initialization
	public virtual void Start () {
        if (weapon == WeaponType.EMP)
            DisableShips();
	}
	
	// Update is called once per frame
	public virtual void Update () {
        if (weapon == WeaponType.EnergyDrain)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
	}

    /// <summary>
    /// Manages the collision between weapon and object
    /// </summary>
    /// <param name="other">The collider of the other object</param>
    void OnTriggerEnter(Collider other)
    {
        ParticleSystem particle = GetComponent<ParticleSystem>();
        if (particle)
        {
            particle.Play();
            Destroy(gameObject, particle.duration);
            GetComponent<Collider>().enabled = false;

            GetComponentInChildren<MeshRenderer>().enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }

        ShipController ship = other.GetComponent<ShipController>();

        if (ship)
            ship.debuff = GetDebuff(ship.shielded);
        else
            Destroy(gameObject);
        if (ship)
            Debug.Log("Weapon Type: " + weapon + ", Speed: " + ship.debuff.speedReduction + ", Drain energy: " + ship.debuff.energyDrain + ", Reduce visability: " + ship.debuff.reduceVisability + ", Shut down: " + ship.debuff.shutDown);

        //CODE FOR MISSILE ACTIVATING DYNAMIC TRACK SEGMENT
    }

    /// <summary>
    /// Disables all ships except ships within a certain distance of the weapon.
    /// </summary>
    void DisableShips()
    {
        ShipController[] ships = FindObjectsOfType<ShipController>();

        foreach (ShipController s in ships)
        {
            if (Vector3.Distance(s.transform.position, transform.position) < 10)
                continue;
            s.debuff = GetDebuff(s.shielded);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Finds out what debuff should be given to a ship when hit by a weapon and returns it
    /// </summary>
    /// <param name="shieldedTarget">Whether the target is shielded or not</param>
    /// <returns>The Debuff, if any, to be applied to the ship hit by the weapon</returns>
    private Debuff GetDebuff(bool shieldedTarget)
    {
        switch (weapon)
        {
            case WeaponType.Missile:
                return shieldedTarget ? null : new Debuff(0.2f, false, false, false, 5f);
            case WeaponType.Mine:
                return shieldedTarget ? new Debuff(0.1f, false, false, false, 5f) : new Debuff(0.25f, false, false, false, 5f);
            case WeaponType.EMP:
                return new Debuff(0, false, false, true, 3f);
            case WeaponType.EnergyDrain:
                return shieldedTarget ? new Debuff(0f, true, false, false, 5f) : new Debuff(0.25f, true, false, false, 1f);
            case WeaponType.DecreasedVision:
                return new Debuff(0f, false, true, false, 5f);
        }
        return null;
    }
}