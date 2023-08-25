using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Mirror.Examples.MultipleAdditiveScenes;
using UnityEngine.UI;
using UnityEngine.Events;

public class Logout : MonoBehaviour
{
    [SerializeField] private CVWNetworkManager manager;
    [SerializeField] private Button logoutButton;
    //[SerializeField] private UnityEvent logoutEvent;
    // Start is called before the first frame update
    void Start()
    {
        manager = (CVWNetworkManager) CVWNetworkManager.singleton;
        logoutButton.onClick.AddListener(LogOut);
    }

    // Update is called once per frame
    void LogOut()
    {
        ReturnCameraToMainMenu();

        if (manager.mode == NetworkManagerMode.Host) {
            manager.StopHost();
        }
        else {
            manager.StopClient();
        }
    }

    private void Update()
    {
        if (manager == null) {
            manager = (CVWNetworkManager) CVWNetworkManager.singleton;
            return;
        }
    }

    // When player disconnects, camera parent is changed to parent in login scene
    // Calling SceneManager.MoveGameObjectToScene requires game object to not have a parent
    // This holds the parent in the menu scene until the camera has been moved back
    private void ReturnCameraToMainMenu() {
        Transform cameraParentInMenu = Camera.main.transform.parent;
        Camera.main.transform.parent = null;
        SceneManager.MoveGameObjectToScene(Camera.main.gameObject, SceneManager.GetSceneAt(0));
        Camera.main.transform.parent = cameraParentInMenu;
    }
}
