using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Newtonsoft.Json;
using UnityEngine.Events;
using TMPro;
using static UnityEngine.GraphicsBuffer;

[Serializable] public class UnityEventPlayer : UnityEvent<Player> { }

public class Player : NetworkBehaviour
{
    

    public Transform cameraLocation;
    private bool isCounselor = false;
    private string account = "";
    private Transform previousCameraLocation;
    
    //FOR TEXT CHAT -- Storing Player Names
    public static Dictionary<string, Player> onlinePlayers = new Dictionary<string, Player>();

    [SyncVar]
    public string playerName = ""; // Set initial value so Start does not throw null errors


    // Made public for more convenient editing access and set inside editor
    public TextMeshPro playerNameOverlay;
    
    GameObject textChat;

    public GameObject chatPrefab;

    KeyCode CHAT_HOTKEY = KeyCode.C;
    private bool chatRenderStatus = true;

    #region Unity Callbacks

    // Start is called before the first frame update
    void Start()
    {
        onlinePlayers[name] = this;

        // This line was added because other player names were not updating when the client enters a scene
        UpdatePlayerName(playerName);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(CHAT_HOTKEY) == true)
            {
                if (textChat == null)
                {
                    textChat = GameObject.FindGameObjectWithTag("ChatWindow");
                }
                textChat.SetActive(!chatRenderStatus);
                chatRenderStatus = !chatRenderStatus;
            }
        }
    }

    #endregion

    #region Network Identity Callbacks
    
    public override void OnStartClient()
    {
        base.OnStartClient();


        if (isLocalPlayer)
        {
            //get CLIENT username from UIController
            playerName = UIController.clientUserName;

            //update player's name for OTHER clients
            CmdUpdatePlayerName(playerName, gameObject);

            //JOINING client sends welcome message
            string welcomeMessage = playerName + " has joined the server!";
            //textChat.GetComponent<VW_ChatBehavior>().SendServerMessage(welcomeMessage);
        }
    }

    public override void OnStopClient()
    {
        string leaveMessage = playerName + " has left the server.";
        //textChat.GetComponent<VW_ChatBehavior>().SendServerMessage(leaveMessage);

        base.OnStopClient();


    }


    // When starting the local player, get the main camera's parent and then reassign its parent, position, and rotation
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        previousCameraLocation = Camera.main.transform.parent;
        //localPlayer = this;
        VW_ChatBehavior.localPlayer = Player.GetLocalPlayer();
        Camera.main.transform.SetParent(cameraLocation);
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (isServer)
        {
            UpdatePlayerName(UIController.serverUserName);
        }


        if (isLocalPlayer)
        {
            //find the chat window object
            textChat = GameObject.FindGameObjectWithTag("ChatWindow");

            //set the playername overlay to player's username
            UpdatePlayerName(playerName);

            //send server message saying player joined
            string welcomeMessage = playerName + " has joined the server!";
            //textChat.GetComponent<VW_ChatBehavior>().SendServerMessage(welcomeMessage);
        }

    }

    // Reset camera to previous position.
    public override void OnStopLocalPlayer() {
        base.OnStopLocalPlayer();

        Camera.main.transform.SetParent(previousCameraLocation);
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    #endregion

    #region Player Name and Overlay Update
    public string getUserName() {
        return playerName;
    }

    /**
        PRIMARY PLAYER NAME UPDATE FUNCTION

        First, tell the server we want to update our player with the new name.
        Then, the server updates the SyncVar playerName and tells all clients to update the given player's overlay.
    */
    public void UpdatePlayerName(string newName) {
        CmdUpdatePlayerName(newName, gameObject);
    }

    /**
        Update the SyncVar playerName on the server (automatically updates on clients) 
        then tell clients to update the name overlay for the given player
    */
    [Command(requiresAuthority = false)]
    private void CmdUpdatePlayerName(string name, GameObject playerObject)
    {
        playerName = name;
        RpcUpdatePlayerNameOverlay(playerObject);
    }

    /**
        Update player name overlay for the given player object on all clients including self
    */
    [ClientRpc]
    private void RpcUpdatePlayerNameOverlay(GameObject playerObject)
    {
        // Get player component and set player name overlay text to their username
        Player player = playerObject.GetComponent<Player>();
        player.playerNameOverlay.text = player.playerName;
    }

    #endregion

    public void LoadChatPrefab()
    {
        Instantiate(chatPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    #region Character Creator Networking

    [Command]
    public void CmdUpdateBlendShapes(string blendShapeJsonData)
    {
        // this is called by client and run on the server
        Debug.Log("running CmdUpdateBlendShapes on server");
        RpcUpdateBlendShapes(this.gameObject, blendShapeJsonData);
    }

    
    [ClientRpc]
    public void RpcUpdateBlendShapes(GameObject target, string blendShapeJsonData)
    {
        // this is called by server and run on every client
        Debug.Log("running RpcUpdateBlendShapes on clients");
        
        // read json data
        Dictionary<string, BlendShape> jsonData = JsonConvert.DeserializeObject<Dictionary<string, BlendShape>>(blendShapeJsonData);

        // set customization targets to player that called the update command
        CharacterCustomization.Instance.target = target;
        CharacterCustomization.Instance.skmr = target.GetComponentInChildren<SkinnedMeshRenderer>();
        
        // apply changes to each blendshape
        foreach (KeyValuePair<string,BlendShape> keyValuePair in jsonData!)
        {
            CharacterCustomization.Instance.ChangeBlendshapeValue(keyValuePair.Key, keyValuePair.Value.currentValue);
        }
        
        // reset targets to local player
        CharacterCustomization.Instance.Initialize();
    }

    
    [Command]
    public void CmdUpdateColor(string colorJsonData)
    {
        // this is called by client and run on the server
        RpcUpdateColor(this.gameObject, colorJsonData);
    }

    
    [ClientRpc]
    public void RpcUpdateColor(GameObject target, string colorJson)
    {
        // this is called by server and run on every client

        // read color data 
        Dictionary<string, float> jsonData = JsonConvert.DeserializeObject<Dictionary<string, float>>(colorJson);

        // update target objects to player that sent update command
        CharacterCustomization.Instance.target = target;
        CharacterCustomization.Instance.skmr = target.GetComponentInChildren<SkinnedMeshRenderer>();
        HueChanger hueChanger = CharacterCustomization.Instance.HueChanger;
        hueChanger.avatar = CharacterCustomization.Instance;
        hueChanger.mat = null;
        
        // change color values
        hueChanger.ChangeHue(jsonData["hue"]);
        hueChanger.ChangeSat(jsonData["saturation"]);
        hueChanger.ChangeVal(jsonData["brightness"]);
        
        // reset targets to local player customization
        CharacterCustomization.Instance.Initialize();
        hueChanger.mat = null;
        hueChanger.avatar = CharacterCustomization.Instance;
    }
    
    #endregion

    // Get the local player from anywhere. This function returns null if the local player is null
    public static Player GetLocalPlayer() {
        
        return NetworkClient.localPlayer == null ? null : NetworkClient.localPlayer.GetComponent<Player>();
    }

    public bool GetIsCounselor()
    {
        return isCounselor;
    }

    public void SetCounselor(bool isCoun)
    {
        isCounselor = isCoun;
    }

    public string GetAccount()
    {
        return account;
    }

    public void SetAccount(string accName)
    {
        account = accName;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }
    
    
}
