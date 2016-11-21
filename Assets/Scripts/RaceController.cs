using UnityEngine;
using System;
using System.Collections;

public class RaceController : MonoBehaviour {

    Waypoint[] waypoints;
    GameObject[] ships;
    float[] currentPositions; 
	// Use this for initialization
	void Start ()
    {
        FindAllWaypoints();
        FindAllShips();
	}
	void FindAllWaypoints()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Waypoint");
        Array.Resize(ref waypoints, objects.Length);
        for(int i = 0; i < objects.Length; ++i)
        {
            waypoints[i] = objects[i].GetComponent<Waypoint>();
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
            int lastWayPoint = (int)currentPositions[i];
            float distanceWaypoints = Vector3.Distance(waypoints[i - 1].transform.position, waypoints[i].transform.position);
            float distanceToShip = Vector3.Distance(waypoints[i - 1].transform.position, ships[i].transform.position);
            float procent = distanceToShip / distanceWaypoints;
            currentPositions[i] = lastWayPoint + procent;
        }
    }
    public int GetRacePosition(ShipController ship)
    {
        for (int i = 0; i < ships.Length; ++i)
        {

        }
        return 0;
    }
	// Update is called once per frame
	void Update ()
    {
        CaclulateShipPositions();   
	}
}
