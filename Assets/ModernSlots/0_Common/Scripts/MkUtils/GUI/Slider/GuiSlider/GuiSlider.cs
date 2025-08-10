using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/*
	22.11.2019 - first
    07.10.2020 -  float parentWidth = parent.rect.width;
    30.03.2021 -  add auto
    19.05.2021 -  fix auto scroll
    24.05.2021 -  ActivateSlides(); public void ShowPrevSlide(); public void ShowNextSlide()
    05.12.2022 - if (slides[i] && slides[i].navi) slides[i].navi.SetActive(current == slides[i]);
*/

namespace Mkey
{
    public class GuiSlider : MonoBehaviour
    {
        [SerializeField]
        private GuiSlide[] slides;
        [SerializeField]
        private GuiSlide current;
        [SerializeField]
        private bool useSlideWidth = true;
        [SerializeField]
        private bool autoScroll;
        [ShowIfTrue("autoScroll"), SerializeField]
        private float changeTime = 3.0f;
        [SerializeField]
        private float speed = 1000;
        [SerializeField]
        private EaseAnim easeSlide = EaseAnim.EaseLinear;

        #region temp vars
        private RectTransform parent;
        private int length = 0;
        private bool moving = false;
        #endregion temp vars

        #region regular
        private void Start()
        {
            parent = GetComponent<RectTransform>();
            length = slides.Length;
            if (length < 2) return;

            for (int i = 0; i < length; i++) // связанный список
            {
                slides[i].Prev  = (i > 0) ? slides[i - 1] : slides[length - 1];
                slides[i].Next = (i < length - 1) ? slides[i + 1] : slides[0];
            }
            if (current == null) { current = slides[0]; ActivateSlides(); }
            SetNavi();
            SetAnchoredPositions(current, length - 1, true);
            StartCoroutine(UpdateC());
        }

        int n = 0;
        private IEnumerator UpdateC()
        {
            yield return new WaitForSeconds(0.5f);
            while (true)
            {
                if (autoScroll)
                {
                    yield return new WaitForSeconds(changeTime);
                    yield return StartCoroutine(MoveToNextC(1, () => { }));
                    n++;

                }
                yield return new WaitForEndOfFrame();
            }
        }
        #endregion regular

        /// <summary>
        /// Set children buttons interactable = activity, toggles, 
        /// </summary>
        /// <param name="activity"></param>
        public void SetControlActivity(bool activity)
        {
            Button[] buttons = GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = activity;
            }

            Toggle[] toggles = GetComponentsInChildren<Toggle>();
            {
                for (int i = 0; i < toggles.Length; i++)
                {
                    toggles[i].interactable = activity;
                }
            }

            InputField[] inputFields = GetComponentsInChildren<InputField>();
            {
                for (int i = 0; i < inputFields.Length; i++)
                {
                    inputFields[i].interactable = activity;
                }
            }
        }

        public void ShowGuiSlide(GuiSlide guiSlide)
        {
            if (moving) return;
            if (current == guiSlide) return;
            GuiSlide nextS = current;
            GuiSlide prevS = current;
            int i = 0;
            bool found = false;
            for (i = 0; i < length; i++)
            {
                nextS = nextS.Next;
                prevS = prevS.Prev;
                if(guiSlide == nextS)
                {
                    i++;
                    found = true;
                    break;
                }
                else if (guiSlide == prevS)
                {
                    i++;
                    found = true;
                    i = -i;
                    break;
                }
            }
            Debug.Log(i);
            if (!found) return;

            SetControlActivity(false);
            bool next = (i > 0);
            if (next)
            { 
                StartCoroutine(MoveToNextC(i, () => { SetControlActivity(true); })); 
            }
            else
            {
                StartCoroutine(MoveToPrevC(-i, () => { SetControlActivity(true); }));
            }
        }

        public void ShowPrevSlide()
        {
            ShowGuiSlide(current.Prev);
        }

        public void ShowNextSlide()
        {
            ShowGuiSlide(current.Next);
        }

        #region move
        private IEnumerator MoveToNextC(int times,  Action completeCallBack)
        {
            if (!moving && times > 0)
            {
                int c = 0;
                while (c < times - 1)
                {
                    yield return StartCoroutine(MoveToNextC(EaseAnim.EaseLinear));
                    c++;
                }
                yield return StartCoroutine(MoveToNextC(easeSlide));
                completeCallBack?.Invoke();
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        private IEnumerator MoveToNextC(EaseAnim ease)
        {
          //  Debug.Log("try to start MoveToNextC()");
            if (!moving)
            {
          //      Debug.Log("start MoveToNextC()");
                moving = true;
                SetAnchoredPositions(current, length - 1, true);
                Vector2 dPos = current.AnchoredPosition - current.Next.AnchoredPosition;
                float time = dPos.magnitude / speed;
               
                ParallelTween pT = new ParallelTween();

                for (int i = 0; i < length; i++)
                {
                    int ii = i;
                    pT.Add((callBack) => { slides[ii].Move(dPos, time, 0, ease, callBack); });
                }
                pT.Start(() => { moving = false; current = current.Next; SetNavi(); ActivateSlides(); });
                yield return new WaitWhile(() => moving);
            //    Debug.Log("end MoveToNextC()");
            }
        }

        private IEnumerator MoveToPrevC(int times, Action completeCallBack)
        {
            if (!moving && times > 0)
            {
                int c = 0;
                while (c < times - 1)
                {
                    yield return StartCoroutine(MoveToPrevC(EaseAnim.EaseLinear));
                    c++;
                }
                yield return StartCoroutine(MoveToPrevC(easeSlide));
                completeCallBack?.Invoke();
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        private IEnumerator MoveToPrevC(EaseAnim ease)
        {
          //  Debug.Log("try to start MoveToPrevC()");
            if (!moving)
            {
              //  Debug.Log("start MoveToPrevC()");
                moving = true;
                SetAnchoredPositions(current, length - 1, false);
                Vector2 dPos = current.AnchoredPosition - current.Prev.AnchoredPosition;
                float time = dPos.magnitude / speed;

                ParallelTween pT = new ParallelTween();

                for (int i = 0; i < length; i++)
                {
                    int ii = i;
                    pT.Add((callBack) => { slides[ii].Move(dPos, time, 0, ease, callBack); });
                }
                pT.Start(() => { moving = false; current = current.Prev; SetNavi(); ActivateSlides(); });
                yield return new WaitWhile(() => moving);
              //  Debug.Log("end MoveToPrevC()");
            }
        }
        #endregion move

        private void SetNavi()
        {
            for (int i = 0; i < length; i++)
            {
              if (slides[i] && slides[i].navi) slides[i].navi.SetActive(current == slides[i]);
            }
        }

        private void ActivateSlides()
        {
            for (int i = 0; i < length; i++)
            {
                slides[i]?.SetCurrentEvent?.Invoke(current == slides[i]);
            }
        }

        private void SetAnchoredPositions(GuiSlide slide, int count, bool after)
        {
            float parentWidth = parent.rect.width;
            GuiSlide s = (after) ? slide.Next : slide.Prev;

            for (int i = 0; i < count; i++)
            {
                if (after)
                {
                    s.SetAnchoredPosition(s.Prev.AnchoredPosition + new Vector2(useSlideWidth ? s.GetDistToPrev() : parentWidth, 0));
                    s = s.Next;
                }
                else
                {
                    s.SetAnchoredPosition(s.Next.AnchoredPosition - new Vector2(useSlideWidth ? s.GetDistToNext() : parentWidth, 0));
                    s = s.Prev;
                }
            }
        }
    }
}