using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Brain : MonoBehaviour {

    public bool isSplitscreen;
    public bool isMultiplayer;

    public string track;

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
        for (int i = 0; i < playerShips.Count; i++)
        {
            GameObject player = (GameObject)Instantiate(playerShips[i], startPositions[7 - i].position, startPositions[7 - i].rotation);

            //Camera.main.transform.parent = player.transform;
            Camera.main.transform.rotation = player.transform.rotation;
            Camera.main.transform.position = new Vector3(0, 100, -5);

            Debug.Log(Camera.main.transform.position);

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

    // Set up race and ask controller to start it.
    public void StartRace()
    {
        SceneManager.LoadScene(track);
    }

    void OnLevelWasLoaded(int level)
    {
        // First scene has to be menus scene.
        if (level == 0)
            return;

        ReadStartPositions();
        SpawnShips();

        startArea.gameObject.SetActive(false);

        // controller, plz, can you plz start the race
    }

}
