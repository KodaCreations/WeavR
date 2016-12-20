using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class MenusScript : MonoBehaviour {

    // Ship variables, singleplayer and splitscreen
    ShipPreview shipPreview;
    //ShipPreview shipPreviewSS1;
    //ShipPreview shipPreviewSS2;
    ////public Text shipName;
    //public Text shipNameSS1; 
    //public Text shipNameSS2;

    //public Button selectButtonSS1;
    //public Button selectButtonSS2;

    //int shipsSelectedCount;

    //// Track variables
    List<string> trackNames;
    //public Text trackName;
    //int trackIndex;
    Dictionary<string, Sprite> trackPreviews;
    //public Image trackPreview;

    // All menus
    Transform mainMenu;
    Transform trackMenu;
    Transform shipMenu;
    Transform splitscreenMenu;
    Transform settingsMenu;

    // Track selection objects
    public Transform trackTogglePrefab;
    Transform trackToggles;
    Image trackPreview;
    Text trackDesc;
    List<string> trackDescs;

    // Ship selection objects
    Text shipName;
    Text shipDesc;
    Toggle gamePadToggle;
    Text shipNameSS;
    Text shipDescSS;
    Text playerTextSS;
    Toggle gamePadToggleSS;

    // Multiplayer manager
    public Transform lobbyManager;

    // Brain
    Brain brain;

	void Start ()
    {
        // Find all menus
        mainMenu = transform.FindChild("MainMenu");
        shipMenu = transform.FindChild("ShipSelection");
        trackMenu = transform.FindChild("TrackSelection");
        splitscreenMenu = transform.FindChild("SplitscreenShipSelection");

        // Find menu objects
        shipName = shipMenu.FindChild("Window1").FindChild("ShipName").GetComponentInChildren<Text>();
        shipDesc = shipMenu.FindChild("Window1").FindChild("ShipDesc").GetComponentInChildren<Text>();
        gamePadToggle = shipMenu.FindChild("Window1").FindChild("UseGamepad").GetComponentInChildren<Toggle>();
        shipNameSS = splitscreenMenu.FindChild("Window1").FindChild("ShipName").GetComponentInChildren<Text>();
        shipDescSS = splitscreenMenu.FindChild("Window1").FindChild("ShipDesc").GetComponentInChildren<Text>();
        playerTextSS = splitscreenMenu.FindChild("Window1").FindChild("Player").GetComponentInChildren<Text>();
        gamePadToggleSS = splitscreenMenu.FindChild("Window1").FindChild("UseGamepad").GetComponentInChildren<Toggle>();

        shipPreview = FindObjectOfType<ShipPreview>();

        // Get loadable track names from brain.
        brain = GameObject.Find("Brain").GetComponent<Brain>();
        trackNames = brain.loadableTrackNames;

        //Load pictures for tracks, if they exist. 
        trackPreviews = new Dictionary<string, Sprite>();
        //foreach (string trackName in System.IO.Directory.GetFiles(Application.dataPath + "/Resources/TrackPreviews"))
        //{
        //    if (trackName.Substring(trackName.Length - 3, 3) == "jpg" || trackName.Substring(trackName.Length - 3, 3) == "png")
        //    {
        //        string name = trackName.Split('.')[0];
        //        name = name.Split('\\')[1];
        //        trackPreviews.Add(name, (Sprite)Resources.Load<Sprite>("TrackPreviews/" + name));
        //    }
        //}

        foreach (string trackName in trackNames)
            trackPreviews.Add(trackName, (Sprite)Resources.Load<Sprite>("TrackPreviews/" + trackName));

        trackPreview = trackMenu.FindChild("Window2").FindChild("TrackPreview").GetComponentInChildren<Image>();
        trackPreview.sprite = trackPreviews[trackNames[0]];

        trackDescs = new List<string>(NamesAndDescs.GetTrackDescs());
        trackDesc = trackMenu.FindChild("Window2").FindChild("TrackDescription").GetComponentInChildren<Text>();
        trackDesc.text = trackDescs[0];


        // Add buttons for tracks
        trackToggles = trackMenu.FindChild("Window1").FindChild("TrackToggles");
        for (int i = 0; i < trackNames.Count; i++)
        {
            Transform newToggle = (Transform)Instantiate(trackTogglePrefab, trackToggles);
            newToggle.localScale = new Vector3(1, 1, 1);
            Vector3 pos = newToggle.localPosition;
            pos.z = 0;
            newToggle.localPosition = pos;

            newToggle.GetComponentInChildren<Text>().text = trackNames[i];

            Toggle toggle = newToggle.GetComponent<Toggle>();

            toggle.group = trackToggles.GetComponent<ToggleGroup>();

            int toggleIndex = i;
            toggle.onValueChanged.AddListener((value) => { SelectTrack(value, toggleIndex); });
        }
        trackToggles.GetChild(0).GetComponent<Toggle>().isOn = true;

    }

    // Functions to be called by menus.
    #region SwitchMenuFunctions
    public void DeactivateAllMenus()
    {
        mainMenu.gameObject.SetActive(false);
        shipMenu.gameObject.SetActive(false);
        trackMenu.gameObject.SetActive(false);
        splitscreenMenu.gameObject.SetActive(false);
    }

    public void SwitchToMainMenu()
    {
        DeactivateAllMenus();
        mainMenu.gameObject.SetActive(true);
        brain.playerShips.Clear();

        brain.gamepadTempCount = 0;
        brain.gamepadUserCount1 = -1;
        brain.gamepadUserCount2 = -1;

        brain.player1UsingGamepad = false;
        brain.player2UsingGamepad = false;

        brain.isMultiplayer = false;
        brain.isSplitscreen = false;
    }

    public void SwitchToShipSelectionMenu()
    {
        DeactivateAllMenus();
        shipMenu.gameObject.SetActive(true);
        //shipPreview.Reset();
        shipName.text = shipPreview.GetPreviewName();
        shipDesc.text = shipPreview.GetPreviewDesc();
    }

    public void SwitchToSplitscreenMenu()
    {
        DeactivateAllMenus();
        playerTextSS.text = "Player " + 1;
        splitscreenMenu.gameObject.SetActive(true);
        brain.isSplitscreen = true;
    }

    public void SwitchToTrackSelectionMenu()
    {
        DeactivateAllMenus();
        trackMenu.gameObject.SetActive(true);
    }


    public void LoadRace()
    {
        shipMenu.gameObject.SetActive(false);

        brain.StartRace();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion



    #region ShipSelection
    public void NextShip(int direction)
    {
        shipPreview.SwitchPreview(direction);
        shipName.text = shipPreview.GetPreviewName();
        shipDesc.text = shipPreview.GetPreviewDesc();
    }

    public void SelectShip()
    {
        brain.AddSelectedShip(shipPreview.GetShipPrefabName().Split(' ')[1]);
    }

    public void SelectShipSS()
    {
        brain.AddSelectedShip(shipPreview.GetShipPrefabName().Split(' ')[1]);
        splitscreenMenu.gameObject.SetActive(false);
        playerTextSS.text = "Player " + 2;
        if (brain.playerShips.Count == 2)
            SwitchToTrackSelectionMenu();
        else
        {
            splitscreenMenu.gameObject.SetActive(true);
            gamePadToggleSS.isOn = false;
        }
    }

    public void UseGamepad()
    {
        brain.UseGamepad(gamePadToggle.isOn);
        //brain.player1UsingGamepad = gamePadToggle.isOn;
    }

    public void UseGamepadSS()
    {
        brain.UseGamepad(gamePadToggleSS.isOn);
        //if (playerTextSS.text[playerTextSS.text.Length - 1] == '0') 
        //    brain.player1UsingGamepad = gamePadToggleSS.isOn;
        //else
        //    brain.player2UsingGamepad = gamePadToggleSS.isOn;
    }

    #endregion

    #region TrackSelection
    public void SelectTrack(bool toggleOn, int index)
    {
        if (!toggleOn)
            return;

        brain.selectedTrack = trackNames[index];
        trackPreview.sprite = trackPreviews[trackNames[index]];
        trackDesc.text = trackDescs[index];
    }

    #endregion






    #region MainMenu
    //public void SwitchToMultiplayerMenu()
    //{
    //    DeactivateAllMenus();
    //    brain.playerShips.Clear();
    //}

    #endregion

    #region MultiplayerMenu
    public void TurnOnLobby()
    {
        brain.isMultiplayer = true;
        DeactivateAllMenus();
        lobbyManager.gameObject.SetActive(true);
    }

    #endregion



}
