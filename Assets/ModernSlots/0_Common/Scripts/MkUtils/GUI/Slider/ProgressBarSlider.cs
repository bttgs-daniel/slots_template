using UnityEngine;
using UnityEngine.UI;
/*
	27.02.2020 - first
    18.02.2021 - OnValueChanged;
*/
namespace Mkey
{
    [ExecuteInEditMode]
    public class ProgressBarSlider : PSlider
    {
        [SerializeField]
        private Image[] full;

        #region temp vars
        int fullCount;
        #endregion temp vars

        #region regular
        private void OnValidate()
        {
            fillAmount = Mathf.Clamp01(fillAmount);
        }

        private void Update()
        {
            fullCount = (int)(fillAmount * 10.0f);
            for (int i = 0; i < full.Length; i++)
            {
                if (full[i]) full[i].enabled = (fullCount >= (i + 1));
            }
        }
        #endregion regular
    }
}