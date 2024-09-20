using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class FlipCard : NetworkBehaviour
{
    public PlayerManager playerManager;

    public void OnClick()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();

        playerManager.CmdFlipCards();
    }
}
