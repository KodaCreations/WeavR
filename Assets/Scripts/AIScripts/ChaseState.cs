using UnityEngine;
using System.Collections;

public class ChaseState : IAiState
{

    private readonly AIController ai;

    public ChaseState(AIController Ai)
    {
        ai = Ai;
    }

    // Use this for initialization
    public void UpdateState()
    {
        if (ai.ship)
        {
            ChaseTheRabbit();

        }
        //ToLookForTrigger();
    }

    public void ToLookForTrigger()
    {
        //if (Vector3.Distance(ai.cube.transform.position, ai.transform.position) < 200 && !ai.cube.triggered)
        //{
        //    ai.currentState = ai.lookForTriggerState;
        //}
    }

    public void ToLookForWeapon()
    {

    }

    public void ToAttackState()
    {

    }

    public void ToChaseState()
    {
        Debug.Log("Can't transition to same state");
    }


    void ChaseTheRabbit()
    {
        ai.ship.AccelerationForce = 0;
        ai.ship.SteeringForce = 0;

        //Debug.Log(accelerationForce + "");
        Vector3 targetDir = ai.rabbit.transform.position + ai.offset - ai.transform.position;
        targetDir.Normalize();
        float dir = ai.AngleDir(ai.transform.forward, -targetDir, ai.transform.up);

        if (Vector3.Distance(ai.rabbit.transform.position, ai.transform.position) > 1)
        {
            ai.ship.AccelerationForce = 1;
        }

        if (dir > 0.0f)
        {
            ai.ship.SteeringForce = -1 * ai.ship.rotationSpeed;
        }
        else if (dir < 0.0f)
        {
            ai.ship.SteeringForce = 1 * ai.ship.rotationSpeed;
        }
    }
}
