using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
/*
   13.04.2021
*/
namespace Mkey
{
    public class DailyRewardController : MonoBehaviour
    {
        [SerializeField]
        private bool startFromZeroDayReward = false;
        [SerializeField]
        private bool repeatingReward = true;
        [HideInInspector]
        public UnityEvent TimePassEvent;

        #region temp vars
        private int hours = 24;
        private int minutes = 0; // for test
        private GlobalTimer gTimer;
        private string timerName = "dailyreward";
        private string nextRewardDayKey = "nextreward";
        private bool debug = false;
        private SoundMaster MSound => SoundMaster.Instance;
        #endregion temp vars

        #region properties
        public float RestDays { get; private set; }
        public float RestHours { get; private set; }
        public float RestMinutes { get; private set; }
        public float RestSeconds { get; private set; }
        public bool IsWork { get; private set; }
        private int NextRewardDay { get; set; }
        public int RewardDay { get; private set; }
        public bool RepeatingReward { get { return repeatingReward; } }

        public static DailyRewardController Instance;
        #endregion properties

        #region regular
        private void Start()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            Debug.Log("Awake: " + name);

            IsWork = false;
            LoadNextRewardDay();
            RewardDay = -1;

            // check existing timer and  last tick
            if (GlobalTimer.Exist(timerName))
            {
                if (debug) Debug.Log("timer exist: " + timerName);
                DateTime lT = GlobalTimer.GetLastTick(timerName);
                TimeSpan tS = DateTime.Now - lT;
                TimeSpan dRTS = new TimeSpan(hours, minutes, 0);

                if (tS > dRTS) // interrupted game
                {
                    if (debug) Debug.Log("daily reward interrupted, timespan: " + tS);
                    ResetNextRewardDay();
                    StartNewTimer();
                }
                else
                {
                    if (debug) Debug.Log("daily reward not interrupted, timespan: " + tS);
                    StartExistingTimer();
                }
            }
            else
            {
                if (debug) Debug.Log("timer not exist: " + timerName);
                StartNewTimer();
                if (startFromZeroDayReward)
                {
                    ResetNextRewardDay();
                    RewardDay = 0;
                    IncNextRewardDay();
                }
            }
        }

        private void Update()
        {
            if (IsWork)
                gTimer.Update();
        }

        private void OnDestroy()
        {

        }
        #endregion regular

        #region reward day
        private void IncNextRewardDay()
        {
            SetNextRewardDay(NextRewardDay + 1);
        }

        private void SetNextRewardDay(int order)
        {
            NextRewardDay = order;
            PlayerPrefs.SetInt(nextRewardDayKey, NextRewardDay);
        }

        private void LoadNextRewardDay()
        {
            NextRewardDay = PlayerPrefs.GetInt(nextRewardDayKey, 0);
        }

        private void ResetNextRewardDay()
        {
            if (debug) Debug.Log("reset reward day");
            SetNextRewardDay(0);
        }
        #endregion reward day

        #region timerhandlers
        private void TickRestDaysHourMinSecHandler(int d, int h, int m, float s)
        {
            RestDays = d;
            RestHours = h;
            RestMinutes = m;
            RestSeconds = s;
        }

        private void TimePassedHandler(double initTime, double realyTime)
        {
            if (debug) Debug.Log("time passed");
            IsWork = false;
            RewardDay = NextRewardDay;
            IncNextRewardDay();
            //   StartCoroutine(ShowRewardPopup(1.5f, RewardDay));
            TimePassEvent?.Invoke();
            StartNewTimer();
        }
        #endregion timerhandlers

        #region timers
        private void StartNewTimer()
        {
            if (debug) Debug.Log("start new");
            TimeSpan ts = new TimeSpan(hours, minutes, 0);
            gTimer = new GlobalTimer(timerName, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        private void StartExistingTimer()
        {
            gTimer = new GlobalTimer(timerName);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }
        #endregion timers

        #region reward
        public void ApplyReward()
        {
            RewardDay = -1;
        }

        public void ResetData()
        {
            ResetNextRewardDay();
            GlobalTimer.RemoveTimerPrefs(timerName);
        }

        internal void NextDayTest()
        {
            RewardDay = NextRewardDay;
            IncNextRewardDay();
        }
        #endregion reward
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(DailyRewardController))]
    public class DailyRewardControllerEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (!EditorApplication.isPlaying)
            {
                if (test = EditorGUILayout.Foldout(test, "Test"))
                {
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Reset reward"))
                    {
                        DailyRewardController t = (DailyRewardController)target;
                        t.ResetData();
                    }
                    EditorGUILayout.EndHorizontal();
                }

            }
            else
            {
                if (GUILayout.Button("Next rew day"))
                {
                    DailyRewardController t = (DailyRewardController)target;
                    t.NextDayTest();
                }
            }
        }
    }
#endif
}

/*mkmatchutils*/ /*mkslotutils*/