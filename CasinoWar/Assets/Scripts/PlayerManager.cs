using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public GameObject Player1;
    public GameObject Player2;
    public GameObject Player3;
    public GameObject Player4;
    public GameObject Player5;
    public GameObject Player6;
    public GameObject DealerCard;
    public GameObject PlayerCardDisplay;
    public GameObject DealerCardDisplay;
    public List<GameObject> cards = new List<GameObject>();

    [SyncVar]
    public int playerIndex; // This will sync the player's index across the network
    public int ClientCount;
    public enum PlayerWinLoseStartus { None, Win, Lose, Tie }
    public PlayerWinLoseStartus PlayersStatus;

    private bool IsHost;
    private GameObject mainCanvas;
    private GameObject ChildCanvas;
    private TextMeshProUGUI StatusText;

    // List of available player indices
    private static List<int> availableIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    private static List<GameObject> connectedPlayers = new List<GameObject>();
    private static List<NetworkIdentity> DealCards = new List<NetworkIdentity>();
    
    private NetworkIdentity DealersCard;
    private NetworkIdentity PlayersCard;

    void Awake()
    {
        IsHost = NetworkServer.active && NetworkClient.active;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        AssignPlayerIndex(); // Assign an index when a player connects

        // Store the connected players
        connectedPlayers.Add(gameObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        mainCanvas = GameObject.Find("MainCanvas");
        ChildCanvas = mainCanvas.transform.Find(IsHost ? "HostCanvas" : "PlayerCanvas").gameObject;
        ChildCanvas.SetActive(true);

        if (IsHost)
        {
            Player1 = GameObject.Find("Player1");
            Player2 = GameObject.Find("Player2");
            Player3 = GameObject.Find("Player3");
            Player4 = GameObject.Find("Player4");
            Player5 = GameObject.Find("Player5");
            Player6 = GameObject.Find("Player6");
            DealerCard = GameObject.Find("Dealer'sCard");
        }
        else
        {
            PlayerCardDisplay = GameObject.Find("PlayerCardDisplay");
            DealerCardDisplay = GameObject.Find("DealerCardDisplay");
        }

        UpdateStatusOfPlayer(playerIndex, true);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        // Free the player's index when they disconnect
        availableIndices.Add(playerIndex);
        connectedPlayers.Remove(gameObject);

        UpdateStatusOfPlayer(playerIndex, false);
    }

    private void AssignPlayerIndex()
    {
        if (availableIndices.Count > 0)
        {
            playerIndex = availableIndices.Min(); // Assign the lowest available index
            availableIndices.Remove(playerIndex); // Remove this index from the available pool
        }
        else
        {
            Debug.LogWarning("No available player indices.");
        }
    }

    //private void Update()
    //{
    //    if (isLocalPlayer)
    //    {
    //        if(PlayersStatus == PlayerWinLoseStartus.Lose)
    //        {
    //            Debug.Log("Lose");
    //        }
    //        else if (PlayersStatus == PlayerWinLoseStartus.Win)
    //        {
    //            Debug.Log("Win");
    //        }
    //        else if (PlayersStatus == PlayerWinLoseStartus.Tie)
    //        {
    //            Debug.Log("Tie");
    //        }
    //    }
    //}

    [Command]
    public void CmdDealCard(bool IsFirstClick)
    {
        if (IsFirstClick)
        {
            ClientCount = connectedPlayers.Count - 1;
        }

        if (ClientCount < 0)
        {
            return;
        }

        int CardIndex = Random.Range(0, cards.Count);
        GameObject CardObj = Instantiate(cards[CardIndex], new Vector2(0, 0), Quaternion.identity);
        NetworkIdentity networkIdentity = CardObj.GetComponent<NetworkIdentity>();
        DealCards.Add(networkIdentity);
        NetworkServer.Spawn(CardObj);
        
        int IndexAtDealCard = connectedPlayers.Count - (ClientCount--);
        if (IndexAtDealCard == connectedPlayers.Count)
        {
            IndexAtDealCard = 0;
            DealersCard = networkIdentity;
            foreach (GameObject item in connectedPlayers)
            {
                item.GetComponent<PlayerManager>().DealersCard = networkIdentity;
            }
        }

        connectedPlayers[IndexAtDealCard].GetComponent<PlayerManager>().PlayersCard = networkIdentity;

        RpcShowCard(CardObj, IndexAtDealCard);
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, int index)
    {
        if (IsHost)
        {
            Transform targetParent = null;
            switch (index)
            {
                case 0:
                    targetParent = DealerCard.transform;
                    break;
                case 1:
                    targetParent = Player1.transform;
                    break;
                case 2:
                    targetParent = Player2.transform;
                    break;
                case 3:
                    targetParent = Player3.transform;
                    break;
                case 4:
                    targetParent = Player4.transform;
                    break;
                case 5:
                    targetParent = Player5.transform;
                    break;
                case 6:
                    targetParent = Player6.transform;
                    break;
            }

            if (targetParent != null)
            {
                card.transform.SetParent(targetParent, false);
            }

            
        }
        else
        {
            if (index == 0)
            {
                card.transform.SetParent(DealerCardDisplay.transform, false);
            }
            else if (index == NetworkClient.localPlayer.GetComponent<PlayerManager>().GetPlayerIndex())
            {
                card.transform.SetParent(PlayerCardDisplay.transform, false);
            }
        }
    }

    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    [Server]
    void UpdateStatusOfPlayer(int index, bool status = false)
    {
        string tempText = status ? "Online" : "Offline";

        switch (index)
        {
            case 1:
                StatusText = Player1.GetComponentInChildren<TextMeshProUGUI>();
                StatusText.text = tempText;
                break;
            case 2:
                StatusText = Player2.GetComponentInChildren<TextMeshProUGUI>();
                StatusText.text = tempText;
                break;
            case 3:
                StatusText = Player3.GetComponentInChildren<TextMeshProUGUI>();
                StatusText.text = tempText;
                break;
            case 4:
                StatusText = Player4.GetComponentInChildren<TextMeshProUGUI>();
                StatusText.text = tempText;
                break;
            case 5:
                StatusText = Player5.GetComponentInChildren<TextMeshProUGUI>();
                StatusText.text = tempText;
                break;
            case 6:
                StatusText = Player6.GetComponentInChildren<TextMeshProUGUI>();
                StatusText.text = tempText;
                break;
        }
    }



    [Command]
    public void CmdFlipCards()
    {
        foreach (NetworkIdentity cardForFlip in DealCards)
        {
            if (cardForFlip != null)
            {
                RpcFlipCard(cardForFlip);
            }
            else
            {
                Debug.LogError("Card in DealCards is null!");
            }
        }

        //CheckForWinLose();
    }

    [ClientRpc]
    void RpcFlipCard(NetworkIdentity card)
    {
        if (card != null)
        {
            // Check if the GameObject has a 'Card' component
            Card cardComponent = card.GetComponent<Card>();
            if (cardComponent != null)
            {
                // Flip the card on all clients
                cardComponent.FlipCard();
            }
            else
            {
                Debug.LogError("Card component not found on the object!");
            }          

        }
        else
        {
            Debug.LogError("NetworkIdentity reference is null!");
        }
    }

    //void CheckForWinLose()
    //{
    //    foreach (var item in connectedPlayers)
    //    {
    //        if (item.GetComponent<PlayerManager>().PlayersCard.GetComponent<Card>().CardValue ==
    //            item.GetComponent<PlayerManager>().DealersCard.GetComponent<Card>().CardValue)
    //        {
    //            item.GetComponent<PlayerManager>().PlayersStatus = PlayerWinLoseStartus.Tie;
    //        }
    //        else if (item.GetComponent<PlayerManager>().PlayersCard.GetComponent<Card>().CardValue <
    //            item.GetComponent<PlayerManager>().DealersCard.GetComponent<Card>().CardValue)
    //        {
    //            item.GetComponent<PlayerManager>().PlayersStatus = PlayerWinLoseStartus.Lose;
    //        }
    //        else if (item.GetComponent<PlayerManager>().PlayersCard.GetComponent<Card>().CardValue >
    //            item.GetComponent<PlayerManager>().DealersCard.GetComponent<Card>().CardValue)
    //        {
    //            item.GetComponent<PlayerManager>().PlayersStatus = PlayerWinLoseStartus.Win;
    //        }
    //    }
    //}
}
