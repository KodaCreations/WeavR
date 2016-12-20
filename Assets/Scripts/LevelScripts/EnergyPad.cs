using UnityEngine;
using System.Collections;

/// <summary>
/// This class represents an object from which ships can gain energy
/// </summary>
public class EnergyPad : MonoBehaviour {
    public float energy;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Gives energy to ships passing the objects hitbox
    /// </summary>
    /// <param name="other">The ship passing the hitbox</param>
    void OnTriggerEnter(Collider other)
    {
        ShipController ship = other.GetComponent<ShipController>();
        if (ship)
            ship.Recharge(energy);
    }
}
