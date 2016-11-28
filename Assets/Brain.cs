using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Brain : MonoBehaviour {

    public bool isSplitscreen;
    public bool isMultiplayer;

    public string track;

    List<Vector3> startPositions;
    List<GameObject> ships;

	void Start () 
    {
        DontDestroyOnLoad(transform.gameObject);

	    startPositions = new List<Vector3>();
        ships = new List<GameObject>();
	}
	

	void Update () 
    {
	
	}

    // Read start positions from prefab.
    void ReadStartPositions()
    {

    }

    // Spawn all ships at start positions.
    void SpawnShips()
    {

    }

    // Call race controller to start the race.
    public void StartRace()
    {
        // Switch to correct scene
        // loadscene Track variable
        SceneManager.LoadScene(track);

        ReadStartPositions();
        SpawnShips();
        
        // controller, plz, can you plz start the race
    }
}
