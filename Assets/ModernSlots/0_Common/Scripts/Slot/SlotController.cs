using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Events;

namespace Mkey
{
    public enum WinLineFlashing { All, Sequenced, None }
    public enum JackPotType { None, Mini, Maxi, Mega }
    public enum JackPotIncType { Const, Percent } // add const value or percent of start value

    public class SlotController : MonoBehaviour
    {
        #region main reference
        [SerializeField]
        public SlotMenuController menuController;
        [SerializeField]
        public SlotControls controls;
        [SerializeField]
        public WinController winController;
        [SerializeField]
        private AudioClip spinSound;
        [SerializeField]
        private AudioClip looseSound;
        [SerializeField]
        private AudioClip winCoinsSound;
        [SerializeField]
        private AudioClip winFreeSpinSound;
        #endregion main reference

        #region icons
        [SerializeField, ArrayElementTitle("iconSprite"), NonReorderable]
        public SlotIcon[] slotIcons;

        [Space(8)]
        [SerializeField]
        public WinSymbolBehavior[] winSymbolBehaviors;
        #endregion icons

        #region payTable
        public List<PayLine> payTable;
        internal List<PayLine> payTableFull; // extended  if useWild
        #endregion payTable

        #region special major
        public int scatter_id;
        public int wild_id;
        public bool useWild;
        public bool useScatter;
        #endregion special major

        #region scatter paytable
        public List<ScatterPay> scatterPayTable;
        #endregion scatter paytable

        #region prefabs
        public GameObject tilePrefab;
        public GameObject particlesStars;
        [SerializeField]
        private WarningMessController BigWinPrefab;
        #endregion prefabs

        #region slotGroups
        [NonReorderable]
        public SlotGroupBehavior[] slotGroupsBeh;
        #endregion slotGroups

        #region tweenTargets
        public Transform bottomJumpTarget;
        public Transform topJumpTarget;
        #endregion tweenTargets

        #region spin options
        [SerializeField]
        private EaseAnim inRotType = EaseAnim.EaseLinear; // in rotation part
        [SerializeField]
        [Tooltip("Time in rotation part, 0-1 sec")]
        private float inRotTime = 0.3f;
        [SerializeField]
        [Tooltip("In rotation part angle, 0-10 deg")]
        private float inRotAngle = 7;

        [Space(16, order = 0)]
        [SerializeField]
        private EaseAnim outRotType = EaseAnim.EaseLinear;   // out rotation part
        [SerializeField]
        [Tooltip("Time out rotation part, 0-1 sec")]
        private float outRotTime = 0.3f;
        [SerializeField]
        [Tooltip("Out rotation part angle, 0-10 deg")]
        private float outRotAngle = 7;

        [Space(16, order = 0)]
        [SerializeField]
        private EaseAnim mainRotateType = EaseAnim.EaseLinear;   // main rotation part
        [SerializeField]
        [Tooltip("Time main rotation part, sec")]
        private float mainRotateTime = 4f;
        [Tooltip("min 0% - max 20%, change rotateTime")]
        [SerializeField]
        private int mainRotateTimeRandomize = 10;
        #endregion spin options

        #region options
        public WinLineFlashing winLineFlashing = WinLineFlashing.Sequenced;
        public bool winSymbolParticles = true;
        public RNGType RandomGenerator = RNGType.Unity;
        [SerializeField]
        [Tooltip("Multiply win coins by bet multiplier")]
        public bool useLineBetMultiplier = true;
        [SerializeField]
        [Tooltip("Multiply win spins by bet multiplier")]
        public bool useLineBetFreeSpinMultiplier = true;
        [SerializeField]
        [Tooltip("Debug to console predicted symbols")]
        private bool debugPredictSymbols = false;
        #endregion options 

