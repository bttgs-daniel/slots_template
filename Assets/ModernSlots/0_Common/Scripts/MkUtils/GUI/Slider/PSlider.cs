using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/*
	27.02.2020 - first
    18.02.2021 - OnValueChanged;
*/
namespace Mkey
{
    public class PSlider : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1f)]
        protected float fillAmount;

        public float FillAmount { get { return fillAmount; } }

        public UnityEvent <float> OnValueChanged;

        public virtual void SetFillAmount(float amount)
        {
            float fA = Mathf.Clamp01(amount);
            bool changed = (fillAmount != fA);
            fillAmount = fA;
            if (changed) OnValueChanged?.Invoke(fillAmount);
        }

        public void AddAmount(float addAmount)
        {
            float fA = Mathf.Clamp01(fillAmount + addAmount);
            SetFillAmount(fA);
        }

        public void SetValueWithoutNotify(float amount)
        {
            fillAmount = Mathf.Clamp01(amount);
        }
    }
}