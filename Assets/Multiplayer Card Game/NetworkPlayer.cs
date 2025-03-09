using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class NetworkPlayer : NetworkBehaviour,IPlayerLeft
{
    //[Networked(onchanged=)]
    public static NetworkPlayer Local { get; set; }
    public PlayerProfileUI playerProfileUI;
    public PlayerRef playerref;
    public override void Spawned()
    {        
        if (Object.HasInputAuthority)
        {
            Local = this;
            CardGameManager.Instance.localPLayer = Local;
        }
        else
        {
            Debug.Log("Spawned remote player"+Object.StateAuthority+Runner.GameMode);
            playerProfileUI = CardGameManager.Instance.AddPlayerProfile(Object.Name);
        }
        CardGameManager.Instance.AddPlayerList(this);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Fold()
    {
        CardGameManager.Instance.RPC_RequestFold(Runner.LocalPlayer);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Call()
    {
        CardGameManager.Instance.RPC_RequestFold(Runner.LocalPlayer);
    }
    public void PlayerLeft(PlayerRef player)
    {
        if(player==Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    public void SentMsg(string msg)
    {
        if(playerProfileUI != null) 
        playerProfileUI.SetMSgBox(msg);
    }

    
}
