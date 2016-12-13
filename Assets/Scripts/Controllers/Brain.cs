using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Brain : MonoBehaviour {

    public int nrOfLaps;

    public float countDownTimer = 5;            // Timer to start race

    public float cameraIntroLength;             // Length in seconds for the cameras to do their introduction on splines
    List<CamScript> cameraReferences;           // References to cameras in scene

    public float winFlashTimer = 1;             // Time that screen flashes when in goal

    public bool isSplitscreen;                  // Is the game in splitscreen mode?
    public bool isMultiplayer;                  // Is the game in online mode?
    public bool player1UsingGamepad;            // Is player 1 using gamepad?
    public bool player2UsingGamepad;            // Is player 2 using gamepad?

    [HideInInspector]
    public string selectedTrack;                // Track that has been selected by the player

    public List<string> loadableTrackNames;     // Names of all loadable tracks, need to match scene names
    public List<GameObject> availableShips;     // All available ship prefabs
    public List<GameObject> availableAIShips;   // All available ai ship prefabs

    public GameObject rabbit;                   // Ai rabbit prefab

    Transform startArea;                        // Start area transform, automatically searched for start tag
    Transform[] startPositions;                 // 8 transforms that are children of startArea.
    GameObject waypoints;                       // AI waypoints

    public List<GameObject> playerShips;        // Ships chosen by the player

    AudioController audioController;            // Audio controller

    List<HUD> HUDs;                             // All huds in the scene

	void Start () 
    {
        // Keep this object through scenes.
        DontDestroyOnLoad(transform.gameObject);

        // If there are no loadable tracks/scenes added to brain, throw an error and stop the code.
        if (loadableTrackNames.Count == 0)
        {
            Debug.LogError("No loadable tracks added to brain!");
            return;
        }

        startPositions = new Transform[8];
        playerShips = new List<GameObject>();
        cameraReferences = new List<CamScript>();
        HUDs = new List<HUD>();

        audioController = GameObject.Find("AudioController").GetComponent<AudioController>();
    }
	
	void Update () 
    {
	
	}

    // Called by race controller when a player crosses the finish line
    public void PlayerFinished(Transform playerShip, int playerIndex, int playerPosition)
    {
        // Enable the effects and text for winning
        HUDs[playerIndex].EnableWinPanel(playerPosition, winFlashTimer);
        HUDs[playerIndex].GetComponentInParent<CamScript>().EnterRotateMode();

        SpawnAIFromPlayer(playerShip);

        // Make a new AI take over the ship

        //    huds[index].GetComponentInParent<CamScript>().EnterSpectatorMode(); spectator mode for multiplayer only
    }

    // Adds the player's chosen ship to the list, called from menusScript.
    public void AddSelectedShip(string name)
    {
        foreach (GameObject go in availableShips)
        {
            if (go.name == name)
                playerShips.Add(go);
        }
    }

    // Read start positions from prefab.
    void ReadStartPositions()
    {
        startArea = GameObject.FindGameObjectWithTag("Start").transform;

        for (int i = 0; i < startArea.childCount; i++)
            startPositions[i] = startArea.GetChild(i);
    }
    void SpawnShipsNetwork()
    {
        if(false)
        {
            for (int i = 0; i < playerShips.Count; i++)
            {
                GameObject player = (GameObject)Instantiate(playerShips[i], startPositions[7 - i].position, startPositions[7 - i].rotation);
                //NetworkIdentity id = GetComponent<NetworkIdentity>();
                //Debug.Log(id.observers.Count);

                //NetworkServer.SpawnWithClientAuthority(player, id.observers[i]);
                // Set the input schemes of the players.
                InputHandler IH = player.GetComponent<InputHandler>();
                KeyCode[] scheme;
                if (i == 0)
                {
                    scheme = ControlSchemes.GetScheme1();
                    if (player1UsingGamepad)
                        IH.usingGamepad = true;
                }
                else
                {
                    scheme = ControlSchemes.GetScheme2();
                    if (player2UsingGamepad)
                        IH.usingGamepad = true;
                }
                IH.SetKeys(scheme[0], scheme[1], scheme[2], scheme[3], scheme[4], scheme[5]);

                // Remove from available ships to let AI choose from remainding ones.
                if (availableShips.Count > 1)
                    availableShips.Remove(playerShips[i]);
            }
            NetworkServer.SpawnObjects();
        }
    }
    // Spawn all ships at start positions.
    void SpawnShips()
    {
        for (int i = 0; i < playerShips.Count; i++)
        {
            GameObject player = (GameObject)Instantiate(playerShips[i], startPositions[7 - i].position, startPositions[7 - i].rotation);
            player.name = "PlayerShip number: " + i;
            // Set the input schemes of the players.
            InputHandler IH = player.GetComponent<InputHandler>();
            KeyCode[] scheme;
            if (i == 0)
            {
                scheme = ControlSchemes.GetScheme1();
                if (player1UsingGamepad)              
                    IH.UseGamepad(0);
            }
            else
            {
                scheme = ControlSchemes.GetScheme2();
                if (player2UsingGamepad)
                    IH.UseGamepad(1);
            }
            IH.SetKeys(scheme[0], scheme[1], scheme[2], scheme[3], scheme[4], scheme[5]);

            // Remove from available ships to let AI choose from remainding ones.
            if (availableShips.Count > 1)
                availableShips.Remove(playerShips[i]);
        }

        // Spawn AI Ships
        for (int i = 7 - playerShips.Count; i >= 0; i--)
        {
            int random = UnityEngine.Random.Range(0, availableAIShips.Count - 1);

            GameObject aiShip = (GameObject)Instantiate(availableAIShips[random], startPositions[i].position, startPositions[i].rotation);
            aiShip.name = "AIShip number " + i; 
            GameObject rabbitObject = (GameObject)Instantiate(rabbit, startPositions[i].position, startPositions[i].rotation);
            TheRabbit theRabbit = rabbitObject.GetComponent<TheRabbit>();

            aiShip.GetComponent<AIController>().rabbit = theRabbit;
            theRabbit.AI = aiShip;

            theRabbit.ePath = waypoints.GetComponent<EditorPath>();
            Array.Resize(ref theRabbit.points, waypoints.transform.childCount);
            theRabbit.points[0] = waypoints.transform.GetChild(0);

            if (availableAIShips.Count > 1)
                availableAIShips.Remove(availableAIShips[random]);
        }

        // MP ships?
    }

    private void SpawnAIFromPlayer(Transform playerShip)
    {
        //Disable input
        playerShip.GetComponent<InputHandler>().enabled = false;

        GameObject rabbitObject = (GameObject)Instantiate(rabbit, playerShip.position, playerShip.rotation);
        TheRabbit theRabbit = rabbitObject.GetComponent<TheRabbit>();

        AIController newAI = playerShip.gameObject.AddComponent<AIController>();
        newAI.ship = playerShip.GetComponent<ShipController>();
        newAI.rabbit = theRabbit;

        theRabbit.AI = playerShip.gameObject;

        theRabbit.ePath = waypoints.GetComponent<EditorPath>();
        Array.Resize(ref theRabbit.points, waypoints.transform.childCount);
        theRabbit.points[0] = waypoints.transform.GetChild(0);

        newAI.activateAI = true;
    }

    // Add cameras to the scene, depending on mode.
    private void AddCameras()
    {
        GameObject[] go = GameObject.FindGameObjectsWithTag("Ship");
        BezierSpline introSplineStart = GameObject.FindGameObjectWithTag("IntroSplineStart").GetComponent<BezierSpline>();
        BezierSpline introSplineLookatStart = GameObject.FindGameObjectWithTag("IntroSplineLookatStart").GetComponent<BezierSpline>();

        if (isSplitscreen)
        {
            GameObject splitScreenCamera = (GameObject)Instantiate(Resources.Load("Prefabs/Cameras/SplitScreenCameraPrefab"), null);

            GameObject camera1 = splitScreenCamera.transform.GetChild(0).gameObject;
            camera1.transform.rotation = go[0].transform.rotation;
            camera1.transform.position = go[0].transform.position - go[0].transform.forward * 8;

            GameObject camera2 = splitScreenCamera.transform.GetChild(1).gameObject;
            camera2.transform.rotation = go[1].transform.rotation;
            camera2.transform.position = go[1].transform.position - go[1].transform.forward * 8;

            CamScript camScript1 = camera1.GetComponent<CamScript>();
            CamScript camScript2 = camera2.GetComponent<CamScript>();

            camScript1.ship = go[0].transform;
            camScript2.ship = go[1].transform;

            camScript1.camSpline = introSplineStart;
            camScript1.camLookAtSpline = introSplineLookatStart;

            camScript2.camSpline = introSplineStart;
            camScript2.camLookAtSpline = introSplineLookatStart;

            cameraReferences.Add(camScript1);
            cameraReferences.Add(camScript2);

            HUDs.Add(camera1.transform.FindChild("HUD").GetComponent<HUD>());
            HUDs.Add(camera2.transform.FindChild("HUD").GetComponent<HUD>());
        }
        else
        {
            GameObject camera = (GameObject)Instantiate(Resources.Load("Prefabs/Cameras/CameraPrefab"), null);

            camera.transform.rotation = go[0].transform.rotation;
            camera.transform.position = go[0].transform.position - go[0].transform.forward * 8;

            CamScript camScript = camera.GetComponent<CamScript>();
            camScript.ship = go[0].transform;

            camScript.camSpline = introSplineStart;
            camScript.camLookAtSpline = introSplineLookatStart;

            cameraReferences.Add(camScript);

            HUDs.Add(camera.transform.FindChild("HUD").GetComponent<HUD>());
        }
    }

    // Load the correct track/scene, called by menusScript.
    public void StartRace()
    {
        audioController.Stop();
        SceneManager.LoadScene(selectedTrack);
    }

    // When a new level has been loaded.
    void OnLevelWasLoaded(int level)
    {
        // Don't continue if you're in the menus (i.e. level 0).
        if (level == 0)
            return;

        StartCoroutine(SetupRace());
    }

    IEnumerator SetupRace()
    {
        // Find the waypoints
        waypoints = GameObject.Find("Waypoints");

        // Look for the start positions.
        ReadStartPositions();
        if (isMultiplayer)
        {
            SpawnShipsNetwork();
        }
        else
        {
            // Spawn all ships at the start positions.
            SpawnShips();
            // Add cameras to the scene.
            AddCameras();
        }

        // Disable visual presentation of start area.
        startArea.gameObject.SetActive(false); 

        // Fade in screen effect


        // Tell cameras to start the introduction
        foreach (CamScript camera in cameraReferences)
            camera.StartIntro(1 / cameraIntroLength);

        // Audio controller play intro song

        // Wait for cameras to finish introduction spline
        yield return new WaitForSeconds(cameraIntroLength);

        // Activate HUD
        foreach (CamScript camera in cameraReferences)
            camera.transform.FindChild("HUD").gameObject.SetActive(true);

        // Ask the race controller to start the race when cameras are done.
        RaceController raceController = GameObject.Find("RaceController").GetComponent<RaceController>();
        raceController.nrOfLaps = nrOfLaps;
        raceController.StartCountDown(countDownTimer);

        // audio controller play countdown
        audioController.PlayFile("Countdown", false);

        // Wait and then start race music
        yield return new WaitForSeconds(countDownTimer + 1);
        audioController.PlayFile("Soundtrack1", true);
    }

}
