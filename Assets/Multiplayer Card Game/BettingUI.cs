using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BettingUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI BetAmount;
    public int Value;
    public void setSilder(int max, int min)
    {
        if (max == min)
        {
            slider.gameObject.SetActive(false);
        }
        else
        {
            slider.minValue = min;
            slider.maxValue = max;
        }
    }
    public void OnValue()
    {
        Value = (int)slider.value;
        BetAmount.text = $"${Value}";
    }
}

