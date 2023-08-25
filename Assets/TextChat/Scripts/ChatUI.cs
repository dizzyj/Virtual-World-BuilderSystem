using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class ChatUI : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Text chatHistory;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] InputField chatMessage;
    [SerializeField] Button sendButton;



    // This is only set on client to the name of the local player
    internal static string localPlayerName;

    GameObject player;


    // Server-only cross-reference of connections to player names
    internal static readonly Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();

    public override void OnStartServer()
    {
        connNames.Clear();
    }

    public override void OnStartClient()
    {
        chatHistory.text = "";
    }

    [Command(requiresAuthority = false)] //Client to Server
    void CmdSend(string message, NetworkConnectionToClient sender = null)
    {
        if (!connNames.ContainsKey(sender))
        {
            connNames.Add(sender, sender.identity.GetComponent<Player>().playerName);
            //connNames.Add(sender, "Temp");
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            RpcReceive(connNames[sender], message.Trim());
            Debug.Log("Sender: "+connNames[sender]);
            Debug.Log("Sent Message");
        }
    }

    // *** Commenting this out makes the messages appear in the chat window for some reason ***
    /*
     Chat Messages display with [Server] tag
     Chat messages DO NOT display with [ClientRpc] tag

     Possible Issue:
     Game is being hosted as [Server] and not [Server + Client]
     */
    [ClientRpc] //Server to Client
    void RpcReceive(string playerName, string message)
    {
        string prettyMessage = playerName == localPlayerName ? //REPLACE temp with localPlayerName when networking is implemented
            $"<color=red>{playerName}:</color> {message}" :
            $"<color=blue>{playerName}:</color> {message}";
        //Debug.Log("Message: " + message);
        AppendMessage(prettyMessage);
    }

    void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatHistory.text += message + "\n";
        Debug.Log("Message: " + message);

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }

    // Called by UI element ExitButton.OnClick
    public void ExitButtonOnClick()
    {
        // StopHost calls both StopClient and StopServer
        // StopServer does nothing on remote clients
        NetworkManager.singleton.StopHost();
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
            CmdSend(chatMessage.text.Trim());
            chatMessage.text = string.Empty;
            chatMessage.ActivateInputField();
        }
    }
}
