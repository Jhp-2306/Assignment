using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    string playername;
    private float chips=100;

    public void Add(float _chips)
    {
        chips += _chips;
    }
}
