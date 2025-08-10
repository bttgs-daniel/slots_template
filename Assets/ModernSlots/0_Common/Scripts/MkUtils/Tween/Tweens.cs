using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    09.07.2020 - first
    23.07.2020 - add DelayAction
    12.10.2020 - tween color
    12.09.2022 - fix tween value calculation error
    22.12.2022 - tween double
	26.09.2023 - RotateZTween
	31.03.2025 - tween ValuesTween
    18.06.2025 - fix ValuesTween time
 */
namespace Mkey
{
    public class TweenIntValue
    {
        private int tValue;
        private int tweenId;
        private float maxTweenTime;
        private float minTweenTime;
        private Action<int> onUpdate;
        private GameObject g;
        private bool onlyPositive;

        public TweenIntValue(GameObject g, int initValue, float minTweenTime, float maxTweenTime, bool onlyPositive, Action<int> onUpdate)
        {
            tValue = initValue;
            this.maxTweenTime = Mathf.Max(0, maxTweenTime);
            this.minTweenTime = Mathf.Clamp(minTweenTime, 0, maxTweenTime);
            this.g = g;
            this.onlyPositive = onlyPositive;
            this.onUpdate = onUpdate;
        }

        public void Tween(int newValue, int valuePerSecond)
        {
            SimpleTween.Cancel(tweenId, false);
            float add = newValue - tValue;

            if ((add > 0 && onlyPositive) || !onlyPositive)
            {
                valuePerSecond = Mathf.Max(1, valuePerSecond);
                float tT = Mathf.Abs((float)add / (float)valuePerSecond);
                tT = Mathf.Clamp(tT, minTweenTime, maxTweenTime);
                int oldValue = tValue;
                tweenId = SimpleTween.Value(g, 0, 1, tT).SetOnUpdate((float val) =>
                {
                    if (this != null)
                    {
                        tValue = MathMk.Lerp( oldValue, newValue, val);
                        onUpdate?.Invoke(tValue);
                    }
                }).AddCompleteCallBack(()=> { tValue = newValue; onUpdate?.Invoke(tValue); }).ID;
            }
            else if (onlyPositive)
            {
                if (this != null)
                {
                    tValue = newValue;
                    onUpdate?.Invoke(tValue);
                }
            }
        }
    }

    public class TweenLongValue
    {
        private long tValue;
        private int tweenId;
        private float maxTweenTime;
        private float minTweenTime;
        private Action<long> onUpdate;
        private GameObject g;
        private bool onlyPositive;

        public TweenLongValue(GameObject g, long initValue, float minTweenTime, float maxTweenTime, bool onlyPositive, Action<long> onUpdate)
        {
            tValue = initValue;
            this.maxTweenTime = Mathf.Max(0, maxTweenTime);
            this.minTweenTime = Mathf.Clamp(minTweenTime, 0, maxTweenTime);
            this.g = g;
            this.onlyPositive = onlyPositive;
            this.onUpdate = onUpdate;
        }

        public void Tween(long newValue, long valuePerSecond)
        {
            SimpleTween.Cancel(tweenId, false);
            long add = newValue - tValue;

            if ((add > 0 && onlyPositive) || !onlyPositive)
            {
                valuePerSecond = Math.Max(1, valuePerSecond);
                float tT = Mathf.Abs((float)add / (float)valuePerSecond);
                tT = Mathf.Clamp(tT, minTweenTime, maxTweenTime);
                long oldValue = tValue;
                tweenId = SimpleTween.Value(g, 0, 1, tT).SetOnUpdate((float val) =>
                {
                    if (this != null)
                    {
                        tValue = MathMk.Lerp(oldValue, newValue, val);
                        onUpdate?.Invoke(tValue);
                    }
                }).AddCompleteCallBack(() => { tValue = newValue; onUpdate?.Invoke(tValue); }).ID;
            }
            else if (onlyPositive)
            {
                if (this != null)
                {
                    tValue = newValue;
                    onUpdate?.Invoke(tValue);
                }
            }
        }
    }