        #region jack pots
        [Space(8)]
        public int jp_symbol_id = -1;
        public bool useMiniJacPot = false;
        [Tooltip("Count identical symbols on screen")]
        public int miniJackPotCount = 7;
        public bool useMaxiJacPot = false;
        [Tooltip("Count identical symbols on screen")]
        public int maxiJackPotCount = 9;
        public bool useMegaJacPot = false;
        [Tooltip("Count identical symbols on screen")]
        public int megaJackPotCount = 10;
        private JackPotIncType jackPotIncType = JackPotIncType.Const;
        public int jackPotIncValue = 1;
        public JackPotController jpController;
        #endregion jack pots 

        #region levelprogress
        [SerializeField]
        [Tooltip("Multiply level progress by bet multiplier")]
        public bool useLineBetProgressMultiplier = true;
        [SerializeField]
        [Tooltip("Player level progress for loose spin")]
        public float loseSpinLevelProgress = 0.5f;
        [SerializeField]
        [Tooltip("Player level progress for win spin per win line")]
        public float winSpinLevelProgress = 2.0f;
        #endregion level progress

        #region temp vars
        private int slotTilesCount = 30;
        private WaitForSeconds wfs1_0;
        private WaitForSeconds wfs0_2;
        private WaitForSeconds wfs0_1;
        private RNG rng; // random numbers generator

        private uint spinCount = 0;
        private bool slotsRunned = false;
        private bool playFreeSpins = false;
        private bool logStatistic = true;

        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        private GuiController MGUI { get { return GuiController.Instance; } }
        private JackPotType jackPotType = JackPotType.None;
        private int jackPotWinCoins = 0;
        private bool infiniteSpinFlag = false;
        private bool sideWinFlag = false; // can be set by some event related to the game situation. you need to raise this flag in a game event: EndWinSearchEvent

        private bool lastMenuActivity = false;
        private bool lastSpinActivity = false;
        private bool lastControlsActivity = false;
        private List<GameObject> externalActivityBlockers;
        #endregion temp vars

        #region events
        public Action SpinPressEvent;           // 1.10 - 266 SpinPress()

        public Action StartFreeGamesEvent;      // 2.10 - 311 RunSlots()
        public Action StartSpinEvent;           // 2.20 - 333 RunSlotsAsyncC()
        public Action EndSpinEvent;             // 2.30 - 356 RunSlotsAsyncC()

        public Action BeginWinCalcEvent;        // 3.10 - 360 RunSlotsAsyncC()
        public Action EndWinSearchEvent;
        public Action AnyWinEvent;              // 3.20 - if win 371 RunSlotsAsyncC()
        public Action<int> WinCoinsEvent;       // 3.22 - if win
        public Action<int> WinSpinsEvent;       // 3.24 - if win
        public Action EndWinCalcEvent;          // 3.30 - 433, 477 RunSlotsAsyncC()

        public Action LooseEvent;               // 3.40 - 489 RunSlotsAsyncC()
        public Action EndFreeGamesEvent;        // 3.50 - 491 RunSlotsAsyncC() - перенес

        public Action StartWinShowEvent;
        public Action EndWinShowEvent_1;
        public Action EndWinShowEvent_2;
        public Action EndWinShowEvent_3;

        public Action<float> EndCalcPayoutEvent;
        #endregion events

        #region dev
        public string payTableJsonString;
        #endregion dev

        public static SlotController CurrentSlot { get; private set; }
        public bool IsFreeSpin { get; private set; }

        public bool useWildInFirstPosition = false;

        #region regular
        private void OnValidate()
        {
            Validate();
        }

        void Validate()
        {
            mainRotateTimeRandomize = (int)Mathf.Clamp(mainRotateTimeRandomize, 0, 20);

            inRotTime = Mathf.Clamp(inRotTime, 0, 1f);
            inRotAngle = Mathf.Clamp(inRotAngle, 0, 10);

            outRotTime = Mathf.Clamp(outRotTime, 0, 1f);
            outRotAngle = Mathf.Clamp(outRotAngle, 0, 10);
            jackPotIncValue = Mathf.Max(0, jackPotIncValue);

            miniJackPotCount = Mathf.Max(1, miniJackPotCount);
            maxiJackPotCount = Mathf.Max((useMiniJacPot) ? miniJackPotCount + 1 : 1, maxiJackPotCount);
            megaJackPotCount = Mathf.Max((useMaxiJacPot) ? maxiJackPotCount + 1 : 1, megaJackPotCount);
            if (scatterPayTable != null)
            {
                foreach (var item in scatterPayTable)
                {
                    if (item != null)
                    {
                        item.payMult = Mathf.Max(1, item.payMult);
                    }
                }
            }
        }

