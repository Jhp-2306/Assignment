using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerProfileUI : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Msg;
    public GameObject MsgBox;

    public void SetPlayerName(string Player)
    {
        Name.text = Player;
    }
    public void SetMSgBox(string _msg)
    {
        Msg.text=_msg;
        StartCoroutine(ActivateMsgBox());
    }

    IEnumerator ActivateMsgBox()
    {
        MsgBox.SetActive(true);
        yield return new WaitForSeconds(5f);
        MsgBox.SetActive(false);
    }
}
