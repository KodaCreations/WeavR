using UnityEngine;
using System.Collections;

public class ChaseState : IAiState {

    private readonly AIShipBaseState ai;

    public ChaseState(AIShipBaseState Ai)
    {
        ai = Ai;
    }

    // Use this for initialization
    public void UpdateState()
    {
        ChaseTheRabbit();
        //ToLookForTrigger();
    }

    public void ToLookForTrigger()
    {
        if (Vector3.Distance(ai.cube.transform.position, ai.transform.position) < 200 && !ai.cube.triggered)
        {
            ai.currentState = ai.lookForTriggerState;
        }
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

        ai.accelerationForce = 0;
        ai.steeringForce = 0;

        //Debug.Log(accelerationForce + "");
        Vector3 targetDir = ai.rabbit.transform.position - ai.transform.position;
        targetDir.Normalize();
        float dir = ai.AngleDir(ai.transform.forward, -targetDir, ai.transform.up);



        if (Vector3.Distance(ai.rabbit.transform.position, ai.transform.position) > 10)
        {
            ai.accelerationForce = 1 * ai.aiMovementSpeed;
        }

        if (dir > 0.0f)
        {
            ai.steeringForce = -1 * ai.aiRotationSpeed;
        }
        else if (dir < 0.0f)
        {
            ai.steeringForce = 1 * ai.aiRotationSpeed;
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
