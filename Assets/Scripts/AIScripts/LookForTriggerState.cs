using UnityEngine;
using System.Collections;

public class LookForTriggerState : IAiState {

    private AIController ai;


    public LookForTriggerState(AIController Ai)
    {
        ai = Ai;
    }
	// Use this for initialization
	public void UpdateState()
    {
        ActivateTriggerTile();
        ToChaseState();
    }

    public void ToLookForTrigger()
    {
        Debug.Log("Can't transition to same state");
    }

    public void ToLookForWeapon()
    {
        ai.currentState = ai.lookForWeaponState;
    }

    public void ToAttackState()
    {
        ai.currentState = ai.attackState;
    }

    public void ToChaseState()
    {
        //if (ai.cube.triggered)
        //{
        //    ai.currentState = ai.chaseState;
        //}
    }

    void ActivateTriggerTile()
    {
    //    Vector3 targetDir = ai.cube.transform.position - ai.transform.position;
    //    targetDir.Normalize();
    //    float dir = ai.AngleDir(ai.transform.forward, -targetDir, ai.transform.up);

    //    if (Vector3.Distance(ai.cube.transform.position, ai.transform.position) > 10)
    //    {
    //        ai.accelerationForce = 1 * ai.aiMovementSpeed;
    //    }

    //    if (dir > 0.0f)
    //    {
    //        ai.steeringForce = -1 * ai.aiRotationSpeed;
    //    }
    //    else if (dir < 0.0f)
    //    {
    //        ai.steeringForce = 1 * ai.aiRotationSpeed;
    //    }

    }

}
