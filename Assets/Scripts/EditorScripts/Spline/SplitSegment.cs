using UnityEngine;
using System.Collections;
using System;

public class SplitSegment : MonoBehaviour {

    [SerializeField]
    private Vector3[] points;
    [SerializeField]
    public BezierSpline[] splines;

    [SerializeField]
    private bool[] attachToBack;




    // Use this for initialization
    void Start () {
	
	}
    public int GetTotalPoints()
    {
        return points.Length;
    }
    public Vector3 GetPoint(int index)
    {
        return points[index];
    }
    public BezierSpline GetSpline(int index)
    {
        return splines[index];
    }
    public bool GetAttachToBack(int index)
    {
        return attachToBack[index];
    }
    public void SetSpline(int index, BezierSpline spline)
    {
        //set the spline
        if (spline)
        {
            splines[index] = spline;

            //move The splines point to right pos
            if (attachToBack[index])
            {
                Vector3 pos = transform.TransformPoint(points[index]) - spline.transform.position;
                spline.SetControlPoint(0, pos);
            }
            else
            {
                Vector3 pos = transform.TransformPoint(points[index]) - spline.transform.position;
                spline.SetControlPoint(spline.ControlPointCount - 1, pos);
            }
        }
    }
    public void SetPoint(int index, Vector3 newPoint)
    {
        points[index] = newPoint;
    }
    public void SetAttachToBack(int index, bool attachToBack)
    {
        this.attachToBack[index] = attachToBack;
    }
    public void AddPoint()
    {
        Array.Resize(ref points, points.Length + 1);
        points[points.Length - 1] = new Vector3(5, 0, 0);
        Array.Resize(ref splines, splines.Length + 1);
        splines[splines.Length - 1] = null;
        Array.Resize(ref attachToBack, attachToBack.Length + 1);
        attachToBack[attachToBack.Length - 1] = true;
    }
    void Reset()
    {
        Array.Resize(ref points, 2);
        points[0] = new Vector3(5, 0, 0);
        points[1] = new Vector3(10, 0, 0);

        Array.Resize(ref splines, 2);
        splines[0] = null;
        splines[1] = null;

        Array.Resize(ref attachToBack, 2);
        attachToBack[0] = false;
        attachToBack[1] = true;
    }
	// Update is called once per frame
	void Update () {
	
	}
}
