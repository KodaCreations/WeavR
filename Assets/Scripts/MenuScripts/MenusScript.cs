using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class MenusScript : MonoBehaviour {

    // Ship variables, singleplayer and splitscreen
    ShipPreview shipPreview;
    ShipPreview shipPreviewSS1;
    ShipPreview shipPreviewSS2;
    public Text shipName;
    public Text shipNameSS1; 
    public Text shipNameSS2;

    public Button selectButtonSS1;
    public Button selectButtonSS2;

    int shipsSelectedCount;

    // Track variables
    List<string> trackNames;
    public Text trackName;
    int trackIndex;
    List<Sprite> trackPreviews;
    public Image trackPreview;

    // All menus
    Transform mainMenu;
    Transform trackMenu;
    Transform shipMenu;
    Transform mpMenu;
    Transform splitscreenMenu;
    Transform settingsMenu;

    // Multiplayer manager
    public Transform lobbyManager;

    // Brain
    public Brain brain;

	void Start ()
    {
        // Find all menus
        mainMenu = transform.FindChild("Main Menu");
        trackMenu = transform.FindChild("Track Selection Menu");
        shipMenu = transform.FindChild("Ship Selection Menu");
        mpMenu = transform.FindChild("Multiplayer Menu");
        splitscreenMenu = transform.FindChild("Splitscreen Menu");
        settingsMenu = transform.FindChild("Settings Menu");

        // Get loadable track names from brain.
        trackNames = brain.loadableTrackNames;

        // Load pictures for tracks, if they exist. 
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

    // Functions to be called by menus.
    #region SharedMenuFunctions
    public void SwitchToTrackSelectionMenu()
    {
        DeactivateAllMenus();
        trackMenu.gameObject.SetActive(true);
        trackName.text = trackNames[0];
        trackPreview.sprite = trackPreviews[0];
    }

    public void DeactivateAllMenus()
    {
        mainMenu.gameObject.SetActive(false);
        trackMenu.gameObject.SetActive(false);
        shipMenu.gameObject.SetActive(false);
        mpMenu.gameObject.SetActive(false);
        splitscreenMenu.gameObject.SetActive(false);
        settingsMenu.gameObject.SetActive(false); 
    }

    public void SwitchToMainMenu()
    {
        DeactivateAllMenus();
        mainMenu.gameObject.SetActive(true);
    }
    #endregion

    #region MainMenu
    public void SwitchToMultiplayerMenu()
    {
        mainMenu.gameObject.SetActive(false);
        mpMenu.gameObject.SetActive(true);
    }
    public void SwitchToSettingsMenu()
    {
        DeactivateAllMenus();
        settingsMenu.gameObject.SetActive(true);
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

    public void SwitchToSplitscreenTrackMenu()
    {
        brain.isSplitscreen = true;
        SwitchToTrackSelectionMenu();
    }

    #endregion

    #region TrackSelectionMenu
    public void NextTrack(int direction)
    {
        trackIndex += direction;
        if (trackIndex > trackNames.Count - 1)
            trackIndex = 0;
        if (trackIndex < 0)
            trackIndex = trackNames.Count - 1;

        trackName.text = trackNames[trackIndex];
    }

    public void SwitchToShipSelectionMenu()
    {
        DeactivateAllMenus();
        brain.selectedTrack = trackName.text;

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
                shipPreviewSS1 = splitscreenMenu.GetChild(2).GetComponentInChildren<ShipPreview>();

            shipNameSS1.text = shipPreviewSS1.GetPreviewName();

            if (!shipPreviewSS2)
                shipPreviewSS2 = splitscreenMenu.GetChild(3).GetComponentInChildren<ShipPreview>();
            shipNameSS2.text = shipPreviewSS2.GetPreviewName();
        }
    }
    #endregion

    #region ShipSelectionMenus
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
        selectButtonSS1.interactable = false;
        brain.AddSelectedShip(shipPreviewSS1.GetShipPrefabName().Split(' ')[1]);
        shipsSelectedCount++;
        if (shipsSelectedCount == 2)
            LoadRace();
    }

    public void SelectShipSS2()
    {
        selectButtonSS2.interactable = false;
        brain.AddSelectedShip(shipPreviewSS2.GetShipPrefabName().Split(' ')[1]);
        shipsSelectedCount++;
        if (shipsSelectedCount == 2)
            LoadRace();
    }

    public void UseGamepad(int playerIndex)
    {
        if (playerIndex == 0)
            brain.player1UsingGamepad = !brain.player1UsingGamepad;
        else if (playerIndex == 1)
            brain.player2UsingGamepad = !brain.player2UsingGamepad;
    }

    void LoadRace()
    {
        shipMenu.gameObject.SetActive(false);

        brain.StartRace();
    }
    #endregion
}
