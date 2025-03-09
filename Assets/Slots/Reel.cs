using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slots
{
    [System.Serializable]
    public struct itemPosition
    {
        public SlotsSymbols Symbol;
        public float y;
    }
    public class Reel : MonoBehaviour
    {
        public List<itemPosition> Items;
        public float SpinSpeed = 1000f;
        public float SpinDuration = 2f;
        public float ReelResetPosition;
        public float ReelStartPosition;
        private bool isSpinning = false;
        public bool IsSpinning { get { return isSpinning; } }
        RectTransform myTransform;

        private void Start()
        {
            myTransform = this.gameObject.GetComponent<RectTransform>();
            //Setitems();//editor helper Script
        }
        public void StartSpin(itemPosition pos,Action callback)
        {
            if (!isSpinning)
                StartCoroutine(SpinReel(pos,callback));
        }

        private IEnumerator SpinReel(itemPosition pos, Action callback)
        {
            isSpinning = true;
            float elapsedTime = 0f;

            while (elapsedTime < SpinDuration)
            {
                myTransform.anchoredPosition -= Vector2.down * SpinSpeed * Time.deltaTime;

                if (myTransform.anchoredPosition.y >= ReelResetPosition)
                {
                    myTransform.anchoredPosition = new Vector2(myTransform.anchoredPosition.x, ReelStartPosition);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            StopReel(pos,callback);
        }

        private void StopReel(itemPosition pos, Action callback)
        {
            myTransform.anchoredPosition = new Vector2(myTransform.anchoredPosition.x, pos.y);
            isSpinning = false;
            callback();
        }

        #region Editor helper Script
        //void Setitems()
        //{
        //    for (int i = 0; i < (int)SlotsSymbols.end; i++)
        //    {
        //        itemPosition _item = new itemPosition();
        //        _item.y = ReelStartPosition+(290*i);
        //        _item.Symbol = (SlotsSymbols)i;
        //        Items.Add(_item);
        //    }
        //}
        #endregion
    }
}
