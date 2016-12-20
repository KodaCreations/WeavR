using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public List<GameObject> availableNetworkShips;     // All available ship prefabs
    public List<GameObject> availableShipsMeshes;     // All available ship prefabs

    public GameObject rabbit;                   // Ai rabbit prefab

    Transform startArea;                        // Start area transform, automatically searched for start tag
    Transform[] startPositions;                 // 8 transforms that are children of startArea.
    GameObject waypoints;                       // AI waypoints

    public List<GameObject> playerShips;        // Ships chosen by the player

    AudioController audioController;            // Audio controller

    List<HUD> HUDs;                             // All huds in the scene


    public Transform ingameCanvasPrefab;        // Pause menu and win screen, not the hud.
    Transform ingameCanvas;                     // Object clone
    public Transform playerInfoPanelPrefab;     // Player info spawned in win screen
    Transform pauseMenu;                        // Pause menu
    Transform winMenu;                          // Win panel
    Transform playerInfoPanels;                 // Container for info panels

    int levelIndex;                             // Index of current scene

    RaceController raceController;              // Reference to race controller

    int playersPassedFinish;                    // Number of players that crossed the finish line

    List<Transform> flashPanels;                 // Panels that have flashImage
    List<Image> flashImages;                    // Image used to flash when crossing finish line

    bool introRunning;                          // Are intro cameras moving?


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
        flashPanels = new List<Transform>();
        flashImages = new List<Image>();

        audioController = GameObject.Find("AudioController").GetComponent<AudioController>();
    }
	
	void Update () 
    {
        if (levelIndex != 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(("joystick 1 button 7")))
            {
                if (introRunning)
                    introRunning = false;

                if (!raceController.raceOngoing || winMenu.gameObject.activeSelf)
                    return;

                // pause game and enable pause menu
                if (Time.timeScale == 1)
                {
                    AudioListener.volume = 0;
                    Time.timeScale = 0;
                    pauseMenu.gameObject.SetActive(true);
                }
                else
                {
                    Resume();
                }
            }
            

        }
	}

    void Resume()
    {
        Time.timeScale = 1;
        AudioListener.volume = 1;
        pauseMenu.gameObject.SetActive(false);
    }

    // Called by race controller when a player crosses the finish line
    public void ShipFinished(GameObject playerShip, int playerPosition, List<float> lapTimes)
    {
        // If ship is a player, activate their win panel, change cam mode and spawn AI
        InputHandler isPlayer = playerShip.GetComponent<InputHandler>();
        if (isPlayer)      
        {
            int playerIndex = int.Parse(playerShip.name[playerShip.name.Length - 1] + "");
            Debug.Log(playerIndex);


            // Flash the screen
            flashPanels[playerIndex].gameObject.SetActive(true);
            StartCoroutine(ScreenFlash(flashImages[playerIndex], winFlashTimer));

            // Disable HUD
            HUDs[playerIndex].gameObject.SetActive(false);

            // Make a new AI take over the ship
            SpawnAIFromPlayer(playerShip.transform);

            // Enter camera rotation mode
            HUDs[playerIndex].GetComponentInParent<CamScript>().EnterRotateMode();


            // spectator mode for MULTIPLAYER
            // huds[playerIndex].GetComponentInParent<CamScript>().EnterSpectatorMode();
        }

        StartCoroutine(AddPlayerInfoPanel(playerShip, playerPosition, lapTimes, isPlayer));
    }

    IEnumerator AddPlayerInfoPanel(GameObject playerShip, int playerPosition, List<float> lapTimes, bool waitForFlash)
    {
        Transform newInfoPanel = Instantiate<Transform>(playerInfoPanelPrefab);

        // Switch color every second panel
        Color backgroundColor;
        if (playerPosition % 2 == 0)
            backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        else
            backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        newInfoPanel.GetChild(0).GetComponent<Image>().color = backgroundColor;

        newInfoPanel.GetChild(1).GetComponent<Text>().text = playerPosition.ToString();

        // More info for panel
        newInfoPanel.GetChild(2).GetComponent<Text>().text = playerShip.name;

        float bestTime = float.MaxValue, totalTime = 0;
        for (int i = 0; i < lapTimes.Count; i++)
        {
            if (bestTime > lapTimes[i])
                bestTime = lapTimes[i];
            totalTime += lapTimes[i];
        }
        // Convert times into correct format
        int bestTimeMinutes = (int)bestTime / 60;
        int bestTimeSeconds = (int)bestTime - (bestTimeMinutes * 60);
        float bestTimeMili = bestTime - bestTimeMinutes * 60 - bestTimeSeconds;

        int totalTimeMinutes = (int)totalTime / 60;
        int totalTimeSeconds = (int)totalTime - (totalTimeMinutes * 60);
        float totalTimeMili = totalTime - totalTimeMinutes * 60 - totalTimeSeconds;

        newInfoPanel.GetChild(4).GetComponent<Text>().text = bestTimeMinutes.ToString("D2") + ":" + bestTimeSeconds + ":" + bestTimeMili.ToString("F2").Remove(0, 2) +
            "\n" + totalTimeMinutes.ToString("D2") + ":" + totalTimeSeconds + ":" + totalTimeMili.ToString("F2").Remove(0, 2);

        newInfoPanel.SetParent(playerInfoPanels);
        newInfoPanel.localScale = new Vector3(1, 1, 1);

        if (waitForFlash)
            yield return new WaitForSeconds(winFlashTimer);
        newInfoPanel.gameObject.SetActive(true);

        playersPassedFinish++;
        if (playersPassedFinish == playerShips.Count)
        {
            winMenu.gameObject.SetActive(true);
        }
    }

    // Called by buttons
    public void RestartRace()
    {
        Time.timeScale = 1;
        ResetVariables();
        StartRace();
    }

    // Called by buttons
    public void LoadMenu()
    {
        Time.timeScale = 1;
        playerShips.Clear();
        ResetVariables();

        SceneManager.LoadScene("Menus");
    }

    void ResetVariables()
    {
        cameraReferences.Clear();
        flashPanels.Clear();
        flashImages.Clear();
        playersPassedFinish = 0;
        HUDs.Clear();
    }

    // Flash the screen
    IEnumerator ScreenFlash(Image flashImage, float flashTime)
    {
        for (float t = 1; t > 0; t -= Time.deltaTime / flashTime)
        {
            Color newColor = new Color(1, 1, 1, t);
            flashImage.color = newColor;
            yield return null;

            if (flashImage.color.a == 0)
                StopCoroutine("ScreenFlash");
        }
    }

    // Adds the player's chosen ship to the list, called from menusScript.
    public void AddSelectedShip(string name)
    {
        if(isMultiplayer)
        {
            foreach (GameObject go in availableNetworkShips)
            {
                if (go.name == name)
                    playerShips.Add(go);
            }
        }
        else
        {
            foreach (GameObject go in availableShips)
            {
                if (go.name == name)
                    playerShips.Add(go);
            }
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
            //if (availableShips.Count > 1)
            //    availableShips.Remove(playerShips[i]);
        }

        // Spawn AI Ships
        for (int i = 7 - playerShips.Count; i >= 0; i--)
        {
            int random = UnityEngine.Random.Range(0, availableAIShips.Count);

            GameObject aiShip = (GameObject)Instantiate(availableAIShips[random], startPositions[i].position, startPositions[i].rotation);
            aiShip.name = "AIShip number " + i; 
            GameObject rabbitObject = (GameObject)Instantiate(rabbit, startPositions[i].position, startPositions[i].rotation);
            TheRabbit theRabbit = rabbitObject.GetComponent<TheRabbit>();

            aiShip.GetComponent<AIController>().rabbit = theRabbit;
            theRabbit.AI = aiShip;

            theRabbit.ePath = waypoints.GetComponent<EditorPath>();
            Array.Resize(ref theRabbit.points, waypoints.transform.childCount);
            theRabbit.points[0] = waypoints.transform.GetChild(0);

            //if (availableAIShips.Count > 1)
            //    availableAIShips.Remove(availableAIShips[random]);
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
            flashPanels.Add(camera1.transform.GetChild(0));
            flashPanels.Add(camera2.transform.GetChild(0));
            flashImages.Add(flashPanels[0].GetComponentInChildren<Image>());
            flashImages.Add(flashPanels[1].GetComponentInChildren<Image>());
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
            flashPanels.Add(camera.transform.GetChild(0));
            flashImages.Add(flashPanels[flashPanels.Count -1].GetComponentInChildren<Image>());
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
        levelIndex = level;
        // Don't continue if you're in the menus (i.e. level 0).
        if (level == 0)
            return;

        StartCoroutine(SetupRace());
    }

    IEnumerator SetupRace()
    {
        // Find the waypoints
        waypoints = GameObject.Find("Waypoints");

        // Find the race controller
        raceController = GameObject.Find("RaceController").GetComponent<RaceController>();
        raceController.nrOfLaps = nrOfLaps;

        // Spawn ingame canvas
        ingameCanvas = Instantiate<Transform>(ingameCanvasPrefab);
        pauseMenu = ingameCanvas.FindChild("PauseMenu");
        winMenu = ingameCanvas.FindChild("Win Menu");
        playerInfoPanels = winMenu.FindChild("Window").FindChild("PlayerInfoPanels");

        // Setup restart and menu buttons
        Button winRestartButton = winMenu.FindChild("Window").FindChild("SubWindow").FindChild("Restart").GetComponent<Button>();
        winRestartButton.onClick.AddListener(() => { RestartRace(); });
        Button pauseRestartButton = pauseMenu.FindChild("Window").FindChild("Restart").GetComponent<Button>();
        pauseRestartButton.onClick.AddListener(() => { RestartRace(); });

        Button winMenuButton = winMenu.FindChild("Window").FindChild("SubWindow").FindChild("Main Menu").GetComponent<Button>();
        winMenuButton.onClick.AddListener(() => { LoadMenu(); });
        Button pauseMenuButton = pauseMenu.FindChild("Window").FindChild("Main Menu").GetComponent<Button>();
        pauseMenuButton.onClick.AddListener(() => { LoadMenu(); });

        Button pauseResumeButton = pauseMenu.FindChild("Window").FindChild("Resume").GetComponent<Button>();
        pauseResumeButton.onClick.AddListener(() => { Resume(); });


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
        //startArea.gameObject.SetActive(false); //Don't disable the Starting Area, the Network script needs it. Disable the Renderer for the Boxes instead
        for (int i = 0; i < startArea.childCount; ++i)
            startArea.GetChild(i).GetComponent<MeshRenderer>().enabled = false;

        // Fade in screen effect? code goes here

        // Intro is now running
        introRunning = true;

        // Audio controller play intro song
        audioController.PlayFile("Intro", false);

        // Tell cameras to start the introduction
        foreach (CamScript camera in cameraReferences)
            camera.StartIntro(1 / cameraIntroLength);


        // Wait for cameras to finish introduction spline or key pressed
        yield return StartCoroutine(WaitForTimeOrIntroEnd(cameraIntroLength));

        // Stop intro sounds
        audioController.Stop();

        // Activate HUD and stop intro camera
        foreach (CamScript camera in cameraReferences)
        {
            camera.StopIntro();
            camera.transform.FindChild("HUD").gameObject.SetActive(true);
        }

        // Brief pause between intro and countdown starting
        yield return new WaitForSeconds(1.5f);

        // Ask the race controller to start the race when cameras are done.
        raceController.StartCountDown(countDownTimer);

        // audio controller play countdown
        audioController.PlayFile("Countdown", false);

        // Wait and then start race music
        yield return new WaitForSeconds(countDownTimer + 1);
        audioController.PlayFile("Soundtrack1", true);
        audioController.CancelInvoke();
    }

    IEnumerator WaitForTimeOrIntroEnd(float seconds)
     {
        float time = seconds;
        while (time > 0.0 && introRunning)
        {
            time -= Time.deltaTime;
            yield return null;
        }
    }
}
