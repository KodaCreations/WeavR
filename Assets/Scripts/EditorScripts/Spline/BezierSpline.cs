using UnityEngine;
using System.Collections;
using System;

public class BezierSpline : MonoBehaviour {

    [SerializeField]
    private Vector3[] points;

    [SerializeField]
    private float[] angles;

    [SerializeField]
    private BezierControlPointMode[] modes;

    [SerializeField]
    private bool loop;

    [SerializeField]
    public GameObject floorSegment;
    [SerializeField]
    public GameObject wallSegment;
    [SerializeField]
    public GameObject colliderSegment;
    [SerializeField]
    public GameObject waypoint;
    [SerializeField]
    public float waypointDistance = 100;

    [SerializeField]
    public float detail = 100;

    public BezierSpline parent;
    public BezierSpline child;
    public TrackEditor trackEditor;

    public bool Loop
    {
        get { return loop; }
        set
        {
            loop = value;
            if (value == true)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }                
        }
    }
    public enum BezierControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }
    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }
    public void EnforceParent()
    {
        if(parent)
        {
            //Set last point to the firstpoint in Parent
            Vector3 point = points[points.Length - 1];
            Vector3 parentPoint = parent.points[0];
            points[points.Length - 1] = parent.transform.TransformPoint(parentPoint) - transform.position;

            //Set the handle to mirror the parents first handle
            Vector3 handlePoint = points[points.Length - 2];
            Vector3 parentHandlePoint = parent.points[1];
            Vector3 offset = parentPoint - parentHandlePoint;

            points[points.Length - 2] = points[points.Length - 1] + offset;

            //Set the Angle to Match the parent
            angles[angles.Length - 1] = parent.angles[0];
        }
    }
    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if(loop)
        {
            if(modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if(modeIndex == modes.Length - 1)
            {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }
    public void SetControlPoint(int index, Vector3 point)
    {
        if(index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if(loop)
            {
                if(index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if(index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }
        points[index] = point;
        EnforceMode(index);
    }
    public void SetControlRotation(int index, float angle)
    {
        angles[index] = angle;
    }
    public Quaternion GetRotationAt(float t)
    {
        if (t > CurveCount)
        {
            if (parent != null)
            {
                float time = t - CurveCount;
                return parent.GetRotationAt(time);
            }
        }
        
        int i = (int)t * 3;
        
        float procent = t - (int)t;

        float angle = (angles[i] * (1 - procent)) + (angles[i + 3] * procent);

        Vector3 currentRotation = GetDirection2(t);
        Quaternion rotation = Quaternion.LookRotation(currentRotation) * Quaternion.Euler(0, 0, angle);
        //Quaternion rotation = new Quaternion(currentRotation.x, currentRotation.y, currentRotation.z, 0);
        return rotation;
    }
    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        if(mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
            return;

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if(index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if(fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if(enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if(fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if(enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }
        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;

    }
    public void BuildMesh()
    {
        if(wallSegment)
            trackEditor.BuildTheWall(wallSegment, this, "Wall", "Ignore Raycast", "WallTest", detail);
        
        if(floorSegment)
            trackEditor.BuildTheWall(floorSegment, this, "Untagged", "Only Raycast", "FloorTest", detail);
        
        if(colliderSegment)
            trackEditor.BuildTheWall(colliderSegment, this, "Wall", "Ignore Raycast", "ColliderTest", detail);
    }
    public int ControlPointCount
    {
        get { return points.Length; }
    }
    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }
    public float GetControlRotation(int index)
    {
        return angles[index];
    }
    public Vector3 GetPoint(float t)
    {
        int i;
        if(t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }
    public Vector3 GetPointConstantSpeed(ref float t, float length, float detail)
    {
        if (t > CurveCount)
        {
            if (parent != null)
            {
                float time = t - CurveCount;
                Vector3 newPos = parent.GetPointConstantSpeed(ref time, length, detail);
                t = time + CurveCount;
                return newPos;
            }
        }
        Vector3 pos = new Vector3();
        for(int i = 0; i < detail; ++i)
        {
            int j = (int)t * 3;
            //Brytt upp T och GetPoint i olika funtioner
            if(t > CurveCount - 0.001f)
            {
                if (parent != null)
                {
                    float time = t - CurveCount;
                    Vector3 newPos = parent.GetPointConstantSpeed(ref time, length - length  * (i / detail), detail);
                    t = time + CurveCount;
                    return newPos;
                }
                t = CurveCount - 0.001f;
                return GetPoint(1);
            }
            pos = transform.TransformPoint(Bezier.GetPointConstantSpeed(points[j], points[j + 1], points[j + 2], points[j + 3], ref t, length / detail));
        }
        return pos;
    }
    public int CurveCount
    {
        get { return (points.Length - 1) / 3; }
    }
    public int CurveCountAll
    {
        get
        {
            if (parent)
                return parent.CurveCountAll + (points.Length - 1) / 3;

            return (points.Length - 1) / 3;
        }
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }
    public Vector3 GetVelocity2(float t)
    {
        float time = t % 1;
        int i = (int)t * 3;
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], time)) - transform.position;
    }
    public Vector3 GetDirection2(float t)
    {
        if (t > CurveCount)
        {
            if (parent != null)
            {
                float time = t - CurveCount;
                return parent.GetDirection2(time);
            }
        }
        return GetVelocity2(t).normalized;
    }
    public void AddCurve()
    {
        //Resize the array to fit the new Points
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        Array.Resize(ref angles, angles.Length + 3);

        Vector3 direction = points[points.Length - 4] - points[points.Length - 5];
        point += 1 * direction;
        points[points.Length - 3] = point;
        point += 1 * direction;
        points[points.Length - 2] = point;
        point += 1 * direction;
        points[points.Length - 1] = point;

        angles[angles.Length - 3] = 0;
        angles[angles.Length - 2] = 0;
        angles[angles.Length - 1] = 0;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);
        if(loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }
    public void SplitSplineAt(int index, GameObject newGameObject)
    {
        //Create a new Spline With nodes higher than index
        int totalNewPoints = ControlPointCount - index;
        newGameObject.AddComponent<BezierSpline>();
        BezierSpline newSpline = newGameObject.GetComponent<BezierSpline>();
        Array.Resize(ref newSpline.points, totalNewPoints);
        Array.Resize(ref newSpline.angles, totalNewPoints);
        for (int i = 0; i < totalNewPoints; ++i)
        {
            newSpline.points[i] = points[index + i];
            newSpline.angles[i] = angles[index + i];
        }

        //Don't forget the modes!
        int totalNewModes = totalNewPoints / 3 + 1;
        Array.Resize(ref newSpline.modes, totalNewModes);
        for (int i = 0; i < totalNewModes; ++i)
        {
            newSpline.modes[i] = BezierControlPointMode.Mirrored;
        }

        //if there is a parent then place it on the new spline
        if (parent)
            newSpline.parent = parent;

        //Cut out the points from the spline and set parent
        Array.Resize(ref modes, (points.Length - totalNewPoints) / 3 + 1);
        Array.Resize(ref points, index + 1);
        Array.Resize(ref angles, index + 1);
        parent = newSpline;
    }
    public void MergeWithParent()
    {
        //Resize the array to fit the new Points With the parents nodes
        int currentNode = points.Length;
        Array.Resize(ref points, points.Length + parent.points.Length - 1);
        Array.Resize(ref angles, angles.Length + parent.angles.Length - 1);
        for(int i = 1; i < parent.points.Length; ++i)
        {
            points[points.Length - parent.points.Length + i] = parent.transform.TransformPoint(parent.points[i]) - transform.position;
            angles[angles.Length - parent.angles.Length + i] = parent.angles[i];
        }

        //Resize the array for modes
        Array.Resize(ref modes, modes.Length + parent.modes.Length - 1);
        for(int i = 1; i < parent.modes.Length; ++i)
        {
            modes[modes.Length - parent.modes.Length + i] = parent.modes[i];
        }
        parent = parent.parent;
    }

    public void Reset()
    {
        gameObject.AddComponent<TrackEditor>();
        trackEditor = gameObject.GetComponent<TrackEditor>();
        //trackEditor.Initialize();
        points = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(30f, 0f, 0f),
            new Vector3(60f, 0f, 0f),
            new Vector3(90f, 0f, 0f)
        };

        angles = new float[]
        {
            0,
            0,
            0,
            0
        };

        modes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Mirrored,
            BezierControlPointMode.Mirrored
        };
    }
    public void DeleteAllWaypoints()
    {
        trackEditor.DeleteWaypoints();
    }
    public void CreateWaypoints()
    {
        trackEditor.CreateWaypointsOnSpline(this, waypoint, waypointDistance);
    }
}
