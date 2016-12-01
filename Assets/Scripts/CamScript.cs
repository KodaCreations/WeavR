using UnityEngine;
using System.Collections;

public class CamScript : MonoBehaviour {


    public Transform ship;
    public float distance = 3.0f;
    public float height = 3.0f;
    public float damping = 5.0f;
    public bool smoothRotation = true;
    public float rotationDamping = 10.0f;

    // Use this for initialization
    void Start()
    {

    }
    void FixedUpdate()
    {
        Vector3 wantedPosition = ship.TransformPoint(0, height, -distance);
        transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

        if (smoothRotation)
        {
            Quaternion wantedRotation = Quaternion.LookRotation(ship.position - transform.position, ship.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
        }

        else transform.LookAt(ship, ship.up);
    }
}
