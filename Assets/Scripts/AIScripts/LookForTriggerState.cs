using UnityEngine;
using System.Collections;

public class LookForTriggerState : IAiState {
    private readonly AIShipBaseState ai;

    public LookForTriggerState(AIShipBaseState Ai)
    {
        ai = Ai;
    }
	// Use this for initialization
	public void UpdateState()
    {
        ActivateTriggerTile();
    }

    public void ToLookForTrigger()
    {
        Debug.Log("Can't transition to same state");
    }

    public void ToLookForWeapon()
    {
        
    }

    public void ToAttackState()
    {
        
    }

    void ActivateTriggerTile()
    {
        if (ai.destTriggPoint == 0)
        {
            return;
        }

        ai.destTriggPoint = (ai.destTriggPoint + 1) % ai.triggerPoints.Length;


    }
}
