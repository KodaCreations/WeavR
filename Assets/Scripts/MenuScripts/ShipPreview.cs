using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPreview : MonoBehaviour {

    public Vector3 previewScale = new Vector3(1, 1, 1);
    public float rotateSpeed = 2;

    List<GameObject> instantiatedPreviews;      // Preview objects
    List<string> shipNames;                     // Names of the ships
    List<string> shipDescs;                     // Descriptions of the ships
    int previewIndex;                           // Which preview where at

    Brain brain;


    void Awake()
    {
        // Find the brain.
        brain = GameObject.Find("Brain").GetComponent<Brain>();

        instantiatedPreviews = new List<GameObject>();
        shipNames = new List<string>(NamesAndDescs.GetShipNames());
        shipDescs = new List<string>(NamesAndDescs.GetShipDescs());

        // Instantiate new meshes from the available ships in brain.
        foreach (GameObject go in brain.availableShips)
        {
            instantiatedPreviews.Add(new GameObject("Preview " + go.name));
            instantiatedPreviews[instantiatedPreviews.Count - 1].transform.localScale = previewScale;
            instantiatedPreviews[instantiatedPreviews.Count - 1].SetActive(false);
            instantiatedPreviews[instantiatedPreviews.Count - 1].transform.parent = transform;
            instantiatedPreviews[instantiatedPreviews.Count - 1].transform.localPosition = Vector3.zero;

            Instantiate(go.transform.GetChild(0), transform.position, transform.rotation, instantiatedPreviews[instantiatedPreviews.Count - 1].transform);

            // Get the names of the ships from their ship controllers.
            //shipNames.Add(go.GetComponent<ShipController>().shipName);
        }

        // Set the first preview as active.
        instantiatedPreviews[0].SetActive(true);
    }

	void Update () 
    {
        // Rotate the preview
        if (instantiatedPreviews.Count > 0)
            RotatePreview();
	}

    // Switch to next preview, in either direction (1 or -1) and loop around if out of bounds
    public void SwitchPreview(int direction)
    {
        instantiatedPreviews[previewIndex].SetActive(false);

        previewIndex += direction;
        if (previewIndex >= instantiatedPreviews.Count)
            previewIndex = 0;
        if (previewIndex < 0)
            previewIndex = instantiatedPreviews.Count - 1;

        instantiatedPreviews[previewIndex].SetActive(true);
    }

    public void Reset()
    {
        //transform.position = startTransform.position;
        //transform.rotation = startTransform.rotation;

        //foreach (GameObject preview in instantiatedPreviews)
        //    preview.SetActive(false);
        //instantiatedPreviews[0].SetActive(true);
    }

    // Get the name of a ship
    public string GetPreviewName()
    {
        return shipNames[previewIndex];
    }

    public string GetPreviewDesc()
    {
        return shipDescs[previewIndex];
    }

    // Get the name of a prefab
    public string GetShipPrefabName()
    {
        return instantiatedPreviews[previewIndex].name;
    }

    // Rotate a preview object
    void RotatePreview()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
