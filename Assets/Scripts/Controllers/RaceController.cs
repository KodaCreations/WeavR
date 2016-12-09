using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RaceController : MonoBehaviour
{

    public Waypoint[] waypoints;
    public bool[][] waypointBooleans;
    public GameObject[] ships;
    GameObject[] originalShips;
    public float[] currentPositions;
    public int[] shipLapCounter;
    public float counter = -1;
    public int nrOfLaps = 3;
    HUD[] huds;
    AudioController audioController;

    // Use this for initialization
    void Start()
    {
        FindAllWaypoints();
        FindAllShips();
        ResetBooleans();
        ActivateShips(false);       
        audioController = GetComponent<AudioController>();
        audioController.LoadFile("Music/Soundtrack1");
    }
    void ActivateShips(bool activate)
    {
        foreach (GameObject s in ships)
        {
            ShipController controller = s.GetComponent<ShipController>();
            if (controller)
                controller.Activate = activate;
        }
    }
    void ActivateAI(bool activate)
    {
        foreach (GameObject s in ships)
        {
            AIController state = s.GetComponent<AIController>();
            if (state)
            {
                state.activateAI = activate;
            }
        }
    }
    void ResetBooleans()
    {
        Array.Resize(ref waypointBooleans, ships.Length);
        for (int i = 0; i < waypointBooleans.Length; ++i)
        {
            Array.Resize(ref waypointBooleans[i], waypoints.Length);
        }
    }
    void FindAllWaypoints()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Waypoint");
        Array.Resize(ref waypoints, objects.Length);
        for (int i = 0; i < objects.Length; ++i)
        {
            string name = objects[i].name;
            name = name.Remove(0, 8);
            int pos = int.Parse(name) - 1;
            waypoints[pos] = objects[i].GetComponent<Waypoint>();
            waypoints[pos].position = pos + 1;
            waypoints[pos].SetRaceController(this);
        }
    }
    void FindAllShips()
    {
        ships = GameObject.FindGameObjectsWithTag("Ship");
        originalShips = ships;

        Array.Resize(ref currentPositions, ships.Length);
        Array.Resize(ref shipLapCounter, ships.Length);
        Array.Resize(ref huds, ships.Length);

        // Really bad way of finding the huds
        List<GameObject> playerShips = new List<GameObject>();
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach (GameObject camera in cameras)
            playerShips.Add(camera.GetComponent<CamScript>().ship.gameObject);
        int cameraCounter = 0;

        for (int i = 0; i < ships.Length; ++i)
        {
            currentPositions[i] = 0;
            shipLapCounter[i] = 0;

            if (playerShips.Contains(ships[i]))
            {
                huds[i] = cameras[cameraCounter].GetComponentInChildren<HUD>();
                cameraCounter++;
            }
        }

    }
    private void ResetLap(int index)
    {
        for (int i = 0; i < waypoints.Length; ++i)
        {
            waypointBooleans[index][i] = false;
        }
    }
    private bool LapIsDone(int index)
    {
        for (int i = 0; i < waypointBooleans[index].Length; ++i)
        {
            if (waypointBooleans[index][i] == false)
                return false;
        }

        // If last lap and ship[index], change cameras target, de-enable ship(ai too) and enable win message panel
        if (shipLapCounter[index] + 1 == nrOfLaps)
        {
            if (huds[index])
            {
                // Doesnt always get here
                huds[index].GetComponentInParent<CamScript>().EnterSpectatorMode();
                huds[index].EnableWinPanel(currentPositions[0]);
            }

            ships[index].SetActive(false);
        }

        ResetLap(index);
        return true;
    }
    public void SetPosition(ShipController ship, int pos)
    {
        for (int i = 0; i < ships.Length; ++i)
        {
            if (ship == ships[i].GetComponent<ShipController>())
            {
                waypointBooleans[i][pos - 1] = true;
                if (pos == 1 && LapIsDone(i))
                    ++shipLapCounter[i];
                currentPositions[i] = pos;
                break;
            }
        }

    }
    void CaclulateShipPositions()
    {
        for (int i = 0; i < ships.Length; ++i)
        {
            int targetWaypoint = (int)currentPositions[i] % waypoints.Length;
            int lastWaypoint = targetWaypoint - 1;
            if (lastWaypoint < 0)
                lastWaypoint = waypoints.Length - 1;
            if (targetWaypoint >= waypoints.Length)
                targetWaypoint = 0;

            //Debug.Log(currentPositions[i]);
            float distanceWaypoints = Vector3.Distance(waypoints[lastWaypoint].transform.position, waypoints[targetWaypoint].transform.position);
            float distanceToShip = Vector3.Distance(waypoints[lastWaypoint].transform.position, ships[i].transform.position);
            float procent = distanceToShip / distanceWaypoints;
            if (targetWaypoint + procent >= waypoints.Length)
            {
                currentPositions[i] = 0;
            }
            else
            {
                currentPositions[i] = targetWaypoint + procent;
            }
            currentPositions[i] += shipLapCounter[i] * waypoints.Length;
        }
    }
    public int GetRacePosition(ShipController ship)
    {
        //find what position the ship is in
        float shipPosition = 0;
        for (int i = 0; i < ships.Length; ++i)
        {
            if (ship == ships[i].GetComponent<ShipController>())
            {
                shipPosition = currentPositions[i];
                break;
            }
        }

        int racePosition = 1;
        //Get how many other ships are in front
        for (int i = 0; i < ships.Length; ++i)
        {
            if (currentPositions[i] > shipPosition)
                ++racePosition;
        }
        return racePosition;
    }
    void PlaceShipsInOrder()
    {
        for (int i = 0; i < ships.Length; ++i)
        {
            for (int j = i - 1; j >= 0; --j)
            {
                if (currentPositions[i] > currentPositions[j])
                {
                    float tempPos = currentPositions[j];
                    currentPositions[j] = currentPositions[i];
                    currentPositions[i] = tempPos;
                    GameObject tempShip = ships[j];
                    ships[j] = ships[i];
                    ships[i] = tempShip;
                    bool[] tempWaypoints = waypointBooleans[j];
                    waypointBooleans[j] = waypointBooleans[i];
                    waypointBooleans[i] = tempWaypoints;
                    int tempLapCounter = shipLapCounter[j];
                    shipLapCounter[j] = shipLapCounter[i];
                    shipLapCounter[i] = tempLapCounter;
                }
            }
        }
    }


    bool CountDown()
    {
        if (counter >= 0)
        {
            counter -= Time.deltaTime;
            if (counter < 0)
            {
                counter = -1;
                return true;
            }
        }
        return false;
    }
    public void StartCountDown(float time)
    {
        ActivateShips(false);

        counter = time;
    }
    // Update is called once per frame
    void Update()
    {
        if (ships.Length != GameObject.FindGameObjectsWithTag("Ship").Length)
            Start();

        CaclulateShipPositions();
        PlaceShipsInOrder();
        if (CountDown())
        {
            ActivateShips(true);
            ActivateAI(true);
            audioController.PlayFile();
        
        }
    }
}
