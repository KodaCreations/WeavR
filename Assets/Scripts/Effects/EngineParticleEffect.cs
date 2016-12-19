using UnityEngine;
using System.Collections;

public class EngineParticleEffect : MonoBehaviour {

    //public Transform sT;
    //public float offsetX;
    //public float offsetY;
    //public float offsetZ;
    //private Vector3 offsetValues;

    public ShipController sC;
    public ParticleSystem pS;

    public float turboStartSpeed;
    public float normalStartSpeed;

	// Use this for initialization
	void Start () {
        //offsetValues = new Vector3(offsetX, offsetY, offsetZ);
	}
	
	// Update is called once per frame
	void Update () {
        //transform.localPosition = sT.localPosition + offsetValues;

        if (sC.CurrentForwardAccelerationForce > 0)
        {
            pS.Play();
            if(sC.Turbo == true)
            {
                pS.startSpeed = turboStartSpeed;
            }
            else
            {
                pS.startSpeed = normalStartSpeed;
            }
        }
        else
        {
            pS.startSpeed = 0.1f;
            //pS.Stop();
        }
            
	}
}
