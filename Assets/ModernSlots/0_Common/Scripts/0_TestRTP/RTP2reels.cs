using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 25.06.2025
namespace Mkey
{
    public class RTP2reels : MonoBehaviour
    {
        public int maxLinesCount = 1;
        public float targetRTP = 90f;
        public float targetVol = 1;             // дисперсия
        public float deltaRTP = 0.5f;

        public List<Sprite> avoid_1;            // avoid on 1 reel
        public List<Sprite> avoid_other;        // avoin on other reels
        public List<String> baseReels;
        public List<String> calculatedReels;

        #region temp vars
        private int maxSymbolsOnReel = 25;
        double sumPayOut = 0;
        double sumPayoutFreeSpins = 0;
        SlotController slotController;
        LineBehavior[] lbs;
        TestSlot testSlot;
        List<int> slotIconsID_1;
        List<int> slotIconsID_2;
        private float maxRTP;
        private float minRTP;

        #endregion temp vars


        #region regular
        private void Awake()
        {
        }

        #endregion regular

        public void CalcReels()
        {
            slotController = GetComponent<SlotController>();
            lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);                                // only active
            testSlot = new TestSlot(slotController, lbs, maxLinesCount);
            sumPayOut = 0;
            sumPayoutFreeSpins = 0;
            testSlot.GetPayout(out sumPayOut, out sumPayoutFreeSpins);
            Debug.Log("start sumPayOut: " + sumPayOut);
            Debug.Log("start sumPayoutFreeSpins: " + sumPayoutFreeSpins);

            slotIconsID_1 = new List<int>();                  // for 1 reel  (without scatter, wild)
            for (int i = 0; i < testSlot.slotIcons.Length; i++)
            {
               if(!avoid_1.Contains(testSlot.slotIcons[i].iconSprite)) slotIconsID_1.Add(i);
            }
            Debug.Log("first reel icons length: " + slotIconsID_1.Count);
            slotIconsID_2 = new List<int>();                    // for 2.3...n reels  (without scatter)
            for (int i = 0; i < testSlot.slotIcons.Length; i++)
            {
                if (!avoid_other.Contains(testSlot.slotIcons[i].iconSprite)) slotIconsID_2.Add(i);
            }
            Debug.Log("other reels icons length: " + slotIconsID_2.Count);
            maxRTP = Mathf.Abs(targetRTP) + Mathf.Abs(deltaRTP);
            minRTP = Mathf.Abs(targetRTP) - Mathf.Abs(deltaRTP);


            if (sumPayoutFreeSpins >= minRTP && sumPayoutFreeSpins <= maxRTP)
            {
                Debug.Log("payout in range");
                return;
            }

            int loops = 0;
            while (sumPayoutFreeSpins < minRTP || sumPayoutFreeSpins > maxRTP)
            {
                loops++;
                if (loops > 1000)
                {
                    Debug.Log("break calculation");
                    break;
                }
                if (sumPayoutFreeSpins < minRTP)
                {
                    IncreaseRTP();
                }
                else if (sumPayoutFreeSpins > maxRTP)
                {
                    DecreaseRTP();
                }
            }

            calculatedReels = new List<string>();
            foreach (var _reel in testSlot.testReels)
            {
                calculatedReels.Add(_reel.OrderToJsonString());
            }
        }

        private void IncreaseRTP()
        {
            double _prevRTP = sumPayoutFreeSpins;
            for (int i = 0; i < 1000; i++)
            {
                // random reel
                var reelNumber = UnityEngine.Random.Range(0, testSlot.testReels.Length);
                var reel = testSlot.testReels[reelNumber];

                // random symbol
                int slotIconID = (reelNumber == 0) ? slotIconsID_1.GetRandomPos() : slotIconsID_2.GetRandomPos();

                // add random symbol to reel
                // testSlot.GetPayout(out sumPayOut, out sumPayoutFreeSpins);
                // Debug.Log(i + ") before adding, sumPayoutFreeSpins: " + sumPayoutFreeSpins);
                reel.AddSymbol(slotIconID);

                // calc RTP
                testSlot.GetPayout(out sumPayOut, out sumPayoutFreeSpins);
                // Debug.Log(i + ") after adding, sumPayoutFreeSpins: " + sumPayoutFreeSpins + "   reel: " + reelNumber);

                // if the payment has not increased -> rollback
                if (sumPayoutFreeSpins <= _prevRTP)
                {
                    reel.RemoveLastSymbol();
                    // Debug.Log("reel: " + reelNumber + ", remove last symbol");
                    if (i < 999) continue;
                }

                _prevRTP = sumPayoutFreeSpins;

                if (_prevRTP >= minRTP)
                {
                    Debug.Log("end calc, sumPayoutFreeSpins: " + sumPayoutFreeSpins);
                    foreach (var _reel in testSlot.testReels)
                    {
                        Debug.Log(_reel.OrderToJsonString());
                    }
                    Debug.Log("calcs: " + i);
                    break;
                }
                else if (i == 999)
                {
                    Debug.Log("target not reached, calcs: " + i);
                    Debug.Log("sumPayoutFreeSpins: " + sumPayoutFreeSpins);
                    foreach (var _reel in testSlot.testReels)
                    {
                        Debug.Log(_reel.OrderToJsonString());
                    }
                }
            }
        }

