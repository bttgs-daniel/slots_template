using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Mkey
{
    public class SpinButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private Text autoText;
        [SerializeField]
        private SlotControls slotControls;
        [SerializeField]
        private bool holdForAuto = false;

        #region events
        public Action ClickEvent;
        public Action <bool> TrySetAutoEvent;
        #endregion events

        #region temp vars
        private bool up = true;
        private float downTime = 0;
        private Button button;
        #endregion temp vars

        #region regular
        private void Start()
        {
            button = GetComponent<Button>();
            if (slotControls) slotControls.ChangeAutoStateEvent += (auto) => { SetButtonText(); };
            SetButtonText();
        }
        #endregion regular

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button && !button.interactable) return;
            up = false;
            if (!slotControls) return;
            if (slotControls.Auto)
            {
                TrySetAutoEvent?.Invoke(false);
                return;
            }
            downTime = Time.time;
            if(holdForAuto) StartCoroutine(CheckAuto());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            up = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            up = true;
            if (button && !button.interactable) return;
            if (!slotControls) return;
            if (!slotControls.Auto)
            {
                Debug.Log(gameObject.name + " Was up." + (slotControls ? " SpinType: auto - " + slotControls.Auto.ToString() : ""));
                ClickEvent?.Invoke();
            }
        }

        private IEnumerator CheckAuto()
        {
            bool cancel = false;
            WaitForEndOfFrame wef = new WaitForEndOfFrame();
            float dTime;
            while (!up && !cancel)
            {
                dTime = Time.time - downTime;
                if (dTime > 2.0f)
                {
                    TrySetAutoEvent?.Invoke(true);
                    cancel = true;
                }
                yield return wef;
            }
        }

        private void SetButtonText()
        {
            if (autoText && slotControls)
            {
                autoText.text = (!slotControls.Auto) ? "Hold for AutoSpin" : "AUTO";
            }
            else if (autoText)
            {
                autoText.text = "";
            }
        }
    }
}