        void Start()
        {
            wfs1_0 = new WaitForSeconds(1.0f);
            wfs0_2 = new WaitForSeconds(0.2f);
            wfs0_1 = new WaitForSeconds(0.1f);
            externalActivityBlockers = new List<GameObject>();

            // create reels
            int slotsGrCount = slotGroupsBeh.Length;
            ReelData[] reelsData = new ReelData[slotsGrCount];
            ReelData reelData;
            int i = 0;
            foreach (SlotGroupBehavior sGB in slotGroupsBeh)
            {
                reelData = new ReelData(sGB.symbOrder);
                reelsData[i++] = reelData;
                sGB.CreateSlotCylinder(slotIcons, slotTilesCount, tilePrefab);
            }

            CreateFullPaytable();
            rng = new RNG(RNGType.Unity, reelsData);
            SetInputActivity(true, true, true);
            CurrentSlot = this;
        }

        void Update()
        {
            rng.Update();
            externalActivityBlockers.RemoveAll((bl) => { return bl == null; });
            if (externalActivityBlockers.Count == 0)
            {
                menuController.SetControlActivity(lastMenuActivity);    // there may also be controls here
                controls.SetControlActivity(lastControlsActivity, lastSpinActivity);
            }
            else
            {
                menuController.SetControlActivity(false);               // there may also be controls here
                controls.SetControlActivity(false, false);
            }
        }

        private void OnDestroy()
        {

        }
        #endregion regular

        /// <summary>
        /// Run slots when you press the button
        /// </summary>
        internal void SpinPress()
        {
            SpinPressEvent?.Invoke();
            RunSlots();
        }

        internal void SetInfiniteSpinFlag()
        {
            infiniteSpinFlag = true;
        }

        internal void ForceStop()
        {
            foreach (var item in slotGroupsBeh)
            {
                item.ForceStopInfiniteSpin();
            }
        }

        #region nudge
        private int nudge = -1;
        private bool nudgeUp = false;
        public bool IsNudge => nudge >= 0;
        public void ReelNudgeDown(int nudge)
        {
            Debug.Log("nudge down: " + nudge);
            this.nudge = nudge; // reel number
            nudgeUp = false;
            RunSlots();
        }

        public void ReelNudgeUp(int nudge)
        {
            Debug.Log("nudge up: " + nudge);
            this.nudge = nudge; // reel number
            nudgeUp = true;
            RunSlots();
        }
        #endregion nudge

        private void RunSlots()
        {
            if (slotsRunned) return;
            winController.WinEffectsShow(false, false);
            winController.WinShowCancel();

            winController.ResetLineWinning();
            controls.JPWinCancel();

            StopCoroutine(RunSlotsAsyncC());

            if (!controls.AnyLineSelected)
            {
                MGUI.ShowMessage(null, "Please select a any line.", 1.5f, null);
                controls.ResetAutoSpinsMode();
                return;
            }

            if (controls.ApllyFreeSpin())
            {
                if (!IsFreeSpin) StartFreeGamesEvent?.Invoke();
                IsFreeSpin = true;
                StartCoroutine(RunSlotsAsyncC());
                return;
            }
            else
            {
                IsFreeSpin = false;
            }

            if (!controls.ApplyBet())
            {
                MGUI.ShowMessage(null, "You have no money.", 1.5f, null);
                controls.ResetAutoSpinsMode();
                SetInputActivity(true, true, true);     // avoid touch blocking in auto spin mode
                return;
            }

            StartCoroutine(RunSlotsAsyncC());
        }