        private void DecreaseRTP()
        {
            double _prevRTP = sumPayoutFreeSpins;
            for (int i = 0; i < 1000; i++)
            {
                // random reel
                var reelNumber = UnityEngine.Random.Range(0, testSlot.testReels.Length);
                var reel = testSlot.testReels[reelNumber];

                //  random symbol
                int slotIconID = (reelNumber == 0) ? slotIconsID_1.GetRandomPos() : slotIconsID_2.GetRandomPos();

                // add random symbol to reel
                // testSlot.GetPayout(out sumPayOut, out sumPayoutFreeSpins);
                // Debug.Log(i + ") before adding, sumPayoutFreeSpins: " + sumPayoutFreeSpins);
                reel.AddSymbol(slotIconID);

                // calc RTP
                testSlot.GetPayout(out sumPayOut, out sumPayoutFreeSpins);
                // Debug.Log(i + ") after adding, sumPayoutFreeSpins: " + sumPayoutFreeSpins + "   reel: " + reelNumber);

                // if the RTP has not decreased -> rollback
                if (sumPayoutFreeSpins >= _prevRTP)
                {
                    reel.RemoveLastSymbol();
                    // Debug.Log("reel: " + reelNumber + ", remove last symbol");
                    if (i < 999) continue;
                }

                _prevRTP = sumPayoutFreeSpins;

                if (_prevRTP <= maxRTP)
                {
                    Debug.Log("end calc, sumPayoutFreeSpins: " + sumPayoutFreeSpins);
                    foreach (var _reel in testSlot.testReels)
                    {
                        Debug.Log(_reel.OrderToJsonString());
                    }
                    Debug.Log("calcs: " + i);
                    break;
                }
                else if (i == 999)
                {
                    Debug.Log("target not reached, calcs: " + i);
                    Debug.Log("sumPayoutFreeSpins: " + sumPayoutFreeSpins);
                    foreach (var _reel in testSlot.testReels)
                    {
                        Debug.Log(_reel.OrderToJsonString());
                    }
                }
            }
        }

        public void SaveBaseReels()
        {
            slotController = GetComponent<SlotController>();
            baseReels = new List<string>();
            Debug.Log("Base Reels");
            foreach (var item in slotController.slotGroupsBeh)
            {
                baseReels.Add(item.OrderToJsonString());
                Debug.Log(item.OrderToJsonString());
            }
        }

        public void SetBaseReels()
        {
            SetReels(baseReels);
        }

        public void SetNewReels()
        {
            SetReels(calculatedReels);
        }

        public void SetReels(List<string> jsReels) 
        {
            slotController = GetComponent<SlotController>();
            if (jsReels == null || jsReels.Count != slotController.slotGroupsBeh.Length)
            {
                Debug.LogError("Failed to set reels");
            }
            foreach (var item in jsReels)
            {
                if (string.IsNullOrEmpty(item))
                {
                    Debug.LogError("Failed item in reels list");
                    return;
                }
            }

            for (int i = 0; i < slotController.slotGroupsBeh.Length; i++)
            {
                slotController.slotGroupsBeh[i].SetOrderFromJson(jsReels[i]);
            }
        }

        public string GetPaytableJson()
        {
            slotController = GetComponent<SlotController>();
            return slotController.PaytableToJsonString();
        }

        public string GetCalcReelsJson()
        {
            string res = "";

            foreach (var item in calculatedReels)
            {
                res += (item + " ");
            }
            return res;
        }

        public string GetCurrReelsJson()
        {
            slotController = GetComponent<SlotController>();

            string res = "";

            foreach (var item in slotController.slotGroupsBeh)
            {
                res += (item.OrderToJsonString() + " ");
            }
            return res;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RTP2reels))]
    public class RTP2reelsEditor : Editor
    {
        RTP2reels rCalc;
        public override void OnInspectorGUI()
        {
            rCalc = (RTP2reels)target;
            DrawDefaultInspector();

            #region calculate
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Save base reels"))
            {
                rCalc.SaveBaseReels();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Calc reels"))
            {
                rCalc.CalcReels();
            }

            if (GUILayout.Button("Set base reels"))
            {
                rCalc.SetBaseReels();
            }

            if (GUILayout.Button("Set new reels"))
            {
                rCalc.SetNewReels();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Copy to CP paytable"))
            {
                GUIUtility.systemCopyBuffer = rCalc.GetPaytableJson();
            }

            if (GUILayout.Button("Copy to CP calc reels"))
            {
                GUIUtility.systemCopyBuffer = rCalc.GetCalcReelsJson();
            }

            if (GUILayout.Button("Copy to CP curr reels"))
            {
                GUIUtility.systemCopyBuffer = rCalc.GetCurrReelsJson();
            }
            EditorGUILayout.EndHorizontal();
            #endregion calculate
        }
    }
#endif
}
