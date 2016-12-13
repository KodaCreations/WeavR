using UnityEngine;
using System;
using System.Collections;

public class Waypoint : MonoBehaviour {

    public int shipPosition;
    RaceController raceController;
	// Use this for initialization
	void Start ()
    {

	}
    public void SetRaceController(RaceController controller)
    {
        raceController = controller;
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ship")
        {
            raceController.SetPosition(other.GetComponent<ShipController>(), shipPosition);
        }
    }
	// Update is called once per frame
	void Update ()
    {

	}
}
