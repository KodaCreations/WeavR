using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
    public WeaponType weapon;

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
	
	}
	
	// Update is called once per frame
	public virtual void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        // Skapa Debuff och ge till skeppet
        // Spela partikel om lämpligt
    }
}