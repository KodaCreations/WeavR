using UnityEngine;
using System.Collections;

public class LookForWeaponState : IAiState {
    private readonly AIController ai;

    public LookForWeaponState(AIController Ai)
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

    public void ToChaseState()
    {

    }

    void PickupWeapon()
    {

    }
}