        private IEnumerator RunSlotsAsyncC()
        {
            StartSpinEvent?.Invoke();

            jackPotWinCoins = 0;
            jackPotType = JackPotType.None;

            slotsRunned = true;
            if (controls.Auto && !IsFreeSpin) controls.IncAutoSpinsCounter();
            Debug.Log("Spins count from game start: " + (++spinCount));

            MPlayer.SetWinCoinsCount(0);

            //1 ---------------start preparation-------------------------------
            SetInputActivity(false, false, controls.Auto);
            winController.HideAllLines();

            //1a ------------- sound -----------------------------------------
            MSound.StopAllClip(false); // stop all clips with background musik
            MSound.PlayClip(0f, true, spinSound);

            //2 --------start rotating ----------------------------------------
            bool fullRotated = false;
            RotateSlots(() => { MSound.StopClips(spinSound); fullRotated = true; });
            while (!fullRotated) yield return wfs0_2;  // wait 
            EndSpinEvent?.Invoke();

            //3 --------check result-------------------------------------------
            BeginWinCalcEvent?.Invoke();
            winController.SearchWinSymbols();
            bool hasLineWin = false;
            bool hasScatterWin = false;
            EndWinSearchEvent?.Invoke();
            bool bigWin = false;
            if (controls.Auto && controls.AutoSpinsCounter >= controls.AutoSpinCount)
            {
                controls.ResetAutoSpinsMode();
            }

            // 3a ----- increase jackpots ----
            IncreaseJackPots();

            if (winController.HasAnyWinn(ref hasLineWin, ref hasScatterWin, ref jackPotType) || sideWinFlag)
            {
                sideWinFlag = false;    // reset flag

                // 3b0 win events
                AnyWinEvent?.Invoke();
                StartWinShowEvent?.Invoke();
                yield return StartCoroutine(WaitActivity());

                //3b1 ---- show particles, line flashing  -----------
                winController.WinEffectsShow(winLineFlashing == WinLineFlashing.All, winSymbolParticles);
                yield return StartCoroutine(WaitActivity());

                //3b2 --------- check Jack pot -------------
                jackPotWinCoins = controls.GetJackPotCoins(jackPotType);
                if (jackPotType != JackPotType.None && jackPotWinCoins > 0)
                {
                    // controls.SetWinInfo(jackPotWinCoins);
                    MPlayer.AddCoins(jackPotWinCoins);

                    if (controls.HasFreeSpin || controls.Auto)
                    {
                        controls.JPWinShow(jackPotWinCoins, jackPotType);
                        yield return new WaitForSeconds(5.0f); // delay
                        controls.JPWinCancel();
                    }
                    else
                    {
                        controls.JPWinShow(jackPotWinCoins, jackPotType);
                        yield return new WaitForSeconds(3.0f);// delay
                    }
                    controls.SetJackPotCount(0, jackPotType); // reset jack pot amount
                }
                yield return StartCoroutine(WaitActivity());

                //3c0 -----------------calc coins -------------------
                int winCoins = winController.GetWinCoins();
                int payMultiplier = winController.GetPayMultiplier();
                winCoins *= payMultiplier;
                if (useLineBetMultiplier) winCoins *= controls.LineBet;
                // controls.SetWinInfo(jackPotWinCoins + winCoins);
                MPlayer.SetWinCoinsCount(winCoins);
                MPlayer.AddCoins(winCoins);
                if (winCoins > 0)
                {
                    WinCoinsEvent?.Invoke(winCoins);
                    bigWin = (winCoins >= MPlayer.MinWin && MPlayer.UseBigWinCongratulation);
                    if (!bigWin) MSound.PlayClip(0, winCoinsSound);
                    else
                    {
                        MGUI.ShowMessage(BigWinPrefab, winCoins.ToString(), "", 3f, null);
                    }
                }
                yield return StartCoroutine(WaitActivity());

                //3c1 ----------- calc free spins ----------------
                int winSpins = winController.GetWinSpins();
                int freeSpinsMultiplier = winController.GetFreeSpinsMultiplier();
                winSpins *= freeSpinsMultiplier;
                int winLinesCount = winController.GetWinLinesCount();
                if (useLineBetFreeSpinMultiplier) winSpins *= controls.LineBet;
                // if (winSpins > 0) MSound.PlayClip((winCoins > 0 || jackPotWinCoins > 0) ? 1.5f : 0, winFreeSpinSound);
                if (winSpins > 0)
                {
                    WinSpinsEvent?.Invoke(winSpins);
                }
                yield return StartCoroutine(WaitActivity());
                controls.AddFreeSpins(winSpins);
                playFreeSpins = (controls.AutoPlayFreeSpins && controls.HasFreeSpin);

                //3d0 ----- invoke scatter win event -----------
                if (winController.scatterWin != null && winController.scatterWin.WinEvent != null) winController.scatterWin.WinEvent.Invoke();
                SlotStatistic.Add(new StatisticData(controls.TotalBet, winCoins, IsFreeSpin, winController.GetWinLines(), winController.scatterWin, winController.jpWinSymbols, jackPotWinCoins));
                EndWinCalcEvent?.Invoke();
                yield return StartCoroutine(WaitActivity());

                // 3d1 -------- add levelprogress --------------
                MPlayer.AddLevelProgress((useLineBetProgressMultiplier) ? winSpinLevelProgress * winLinesCount * controls.LineBet : winSpinLevelProgress * winLinesCount); // for each win line

                // 3d2 ------------ start line events ----------
                winController.StartLineEvents();
                yield return StartCoroutine(WaitActivity());

                //3e ---- ENABLE player interaction -----------
                slotsRunned = false;
                if (!playFreeSpins)
                {
                    SetInputActivity(!controls.Auto, !controls.Auto, true);
                }
                else
                {
                    SetInputActivity(false, false, false);
                }
                MSound.PlayCurrentMusic();

                //3f ----------- show line effects, sccater win, jp win symbols events can be interrupted by player----------------
                bool showEnd = true;
                if (hasLineWin || hasScatterWin || jackPotType != JackPotType.None)
                {
                    showEnd = false;
                    winController.WinSymbolShow(winLineFlashing,
                       (windata) => //linewin
                       {
                           //event can be interrupted by player
                           if (windata != null) Debug.Log("lineWin : " + windata.ToString());
                       },
                       () => //scatter win
                       {
                           //event can be interrupted by player
                       },
                       () => //jack pot 
                       {
                           //event can be interrupted by player
                       },
                       () =>
                       {
                           showEnd = true;
                       }
                       );
                }

                while (!showEnd) yield return null;  // wait for show end
                yield return StartCoroutine(WaitActivity());

                EndWinShowEvent_1?.Invoke();
                yield return StartCoroutine(WaitActivity());
                EndWinShowEvent_2?.Invoke();
                yield return StartCoroutine(WaitActivity());
                EndWinShowEvent_3?.Invoke();
                yield return StartCoroutine(WaitActivity());

                // while (true) yield return new WaitForEndOfFrame(); // pause for win ;
            } // end win
            else // lose
            {
                EndWinCalcEvent?.Invoke();
                SlotStatistic.Add(new StatisticData(controls.TotalBet, IsFreeSpin));
                MSound.PlayClip(0, looseSound);

                MPlayer.AddLevelProgress(loseSpinLevelProgress);

                //3e ---- ENABLE player interaction -----------
                slotsRunned = false;
                playFreeSpins = (controls.AutoPlayFreeSpins && controls.HasFreeSpin);
                if (!playFreeSpins)
                {
                    SetInputActivity(!controls.Auto, !controls.Auto, true);
                }
                else
                {
                    SetInputActivity(false, false, false);
                }
                MSound.PlayCurrentMusic();
                LooseEvent?.Invoke();

                // while (true) yield return new WaitForEndOfFrame(); // pause for loose ;
            }

            playFreeSpins = (controls.AutoPlayFreeSpins && controls.HasFreeSpin);
            if (IsFreeSpin && !playFreeSpins) EndFreeGamesEvent?.Invoke();
            SlotStatistic.CalcStatistic();
            EndCalcPayoutEvent?.Invoke((float)SlotStatistic.PayOut);
            if (logStatistic) SlotStatistic.LogStatistic();
            yield return StartCoroutine(WaitActivity());

            playFreeSpins = (controls.AutoPlayFreeSpins && controls.HasFreeSpin);

            if (controls.Auto || playFreeSpins)
            {
                RunSlots();
            }
        }

