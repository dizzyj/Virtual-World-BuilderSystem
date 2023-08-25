using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class InstanceTravelUI : MonoBehaviour
{
    public Button waitingRoomButton;
    public Button officeButton;

    private Player localPlayer;
    private PlayerInstanceManager playerInstanceManager;

    void Start() {
        waitingRoomButton.gameObject.SetActive(false);
        officeButton.gameObject.SetActive(true);

        waitingRoomButton.onClick.AddListener(MovePlayerToWaitingRoom);
        officeButton.onClick.AddListener(MovePlayerToOffice);
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null) {
            localPlayer = Player.GetLocalPlayer();
            return;
        }

        if (playerInstanceManager == null) {
            playerInstanceManager = localPlayer.GetComponent<PlayerInstanceManager>();
            return;
        }
    }
    
    public void MovePlayerToWaitingRoom() {
        if (playerInstanceManager == null) return;

        playerInstanceManager.MoveToWaitingRoom();
        waitingRoomButton.gameObject.SetActive(false);
        officeButton.gameObject.SetActive(true);
    }

    public void MovePlayerToOffice() {
        if (playerInstanceManager == null) return;

        playerInstanceManager.CreateAndMoveToOffice();
        waitingRoomButton.gameObject.SetActive(true);
        officeButton.gameObject.SetActive(false);
    }
}
