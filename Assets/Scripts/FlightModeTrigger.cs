using UnityEngine;
using System.Collections;

public class FlightModeTrigger : MonoBehaviour {
    
    public bool ActivateFlightMode;
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ship")
        {
            ShipController controller = other.gameObject.GetComponent<ShipController>();
            if(ActivateFlightMode)
            {
                controller.EnableFlightMode();
            }
            else
            {
                controller.DisableFlightMode();
            }
        }
    }
}