        private IEnumerator WaitActivity()
        {
            while (!MGUI.HasNoPopUp) yield return wfs0_1;  // wait for the closin all popups
            while (SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted) yield return wfs0_1;  // wait for the mini game to close
        }

        private void IncreaseJackPots()
        {
            if (useMiniJacPot) controls.AddMiniJackPot((jackPotIncType == JackPotIncType.Const) ?
                     jackPotIncValue : (int)((float)controls.MiniJackPotStart * (float)jackPotIncValue / 100f));
            if (useMaxiJacPot) controls.AddMaxiJackPot((jackPotIncType == JackPotIncType.Const) ?
                  jackPotIncValue : (int)((float)controls.MaxiJackPotStart * (float)jackPotIncValue / 100f));
            if (useMegaJacPot) controls.AddMegaJackPot((jackPotIncType == JackPotIncType.Const) ?
                  jackPotIncValue : (int)((float)controls.MegaJackPotStart * (float)jackPotIncValue / 100f));
        }

        private void RotateSlots(Action rotCallBack)
        {
            ParallelTween pT = new ParallelTween();
            if (IsNudge)
            {
                {
                    pT.Add((callBack) =>
                    {
                        slotGroupsBeh[nudge].ReelNudge(mainRotateType, nudgeUp, callBack);
                        nudge = -1; // reset nudge
                    });
                }
                pT.Start(rotCallBack);
                return;
            }

            int[] rands = rng.GetRandSymbols(); // next symbols for reel (bottom raycaster) or -1

            // begin : use real-time payout adjustments
            RTPAdj rTPoutAdj = GetComponent<RTPAdj>();
            if (rTPoutAdj && rTPoutAdj.isActiveAndEnabled) rands = rTPoutAdj.GetNextPositions(rands).ToArray();
            // end : use real-time payout adjustments

            if (infiniteSpinFlag)
            {
                for (int i = 0; i < rands.Length; i++) rands[i] = -1;
                infiniteSpinFlag = false;
            }

            //hold feature
            HoldFeature hold = controls.Hold;
            bool[] holdReels = null;
            if (controls.UseHold && hold && hold.Length == rands.Length)
            {
                holdReels = hold.GetHoldReels();

                for (int i = 0; i < rands.Length; i++)
                {
                    rands[i] = (holdReels[i]) ? slotGroupsBeh[i].CurrOrderPosition : rands[i]; // hold position
                }
            }

            #region prediction visible symbols on reels
            if (debugPredictSymbols)
                for (int i = 0; i < rands.Length; i++)
                {
                    Debug.Log("------- Reel: " + i + " ------- (down up)");
                    for (int r = 0; r < slotGroupsBeh[i].RayCasters.Length; r++)
                    {
                        int sO = (int)Mathf.Repeat(rands[i] + r, slotGroupsBeh[i].symbOrder.Count);
                        int sID = slotGroupsBeh[i].symbOrder[sO];
                        string sName = slotIcons[sID].iconSprite.name;
                        Debug.Log("NextSymb ID: " + sID + " ;name : " + sName);
                    }
                }
            #endregion prediction

            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                int n = i;
                int r = rands[i];

                if (holdReels == null || (holdReels != null && !holdReels[i]))
                {
                    pT.Add((callBack) =>
                    {
                        slotGroupsBeh[n].NextRotateCylinderEase(mainRotateType, inRotType, outRotType,
                            mainRotateTime, mainRotateTimeRandomize / 100f,
                            inRotTime, outRotTime, inRotAngle, outRotAngle,
                            r, callBack);
                    });
                }
            }

