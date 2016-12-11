using UnityEngine;
using System.Collections;

public class CamScript : MonoBehaviour {

    bool active;

    [Header("In game follow settings")]
    public Transform ship;
    ShipController shipController;
    public float idleDistance = 3.0f;
    public float subtractedDistanceWhenMoving = 1.0f;
    public float idleHeight = 3.0f;
    public float targetHeightOverShip = 2.0f;
    public float addedTargetHeightWhenMoving = 1.0f;
    public float damping = 5.0f;
    public bool smoothRotation = true;
    public bool followBehind = true;
    public float rotationDamping = 10.0f;

    Vector3 wantedPosition;

    GameObject[] allShips;

    [Header("Introduction spline settings")]
    public BezierSpline camSpline;
    public BezierSpline camLookAtSpline;
    [Tooltip("1 means 1 second to go through all splines, 0.33 means 3, 0.16 means 6")]
    float camSplineSpeed = 1;
    int camSplinesCount;
    float splineTime;
    bool onSpline = true;

    // Use this for initialization
    void Start()
    {
        allShips = GameObject.FindGameObjectsWithTag("Ship");
        shipController = ship.GetComponent<ShipController>();
    }

    public void StartIntro(float length)
    {
        active = true;

        camSplineSpeed = length;

        camSplinesCount = 1;
        BezierSpline spline = camSpline;
        while (spline.parent != null)
        {
            spline = spline.parent;
            camSplinesCount++;
        }

        camSplineSpeed = camSplineSpeed * camSplinesCount;
    }

    void Update()
    {
        if (active && onSpline)
            UpdateSplinePos();
    }

    void UpdateSplinePos()
    {
        transform.position = camSpline.GetPointCam(splineTime, ref onSpline);
        transform.LookAt(camLookAtSpline.GetPointCam(splineTime, ref onSpline));
        splineTime += camSplineSpeed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!onSpline)
        {
            float speedPercentage = Mathf.Clamp(shipController.CurrentForwardAccelerationForce / shipController.maxForwardAccelerationSpeed, 0, 1);

            if (followBehind)
                wantedPosition = ship.TransformPoint(0, idleHeight, -idleDistance + subtractedDistanceWhenMoving * speedPercentage);
            else
                wantedPosition = ship.TransformPoint(0, idleHeight, idleDistance - subtractedDistanceWhenMoving * speedPercentage);

            transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

            if (smoothRotation)
            {
                Quaternion wantedRotation = Quaternion.LookRotation(ship.position - (transform.position - new Vector3(0, targetHeightOverShip + addedTargetHeightWhenMoving * speedPercentage, 0)), ship.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.fixedDeltaTime * rotationDamping);
            }
            else transform.LookAt(ship.position + new Vector3(0, targetHeightOverShip + addedTargetHeightWhenMoving * speedPercentage, 0), ship.up);
        }
    }

    // Function to leave transform and move to spectator mode (lacking specific features, such as changing spectator target)
    public void EnterSpectatorMode()
    {
        foreach (GameObject go in allShips)
            if (go)
                ship = go.transform;
    }
}