    public class TweenDoubleValue
    {
        private double tValue;
        private int tweenId;
        private float maxTweenTime;
        private float minTweenTime;
        private Action<double> onUpdate;
        private GameObject g;
        private bool onlyPositive;

        public TweenDoubleValue(GameObject g, double initValue, float minTweenTime, float maxTweenTime, bool onlyPositive, Action<double> onUpdate)
        {
            tValue = initValue;
            this.maxTweenTime = Mathf.Max(0, maxTweenTime);
            this.minTweenTime = Mathf.Clamp(minTweenTime, 0, maxTweenTime);
            this.g = g;
            this.onlyPositive = onlyPositive;
            this.onUpdate = onUpdate;
        }

        public void Tween(double newValue, double valuePerSecond)
        {
            SimpleTween.Cancel(tweenId, false);
            double add = newValue - tValue;

            if ((add > 0 && onlyPositive) || !onlyPositive)
            {
                valuePerSecond = Math.Max(1, valuePerSecond);
                float tT = Mathf.Abs((float)add / (float)valuePerSecond);
                tT = Mathf.Clamp(tT, minTweenTime, maxTweenTime);
                double oldValue = tValue;
                tweenId = SimpleTween.Value(g, 0, 1, tT).SetOnUpdate((float val) =>
                {
                    if (this != null)
                    {
                        tValue = MathMk.Lerp(oldValue, newValue, val);
                        onUpdate?.Invoke(tValue);
                    }
                }).AddCompleteCallBack(() => { tValue = newValue; onUpdate?.Invoke(tValue); }).ID;
            }
            else if (onlyPositive)
            {
                if (this != null)
                {
                    tValue = newValue;
                    onUpdate?.Invoke(tValue);
                }
            }
        }
    }

    public class TweenExt
    {
        public static void DelayAction(GameObject g, float delay, Action completeCallBack)
        {
            SimpleTween.Value(g, 0, 1, delay).AddCompleteCallBack(completeCallBack);
        }

        public static void TweenColor(GameObject g,  Color start, Color end, float tweenTime, float delayTime, Action<Color> onUpdate, Action completeCallBack)
        {
            SimpleTween.Value(g, 0f, 1f, tweenTime).SetOnUpdate((float val)=> {onUpdate?.Invoke(Color.Lerp( start, end, val)); }).AddCompleteCallBack(completeCallBack).SetDelay(delayTime);
        }
		
		public static void RotateZTween(GameObject controlObject, Transform target, float angleZ, float time, EaseAnim easeAnim, Action callBack)
        {
            float oldVal = 0f;
            SimpleTween.Value(controlObject, 0, -angleZ, 0.5f)
                              .SetOnUpdate((float val) =>
                              {
                                  float angleSpeed = (-val + oldVal);
                                  if (target) target.Rotate(0, 0, angleSpeed);
                                  oldVal = val;
                              })
                              .SetEase(easeAnim)
                              .AddCompleteCallBack(callBack);
        }
		
		 /// <summary>
        /// sequnced tween between array values
        /// </summary>
        public static void ValuesTween (GameObject g, float[] values, float time, Action<float> update, Action completeCallBack)
        {
            if (values == null || values.Length == 0)
            {
                completeCallBack?.Invoke();
                return;
            }
            if (values.Length == 1)
            {
                update?.Invoke(values[0]);
                completeCallBack?.Invoke();
                return;
            }

            float[] v = new float[values.Length];
            values.CopyTo(v, 0);

            TweenSeq tS = new TweenSeq();
            float _time = time / (float)(v.Length - 1);
            for (int i = 0; i < v.Length - 1; i++)
            {
                float a = v[i];
                float b = v[i + 1];
                tS.Add((callBack) => { SimpleTween.Value(g, a, b, _time).SetOnUpdate((float val) => { update?.Invoke(val); }).AddCompleteCallBack(callBack); });
            }

            tS.Add((callBack) =>
            {
                completeCallBack?.Invoke();
                callBack();
            });

            tS.Start();
        }
    }
}
