using UnityEngine;
using System.Collections;

public class NamesAndDescs {

    static string ship1Name = "Ship 1";
    static string ship2Name = "Ship 2";
    static string ship3Name = "Ship 3";

    static string ship1Desc = "Ship 1 description";
    static string ship2Desc = "Ship 2 description";
    static string ship3Desc = "Ship 3 description";

    static string track1Desc = "Track 1 description";
    static string track2Desc = "Track 2 description";
    static string testingTrackDesc = "Testing track for physics and other things";

    public static string[] GetShipNames()
    {
        string[] names = new string[] { ship1Name, ship2Name, ship3Name };
        return names;
    }

    public static string[] GetShipDescs()
    {
        string[] names = new string[] { ship1Desc, ship2Desc, ship3Desc };
        return names;
    }

    public static string[] GetTrackDescs()
    {
        string[] names = new string[] { track1Desc, track2Desc, testingTrackDesc };
        return names;
    }
}
