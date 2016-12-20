using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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
    private bool raceStarted;
    private float countdownTimer;

    // Use this for initialization
    void Start()
    {
        rc = FindObjectOfType<RaceController>();
        lapCounter = transform.FindChild("LapCounter").GetComponent<Text>();
        positionCounter = transform.FindChild("PositionCounter").GetComponent<Text>();
        countdown = transform.FindChild("Countdown").GetComponent<Text>();
        finishPanel = transform.FindChild("FinishPanel");
        energy = transform.FindChild("EnergyPercentPanel").GetComponent<RectTransform>();
        overheat = transform.FindChild("OverheatPercentPanel").GetComponent<RectTransform>();
        ship = GetComponentInParent<CamScript>().ship.gameObject.GetComponent<ShipController>();

        countdownTimer = 1.0f;
        raceStarted = true;
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

        lapCounter.text = (rc.shipLapCounter[rc.GetRacePosition(ship) - 1] + 1) + "/" + rc.nrOfLaps;

        positionCounter.text = rc.GetRacePosition(ship) + "/" + rc.ships.Length;
        if (rc.counter >= 0)
        {
            countdown.text = ((int)Math.Ceiling(rc.counter)).ToString();
            raceStarted = false;
        }
        else
        {
            if (raceStarted == false && countdownTimer >= 0)
            {
                countdown.text = "GO!";
                countdownTimer -= Time.deltaTime;
            }
            else
            {
                countdown.text = "";
                raceStarted = true;
            }
        }

            

        //weapon.text = GetWeaponName(rc.ships[placeInList].GetComponent<ShipController>());

        float energyScale = ship.Energy / ship.maxEnergy;
        energy.localScale = new Vector3(energyScale, 1, 1);
        float overheatScale = ship.CurrentHeat / ship.overheatAfter;
        overheat.localScale = new Vector3(overheatScale, 1, 1);


        //energy.localScale = new Vector3(ship.Energy / ship.maxEnergy, 1, 1);
        //overheat.localScale = new Vector3(ship.CurrentHeat / ship.overheatAfter, 1, 1);

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