            pT.Start(rotCallBack);
        }

        /// <summary>
        /// Set touch activity for game and gui elements of slot scene
        /// </summary>
        //private void SetInputActivity(bool activity)
        //{
        //    if (activity)
        //    {
        //        if (controls.HasFreeSpin)
        //        {
        //            menuController.SetControlActivity(false); // preserve bet change if free spin available
        //            controls.SetControlActivity(false, true);
        //        }
        //        else
        //        {
        //            menuController.SetControlActivity(activity);
        //            controls.SetControlActivity(true, true);
        //        }
        //    }
        //    else
        //    {
        //        menuController.SetControlActivity(activity);
        //        controls.SetControlActivity(activity, controls.Auto);
        //    }
        //}

        private void SetInputActivity(bool menuActivity, bool slotControlsActivity, bool spinButtonActivity)
        {
            // cache last activity
            lastMenuActivity = menuActivity;
            lastControlsActivity = slotControlsActivity;
            lastSpinActivity = spinButtonActivity;

            return;

            // controls.SetControlActivity(slotControlsActivity, spinButtonActivity);
            // menuController.SetControlActivity(menuActivity);
        }

        public void AddExternalBlocker(GameObject gameObject)
        {
            externalActivityBlockers.Add(gameObject);
        }

        public void RemoveExternalBlocker(GameObject gameObject)
        {
            if (externalActivityBlockers.Contains(gameObject)) externalActivityBlockers.Remove(gameObject);
        }

