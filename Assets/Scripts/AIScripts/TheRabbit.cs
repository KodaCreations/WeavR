using UnityEngine;
using System.Collections;

public class TheRabbit : MonoBehaviour
{

    public Transform[] points;
    private int destPoint = 0;
    public NavMeshAgent navAgent;
    public GameObject AI;
    public float maxDist = 2000;
    public float minDist = 200;

    // Use this for initialization
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        GoToNextPoint();


    }
    void GoToNextPoint()
    {
      

            if (points.Length == 0)
            {
                return;

            }

            navAgent.destination = points[destPoint].position;

            destPoint = (destPoint + 1) % points.Length;
        


    }

    void Update()
    {
        if (navAgent.remainingDistance < 20.0f)
        {
            GoToNextPoint();
        }
        //WaitforChaser();
        //SpeedUp();
        //ReSpawnAtAI();

    }
    void WaitforChaser()
    {
        
       

        if ( Vector3.Distance(AI.transform.position, transform.position) > maxDist)
        {
            navAgent.speed = 0;
        }
        else if (Vector3.Distance(AI.transform.position, transform.position) < maxDist)
        {

            navAgent.speed = 250;
        }
       
    }

    void SpeedUp()
    {
        if (Vector3.Distance(AI.transform.position, transform.position) < minDist)
        {
            navAgent.speed = 700;
        }
        else
            navAgent.speed = 250;
    }

    // Update is called once per frame

    void ReSpawnAtAI()
    {
        if (Vector3.Distance(transform.position, AI.transform.position) > 50)
        {
            transform.position = AI.transform.position;

        }
    }


}
