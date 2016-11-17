using UnityEngine;
using System.Collections;

public class CylinderShaderScript : MonoBehaviour {
    GameObject ship;
	// Use this for initialization
	void Start () {
        //ship = GameObject.Find("PhysicsTestShip(Clone)");
        //ship.GetComponent<Renderer>().material.SetVector("Ship Pos", new Vector4(ship.transform.position.x, ship.transform.position.y, ship.transform.position.z, 0));
    }
	
	// Update is called once per frame
	void Update () {
	    if(!ship)
        {
            ship = GameObject.Find("PhysicsTestShip(Clone)");
        }
        if(ship)
        {
            Debug.Log("Ship Found!");
            ship.GetComponent<Renderer>().material.SetVector("_ShipPos", new Vector4(ship.transform.position.x, ship.transform.position.y, ship.transform.position.z, 0));
        }
	}
}
