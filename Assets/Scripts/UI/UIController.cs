using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Mirror;
using UnityEngine.Events;
//using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public CVWNetworkManager manager;
    //public VisualTreeAsset debugJoinTemplate;
    private VisualElement root;
    private VisualElement startMenuBase;

    private Player localPlayer;

    public UnityEvent join;

    private List<string> scenes;
    private VisualElement joinBox;
    private VisualElement debugJoin;
    private VisualElement joinMenu;

    // This is only set on client to the name of the local player
    public static string clientUserName;
    public static string serverUserName;
    public enum AccountMessage { Username, Password }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        startMenuBase = root.Q<VisualElement>("base");

        Toggle debugToggle = startMenuBase.Q<Toggle>("DebugToggle");
        debugToggle.RegisterValueChangedCallback(debugClicked);
        TextField usernameField = startMenuBase.Q<TextField>("UsernameField");
        TextField passwordField = startMenuBase.Q<TextField>("PasswordField");
        Button signInButton = startMenuBase.Q<Button>("SignInButton");
        joinBox = startMenuBase.Q<VisualElement>("JoinBox");
        joinMenu = startMenuBase.Q<VisualElement>("JoinMenu");


        VisualTreeAsset debugJoinTemplate = Resources.Load<VisualTreeAsset>("DebugJoin");
        debugJoin = debugJoinTemplate.CloneTree();
        DropdownField sceneSelector = debugJoin.Q<DropdownField>("SceneSelector");
        scenes = new List<string> { "Counselor", "Patient" };
        sceneSelector.choices = scenes;
        Button debugHost = debugJoin.Q<Button>("HostButton");
        debugHost.clicked += () => debugHostClicked(sceneSelector.value);
        TextField ipField = debugJoin.Q<TextField>("IpField");
        Button debugJoinButton = debugJoin.Q<Button>("JoinButton");
        debugJoinButton.clicked += () => debugJoinClicked(ipField.text, usernameField.text, passwordField.text);

        Button JoinButton = startMenuBase.Q<Button>("JoinButton");
        JoinButton.clicked += () => joinClicked("ip placeholder", usernameField.text, passwordField.text);
    }

    private void Update()
    {
        if (localPlayer == null)
        {
            localPlayer = Player.GetLocalPlayer();
            return;
        }
    }


    private void debugClicked(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        {
            joinBox.Clear();
            joinBox.Add(debugJoin);
        }
        else
        {
            joinBox.Clear();
            joinBox.Add(joinMenu);
        }
    }

    private void debugJoinClicked(string ip, string username, string password)
    {
        manager.networkAddress = ip;
        clientUserName = username;
        manager.StartClient();
        disable(startMenuBase);
        join.Invoke();
        CVWNetworkManager.SendAccountMsg(username, password, true);
    }



    private void joinClicked(string ip, string username, string password)
    {
        Debug.Log("join clicked");
        manager.networkAddress = ip;
        clientUserName = username;
        manager.StartClient();
        disable(startMenuBase);
        //if (Database.AccountExists(username))
        //{
            //Database.CreateAccount(username, password);
            join.Invoke();
        //}
        //else
            //Debug.LogWarning("Invalid account");

        CVWNetworkManager.SendAccountMsg(username, password, false);
    }

    private void debugHostClicked(string playerType)
    {
        //set username -- TEMPORARY UNTIL HOST USERNAME UI IS IMPLEMENTED
        serverUserName = "Host";
        if (playerType == "Counselor")
        {
            manager.isCounselor = true;
        }
        else
        {
            manager.isCounselor = false;
        }
        disable(startMenuBase);
        manager.StartHost();
        join.Invoke();
    }

    private void disable(VisualElement ve)
    {
        ve.style.display = DisplayStyle.None;
    }

    private void enable(VisualElement ve)
    {
        ve.style.display = DisplayStyle.Flex;
    }


}
