using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slots
{

    public enum SlotsSymbols
    {
        chreey,
        brinjal,
        bell,
        clove,
        lemon,
        coin,
        bar,
        apple,
        heart,
        grape,
        diamond,
        orange,
        horseshoe,
        seven,
        questionmark,
        watermelon,
        end
    }
    public class SlotsManager : MonoBehaviour
    {
        public List<itemPosition> Items;
        public Reel Left, Middle, Right;
        [Range(0f, 100f)]
        public float ProbForthreematch;
        public bool isFixedProbality = false;
        public Button SpinBtn;
        bool isWon;
        public TMPro.TextMeshProUGUI Subtitle;
        Coroutine WinScreen;
        public void Spin()
        {
            SpinBtn.interactable = false;
            isWon = false;
            //Check for the FixedProbality
            //if true get the precentage and run accordingly
            //else Get Random Reward 
            Subtitle.text = " Spinning";
            if (isFixedProbality)
            {
                var isthreematch = Random.RandomRange(0, 100) < ProbForthreematch;
                if (isthreematch)
                {
                    var symbol = GetRandom();
                    Left.StartSpin(symbol, SpinDone);
                    Middle.StartSpin(symbol, SpinDone);
                    Right.StartSpin(symbol, SpinDone);
                    isWon = true;
                    WinScreen = StartCoroutine(DisplayRoundEndMsg());
                }
                else
                {
                    var _left = GetRandom();
                    var _middle = GetRandom();
                    _middle = _middle.y == _left.y ? GetRandomexcluding(Items.IndexOf(_middle)) :_middle;
                    var _right = GetRandom();
                    _right = _right.y == _middle.y ? GetRandomexcluding(Items.IndexOf(_right)) : _right;
                    Left.StartSpin(_left, SpinDone);
                    Middle.StartSpin(_middle, SpinDone);
                    Right.StartSpin(_right, SpinDone);
                    WinScreen = StartCoroutine(DisplayRoundEndMsg());
                }
            }
            else
            {
                var _left = GetRandom();
                var _middle = GetRandom();
                var _right = GetRandom();
                Left.StartSpin(_left, SpinDone);
                Middle.StartSpin(_middle, SpinDone);
                Right.StartSpin(_right, SpinDone);
                if (_left.y == _middle.y && _middle.y == _right.y)
                {
                    //Win method call here
                    isWon=true;
                }
                WinScreen = StartCoroutine(DisplayRoundEndMsg());

            }
        }
        void SpinDone()
        {
            SpinBtn.interactable = true;

        }
        IEnumerator DisplayRoundEndMsg()
        {
            while(Right.IsSpinning|Left.IsSpinning|Middle.IsSpinning)
            {
                yield return null;
            }
            if(isWon)
            {
                Subtitle.text = " You Won";
            }
            else
            {
                Subtitle.text = " You Lost";

            }
        }

        itemPosition GetRandom()
        {
            // var t = [Random.RandomRange[0, Items.Count]];
            return Items[Random.RandomRange(0, Items.Count)];
        }
        itemPosition GetRandomexcluding(int idx)
        {
            // var t = [Random.RandomRange[0, Items.Count]];
            var temp = Random.RandomRange(0, Items.Count);
            while (idx==temp)
            {
                temp = Random.RandomRange(0, Items.Count);
            }
            return Items[temp];
        }

    }
}
