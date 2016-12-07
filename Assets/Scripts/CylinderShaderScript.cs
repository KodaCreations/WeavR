using UnityEngine;
using System.Collections;

public class CylinderShaderScript : MonoBehaviour {
    public GameObject ship;
	// Use this for initialization
	void Start () {
        //ship = GameObject.Find("PhysicsTestShip(Clone)");
        //ship.GetComponent<Renderer>().material.SetVector("Ship Pos", new Vector4(ship.transform.position.x, ship.transform.position.y, ship.transform.position.z, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (!ship)
        {
            GameObject[] ships = GameObject.FindGameObjectsWithTag("Ship");// ("PlaceholderShipPrefab(Clone)");
            foreach (GameObject s in ships)
            {
                if (s.GetComponent<ShipNetworkController>())
                {
                    if (s.GetComponent<ShipNetworkController>().isLocalPlayer)
                    {
                        ship = s;
                    }
                }
                else
                {
                    ship = s;
                }
            }
        }
        if (ship)
        {
            ShipController controller = ship.GetComponent<ShipController>();
            if(controller)
            {
                //ship.GetComponent<Renderer>().material.SetVector("_ShipPos", new Vector4(ship.transform.position.x, ship.transform.position.y, ship.transform.position.z, 0));
                if (controller.FlightMode)
                {
                    GetComponent<MeshCollider>().enabled = true;
                    GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    GetComponent<MeshCollider>().enabled = false;
                    GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }
}
