using UnityEngine;
using System.Collections;

public class TheRabbit : MonoBehaviour
{

    public Transform[] points;
    private int destPoint = 0;
    public NavMeshAgent navAgent;
    public GameObject AI;
    public int maxDist = 30;
    private bool active;

    // Use this for initialization
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        GoToNextPoint();


    }
    void GoToNextPoint()
    {
        if (active)
        {
            if (points.Length == 0)
            {
                return;

            }

            navAgent.destination = points[destPoint].position;

            destPoint = (destPoint + 1) % points.Length;

        }



    }

    void WaitforChaser()
    {
      
        if (Vector3.Distance(AI.transform.position, transform.position) <= maxDist)
        {
            active = true;
        }
        else
        {
            active = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (navAgent.remainingDistance < 20.0f)
        {
            GoToNextPoint();
        }
        WaitforChaser();
        //ReSpawnAtAI();

    }

    void ReSpawnAtAI()
    {
        if (Vector3.Distance(transform.position, AI.transform.position) > 50)
        {
            transform.position = AI.transform.position;

        }
    }

    void GoFaster()
    {

    }
}
