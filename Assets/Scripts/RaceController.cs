using UnityEngine;
using System;
using System.Collections;

public class RaceController : MonoBehaviour {

    public Waypoint[] waypoints;
    public GameObject[] ships;
    float[] currentPositions; 
	// Use this for initialization
	void Start ()
    {
        FindAllWaypoints();
        FindAllShips();
        foreach(Waypoint w in waypoints)
        {
            Debug.Log(w.name);
        }
	}
	void FindAllWaypoints()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Waypoint");
        Array.Resize(ref waypoints, objects.Length);
        for (int i = 0; i < objects.Length; ++i)
        {
            waypoints[i] = objects[objects.Length - 1 - i].GetComponent<Waypoint>();
            waypoints[i].SetRaceController(this);
        }
    }
    void FindAllShips()
    {
        ships = GameObject.FindGameObjectsWithTag("Ship");
        Array.Resize(ref currentPositions, ships.Length);
        for(int i = 0; i < ships.Length; ++i)
        {
            currentPositions[i] = 1;
        }
    }
    void CaclulateShipPositions()
    {
        for(int i = 0; i < ships.Length; ++i)
        {
            int targetWaypoint = (int)currentPositions[i];
            int lastWaypoint = targetWaypoint - 1;
            if (lastWaypoint < 0)
                lastWaypoint = waypoints.Length - 1;

            Debug.Log(currentPositions[i]);
            float distanceWaypoints = Vector3.Distance(waypoints[lastWaypoint].transform.position, waypoints[targetWaypoint].transform.position);
            float distanceToShip = Vector3.Distance(waypoints[targetWaypoint].transform.position, ships[i].transform.position);
            float procent = distanceToShip / distanceWaypoints;
            if(targetWaypoint + procent >= waypoints.Length)
            {
                currentPositions[i] = 0;
            }
            else
            {
                currentPositions[i] = targetWaypoint + procent;
            }
        }
    }
    public int GetRacePosition(ShipController ship)
    {
        //find what position the ship is in
        float shipPosition = 0;
        for (int i = 0; i < ships.Length; ++i)
        {
            if(ship == ships[i])
            {
                shipPosition = currentPositions[i];
            }
        }

        int racePosition = 1;
        //Get how many other ships are in front
        for(int i = 0; i < ships.Length; ++i)
        {
            if (currentPositions[i] > shipPosition)
                ++racePosition;
        }
        return racePosition;
    }
	// Update is called once per frame
	void Update ()
    {
        CaclulateShipPositions();   
	}
}
