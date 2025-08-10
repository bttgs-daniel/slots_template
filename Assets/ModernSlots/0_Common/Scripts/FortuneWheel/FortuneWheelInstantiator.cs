using System;
using System.Collections.Generic;
using UnityEngine;
using Mkey;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MkeyFW
{
    public class FortuneWheelInstantiator : MonoBehaviour
    {
        [SerializeField]
        private WheelController fortuneWheelPrefab;

        [SerializeField]
        private WheelController fortuneWheel;

        [SerializeField]
        private EaseAnim ease = EaseAnim.EaseOutBack;
        [SerializeField]
        private bool autoClose = false;
        [SerializeField]
        private float autoCloseTime = 5f;

        #region temp vars
        private bool closeInProcess = false;
        private bool createInProcess = false;
        private Vector3 sourceScale = Vector3.one;
        #endregion temp vars

        #region properties
        public WheelController MiniGame => fortuneWheel;
        private SlotPlayer MPlayer => SlotPlayer.Instance;
        private GuiController MGui => GuiController.Instance;
        #endregion properties

        #region events
        public Action<int, bool> SpinResultEvent;
        public Action <WheelController> CreateEvent;
        public UnityEvent CreateWheelEvent;
        public Action CloseEvent;
        #endregion events

        public void Create(bool autoStart)
        {
            if (autoStart) CreateAutoStart();
            else Create();
        }

        internal void CreateAutoStart()
        {
            if (!fortuneWheelPrefab) return;
            if (fortuneWheel) return;
            if (closeInProcess || createInProcess) return;
            createInProcess = true;

            fortuneWheel = Instantiate(fortuneWheelPrefab);
            sourceScale = fortuneWheel.transform.localScale;
            fortuneWheel.transform.localScale = Vector3.zero;
            fortuneWheel.gameObject.SetActive(true);
            fortuneWheel.SpinResultEvent += ResultEventHandler;
            if (fortuneWheel) fortuneWheel.SetBlocked(true, true);

            SimpleTween.Value(gameObject, 0, sourceScale.x, 0.25f)
                .SetOnUpdate((float val) =>
                {
                    if (fortuneWheel) fortuneWheel.transform.localScale = new Vector3(val, val, val);
                })
                .AddCompleteCallBack(() =>
                {
                    createInProcess = false;
                    CreateEvent?.Invoke(fortuneWheel);
                    CreateWheelEvent?.Invoke();
                })
                .SetEase(ease);

            TweenExt.DelayAction(gameObject, 0.5f, () =>
            {
               fortuneWheel.StartSpin(() =>
                {
                    if (fortuneWheel) fortuneWheel.SetBlocked(true, true);
                });
            });
        }

        public void Create()
        {
            if (!fortuneWheelPrefab) return;
            if (fortuneWheel) return;
            if (closeInProcess || createInProcess) return;
            createInProcess = true;

            fortuneWheel = Instantiate(fortuneWheelPrefab);
            sourceScale = fortuneWheel.transform.localScale;
            fortuneWheel.transform.localScale = Vector3.zero;
            fortuneWheel.gameObject.SetActive(true);
            fortuneWheel.SpinResultEvent += ResultEventHandler;
            if (fortuneWheel) fortuneWheel.SetBlocked(true, true);

            SimpleTween.Value(gameObject, 0, sourceScale.x, 0.5f)
                .SetOnUpdate((float val) =>
                {
                    if (fortuneWheel) fortuneWheel.transform.localScale = new Vector3(val, val, val);
                })
                .AddCompleteCallBack(() =>
                {
                    createInProcess = false;
                    if (fortuneWheel) fortuneWheel.SetBlocked(false, false);
                    CreateEvent?.Invoke(fortuneWheel);
                    CreateWheelEvent?.Invoke();
                })
                .SetEase(ease);
        }

        internal void Close(float delay, Action completeCallBack)
        {
            if (closeInProcess || createInProcess) return;

            if (fortuneWheel)
            {
                closeInProcess = true;
                fortuneWheel.SetBlocked(true, true);
                fortuneWheel.SpinResultEvent -= ResultEventHandler;
                SimpleTween.Value(gameObject, 1, 0, 0.25f)
                   .SetOnUpdate((float val) =>
                   {
                       if (fortuneWheel) fortuneWheel.transform.localScale = new Vector3(val, val, val);

                   })
                   .AddCompleteCallBack(() =>
                   {
                       if (fortuneWheel)
                       {
                           WheelController fW = fortuneWheel;
                           fortuneWheel = null;
                           Destroy(fW.gameObject);
                       }
                       closeInProcess = false;
                       CloseEvent?.Invoke();
                       completeCallBack?.Invoke();
                   })
                   .SetDelay(delay);
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        public void Close()
        {
            Close(autoCloseTime, null);
        }

        public void ForceClose()
        {
            Close(0, null);
        }

        private void ResultEventHandler(int coins, bool isBigWin)
        {
            SpinResultEvent?.Invoke(coins, isBigWin);
            if (autoClose)
            {
                Close();
            }
        }
    }
}