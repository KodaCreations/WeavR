using UnityEngine;
using System.Collections;

public class LookForTriggerState : IAiState {
    private readonly ChasingTheRabbit ai;

    public LookForTriggerState(ChasingTheRabbit Ai)
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
        ai.currentState = ai.toLookForWeaponState;
    }

    public void ToAttackState()
    {
        ai.currentState = ai.toAttackState;
    }

    void ActivateTriggerTile()
    {
        if (ai.destTriggPoint == 0)
        {
            return;
        }

        ai.destTriggPoint = (ai.destTriggPoint + 1) % ai.triggerPoints.Length;


        if (!ai.chasingRabbit)
        {
            if (ai.triggerOrWepaon)
            {

                ai.transform.LookAt(ai.triggerPoints[ai.destTriggPoint].transform);
                ai.transform.position += ai.transform.forward * ai.speed * Time.deltaTime;
            }
        }
    }
}
