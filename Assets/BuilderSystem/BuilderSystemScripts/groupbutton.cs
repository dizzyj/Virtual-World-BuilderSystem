using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class groupbutton : MonoBehaviour
{
    public Button GroupButton;
    public Button SaveButton;
    public Button Prefab1;
    public Button Prefab2;
    public Button EditButton;
    public Button SaveEditButton;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        Prefab1.gameObject.SetActive(false);
        Prefab2.gameObject.SetActive(false);
        SaveButton.gameObject.SetActive(false);
        GroupButton.onClick.AddListener(GroupButtonHandle);
        SaveButton.onClick.AddListener(SaveButtonHandle);
        EditButton.onClick.AddListener(EditButtonHandle);
        SaveEditButton.onClick.AddListener(SaveEditButtonHandle);
    }
    private void GroupButtonHandle()
    {
        player = Player.GetLocalPlayer();
        SaveButton.gameObject.SetActive(true);
        GroupButton.gameObject.SetActive(false);
        player.GetComponent<PrefabBuild>().buildermode = true;

    }
    private void SaveButtonHandle()
    {
        player = Player.GetLocalPlayer();
        SaveButton.gameObject.SetActive(false);
        GroupButton.gameObject.SetActive(true);
        player.GetComponent<PrefabBuild>().SavePrefab();

    }
    private void EditButtonHandle() 
    {
        player = Player.GetLocalPlayer();
        player.GetComponent<PrefabBuild>().EditPrefab();

    }
    private void SaveEditButtonHandle()
    {
        player = Player.GetLocalPlayer();
        player.GetComponent<PrefabBuild>().SaveEditPrefab();
    }
}
