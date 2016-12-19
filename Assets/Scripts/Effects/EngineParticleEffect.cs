using UnityEngine;
using System.Collections;

public class EngineParticleEffect : MonoBehaviour {

    public Transform shipTransform;
    public float offsetX;
    public float offsetY;
    public float offsetZ;
    private Vector3 offsetValues;

	// Use this for initialization
	void Start () {
        offsetValues = new Vector3(offsetX, offsetY, offsetZ);
	}
	
	// Update is called once per frame
	void Update () {
        transform.localPosition = shipTransform.localPosition + offsetValues;
	}
}