        /// <summary>
        /// Calculate propabilities
        /// </summary>
        /// <returns></returns>
        public string[,] CreatePropabilityTable()
        {
            List<string> rowList = new List<string>();
            string[] iconNames = GetIconNames(false);
            int length = slotGroupsBeh.Length;
            string[,] table = new string[length + 1, iconNames.Length + 1];

            rowList.Add("reel / icon");
            rowList.AddRange(iconNames);
            SetRow(table, rowList, 0, 0);

            for (int i = 1; i <= length; i++)
            {
                table[i, 0] = "reel #" + i.ToString();
                SetRow(table, new List<double>(slotGroupsBeh[i - 1].GetReelSymbHitPropabilities(slotIcons)), 1, i);
            }
            return table;
        }

        /// <summary>
        /// Calculate propabilities
        /// </summary>
        /// <returns></returns>
        public string[,] CreatePayTable(out double sumPayOut, out double sumPayoutFreeSpins)
        {
            List<string> row = new List<string>();
            List<float[]> reelSymbHitPropabilities = new List<float[]>();
            string[] iconNames = GetIconNames(false);

            sumPayOut = 0;
            CreateFullPaytable();
            int rCount = payTableFull.Count + 1;
            int cCount = slotGroupsBeh.Length + 3;
            string[,] table = new string[rCount, cCount];
            row.Add("PayLine / reel");
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                row.Add("reel #" + (i + 1).ToString());
            }
            row.Add("Payout");
            row.Add("Payout, %");
            SetRow(table, row, 0, 0);

            PayLine pL;
            List<PayLine> freeSpinsPL = new List<PayLine>();  // paylines with free spins
            double anyWinProb = 0;
            for (int i = 0; i < payTableFull.Count; i++)
            {
                pL = payTableFull[i];
                table[i + 1, 0] = "Payline #" + (i + 1).ToString();
                table[i + 1, cCount - 2] = pL.pay.ToString();
                double pOut = pL.GetPayOutProb(this);
                sumPayOut += pOut;
                table[i + 1, cCount - 1] = pOut.ToString("F6");
                SetRow(table, new List<string>(pL.Names(slotIcons, slotGroupsBeh.Length)), 1, i + 1);
                if (pL.freeSpins > 0) freeSpinsPL.Add(pL);
                anyWinProb += pL.GetProbability(this);
            }
            Debug.Log("any win pobability: " + anyWinProb);
            Debug.Log("sum (without free spins) % = " + sumPayOut);

