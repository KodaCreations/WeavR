using UnityEngine;
using System.Collections;

public class AttackState : IAiState {


    private readonly AIController ai;
	// Use this for initialization

	public AttackState (AIController Ai)
    {
        ai = Ai;
	
	}
	
	// Update is called once per frame
	public void UpdateState () {
	
	}

    public void ToLookForTrigger()
    {

    }

    public void ToLookForWeapon()
    {

    }

    public void ToAttackState()
    {
        Debug.Log("Can't transition to same state");
    }

    public void ToChaseState()
    {
        
    }
}
