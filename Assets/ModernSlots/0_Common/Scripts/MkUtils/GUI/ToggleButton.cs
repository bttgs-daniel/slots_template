using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
  28.02.2020 - first
  18.02.2021 - SetOnWithoutNotify
 */

namespace Mkey
{
    public class ToggleButton : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Sprite onSprite;
        [SerializeField]
        private Sprite offSprite;
        [SerializeField]
        private Text onOffText;
        [SerializeField]
        private string onText;
        [SerializeField]
        private string offText;

        public Button.ButtonClickedEvent clickEvent;

        public bool IsOn { get { return isOn; } set { isOn = value; Refresh(); } }
        #region temp vars
        private bool isOn;
        Image image;
        #endregion temp vars

        #region regular

        #endregion regular

        private void Refresh()
        {
            if (image == null) image = GetComponent<Image>();
            if (image) image.sprite = isOn ? onSprite : offSprite;
            if (onOffText) onOffText.text = isOn ? onText : offText;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsOn = !IsOn;
            clickEvent?.Invoke();
        }

        public void SetOnWithoutNotify(bool on)
        {
            isOn = on; 
            Refresh();
        }
    }
}
