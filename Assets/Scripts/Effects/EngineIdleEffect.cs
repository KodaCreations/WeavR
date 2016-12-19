using UnityEngine;
using System.Collections;

public class EngineIdleEffect : MonoBehaviour {

    public float frequency;
    public float scaleChangeValue;
    public float scaleRange;
    public Transform shipTransform;
    public float offsetX;
    public float offsetY; 
    public float offsetZ;

    private Vector3 offsetValues;
    private float maxScale;
    private float minScale;
    private float currentScale;
    private float timer;
    private float startScale;

	// Use this for initialization
	void Start () {

        startScale = transform.localScale.x;
        currentScale = startScale;        
        maxScale = currentScale + scaleRange;
        minScale = currentScale - scaleRange;
        timer = frequency;
        offsetValues = new Vector3(offsetX, offsetY, offsetZ);
	}

    void ChangeScale()
    {
        if (currentScale >= startScale)
        {
            if (currentScale >= maxScale)
            {
                currentScale -= scaleChangeValue;
            }
            else
            {
                if (Random.Range(1, 3) != 1)
                {
                    currentScale -= scaleChangeValue;
                }
                else
                {
                    currentScale += scaleChangeValue;
                }
            }
        }
        else
        {

            if (currentScale <= minScale)
            {
                currentScale += scaleChangeValue;
            }
            else
            {
                if (Random.Range(1, 3) != 1)
                {
                    currentScale += scaleChangeValue;
                }
                else
                {
                    currentScale -= scaleChangeValue;
                }
            }
        }

        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }
    
	
	// Update is called once per frame
	void Update () {

        transform.localPosition = shipTransform.localPosition + offsetValues;

        if(timer <= 0)
        {
            ChangeScale();
            timer = frequency;
        }
        timer -= Time.deltaTime;

	}
}
