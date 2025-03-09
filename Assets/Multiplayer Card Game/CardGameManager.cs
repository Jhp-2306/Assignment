using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CardGameManager : NetworkBehaviour
{
    //List<int> cardNumbers;
    public static CardGameManager Instance;
    float potChips;
    public NetworkPlayer localPLayer;
    public GameObject Slider;
    public Transform profileParent;
    private Dictionary<PlayerRef, int> playerNumbers = new Dictionary<PlayerRef, int>();
    private Dictionary<PlayerRef, bool> playerStatus = new Dictionary<PlayerRef, bool>(); // True = Contest, False = Fold
    private List<int> assignedNumbers = new List<int>();
    private int currentPlayerIndex = 0;
    public List<NetworkPlayer> playerList = new List<NetworkPlayer>();
    public TextMeshProUGUI NumberTxt;
    public GameObject StartButton;
    bool gameStarted;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void AddPlayerList(NetworkPlayer player)

    {
        playerList.Add(player);
    }
    public override void Spawned()
    {
        StartButton.SetActive(!gameStarted && Runner.GameMode == GameMode.Host && Runner.ActivePlayers.Count() >= 2);
       
    }
    private void Update()
    {        
        if(Runner!=null)
        StartButton.SetActive(!gameStarted&&Runner.GameMode == GameMode.Host && Runner.ActivePlayers.Count() >= 2);
    }
    public void OnStartPressed()
    {
        gameStarted = true;
        if (Runner.IsServer)
        {
            TryStartNewRound();
        }
    }
    private void TryStartNewRound()
    {
        if (Runner.ActivePlayers.Count() >= 2)
        {
            StartNewRound();
        }
        else
        {
            Debug.Log("Waiting for at least 2 players to start the round.");
        }
    }

    private void StartNewRound()
    {
        ResetGameState();
        AssignNumbersToPlayers();
        StartTurn();
    }

    private void StartTurn()
    {
        if (Runner.ActivePlayers.Count() == 0)
            return;

        if (currentPlayerIndex >= Runner.ActivePlayers.Count())
            currentPlayerIndex = 0;

        PlayerRef currentPlayer = Runner.ActivePlayers.ElementAt(currentPlayerIndex);
        Debug.Log($"It's Player {currentPlayer.PlayerId}'s turn.");
        RPC_StartPlayerTurn(currentPlayer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartPlayerTurn(PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId}'s turn started.");
        // Enable input for this player
    }
    public void Fold()
    {
        //RPC_RequestFold(Runner.LocalPlayer);
        localPLayer.RPC_Fold();
    }

    public void Contest()
    {
        //RPC_RequestContest(Runner.LocalPlayer);
        localPLayer.RPC_Call();
    }

    
    public void RPC_RequestFold(PlayerRef player)
    {
        //if (playerStatus.ContainsKey(player) && player == Runner.ActivePlayers.ElementAt(currentPlayerIndex))
        {
            playerStatus[player] = false;
            Debug.Log($"Player {player.PlayerId} folded.");
            NextTurn();
        }
    }

    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestContest(PlayerRef player)
    {
        //if (playerStatus.ContainsKey(player) && player == Runner.ActivePlayers.ElementAt(currentPlayerIndex))
        {
            playerStatus[player] = true;
            Debug.Log($"Player {player.PlayerId} contested.");
            NextTurn();
        }
    }


    private void NextTurn()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= Runner.ActivePlayers.Count())
        {
            Debug.Log("Determine the Winner");
            DetermineWinner();
        }
        else
        {
            StartTurn();
        }
    }

    private void DetermineWinner()
    {
        var remainingPlayers = playerStatus.Where(x => x.Value == true).ToList();
        if (remainingPlayers.Count == 1)
        {
            DeclareWinner(remainingPlayers[0].Key);
        }
        else if (remainingPlayers.Count > 1)
        {
            var winner = remainingPlayers.OrderByDescending(x => playerNumbers[x.Key]).First().Key;
            DeclareWinner(winner);
        }
        else
        {
            Debug.Log("No one contested. Round is void.");
        }

        // Auto-restart the game after 3 seconds only if 2 or more players are present
        Invoke(nameof(TryStartNewRound), 3f);
    }

    private void DeclareWinner(PlayerRef winner)
    {
        Debug.Log($"Player {winner.PlayerId} wins with number: {playerNumbers[winner]}");
    }

    private void AssignNumbersToPlayers()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            AssignUniqueNumber(player);
        }
    }

    private void AssignUniqueNumber(PlayerRef player)
    {
        int randomNumber = GenerateUniqueNumber();
        playerNumbers[player] = randomNumber;
        Debug.Log($"Player {player.PlayerId} assigned number: {randomNumber}");
       
        // Sync the number across the network
        RPC_SetPlayerNumber(player, randomNumber);
    }

    private int GenerateUniqueNumber()
    {
        int randomNumber;
        do
        {
            randomNumber = UnityEngine.Random.RandomRange(1, 101);
        }
        while (assignedNumbers.Contains(randomNumber));

        assignedNumbers.Add(randomNumber);
        return randomNumber;
    }

    private void ResetGameState()
    {
        playerNumbers.Clear();
        playerStatus.Clear();
        assignedNumbers.Clear();
        currentPlayerIndex = 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPlayerNumber(PlayerRef player, int number)
    {
        if (!playerNumbers.ContainsKey(player))
        {
            playerNumbers[player] = number;
        }
        if (player.PlayerId == Runner.LocalPlayer.PlayerId)
        {
            NumberTxt.text = $"Card Number\n{number}";
        }
    }

    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            AssignUniqueNumber(player);
            playerStatus[player] = true;
            TryStartNewRound();
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (playerNumbers.ContainsKey(player))
        {
            assignedNumbers.Remove(playerNumbers[player]);
            playerNumbers.Remove(player);
            playerStatus.Remove(player);
        }
    }
    public PlayerProfileUI AddPlayerProfile(string name)
    {
        //throw new NotImplementedException();
        var obj=Instantiate(Slider,profileParent).GetComponent<PlayerProfileUI>();
        obj.SetPlayerName(name);
        return obj;
    }
    NetworkPlayer GetNetworkPlayer(PlayerRef player)
    {
        foreach(var t in playerList)
        {
            if(player==t.playerref)
            {
                return t;
            }
        }
        return null;
    }
}
