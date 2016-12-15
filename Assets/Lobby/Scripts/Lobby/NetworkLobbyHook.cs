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
        ShipNetworkController ship = gamePlayer.GetComponent<ShipNetworkController>();
        ShipController shipPrefab = GameObject.Find("Brain").GetComponent<Brain>().availableShips[lobby.shipDropdown.value].GetComponent<ShipController>();

        ship.GetComponent<ShipNetworkController>().modelID = lobby.shipDropdown.value;
        ship.name = shipPrefab.name;
        ship.energyEfficiency = shipPrefab.energyEfficiency;
        ship.overheatAfter = shipPrefab.overheatAfter;
        ship.overheatLockTime = shipPrefab.overheatLockTime;
        ship.maxEnergy = shipPrefab.maxEnergy;
        ship.maxspeedBoost = shipPrefab.maxspeedBoost;
        ship.speedBoost = shipPrefab.speedBoost;
        ship.rechargePerSecond = shipPrefab.rechargePerSecond;
        ship.heatReductionPerSecond = shipPrefab.heatReductionPerSecond;

        ship.forwardAccelerationSpeed = shipPrefab.forwardAccelerationSpeed;
        ship.noAccelerationDrag = shipPrefab.noAccelerationDrag;
        ship.maxForwardAccelerationSpeed = shipPrefab.maxForwardAccelerationSpeed;
        ship.maxBackwardAccelerationSpeed = shipPrefab.maxBackwardAccelerationSpeed;
        ship.brakingAcceleration = shipPrefab.brakingAcceleration;
        ship.downwardSpeed = shipPrefab.downwardSpeed;
        ship.rotationSpeed = shipPrefab.rotationSpeed;
        ship.hoverHeight = shipPrefab.hoverHeight;
        ship.gravity = shipPrefab.gravity;
        ship.fallOffset = shipPrefab.fallOffset;
        ship.shipBankReturnSpeed = shipPrefab.shipBankReturnSpeed;
        ship.shipSpeedBank = shipPrefab.shipSpeedBank;
        ship.shipMaxBank = shipPrefab.shipMaxBank;
        ship.engineAudioName = shipPrefab.engineAudioName;
        ship.boostAudioName = shipPrefab.boostAudioName;
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
