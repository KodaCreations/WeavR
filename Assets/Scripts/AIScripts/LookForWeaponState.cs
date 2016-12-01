using UnityEngine;
using System.Collections;

public class LookForWeaponState : IAiState {
    private readonly AIShipBaseState ai;

    public LookForWeaponState(AIShipBaseState Ai)
    {
        ai = Ai;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public void UpdateState () {
        PickupWeapon();
	
	}

    public void ToLookForTrigger()
    {

    }

    public void ToLookForWeapon()
    {

    }

    public void ToAttackState()
    {

    }

    void PickupWeapon()
    {
        if (ai.destWeapPoint == 0)
        {
            return;
        }

        ai.destWeapPoint = (ai.destWeapPoint + 1) % ai.weaponPoints.Length;
    }
}
