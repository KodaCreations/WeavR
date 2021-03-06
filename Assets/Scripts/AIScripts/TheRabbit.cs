﻿using UnityEngine;
using System.Collections;

public class TheRabbit : MonoBehaviour
{

    public Transform[] points;
    private int destPoint = 0;
    public NavMeshAgent navAgent;
    public EditorPath ePath;
    public GameObject AI;
    public float timer = 3;
    public float maxDist = 2000;
    public float minDist = 200;
    private bool started = false;
    Vector3 offset;

    // Use this for initialization
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        GoToNextPoint();

        for (int i = 0; i < ePath.pathObjs.Count; i++)
        {
            points[i] = ePath.pathObjs[i];
        }


    }
    void GoToNextPoint()
    {

        if (points.Length == 0)
        {
            return;

        }
        offset = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        navAgent.destination = points[destPoint].position + offset;

        destPoint = (destPoint + 1) % points.Length;



    }

    void Update()
    {

        if (navAgent.remainingDistance < 60.0f)
        {
            GoToNextPoint();
        }


        //WaitforChaser();
        //SpeedUp();
        ReSpawnAtAI();

    }

    public bool ActivateRabbit()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            return true;
        }
        return false;
    }
    void WaitforChaser()
    {



        if (Vector3.Distance(AI.transform.position, transform.position) > maxDist)
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
            transform.position = AI.transform.position + AI.transform.forward * 3f;

        }
    }


}
