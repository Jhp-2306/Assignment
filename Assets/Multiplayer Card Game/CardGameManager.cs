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
    public enum GameState
    {
        GonnaStart,
        onGoing,
    }
    //List<int> cardNumbers;
    public static CardGameManager Instance;
    float potChips;
    public NetworkPlayer localPLayer;
    public GameObject Slider;
    public Transform profileParent;
    private Dictionary<PlayerRef, int> playerNumbers = new Dictionary<PlayerRef, int>();
    private Dictionary<PlayerRef, bool> playerStatus = new Dictionary<PlayerRef, bool>(); // True = Contest, False = Fold
    private Dictionary<PlayerRef, int> playerBets = new Dictionary<PlayerRef, int>(); // True = AsRasie, False = called
    private List<int> assignedNumbers = new List<int>();
    private int currentPlayerIndex = 0;
    public List<NetworkPlayer> playerList = new List<NetworkPlayer>();
    public TextMeshProUGUI NumberTxt;
    public GameObject StartButton;
    bool gameStarted;
    public TextMeshProUGUI PotSize;
    public BettingUI bettingUI;
    public GameObject RoundStatusGO;
    public TextMeshProUGUI RoundStatustxt;
    public TextMeshProUGUI RoundDeclareStatustxt;
    public TextMeshProUGUI TotalChips;
    bool isBetMatched=false;
    //private GameState _state = GameState.GonnaStart;
    [Networked] public GameState _state { get; set; }
    //int cout = 0;
    [Networked] public int Pot_Size { get; set; } = 0;
    [Networked] public int Rasie { get; set; } = 0;//Max Bet for the currentpot
    bool isSpawnedCalled=false;
    public GameObject MyInput;
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
        isSpawnedCalled = true;
    }
    private void Update()
    {
        if (Runner != null)
        {
            StartButton.SetActive(!gameStarted && Runner.GameMode == GameMode.Host && Runner.ActivePlayers.Count() >= 2);
        }
        if (isSpawnedCalled)
        {
            PotSize.text = $"${Pot_Size}";

        if(localPLayer != null)
            TotalChips.text=$"${localPLayer.Chips.ToString()}";
        }
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
        _state = GameState.onGoing;
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
   
    private void NextTurn()
    {
        currentPlayerIndex++;
        //currentPlayerIndex=currentPlayerIndex>= Runner.ActivePlayers.Count()&&!isBetMatched?0:;
        checkplayerBets();
        Debug.Log("Determine the Winner"+isBetMatched);
        if (!DetermineWinner())
        {
            StartTurn();
        }
    }

    private bool DetermineWinner()
    {
        var remainingPlayers = playerStatus.Where(x => x.Value == true).ToList();
        if (remainingPlayers.Count == 1)
        {
            RPC_DeclareWinner(remainingPlayers[0].Key);
            GetNetworkPlayer(remainingPlayers[0].Key).updateChips(Pot_Size);
            Invoke(nameof(TryStartNewRound), 3f);
            //StartCoroutine(RestartingIN(3));
            return true;
        }
        else if (remainingPlayers.Count > 1 && /*currentPlayerIndex >= Runner.ActivePlayers.Count()&&*/ isBetMatched)//need to change it according to bets
        {
            var winner = remainingPlayers.OrderByDescending(x => playerNumbers[x.Key]).First().Key;

            RPC_DeclareWinner(winner);
            GetNetworkPlayer(winner).updateChips(Pot_Size);
            Invoke(nameof(TryStartNewRound), 3f);
            //StartCoroutine(RestartingIN(3));
            return true;
        }
        else if (remainingPlayers.Count < 1 && /*currentPlayerIndex >= Runner.ActivePlayers.Count() &&*/ isBetMatched)//need to change it according to bets
        {
            //DeclareWinner(null);
            Debug.Log("No one contested. Round is void.");
            StartCoroutine(RestartingIN(3));
            return true;
        }
        else
        {
            return false;
        }

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
        foreach (var player in Runner.ActivePlayers)
        {
            playerStatus[player] = true;
        }
        foreach (var player in Runner.ActivePlayers)
        {
            playerBets[player] = 0;
        }
        foreach (var player in playerList)
        {
            player.PreviousBet = 0;
        }
        assignedNumbers.Clear();
        Pot_Size = 0;
        currentPlayerIndex = 0;
        Rasie = 0;
    }

    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            AssignUniqueNumber(player);
            TryStartNewRound();
        }
        playerBets[player] = 0;
        playerStatus[player] = true;
        Debug.Log($"player Joined addeding him to playerStatus{playerStatus.Count},{playerBets.Count}");
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
        var obj = Instantiate(Slider, profileParent).GetComponent<PlayerProfileUI>();
        obj.SetPlayerName(name);
        return obj;
    }
    NetworkPlayer GetNetworkPlayer(PlayerRef player)
    {
        foreach (var t in playerList)
        {
            if (player.PlayerId == t.playerref.PlayerId)
            {
                return t;
            }
        }
        return null;
    }

    IEnumerator RestartingIN(int delaysec)
    {
        int timer = delaysec;
        while (timer >= 0)
        {
            Debug.Log(timer);
            yield return new WaitForSeconds(1f);
            RoundDeclareStatustxt.text = $"next round starting in... {timer}";
            timer--;
        }
        yield return new WaitForSeconds(1f);
        RoundStatusGO.SetActive(false);
    }

    List<KeyValuePair<PlayerRef,int>> checkplayerBets()
    {
        var BetsnotMatched = playerBets.Where(x => x.Value !=Rasie).ToList();
        Debug.Log($" List Size{playerBets.Count}");
        isBetMatched = BetsnotMatched.Count == 0 && Rasie!=0;
        return BetsnotMatched;
    }

    #region InputMethod
    public void Fold()
    {
        RPC_RequestFold(Runner.LocalPlayer);
    }

    public void Contest()
    {
        RPC_RequestContest(Runner.LocalPlayer);
    }

    public void Bet()
    {
        RPC_Bet(Runner.LocalPlayer, bettingUI.Value);
    }
    #endregion
    #region RPC
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartPlayerTurn(PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId}'s turn started.{GetNetworkPlayer(player) == null}");
        RPC_SetMsg(player, "Its My Turn");
        bettingUI.setSilder(localPLayer.Chips, 10);
        MyInput.SetActive(player == Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestFold(PlayerRef player)
    {
        if (playerStatus.ContainsKey(player) && player == Runner.ActivePlayers.ElementAt(currentPlayerIndex))
        {
            playerStatus[player] = false;
            Debug.Log($"Player {player.PlayerId} folded.");
            RPC_SetMsg(player, "I have Folded");
            NextTurn();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestContest(PlayerRef player)
    {
        if (playerStatus.ContainsKey(player) && player == Runner.ActivePlayers.ElementAt(currentPlayerIndex))
        {
            playerStatus[player] = true;
            Debug.Log($"Player {player.PlayerId} contested{player.PlayerId},{GetNetworkPlayer(player).playerref.PlayerId}.");
            if (Rasie == 0)
            {
                Rasie = 10;
                Pot_Size += Rasie;
                playerBets[player] = Rasie;
                GetNetworkPlayer(player).PreviousBet = 10;
                GetNetworkPlayer(player).updateChips(-10);
            }
            else
            {
                if (GetNetworkPlayer(player).PreviousBet == 0)
                {
                    Pot_Size += Rasie;
                    playerBets[player] = Rasie;
                    GetNetworkPlayer(player).PreviousBet = Rasie;
                    GetNetworkPlayer(player).updateChips(-Rasie);
                }
                else
                {
                    var diff = Mathf.Abs(Rasie - GetNetworkPlayer(player).PreviousBet);
                    Pot_Size += diff;
                    playerBets[player] = Rasie;
                    GetNetworkPlayer(player).PreviousBet = Rasie;
                    GetNetworkPlayer(player).updateChips(-diff);
                }
            }
            RPC_SetMsg(player, "I have Called");
            NextTurn();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DeclareWinner(PlayerRef winner)
    {
        //GetNetworkPlayer(winner).
        RoundStatusGO.SetActive(true);
        if (Runner.LocalPlayer != winner)
        {
            RoundStatustxt.text = "You Lost";

            Debug.Log("You Lost");
        }
        else
        {
            RoundStatustxt.text = "You Won";
            Debug.Log("You Won");
        }
        Debug.Log($"Player {winner.PlayerId} wins with number: {playerNumbers[winner]}");
        StartCoroutine(RestartingIN(3));
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Bet(PlayerRef player, int Amount)
    {
        if (playerStatus.ContainsKey(player) && player == Runner.ActivePlayers.ElementAt(currentPlayerIndex))
        {
            playerStatus[player] = true;
            Debug.Log($"Player {player.PlayerId} Bet {Amount}.");
            //TODO: Writing the Betting Logic
            Pot_Size += Amount;
            Rasie = GetNetworkPlayer(player).PreviousBet + Amount;
            playerBets[player] = Rasie;
            GetNetworkPlayer(player).PreviousBet = Rasie;
            GetNetworkPlayer(player).updateChips(-Amount);
            checkplayerBets();
            RPC_SetMsg(player, "I have Folded");
            NextTurn();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetMsg(PlayerRef player, string msg)
    {
        if (player != Runner.LocalPlayer && GetNetworkPlayer(player) != null)
            GetNetworkPlayer(player).SentMsg(msg);
    }


    #endregion
}
