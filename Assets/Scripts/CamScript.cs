﻿using UnityEngine;
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

    public int startFOV = 60;
    public int maxFOV = 80;
    private int rangeFOV;

    Vector3 wantedPosition;

    GameObject[] allShips;

    [Header("In game rotate settings")]
    public float rotateSpeed = 3;
    public float rotateHeight = 7;
    public float rotateDistance = 2;
    private Vector3 rotateOffset;

    [Header("Introduction spline settings")]
    public BezierSpline camSpline;
    public BezierSpline camLookAtSpline;
    [Tooltip("1 means 1 second to go through all splines, 0.33 means 3, 0.16 means 6")]
    float camSplineSpeed = 1;
    int camSplinesCount;
    float splineTime;
    bool onSpline = true;
    bool rotate = false;

    ParticleSystem speedLines;
    ParticleSystem.EmissionModule speedLinesEmit;
    Camera myCamera;


    // Use this for initialization
    void Start()
    {
        allShips = GameObject.FindGameObjectsWithTag("Ship");
        shipController = ship.GetComponent<ShipController>();

        rangeFOV = maxFOV - startFOV;

        speedLines = GetComponentInChildren<ParticleSystem>();
        //speedLinesEmit = speedLines.emission;
        myCamera = GetComponent<Camera>();

        rotateOffset = new Vector3(0, ship.position.y + rotateHeight, 0);
        rotateOffset -= ship.transform.right * rotateDistance;
    }
    public void StartNetworkIntro()
    {
        active = true;
        onSpline = false;
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

    public void StopIntro()
    {
        onSpline = false;
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
        if (!onSpline && !rotate)
        {
            // Calculate the %, current speed / max speed.
            float speedPercentage = Mathf.Clamp(shipController.CurrentForwardAccelerationForce / shipController.maxForwardAccelerationSpeed, 0, 1);

            // Follow from behind or from front
            if (followBehind)
                wantedPosition = ship.TransformPoint(0, idleHeight, -idleDistance + subtractedDistanceWhenMoving * speedPercentage);
            else
                wantedPosition = ship.TransformPoint(0, idleHeight, idleDistance - subtractedDistanceWhenMoving * speedPercentage);

            // Update position
            transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

            // Rotate towards point
            if (smoothRotation)
            {
                Quaternion wantedRotation = Quaternion.LookRotation(ship.position - (transform.position - new Vector3(0, targetHeightOverShip + addedTargetHeightWhenMoving * speedPercentage, 0)), ship.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.fixedDeltaTime * rotationDamping);
            }
            else transform.LookAt(ship.position + new Vector3(0, targetHeightOverShip + addedTargetHeightWhenMoving * speedPercentage, 0), ship.up);

            // Change FOV
            float newFOV = startFOV + rangeFOV * speedPercentage;
            myCamera.fieldOfView = newFOV;

            // Handle particle emmiter
            //if (speedPercentage > 0.7f)
            //{
            //    if (!speedLines.isPlaying)
            //    {
            //        speedLines.Simulate(0, true, true);
            //        speedLinesEmit.enabled = true;
            //        speedLines.Play();
            //    }
            //    //if (speedLines.isStopped)
            //    //    speedLines.Play();
            //}
            //else
            //{
            //    if (speedLines.isPlaying)
            //    {
            //        speedLinesEmit.enabled = false;
            //        speedLines.Stop();
            //    }
            //    //if (!speedLines.isStopped)
            //    //{
            //    //    speedLines.Stop();
            //    //    Debug.Log("STOPPING");
            //    //}
            //}
        }

        if (!onSpline && rotate)
        {
            rotateOffset = Quaternion.AngleAxis(rotateSpeed * Time.fixedDeltaTime, Vector3.up) * rotateOffset;
            transform.position = ship.transform.position + rotateOffset;
            transform.LookAt(ship);
        }
    }

    // Rotate around transform when finished last lap (for singleplayer and splitscreen)
    public void EnterRotateMode()
    {
        //speedLinesEmit.enabled = false;
        rotate = true;
    }

    // Function to leave transform and move to spectator mode (follow other players) (lacking specific features, such as changing spectator target)
    public void EnterSpectatorMode()
    {
        foreach (GameObject go in allShips)
            if (go)
                ship = go.transform;
    }
}
