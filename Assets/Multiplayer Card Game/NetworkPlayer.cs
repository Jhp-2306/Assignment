using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    //[Networked(onchanged=)]
    public static NetworkPlayer Local { get; set; }
    public PlayerProfileUI playerProfileUI;
    public PlayerProfile profile;
    [Networked] public string playername { get; set; }
    [Networked] public int Chips { get; set; } = 1000;
    [Networked] public int PreviousBet { get; set; } = 0;
    [Networked] public PlayerRef playerref { get; set; }
    [Networked] public bool isPlayerAllIn { get; set; }
    bool isSpawnedCalled = false;
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            CardGameManager.Instance.localPLayer = Local;
            //playername = PlayerPrefs.GetString(Constants.ConstantsString.playerNamePrefsID,$"Random{Runner.LocalPlayer.PlayerId}");
            //RPC_SendMyName();
            Object.name = playername;
            Debug.Log("Spawned player" + Object.StateAuthority + Runner.GameMode);
        }
        else
        {
            Debug.Log("Spawned remote player" + Object.StateAuthority + Runner.GameMode);
            //Invoke("Setname",3f);
            Setname();
        }
        CardGameManager.Instance.AddPlayerList(this);
        isSpawnedCalled = true;
    }
    private void Update()
    {
        if (isSpawnedCalled)
        {
            Debug.Log("player" + Object.Id + playername);
            //    if (Object.HasInputAuthority)
            //    playername = PlayerPrefs.GetString(Constants.ConstantsString.playerNamePrefsID, $"Random{Runner.LocalPlayer.PlayerId}");
            //else
            //playerProfileUI.SetPlayerName(Object.name);
        }
    }
    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    public void SentMsg(string msg)
    {
        if (playerProfileUI != null)
            playerProfileUI.SetMSgBox(msg);
    }
    public void Setname()
    {
        playerProfileUI = CardGameManager.Instance.AddPlayerProfile(Object.Id.ToString());
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMyName()
    {
        playername = PlayerPrefs.GetString(Constants.ConstantsString.playerNamePrefsID, $"Random{Runner.LocalPlayer.PlayerId}");
        if(playerProfileUI != null)
        playerProfileUI.SetPlayerName(playername);
    }
    public void updateChips(int _chips)
    {
        Chips += _chips;
    }
}
