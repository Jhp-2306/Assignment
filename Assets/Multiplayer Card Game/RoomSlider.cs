using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSlider : MonoBehaviour
{
    TMPro.TextMeshProUGUI RoomName;

    void SetRoom(string roomName)
    {
        RoomName.text = roomName;
    }
    public void joinGame()
    {

    }
}
