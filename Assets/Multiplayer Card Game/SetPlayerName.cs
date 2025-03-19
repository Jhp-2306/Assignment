using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetPlayerName : MonoBehaviour
{
    public TMPro.TMP_InputField name;
    public GameObject NamePanel;
    public GameObject LobbyPanel;
    public void AddPlayerName()
    {
        PlayerPrefs.SetString(Constants.ConstantsString.playerNamePrefsID, name.text);
        SceneManager.LoadScene(1);
    }

}

namespace Constants
{
    static class ConstantsString
    {
        public const string playerNamePrefsID = "Player_Name";
    }
}
