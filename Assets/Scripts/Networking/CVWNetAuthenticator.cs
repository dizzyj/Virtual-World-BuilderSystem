using Mirror;
using Mirror.Authenticators;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CVWNetAuthenticator : BasicAuthenticator
{
    [SerializeField] private UIController controller;
    public void SetCredentials()
    {
        Debug.Log("setcred");
        //serverUsername = controller.GetUserNameField();
    }

    protected override void ServerAccept(NetworkConnectionToClient conn)
    {
        Debug.Log("serveraccept");
        OnServerAuthenticated.Invoke(conn);
        SetCredentials();
    }

}
