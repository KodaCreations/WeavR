using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class HUD : MonoBehaviour
{
    RaceController rc;
    ShipController ship;
    Text lapCounter;
    Text positionCounter;
    Text countdown;
    Text weapon;
    Transform finishPanel;

    // Use this for initialization
    void Start()
    {
        rc = FindObjectOfType<RaceController>();
        lapCounter = transform.FindChild("LapCounter").GetComponent<Text>();
        positionCounter = transform.FindChild("PositionCounter").GetComponent<Text>();
        countdown = transform.FindChild("Countdown").GetComponent<Text>();
        weapon = transform.FindChild("WeaponText").GetComponent<Text>();
        finishPanel = transform.FindChild("FinishPanel");
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
    }

    public void EnableWinPanel(float position)
    {
        finishPanel.GetComponentInChildren<Text>().text = "Goal!";
        finishPanel.gameObject.SetActive(true);
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