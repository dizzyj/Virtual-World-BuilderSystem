using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counselor : NetworkBehaviour
{
    private OnlinePlayersData playersInWaitingRoom;
    private CVWNetworkManager manager;
    public bool newListLoaded = true;


    // Start is called before the first frame update
    void Start()
    {
        manager = CVWNetworkManager.singleton;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Public Functions

    // Get the current list of players in the waiting room
    public OnlinePlayersData GetAllPlayersInWaitingRoom() {
        return playersInWaitingRoom;
    }

    // Update the players in the waiting room list. 
    public void UpdatePlayersInWaitingRoomList() {
        newListLoaded = false;
        CmdUpdatePlayersInWaitingRoomList();
    }

    #endregion

    #region Waiting Room List Update

    // Server: First, the manager gets the waiting room players on the server side. This is then sent back to the user
    [Command(requiresAuthority=false)]
    private void CmdUpdatePlayersInWaitingRoomList(NetworkConnectionToClient sender = null) {
        
        // OnlinePlayersData waitingRoomPlayers = ServerGetAllPlayers();
        OnlinePlayersData waitingRoomPlayers = ServerGetAllWaitingRoomPlayers();

        TargetUpdatePlayersInWaitingRoomList(sender, waitingRoomPlayers);
    }

    // Client: The client then calls its own update function with the given list of players
    [TargetRpc]
    private void TargetUpdatePlayersInWaitingRoomList(NetworkConnection connection, OnlinePlayersData players) {
        UpdatePlayersInWaitingRoomList(players);
    }
    
    // Client: Finally, local player list is updated and flag is set to true
    private void UpdatePlayersInWaitingRoomList(OnlinePlayersData players) {
        playersInWaitingRoom = players;
        newListLoaded = true;
    }

    #endregion

    
    public OnlinePlayersData ServerGetAllWaitingRoomPlayers() {
        List<string> playerNames = new List<string>();
        List<uint> playerIds = new List<uint>();

        foreach (NetworkConnection conn in NetworkServer.connections.Values) {
            if (conn.identity.gameObject.scene.path == manager.waitingRoomSceneName) {
                playerNames.Add(conn.identity.GetComponent<Player>().playerName);
                playerIds.Add(conn.identity.netId);
            }
        }


        return new OnlinePlayersData(playerNames, playerIds);
    }

    
    public OnlinePlayersData ServerGetAllPlayers() {
        List<string> playerNames = new List<string>();
        List<uint> playerIds = new List<uint>();

        foreach (NetworkConnection conn in NetworkServer.connections.Values) {
            playerNames.Add(conn.identity.GetComponent<Player>().playerName);
            playerIds.Add(conn.identity.netId);
        }


        return new OnlinePlayersData(playerNames, playerIds);
    }
}