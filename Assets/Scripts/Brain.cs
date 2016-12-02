using UnityEngine;
using System.Collections;
using Prototype.NetworkLobby;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Brain : NetworkBehaviour {

    public bool isSplitscreen;
    public bool isMultiplayer;

    public string track;

    public float countDownTimer = 5;

    Transform[] startPositions;

    public List<GameObject> availableShips;
    List<GameObject> playerShips;

    Transform startArea;

	void Start () 
    {
        DontDestroyOnLoad(transform.gameObject);

        startPositions = new Transform[8];
        playerShips = new List<GameObject>();
	}
	

	void Update () 
    {
	
	}

    public void AddSelectedShip(string name)
    {
        foreach (GameObject go in availableShips)
        {
            if (go.name == name)
                playerShips.Add(go);
        }
    }

    public void AddSelectedShip(int index)
    {
        playerShips.Add(availableShips[index]);
    }

    // Read start positions from prefab.
    void ReadStartPositions()
    {
        startArea = GameObject.FindGameObjectWithTag("Start").transform;

        for (int i = 0; i < startArea.childCount; i++)
            startPositions[i] = startArea.GetChild(i);
    }

    // Spawn all ships at start positions.
    void SpawnShips()
    {
        NetworkIdentity networkID = GetComponent<NetworkIdentity>();
        networkID.RebuildObservers(false);

        Debug.Log(playerShips.Count + "  PlayerShips");
        Debug.Log(networkID.observers.Count + "  Observers");
        for (int i = 0; i < playerShips.Count; i++)
        {
            Debug.Log("Players");
            GameObject ship = (GameObject)Instantiate(playerShips[i], startPositions[7 - i].position, startPositions[7 - i].rotation);

            if (isMultiplayer)
            {
                Debug.Log("IsMultiplayer");
                if (networkID.isServer)
                {
                    Debug.Log("isServer");
                    NetworkServer.SpawnWithClientAuthority(ship, networkID.observers[i]);
                }
            }

            //Camera.main.transform.parent = player.transform;
            //Camera.main.transform.rotation = player.transform.rotation;
            //Camera.main.transform.position = new Vector3(0, 100, -5);


            // Remove from available ships to let AI choose from remainding ones.
            if (availableShips.Count > 1)
                availableShips.Remove(playerShips[i]);
        }

        // Spawn AI Ships
        //for (int i = 7 - playerShips.Count; i >= 0; i--)
        //{
        //    int random = Random.Range(0, availableShips.Count - 1);

        //    Instantiate(availableShips[random], startPositions[i].position, startPositions[i].rotation);
        //    if (availableShips.Count > 1)
        //        availableShips.Remove(availableShips[random]);
        //}

        // MP ships?
    }
    private void AddCameras()
    {
        GameObject[] go = GameObject.FindGameObjectsWithTag("Ship");
        if(isSplitscreen)
        {
            GameObject splitScreenCamera = (GameObject)Instantiate(Resources.Load("Prefabs/SplitScreenCameraPrefab"), null);

            GameObject camera1 = splitScreenCamera.transform.GetChild(0).gameObject;
            camera1.transform.rotation = go[0].transform.rotation;
            camera1.transform.position = go[0].transform.position - go[0].transform.forward * 8;

            GameObject camera2 = splitScreenCamera.transform.GetChild(1).gameObject;
            camera2.transform.rotation = go[1].transform.rotation;
            camera2.transform.position = go[1].transform.position - go[1].transform.forward * 8;

            camera1.GetComponent<CamScript>().ship = go[0].transform;
            camera2.GetComponent<CamScript>().ship = go[1].transform;
        }
        else
        {
            GameObject camera = (GameObject)Instantiate(Resources.Load("Prefabs/CameraPrefab"), null);

            camera.transform.rotation = go[0].transform.rotation;
            camera.transform.position = go[0].transform.position - go[0].transform.forward * 8;
            camera.GetComponent<CamScript>().ship = go[0].transform;
        }
    }
    // Set up race and ask controller to start it.
    public void StartRace()
    {
        SceneManager.LoadScene(track);
    }
    void Test()
    {

        //LobbyManager nLM = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        //nLM.m
        //nLM.is
    }
    void OnLevelWasLoaded(int level)
    {
        // First scene has to be menus scene.
        if (level == 0)
            return;
        Debug.Log("Hello World! 1");
        ReadStartPositions();
        Debug.Log("Hello World! 2");
        SpawnShips();
        Debug.Log("Hello World! 3");
        AddCameras();
        Debug.Log("Hello World! 4");

        startArea.gameObject.SetActive(false);

        // controller, plz, can you plz start the race
        RaceController raceController = GameObject.Find("RaceController").GetComponent<RaceController>();
        raceController.StartCountDown(countDownTimer);
    }

}
