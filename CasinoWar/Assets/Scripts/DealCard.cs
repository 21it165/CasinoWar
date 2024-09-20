using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
public class DealCard : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    
    private int Click = 0;
    private bool IsFirstClick;

    public override void OnStopServer()
    {
        base.OnStopServer();
        Click = 0;
    }

    public void Onclick()
    {
        if(Click == 0)
        {
            IsFirstClick = true;
        }
        else
        {
            IsFirstClick = false;
        }
        Click++;
        
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();

        PlayerManager.CmdDealCard(IsFirstClick);
        
    }
}
