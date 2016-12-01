using UnityEngine;
using System.Collections;

public class ShipPreviewRotation : MonoBehaviour {

    MeshFilter meshFilter;

	void Start ()
    {
        meshFilter = GetComponent<MeshFilter>();
	}
	
	void Update () 
    {
	    // rotate
	}

    public void UpdateMesh(Mesh mesh)
    {
        meshFilter.sharedMesh = mesh;
    }
}
