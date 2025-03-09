using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;
using TMPro;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    private NetworkRunner runnerInstance;
    string LobbyId = "Lobby01";
    private void Awake()
    {
        NetworkRunner networkRunnerinScene= FindAnyObjectByType<NetworkRunner>();
        if (networkRunnerinScene != null)
            runnerInstance = networkRunnerinScene;
    }
    private void Start()
    {
        
        if(runnerInstance == null)
        {
            runnerInstance=Instantiate(networkRunnerPrefab);
            runnerInstance.name = "runner Instance";
            InitNR(runnerInstance,GameMode.AutoHostOrClient,"testing",NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));
            Debug.Log("Create a server");
        }
    }
    public void StartHostMigration(HostMigrationToken token)
    {
        if (runnerInstance == null)
        {
            runnerInstance = Instantiate(networkRunnerPrefab);
            runnerInstance.name = "runner Instance-Migrated";
            InitNRHostMigration(runnerInstance, token);
        }
    }
    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sm = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if (sm != null)
        {
            sm=runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return sm;
    }
    protected virtual Task InitNR(NetworkRunner runner,GameMode gameMode, string SessionName,NetAddress address, SceneRef scene)
    {
        var SceneManager=GetSceneManager(runner);
        runner.ProvideInput = true;
        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene=scene,
            SessionName = SessionName,
            CustomLobbyName = LobbyId,
            SceneManager = SceneManager,
            //ConnectionToken=connectionToken
            PlayerCount= 4,
        });
    }
    protected virtual Task InitNRHostMigration(NetworkRunner runner, HostMigrationToken token)
    {
        var SceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;
        return runner.StartGame(new StartGameArgs
        {
            //GameMode = gameMode,
            //Address = address,
            //Scene = scene,
            //SessionName = SessionName,
            //CustomLobbyName = LobbyId,
            SceneManager = SceneManager,
            //ConnectionToken=connectionToken
            HostMigrationToken = token,
            HostMigrationResume=HostResume,
            PlayerCount = 4,
        }) ;
    }

    private void HostResume(NetworkRunner runner)
    {
        foreach( var resume in runner.GetResumeSnapshotNetworkObjects())
        {
            //if(resume.TryGetBehaviour)
            runner.Spawn(new GameObject());
        }
    }

    public void OnJoinLobby()
    {
        JoinLobby();
    }
    async Task JoinLobby()
    {
      var result= await runnerInstance.JoinSessionLobby(SessionLobby.Custom,LobbyId);
        if(!result.Ok)
        {
            Debug.Log("FailedToconnect Lobby");

        }
        else
        {
            Debug.Log("connected Lobby");

        }
    }

    public void CreateGame(string SessionName,string SceneName)
    {
        InitNR(runnerInstance, GameMode.Host, SessionName, NetAddress.Any(), SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"scene/{SceneName}")));
    }
    public void JoinGame(SessionInfo info)
    {
        InitNR(runnerInstance, GameMode.Client,info.Name , NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));
    }

}

