using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenusScript : MonoBehaviour {

    ShipPreview shipPreview;
    ShipPreview shipPreviewSS1;
    ShipPreview shipPreviewSS2;
    public Text shipName;
    public Text shipNameSS1; 
    public Text shipNameSS2;

    List<string> tracks;
    public Text trackName;
    int trackIndex;
    List<Sprite> trackPreviews;
    public Image trackPreview;

    Transform mainMenu;
    Transform trackMenu;
    Transform shipMenu;
    Transform mpMenu;
    Transform splitscreenMenu;

    public Transform lobbyManager;

    public Brain brain;

	// Use this for initialization
	void Start () {
        mainMenu = transform.FindChild("Main Menu");
        trackMenu = transform.FindChild("Track Selection Menu");
        shipMenu = transform.FindChild("Ship Selection Menu");
        mpMenu = transform.FindChild("Multiplayer Menu");
        splitscreenMenu = transform.FindChild("Splitscreen Menu");

        tracks = new List<string>();
        foreach (string trackName in System.IO.Directory.GetFiles(Application.dataPath + "/Scenes/Tracks"))
        {
            string name = trackName.Split('.')[0];
            name = name.Split('\\')[1];
            if (!tracks.Contains(name))
                tracks.Add(name);
        }

        trackPreviews = new List<Sprite>();
        foreach (string trackName in System.IO.Directory.GetFiles(Application.dataPath + "/Resources/TrackPreviews"))
        {
            if (trackName.Substring(trackName.Length - 3, 3) == "jpg")
            {
                string name = trackName.Split('.')[0];
                name = name.Split('\\')[1];
                trackPreviews.Add((Sprite)Resources.Load<Sprite>("TrackPreviews/" + name));
            }
        }
	}

    // Functions to be called by canvases

    #region SharedMenuFunctions
    public void SwitchTrackSelection()
    {
        DeactivateAllMenus();
        trackMenu.gameObject.SetActive(true);
        trackName.text = tracks[0];
        trackPreview.sprite = trackPreviews[0];
    }

    public void DeactivateAllMenus()
    {
        mainMenu.gameObject.SetActive(false);
        trackMenu.gameObject.SetActive(false);
        shipMenu.gameObject.SetActive(false);
        mpMenu.gameObject.SetActive(false);
        splitscreenMenu.gameObject.SetActive(false);
    }

    public void SwitchMainMenu()
    {
        DeactivateAllMenus();
        mainMenu.gameObject.SetActive(true);
    }
    #endregion

    #region MainMenu
    public void SwitchMultiplayer()
    {
        mainMenu.gameObject.SetActive(false);
        mpMenu.gameObject.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region MultiplayerMenu
    public void TurnOnLobby()
    {
        brain.isMultiplayer = true;
        DeactivateAllMenus();
        lobbyManager.gameObject.SetActive(true);
    }

    public void SwitchSplitscreen()
    {
        brain.isSplitscreen = true;
        SwitchTrackSelection();
    }

    #endregion

    #region TrackSelection
    public void NextTrack(int direction)
    {
        trackIndex += direction;
        if (trackIndex > tracks.Count - 1)
            trackIndex = 0;
        if (trackIndex < 0)
            trackIndex = tracks.Count - 1;

        trackName.text = tracks[trackIndex];
    }

    public void SwitchShipSelection()
    {
        DeactivateAllMenus();
        brain.track = trackName.text;

        if (!brain.isSplitscreen)
        {
            shipMenu.gameObject.SetActive(true);

            if (!shipPreview)
                shipPreview = shipMenu.GetComponentInChildren<ShipPreview>();

            shipName.text = shipPreview.GetPreviewName();
        }      
        else
        {
            splitscreenMenu.gameObject.SetActive(true);

            if (!shipPreviewSS1)
                shipPreviewSS1 = splitscreenMenu.GetChild(0).GetComponentInChildren<ShipPreview>();

            shipNameSS1.text = shipPreviewSS1.GetPreviewName();

            if (!shipPreviewSS2)
                shipPreviewSS2 = splitscreenMenu.GetChild(1).GetComponentInChildren<ShipPreview>();
            shipNameSS2.text = shipPreviewSS2.GetPreviewName();
        }
    }
    #endregion

    #region ShipSelection

    public void NextShip(int direction)
    {
        shipPreview.SwitchPreview(direction);
        shipName.text = shipPreview.GetPreviewName();
    }

    public void NextShipSS1(int direction)
    {
        shipPreviewSS1.SwitchPreview(direction);
        shipNameSS1.text = shipPreviewSS1.GetPreviewName();
    }

    public void NextShipSS2(int direction)
    {
        shipPreviewSS2.SwitchPreview(direction);
        shipNameSS2.text = shipPreviewSS2.GetPreviewName();
    }

    public void SelectShip()
    {
        brain.AddSelectedShip(shipPreview.GetShipPrefabName().Split(' ')[1]);
        LoadRace();
    }

    public void SelectShipSS1()
    {
        brain.AddSelectedShip(shipPreviewSS1.GetShipPrefabName().Split(' ')[1]);
        //LoadRace();  only if both selected
    }

    public void SelectShipSS2()
    {
        brain.AddSelectedShip(shipPreviewSS2.GetPreviewName().Split(' ')[1]);
        LoadRace(); //only if both selected
    }

    void LoadRace()
    {
        shipMenu.gameObject.SetActive(false);

        brain.StartRace();
    }
    #endregion
}
