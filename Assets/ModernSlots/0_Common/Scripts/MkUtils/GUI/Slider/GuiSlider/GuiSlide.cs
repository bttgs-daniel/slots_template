using System;
using UnityEngine;
using UnityEngine.Events;
/*
    01.04.2021
    19.05.2021 add OnDestroy()
    24.05.2021 - SetCurrentEvent;
 */

namespace Mkey
{
    public class GuiSlide : MonoBehaviour
    {
        public GameObject navi;
        public RectTransform SlideRT => (!slide) ? slide = GetComponent<RectTransform>() : slide;
        public GuiSlide Prev { get; set; }
        public GuiSlide Next { get; set; }

        public UnityEvent <bool> SetCurrentEvent;
        #region temp vars
        private RectTransform slide;
        #endregion temp vars


        private void OnDestroy()
        {
            if (gameObject) SimpleTween.Cancel(SlideRT.gameObject, false) ;
        }

        public float Width => (SlideRT) ? SlideRT.rect.width : 0;

        public Vector2 AnchoredPosition => (SlideRT) ? SlideRT.anchoredPosition : Vector2.zero;

        public void Move(Vector2 dPosition, float time, float delay, EaseAnim ease, Action completeCallBack)
        {
            if (!SlideRT)
            {
                completeCallBack?.Invoke();
                return;
            }

            Vector2 cPos = SlideRT.anchoredPosition;
            SimpleTween.Value(SlideRT.gameObject, cPos, cPos + dPosition, time).SetOnUpdate((pos) =>
            {
                if (SlideRT) SlideRT.anchoredPosition = pos;
            })
                .SetDelay(delay)
                .SetEase(ease)
                .AddCompleteCallBack(completeCallBack);
        }

        public float GetDistToNext()
        {
            return (SlideRT && Next != null) ? (Width + Next.Width) / 2f : 0;
        }

        public float GetDistToPrev()
        {
            return (SlideRT && Prev != null) ? (Width + Prev.Width) / 2f : 0;
        }

        public void SetAnchoredPosition(Vector2 aPosition)
        {
            if (SlideRT) SlideRT.anchoredPosition = aPosition;
        }
    }
}