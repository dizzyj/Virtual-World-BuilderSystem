using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

//Singleton Manager that will track all players on the network
//This is outdated and should probably be removed but is kept for reference for now
public sealed class PlayerTracker
{
    private PlayerTracker()
    {
        onlinePlayers = new Dictionary<string, Player>();
    }
    public static PlayerTracker instance = null; //this should be private, is public for debugging
    private Dictionary<string, Player> onlinePlayers; //will this cause an issue if the player disconnects and the GO becomes null?

    /// <summary>
    /// Returns the instance of the PlayerTracker Singleton. Only useable by host.
    /// </summary>
    /// <returns></returns>
    public static PlayerTracker GetInstance()
    {
        Debug.LogWarning("Getting player access");
        if (instance == null)
        {
            if(Player.GetLocalPlayer() == null || Player.GetLocalPlayer().isServer) {
                instance = new PlayerTracker();
            }
            else if (Player.GetLocalPlayer().isClientOnly)
            {
                Debug.LogError("Client attempted to access the playerTracker");
            }
        }
        return instance;
    }

    //Begins listening for when the server has clients added
    //Might need to be secured against clients
    public void ListenUp()
    {
        //NetworkServer.addCon.AddListener(GetInstance().AddPlayer);
    }

    //Adds a player to the online player registry
    //TODO: Remove players when they logout
    private async void AddPlayer(NetworkConnectionToClient conn)
    {
        var DuplicatesTask = CheckDuplicate(conn);
        bool dup = await DuplicatesTask;

        if (Player.GetLocalPlayer() != null && !dup)
        {
            Debug.Log("Player Added");
            instance.onlinePlayers.Add(conn.identity.name, conn.identity.gameObject.GetComponent<Player>());

            /*CharacterCreateMsg message = new CharacterCreateMsg
            {
                //name = conn.identity.name,
                name = "bob the builder"
            };
            NetworkClient.Send(message);*/
            //TODO: Should probably be using a networkmessage for this to be safe
            //Database.singleton.CharacterSave(conn.identity.gameObject.GetComponent<Player>(), true);
        }
    }

    /*
     * async void askMove
     *  var wantstomove = waitforanswer()
     *  bool wants = await wantstomove;
     *  if(wants)
     *      move()
     *      
     *  asynch Task waitforanswer()
         *  return Task.Factory.StartNew(() =>
            {
                while (has not responded){;}
            });
     */



    //before adding a new log, check if one already exists for that player
    private async Task<bool> CheckDuplicate(NetworkConnectionToClient conn)
    {
        Task<Player> PlayerTask = WaitForIdentity(conn);
        Player newPlay = await PlayerTask;
        foreach (Player play in instance.onlinePlayers.Values)
            if (play == newPlay){return true;}
        return false;
    }

    //These two methods allow execution to continue when the player hasn't spawned yet so that it does not get stuck in an infinite loop
    private async Task<Player> WaitForIdentity(NetworkConnectionToClient conn)
    {
        await Idle(conn); 
        return conn.identity.gameObject.GetComponent<Player>();
    }
    private Task Idle(NetworkConnectionToClient conn)
    {
        return Task.Factory.StartNew(() =>
        {
            while (conn.identity == null){;}
        });
    }

    /// <summary>
    /// Testing method to ensure that onLinePlayers is being tracked correctly. Should be deactivated before deployment. 
    /// </summary>
    public void PrintPlayers()
    {
        //always prints as zeroes the first time for some reason
        Debug.Log("Printing players: ");
        foreach (Player play in instance.onlinePlayers.Values)
        {
            Debug.Log("Player: " + play.name);
        }
    }
}