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
        ai.ship.DownwardForce = 0;
        ai.ship.shielded = false;
        ai.ship.Turbo = false;

        //Debug.Log(accelerationForce + "");
        Vector3 targetDir = ai.rabbit.transform.position - ai.transform.position;
        targetDir.Normalize();
        float dir = ai.AngleDir(ai.transform.forward, -targetDir, ai.transform.up);

        if (Vector3.Distance(ai.rabbit.transform.position, ai.transform.position) > 10)
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
    //public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    //{
    //    Vector3 perp = Vector3.Cross(fwd, targetDir);
    //    float dir = Vector3.Dot(perp, up);

    //    if (dir > 0.023)
    //    {
    //        return 1.0f;
    //    }
    //    else if (dir < -0.023)
    //    {
    //        return -1.0f;
    //    }
    //    else {
    //        return 0.0f;
    //    }
    //}
}
