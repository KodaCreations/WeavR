using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class HUD : MonoBehaviour
{
    private RaceController rc;
    private ShipController ship;
    private Text lapCounter;
    private Text positionCounter;
    private Text countdown;
    private Text weapon;
    private Transform finishPanel;
    private Image flashPanelImage;
    private RectTransform energy;
    private RectTransform overheat;

    private float flashTime = 1;

    // Use this for initialization
    void Start()
    {
        rc = FindObjectOfType<RaceController>();
        lapCounter = transform.FindChild("LapCounter").GetComponent<Text>();
        positionCounter = transform.FindChild("PositionCounter").GetComponent<Text>();
        countdown = transform.FindChild("Countdown").GetComponent<Text>();
        weapon = transform.FindChild("WeaponText").GetComponent<Text>();
        finishPanel = transform.FindChild("FinishPanel");
        flashPanelImage = finishPanel.FindChild("WinFlashPanel").GetComponent<Image>();
        energy = transform.FindChild("EnergyPercentPanel").GetComponent<RectTransform>();
        overheat = transform.FindChild("OverheatPercentPanel").GetComponent<RectTransform>();
        ship = GetComponentInParent<CamScript>().ship.gameObject.GetComponent<ShipController>();
    }

    // Update is called once per frame
    void Update()
    {
        int placeInList = 0;
        ShipController thisShip = ship;
        for (int i = 0; i < rc.ships.Length; ++i)
        {
            if (rc.ships[i] == thisShip)
            {
                placeInList = i;
                break;
            }
        }
        lapCounter.text = rc.shipLapCounter[placeInList] + "/" + "?";
        positionCounter.text = rc.GetRacePosition(ship) + "/" + rc.ships.Length;
        if (rc.counter >= 0)
            countdown.text = ((int)Math.Ceiling(rc.counter)).ToString();
        else
            countdown.text = "";
        //weapon.text = GetWeaponName(rc.ships[placeInList].GetComponent<ShipController>());

        energy.localScale = new Vector3(ship.Energy / ship.maxEnergy, 1, 1);
        //overheat.localScale = new Vector3(ship.CurrentHeat / ship.overheatAfter, 1, 1);
    }

    public void EnableWinPanel(float position, float flashTime)
    {
        StartCoroutine("ScreenFlash");

        finishPanel.GetComponentInChildren<Text>().text = "Goal!";
        finishPanel.gameObject.SetActive(true);
    }

    IEnumerator ScreenFlash()
    {
        Color newColor = flashPanelImage.color;
        newColor.a -= flashTime * Time.deltaTime;
        flashPanelImage.color = newColor;

        if (flashPanelImage.color.a > 0)
            yield return ScreenFlash();
        yield return null;
    }

    private string GetWeaponName(ShipController sc)
    {
        switch (sc.weapon)
        {
            case Weapon.WeaponType.Missile:
                return "Missile";
            case Weapon.WeaponType.Mine:
                return "Mine";
            case Weapon.WeaponType.EnergyDrain:
                return "Drain";
            case Weapon.WeaponType.DecreasedVision:
                return "Vision";
            case Weapon.WeaponType.EMP:
                return "EMP";
            default:
                return "";
        }
    }
}