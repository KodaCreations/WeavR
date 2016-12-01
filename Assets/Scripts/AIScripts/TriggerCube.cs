using UnityEngine;
using System.Collections;

public class TriggerCube : MonoBehaviour {


    public bool triggered = false;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(triggered + "");
   
	
	}

    void OnTriggerEnter(Collider other)
    {
        triggered = true;
    }
}
