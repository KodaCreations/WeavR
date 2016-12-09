using UnityEngine;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{
    int index = 0;
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        ShipController ship = gamePlayer.GetComponent<ShipController>();
        ship.name = lobby.name;
        Transform startPosTransform = FindStartTransform(index);
        ++index;

        if(startPosTransform)
        {
            ship.transform.position = startPosTransform.position;
            ship.transform.rotation = startPosTransform.rotation;
        }
    }
    private Transform FindStartTransform(int index)
    {
        GameObject go = GameObject.FindGameObjectWithTag("Start");

        if (go)
            return go.transform.GetChild(index);

        return null;
    }
}
