using UnityEngine;
using System;
using System.Collections;

public class Waypoint : MonoBehaviour {

    public int position;
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
        if(other.tag == "Model")
        {
            raceController.SetPosition(other.GetComponent<ShipController>(), position);
        }
    }
	// Update is called once per frame
	void Update ()
    {

	}
}
