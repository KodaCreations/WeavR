using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPreview : MonoBehaviour {

    List<GameObject> instantiatedPreviews;
    int previewIndex;

    public Vector3 previewScale = new Vector3(1, 1, 1);
    public float rotateSpeed = 2;

    Brain brain;
	
    void Awake()
    {
        brain = GameObject.Find("Brain").GetComponent<Brain>();

        instantiatedPreviews = new List<GameObject>();

        foreach (GameObject go in brain.availableShips)
        {
            instantiatedPreviews.Add(new GameObject("Preview " + go.name));
            instantiatedPreviews[instantiatedPreviews.Count - 1].transform.localScale = previewScale;
            instantiatedPreviews[instantiatedPreviews.Count - 1].SetActive(false);
            instantiatedPreviews[instantiatedPreviews.Count - 1].transform.parent = transform;
            instantiatedPreviews[instantiatedPreviews.Count - 1].transform.localPosition = Vector3.zero;

            Instantiate(go.transform.GetChild(0), transform.position, transform.rotation, instantiatedPreviews[instantiatedPreviews.Count - 1].transform);
        }

        instantiatedPreviews[0].SetActive(true);
    }

	void Update () 
    {
        if (instantiatedPreviews.Count > 0)
            RotatePreview();
	}

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

    public string GetPreviewName()
    {
        return instantiatedPreviews[previewIndex].name;
    }

    void RotatePreview()
    {
        instantiatedPreviews[previewIndex].transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
