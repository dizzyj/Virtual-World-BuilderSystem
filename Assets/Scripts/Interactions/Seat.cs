using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Seat : NetworkBehaviour
{
    public Transform userSittingPoint;

    [SyncVar]
    private Player playerInSeat;

    public bool IsOccupied() {
        return playerInSeat != null;
    }

    public void AssignPlayerToSeat(Player player) {
        playerInSeat = player;
    }

    public void RemovePlayerFromSeat() {
        playerInSeat = null;
    }
}
