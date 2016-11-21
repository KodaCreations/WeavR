using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
    public WeaponType weapon;
    public Debuff debuff;
    protected float speed;

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
        if (weapon == WeaponType.EnergyDrain || weapon == WeaponType.DecreasedVision || weapon == WeaponType.Missile)
            speed = 100;
        else
            speed = 0;
	}
	
	// Update is called once per frame
	public virtual void Update () {
	    //MÅSTE HANTERA EMP HÄR
        if (weapon == WeaponType.EnergyDrain)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
	}

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

    }

    private Debuff GetDebuff(bool shieldedTarget)
    {
        switch (weapon)
        {
            case WeaponType.Missile:
                return shieldedTarget ? null : new Debuff(0.2f, false, false, false, 5f);
            case WeaponType.Mine:
                return shieldedTarget ? new Debuff(0.1f, false, false, false, 5f) : new Debuff(0.25f, false, false, false, 5f);
            case WeaponType.EMP:
                return new Debuff(0, false, false, true, 5f);
            case WeaponType.EnergyDrain:
                return shieldedTarget ? new Debuff(0.25f, true, false, false, 5f) : new Debuff(0f, true, false, false, 1f);
            case WeaponType.DecreasedVision:
                return new Debuff(0f, false, true, false, 5f);
        }
        return null;
    }
}