            sumPayoutFreeSpins = 0;
            foreach (var item in freeSpinsPL)
            {
                //sumPayoutFreeSpins += sumPayOut * item.GetProbability(this) * item.freeSpins;
                sumPayoutFreeSpins += (item.GetProbability(this) * (double)item.freeSpins * 1.0);
            }

            sumPayoutFreeSpins = sumPayOut / (1.0 - sumPayoutFreeSpins * 1.0);

            Debug.Log("sum (with free spins) % = " + sumPayoutFreeSpins);

            return table;
        }

        private void SetRow<T>(string[,] table, List<T> row, int beginColumn, int rowNumber)
        {
            if (rowNumber >= table.GetLongLength(0)) return;
            if (typeof(T) == typeof(double))
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i + beginColumn < table.GetLongLength(1)) table[rowNumber, i + beginColumn] = (Convert.ToDouble(row[i])).ToString("F6");
                }
            }
            else
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i + beginColumn < table.GetLongLength(1)) table[rowNumber, i + beginColumn] = row[i].ToString();
                }
            }
        }

        public string[] GetIconNames(bool addAny)
        {
            if (slotIcons == null || slotIcons.Length == 0) return null;
            int length = (addAny) ? slotIcons.Length + 1 : slotIcons.Length;
            string[] sName = new string[length];
            if (addAny) sName[0] = "any";
            int addN = (addAny) ? 1 : 0;
            for (int i = addN; i < length; i++)
            {
                if (slotIcons[i - addN] != null && slotIcons[i - addN].iconSprite != null)
                {
                    sName[i] = slotIcons[i - addN].iconSprite.name;
                }
                else
                {
                    sName[i] = (i - addN).ToString();
                }
            }
            return sName;
        }

        internal WinSymbolBehavior GetWinPrefab(string tag)
        {
            if (winSymbolBehaviors == null || winSymbolBehaviors.Length == 0) return null;
            foreach (var item in winSymbolBehaviors)
            {
                if (item.WinTag.Contains(tag))
                {
                    return item;
                }
            }
            return null;
        }

        private void CreateFullPaytable()
        {
            payTableFull = new List<PayLine>();
            for (int j = 0; j < payTable.Count; j++)
            {
                payTable[j].ClampLine(slotGroupsBeh.Length);
                payTableFull.Add(payTable[j]);
                if (useWild) payTableFull.AddRange(payTable[j].GetWildLines(this));
            }
        }

        public void SetSideWinFlag()
        {
            sideWinFlag = true;
        }

        #region dev
        public void RebuildLines()
        {
            foreach (var item in payTable)
            {
                int[] line = new int[slotGroupsBeh.Length];

                for (int i = 0; i < line.Length; i++)
                {
                    if (i < item.line.Length) line[i] = item.line[i];
                    else line[i] = -1;
                }
                item.line = line;
            }
        }

        public string PaytableToJsonString()
        {
            string res = "";
            ListWrapper<PayLine> lW = new ListWrapper<PayLine>(payTable);
            res = JsonUtility.ToJson(lW);
            return res;
        }

        public void SetPayTableFromJson()
        {
            Debug.Log("Json viewer - " + "http://jsonviewer.stack.hu/");
            Debug.Log("old paytable json: " + PaytableToJsonString());

            if (string.IsNullOrEmpty(payTableJsonString))
            {
                Debug.Log("payTableJsonString : empty");
                return;
            }

            ListWrapper<PayLine> lWPB = JsonUtility.FromJson<ListWrapper<PayLine>>(payTableJsonString);
            if (lWPB != null && lWPB.list != null && lWPB.list.Count > 0)
            {
                payTable = lWPB.list;
            }
        }
        #endregion dev
    }
}

