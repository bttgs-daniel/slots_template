using System;
using UnityEngine;
using UnityEngine.UI;
/*
 13.01.19
    -add fillImage exist
 13.05.20
    - PSlider
  18.02.2021 - OnValueChanged;
 */

namespace Mkey
{
    [ExecuteInEditMode]
    public class SimpleSlider :  PSlider
    {
        public Image fillImage;

        #region temp vars
        private RectTransform rtL;
        private RectTransform rtR;
        #endregion temp vars

        #region regular
        private void OnEnable()
        {
          
        }

        private void OnValidate()
        {
            fillAmount = Mathf.Clamp01(fillAmount);
        }

        private void Update()
        {
            if (!fillImage) return;
            fillImage.fillAmount = fillAmount;
        }
        #endregion regular

    }
}