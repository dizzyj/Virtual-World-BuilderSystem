using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AdminUIController : MonoBehaviour
{
    private VisualElement root;
    private  ScrollView playersScrollView;
    public List<string> priviledges = new List<string> { "Patient", "Counselor", "Admin" };

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        playersScrollView = root.Q<ScrollView>("PlayersScrollView");
        Button saveButton = root.Q<Button>("SaveButton");
        saveButton.clickable.clicked += () => savePermissionChanges(root);
        Button exitButton = root.Q<Button>("Close");
        exitButton.clickable.clicked += () => closeWindow();

        Player[] players = FindObjectsOfType<Player>();
        loadPlayers(players); 
    }

    private void loadPlayers(Player[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            VisualTreeAsset playerOptionsTemplate = Resources.Load<VisualTreeAsset>("PlayerOption");
            VisualElement playerOptions = playerOptionsTemplate.Instantiate();

            playerOptions.Q<Label>("Name").text = players[i].getUserName();
            playerOptions.Q<DropdownField>("Priviledges").choices = priviledges;
            //TODO - Initialize the priviledge show to the player's priviledge
            VisualElement kickImage = playerOptions.Q<VisualElement>("kickImage");
            kickImage.RegisterCallback<ClickEvent>(kickPressed);



            playersScrollView.Add(playerOptions);

        }
    }

    private void kickPressed(ClickEvent evt)
    {
        VisualElement playerOptions = (VisualElement)evt.currentTarget;
        string playerName = playerOptions.Q<Label>("Name").text;
        //TODO - kick them
    }

    //Get permissions from each playerOptions and update them on the database
    private void savePermissionChanges(VisualElement root)
    {
        //TODO - Talk with Griffin about how to do this
    }

    private void closeWindow()
    {
        root.style.display = DisplayStyle.None;
    }

}
