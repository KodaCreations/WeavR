using UnityEngine;
using System;
using System.Collections;

public class Waypoint : MonoBehaviour {

    GameObject[] ships;
    RaceController raceController;
	// Use this for initialization
	void Start ()
    {

	}
    public void SetRaceController(RaceController controller)
    {
        raceController = controller;
    }

	// Update is called once per frame
	void Update ()
    {

	}
}
