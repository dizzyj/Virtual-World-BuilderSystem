using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.MultipleAdditiveScenes;

public class VW_ChatBehavior : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Text chatHistory;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] InputField chatMessage;
    [SerializeField] Button sendButton;

    // Server-only cross-reference of connections to player names
    internal static readonly Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();

    GameObject player;
    public static Player localPlayer;


    private void Start()
    {
        //Display some info to the player in the chat window
        AddMessageAndScroll("Use CTRL+C to hide/show the chat window!\n");
        AddMessageAndScroll("Use /w NAME to send private messages\n");
        AddMessageAndScroll("---VW Counseling---\n");
    }

    [Command(requiresAuthority = false)] //Client to Server
    void CmdSend(string message, NetworkConnectionToClient sender = null)
    {

        if (!connNames.ContainsKey(sender))
        {
            connNames.Add(sender, sender.identity.GetComponent<Player>().playerName);
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            //Debug.Log(messageWithName);
            RpcReceiveGlobalMessage(message.Trim());
            //Debug.Log("Sent Message");
        }
    }

    //PRIVATE MESSAGES
    [Command(requiresAuthority = false)]
    void CmdSendWhisper(string to, string message)
    {
        Player[] players = FindObjectsOfType<Player>();
        //FIND matching player object
        foreach (Player player in players)
        {
            if(player.getUserName() == to)
            {
                NetworkIdentity receiverID = player.GetComponent<NetworkIdentity>();
                TargetSendWhisper(receiverID.connectionToClient, message); //for receiver
            }
        }
        Debug.Log("Player does not exist!");

    }

    [TargetRpc]
    public void TargetSendWhisper(NetworkConnection receiver, string message)
    {
        Debug.Log("Message: " + message);
        AddMessageAndScroll(message);
    }

    [ClientRpc]
    void RpcReceiveGlobalMessage(string message)
    {

        // REIMPLEMENT COLORS LATER

        //string messageWithName;

        /*
        if (playerName == p.getUserName()) //local player name
        {
            messageWithName = $"<color=red>{playerName}:</color> {message}";
        }
        else
        {
            messageWithName = $"<color=blue>{playerName}:</color> {message}";
        }
        */

        Debug.Log("Message: " + message);
        AddMessageAndScroll(message);
    }

    // GLOBAL SERVER MESSAGES
    public void SendServerMessage(string message)
    {
        CmdSendServerMessage(message);
    }

    [Command(requiresAuthority = false)]
    void CmdSendServerMessage(string message)
    {
        //Server message are green and spaced out
        string serverMessage = $"<color=green>---{message}---\n</color>";

        RpcServerMessage(serverMessage);
    }

    [ClientRpc]
    public void RpcServerMessage(string message)
    {
        AddMessageAndScroll(message);
    }


    void AddMessageAndScroll(string message)
    {
        chatHistory.text += message + "\n";

        scrollbar.value = 0;
    }

    // Called by UI element MessageField.OnValueChanged
    public void ToggleButton(string input)
    {
        sendButton.interactable = !string.IsNullOrWhiteSpace(input);
    }

    // Called by UI element MessageField.OnValueChanged
    public void DisablePlayerMovement()
    {
        //find the Player object
        player = GameObject.FindGameObjectWithTag("Player");
        //disable movement
        player.GetComponent<PlayerMovement>().enabled = false;
    }

    // Called by UI element MessageField.OnSubmit
    public void EnablePlayerMovement()
    {
        //find the Player object
        player = GameObject.FindGameObjectWithTag("Player");
        //enable movement
        player.GetComponent<PlayerMovement>().enabled = true;
    }

    // Called by UI element MessageField.OnSubmit
    public void OnEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit"))
            SendMessage();
    }

    // Called by OnEndEdit above and UI element SendButton.OnClick
    public void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(chatMessage.text))
        {

            Debug.Log("Server?:" + isServer);

            //player = GameObject.FindGameObjectWithTag("Player");
            //p = player.GetComponent<Player>();

            string senderName = localPlayer.getUserName();

            //check for private message command
            if (chatMessage.text.StartsWith("/w")){

                (string toUser, string message) = ParsePM("/w", chatMessage.text);
                if (!string.IsNullOrWhiteSpace(toUser) && !string.IsNullOrWhiteSpace(message))
                {
                    Debug.Log("Sending private message to " + toUser + " from " + senderName);
                    string receiverMessage = $"<color=blue>{senderName}</color>->{toUser}: {message}";
                    AddMessageAndScroll(receiverMessage); //display on sender's chat UI
                    CmdSendWhisper(toUser, receiverMessage); //send to receiver
                    chatMessage.text = string.Empty;
                    chatMessage.ActivateInputField();
                }
                else Debug.Log("Invalid whisper format");
            }
            else
            {
                string messageWithName = $"<color=red>{localPlayer.getUserName()}</color>: {chatMessage.text.Trim()}";

                CmdSend(messageWithName);
                chatMessage.text = string.Empty;
                chatMessage.ActivateInputField();
            }

        }
    }

    // parse a private message
    private (string user, string message) ParsePM(string command, string pm)
    {
        // parse /w
        string content = pm.StartsWith(command + " ") ? pm.Substring(command.Length + 1) : "";

        // now split the content in "user msg"
        if (content != "")
        {
            // find the first space that separates the name and the message
            int i = content.IndexOf(" ");
            if (i >= 0)
            {
                string user = content.Substring(0, i);
                string msg = content.Substring(i + 1);
                return (user, msg);
            }
        }
        return ("", "");
    }

}
