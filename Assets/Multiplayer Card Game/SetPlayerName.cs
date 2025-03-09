using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerName : MonoBehaviour
{
    public TMPro.TMP_InputField name;
    public GameObject NamePanel;
    public GameObject LobbyPanel;
    public void AddPlayerName()
    {
        PlayerPrefs.SetString(Constants.ConstantsString.playerNamePrefsID, name.text);
        NamePanel.SetActive(false);
        NamePanel.SetActive(true);

        NetworkRunnerHandler handler = FindAnyObjectByType<NetworkRunnerHandler>();
        if (handler != null)
        {
            handler.OnJoinLobby();
        }
        else
        {
            Debug.Log("Failed to identity the networkrunnerhandler script");
        }
    }

}

namespace Constants
{
    static class ConstantsString
    {
        public const string playerNamePrefsID = "Player_Name";
    }
}
