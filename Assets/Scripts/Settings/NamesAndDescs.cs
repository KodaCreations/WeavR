using UnityEngine;
using System.Collections;

public class NamesAndDescs {

    static string ship1Name = "Ship 1";
    static string ship2Name = "Ship 2";
    static string ship3Name = "Ship 3";

    static string ship1Desc = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent porttitor ultricies mollis. Sed id enim malesuada, porta ex sit amet, maximus nibh. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce nec purus non eros maximus aliquam a quis est. Sed hendrerit ante id ultricies luctus. Nunc a ornare mauris. Pellentesque dapibus, eros sit amet consequat porttitor, diam odio consequat nunc, eget commodo velit nibh id risus.";
    static string ship2Desc = "Turny";
    static string ship3Desc = "Boring";

    static string track1Desc = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent porttitor ultricies mollis. Sed id enim malesuada, porta ex sit amet, maximus nibh. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce nec purus non eros maximus aliquam a quis est. Sed hendrerit ante id ultricies luctus. Nunc a ornare mauris. Pellentesque dapibus, eros sit amet consequat porttitor, diam odio consequat nunc, eget commodo velit nibh id risus.";
    static string track2Desc = "Track 2 description";

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
        string[] names = new string[] { track1Desc, track2Desc};
        return names;
    }
}
