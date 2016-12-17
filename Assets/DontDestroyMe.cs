using UnityEngine;
using System.Collections;

public class DontDestroyMe : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        // Keep this object through scenes.
        DontDestroyOnLoad(transform.gameObject);
	}
	
}
