using UnityEngine;
using System.Collections;

public class ControlSchemes {

    // Input controls
    static KeyCode player1Forward = KeyCode.W;
    static KeyCode player1Backward = KeyCode.S;
    static KeyCode player1Left = KeyCode.A;
    static KeyCode player1Right = KeyCode.D;
    static KeyCode player1Brake = KeyCode.Space;
    static KeyCode player1Boost = KeyCode.LeftShift;

    static KeyCode player2Forward = KeyCode.UpArrow;
    static KeyCode player2Backward = KeyCode.DownArrow;
    static KeyCode player2Left = KeyCode.LeftArrow;
    static KeyCode player2Right = KeyCode.RightArrow;
    static KeyCode player2Brake = KeyCode.RightControl;
    static KeyCode player2Boost = KeyCode.RightShift;


    public static KeyCode[] GetScheme1()
    {
        return new KeyCode[] { player1Forward, player1Backward, player1Left, player1Right, player1Brake, player1Boost };
    }
    public static KeyCode[] GetScheme2()
    {
        return new KeyCode[] { player2Forward, player2Backward, player2Left, player2Right, player2Brake, player2Boost };
    }
}
