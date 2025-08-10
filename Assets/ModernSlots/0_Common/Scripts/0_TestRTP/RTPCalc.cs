using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading;

// 08.07.2025 - fix
// 25.06.2025
namespace Mkey
{
    public class RTPCalc : MonoBehaviour
    {
        public int threadsCount = 8;
        public bool saveToFile = true;
        public TextAsset loosesFile;
        public TextAsset winsFile;
        public TextAsset winFSFile;
        public TextAsset winsJPFile;
        public TextAsset winsScatterFile;

        public Action RebuildAction;

        public int maxLinesCount = 1;
        private double lineBet = 1;  // default value

        #region temp vars
        private Dictionary<SlotGroupBehavior, List<ReelWindow>> sgReelWindows;
        private Dictionary<LineBehavior, WinDataCalc> lbWinDict;

        private List<List<ReelWindow>> slotWindowsList;
        private List<SlotWinPosition> winSlotWindows;
        private List<SlotWinPosition> winScatterSlotWindows;   // this array may intersect with array winSlotWindows
        private List<SlotWinPosition> winJPSlotWindows;        // this array may intersect with array winSlotWindows
        private List<SlotPosition> looseSlotWindows;

        private LineBehavior[] lbs;
        private List<PayLine> payTableFull;
        private List<LineBehavior> winLines;
        #endregion temp vars

        #region regular
        private void OnValidate()
        {
            if (threadsCount < 1) threadsCount = 1;
            if (maxLinesCount < 1) maxLinesCount = 1;
        }
        #endregion regular

        /// <summary>
        /// create all possible slot windows 
        /// </summary>
        public void CreateSlotWindows()
        {
            SlotController slotController = GetComponent<SlotController>();
            // first create reels windows for each reel
            sgReelWindows = new Dictionary<SlotGroupBehavior, List<ReelWindow>>();
            for (int i = 0; i < slotController.slotGroupsBeh.Length; i++)
            {
                CreateReelWindows(slotController.slotGroupsBeh[i]);
            }

            // create all possible slot windows
            Measure("slot windows creation time", () =>
            {
                // first create all possible reels positions
                List<List<int>> positionsComboNumbers;  //0 0 0 0 0; 0 0 0 0 1 .... 24 24 24 24 24
                positionsComboNumbers = new List<List<int>>();
                ComboCounterT cct = new ComboCounterT(sgReelWindows);
                List<int> combo = cct.combo;
                positionsComboNumbers.Add(new List<int>(combo));

                int i = 0;
                while (cct.NextCombo())
                {
                    combo = cct.combo;
                    positionsComboNumbers.Add(new List<int>(combo));
                    i++;
                }
                slotWindowsList = new List<List<ReelWindow>>();

                // foreac reels position create slot window - to speed up subsequent calculations
                List<ReelWindow> trList;
                ReelWindow tr = null;
                foreach (var item in positionsComboNumbers)
                {
                    trList = new List<ReelWindow>();
                    for (int t = 0; t < item.Count; t++)
                    {
                        tr = sgReelWindows [slotController.slotGroupsBeh[t]] [item[t]];
                        trList.Add(tr);
                    }
                    slotWindowsList.Add(trList);
                }

                Debug.Log("slotWindows combinations count " + slotWindowsList.Count);
            });
        }

        /// <summary>
        /// use virtual slot machine
        /// </summary>
        public void CalcWin()
        {
            SlotController slotController = GetComponent<SlotController>();

            lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);                                // only active

            TestSlot testSlot = new TestSlot(slotController, lbs, maxLinesCount);
            winSlotWindows = new List<SlotWinPosition>();
            looseSlotWindows = new List<SlotPosition>();
            winScatterSlotWindows = new List<SlotWinPosition>();
            winJPSlotWindows = new List<SlotWinPosition>();

            CreateSlotWindows();
            int linesCount = testSlot.testSlotLines.Length;
            Debug.Log("lines count: " + linesCount);

            int swLength = slotWindowsList.Count;


            Measure("win calc time", () =>
            {
                testSlot.CalcWin(slotWindowsList);
            });

            int wins = testSlot.CalcResult.winsCount;
            double pOut = testSlot.CalcResult.pay;
            int freeSpins = testSlot.CalcResult.freeSpins;
            int calcsCount = testSlot.CalcResult.calcsCount;

            winSlotWindows = testSlot.winSlotWindows;
            looseSlotWindows = testSlot.looseSlotWindows;
            winScatterSlotWindows = testSlot.winScatterSlotWindows;
            winJPSlotWindows = testSlot.winJPSlotWindows;

            double sumBet = (slotWindowsList.Count - freeSpins) * linesCount * lineBet;
            pOut = pOut / sumBet;

            Debug.Log("calcs: " + calcsCount + " ; wins: " + wins + " ;payout %: " + (pOut * 100f));
            Debug.Log("freeSpins: " + freeSpins);
            Debug.Log("winSlotWindows.Count " + winSlotWindows.Count);
            Debug.Log("looseSlotWindows.Count " + looseSlotWindows.Count);
            Debug.Log("winScatterSlotWindows.Count " + winScatterSlotWindows.Count);
            Debug.Log("winJPSlotWindows.Count " + winJPSlotWindows.Count);

            if (saveToFile) SaveToFile();
            RebuildAction?.Invoke();
        }

        /// <summary>
        /// use virtual slot machine with threads
        /// </summary>
        public void CalcWinThr()
        {
#if UNITY_EDITOR
            SlotController slotController = GetComponent<SlotController>();

            if (threadsCount < 1) threadsCount = 1;

            lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);        // only active

            Debug.Log("available processors count: " + AvailableProcessors);

            // create virtual slots
            TestSlot[] testSlots = new TestSlot[threadsCount];
            winSlotWindows = new List<SlotWinPosition>();
            looseSlotWindows = new List<SlotPosition>();
            winScatterSlotWindows = new List<SlotWinPosition>();
            winJPSlotWindows = new List<SlotWinPosition>();

            for (int i = 0; i < threadsCount; i++)
            {
                testSlots[i] = new TestSlot(slotController, lbs, maxLinesCount);
            }

            // create test windows for slot machine
            CreateSlotWindows();
            int linesCount = testSlots[0].testSlotLines.Length;
            Debug.Log("lines count: " + linesCount);

            int swLength = slotWindowsList.Count;
            int partLength = slotWindowsList.Count / threadsCount;

            Action<object> _calc = (o) =>
            {
                //Measure("win calc time", () =>
                //{
                    int procNumber = (int)o;
                    int startIndex = procNumber * partLength;
                    int endIndex = (procNumber == threadsCount - 1) ? slotWindowsList.Count - 1: startIndex + partLength - 1; // 08.07.2025
                    testSlots[procNumber].CalcPartWin(slotWindowsList, startIndex, endIndex);
                //});
            };

            Thread[] threads = new Thread[threadsCount];
            object lockOn = new object();

            // start all threads 
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i] = new Thread((object o) =>
                {
                    int tNumber = (int)o;
                    _calc.Invoke(o);
                });
                threads[i].Start(i);
            }

            // wait all threads
            float counter = 0;
            bool complete = false;
            while (!complete)
            {
                counter++;
                bool check = true;
                for (int i = 0; i < threadsCount; i++)
                {
                    if (threads[i].IsAlive) check = false;
                }
                complete = check;

                if (EditorUtility.DisplayCancelableProgressBar("Calculate", "Wait...", counter / 1000.0f))
                {
                    for (int i = 0; i < threadsCount; i++)
                    {
                        threads[i].Abort();
                    }
                    complete = true;
                    EditorUtility.ClearProgressBar();
                    Debug.Log("work cancelled...");
                    GC.Collect();
                    return;
                }
                Thread.Sleep(100);
            }
            Debug.Log("all threads complete");

            // ====== begin handle threads errors ======
            bool error = false;
            for (int i = 0; i < threadsCount; i++)
            {
                if (testSlots[i].TException != null) 
                {
                    Debug.LogError("Thread #: " + i + " error: " + testSlots[i].TException.Message +";  " + testSlots[i].TException.StackTrace);
                    error = true;
                }
            }

            if (error){
                EditorUtility.ClearProgressBar();
                Debug.Log("work was interrupted due to errors in threads");
                GC.Collect();
                return;
            }
            // ====== end handle thread errors ======


            // calculate complete result for each test slot machine
            int wins = 0;
            double pOut = 0;

            int freeSpins = 0;
            int calcsCount = 0;
            for (int i = 0; i < threadsCount; i++)
            {
                pOut += testSlots[i].CalcResult.pay;
                wins += testSlots[i].CalcResult.winsCount;
                freeSpins += testSlots[i].CalcResult.freeSpins;
                calcsCount += testSlots[i].CalcResult.calcsCount;
                winSlotWindows.AddRange(testSlots[i].winSlotWindows);
                looseSlotWindows.AddRange(testSlots[i].looseSlotWindows);
                winScatterSlotWindows.AddRange(testSlots[i].winScatterSlotWindows);
                winJPSlotWindows.AddRange(testSlots[i].winJPSlotWindows);

                testSlots[i] = null;
            }

            double sumBet = ((double)slotWindowsList.Count - (double)freeSpins) * (double)linesCount * lineBet; //
            double RTP = pOut / sumBet;
            Debug.Log("calcs: " + calcsCount + " ; wins: " + wins + " ;payout %: " + (RTP * 100f).ToString("F6"));
            Debug.Log("sum free spins: " + freeSpins);
            Debug.Log("winSlotWindows.Count: " + winSlotWindows.Count);
            Debug.Log("looseSlotWindows.Count: " + looseSlotWindows.Count);
            Debug.Log("winScatterSlotWindows.Count: " + winScatterSlotWindows.Count);
            Debug.Log("winJPSlotWindows.Count: " + winJPSlotWindows.Count);

            Debug.Log("Any Win probability = wins count / sum spins  = " +  (float)wins / (float)slotWindowsList.Count);

            // ======= mathematical expectation and variance ========= //
            // dispersion = sum ((pOut[i] - meanpout)pow2)
            double maxWin = 0;
            List<int> maxWinReelspositions = new List<int>();
            double meanPOut = pOut / slotWindowsList.Count;                                 // mathematical expectation
            double wM = 0;
            for (int i = 0; i < winSlotWindows.Count; i++)                                  // wins part
            {
                wM += Math.Pow((winSlotWindows[i].p - meanPOut), 2.0);
                if (winSlotWindows[i].p > maxWin) { maxWin = winSlotWindows[i].p; maxWinReelspositions = new List<int>( winSlotWindows[i].rP); }
            }
            double lM = (double)looseSlotWindows.Count * Math.Pow((0 - meanPOut), 2.0);     // looses part
            double dispersion = (wM + lM) / (double)slotWindowsList.Count;
            Debug.Log("Max Win: " + maxWin + ";   Mean Win:" + meanPOut.ToString("F4") + ";   Dispersion: " + dispersion.ToString("F4") + "; Standard Deviation: " + Math.Sqrt(dispersion).ToString("F4"));
            Debug.Log(maxWinReelspositions.MakeString(";"));
            if (saveToFile) SaveToFile();
            RebuildAction?.Invoke();
            winSlotWindows = null;
            looseSlotWindows = null;
            winJPSlotWindows = null;
            winScatterSlotWindows = null;
            GC.Collect();
            EditorUtility.ClearProgressBar();
#endif
        }

        #region private
        private static void Measure(string message, Action measProc)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();//https://msdn.microsoft.com/ru-ru/library/system.diagnostics.stopwatch%28v=vs.110%29.aspx
            stopWatch.Start();
            if (measProc != null) { measProc(); }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            UnityEngine.Debug.Log(message + "- elapsed time: " + elapsedTime);
        }

        /// <summary>
        /// create all possibles reel windows for reel
        /// </summary>
        /// <param name="slotGroup"></param>
        private void CreateReelWindows(SlotGroupBehavior slotGroup)
        {
            List<ReelWindow> reelWindows = new List<ReelWindow>();
            List<int> symbOrder = slotGroup.symbOrder;
            int windowSize = slotGroup.RayCasters.Length;
            List<int> rWind;

            for (int i = 0; i < symbOrder.Count; i++)
            {
                rWind = new List<int>(windowSize);
                for (int j = 0; j < windowSize; j++)
                {
                    rWind.Add(symbOrder[(int)Mathf.Repeat(i + j, symbOrder.Count)]);
                }
                reelWindows.Add(new ReelWindow(rWind, i));
            }

            sgReelWindows[slotGroup] = reelWindows;
        }

        private static int AvailableProcessors
        {
            get
            {
#if !NO_UNITY
                return UnityEngine.SystemInfo.processorCount;
#else
				return Environment.ProcessorCount;
#endif
            }
        }

        private void SaveToFile()
        {
#if UNITY_EDITOR
            if (winsFile)
            {
                string json = JsonUtility.ToJson(new ListWrapper<SlotWinPosition>(winSlotWindows));
                List<string> lines = new List<string>(json.GetLines());
                FileWorker.SaveToTextFile(AssetDatabase.GetAssetPath(winsFile), lines.ToArray());


                List<SlotWinPosition> fsSlotWindows = new List<SlotWinPosition>();

                foreach (var item in winSlotWindows)
                {
                    if (item.fS > 0) fsSlotWindows.Add(item);
                }

                json = JsonUtility.ToJson(new ListWrapper<SlotWinPosition>(fsSlotWindows));
                lines = new List<string>(json.GetLines());
                FileWorker.SaveToTextFile(AssetDatabase.GetAssetPath(winFSFile), lines.ToArray());
                Debug.Log("fsSlotWindows.count: " + fsSlotWindows.Count);
            }
            else
            {
                Debug.LogError("winSlotWindows failed file reference");
            }

            if (loosesFile)
            {
                string json = JsonUtility.ToJson(new ListWrapper<SlotPosition>(looseSlotWindows));
                List<string> lines = new List<string>(json.GetLines());
                FileWorker.SaveToTextFile(AssetDatabase.GetAssetPath(loosesFile), lines.ToArray());
            }
            else
            {
                Debug.LogError("looseSlotWindows failed file reference");
            }

            if (winsScatterFile)
            {
                string json = JsonUtility.ToJson(new ListWrapper<SlotWinPosition>(winScatterSlotWindows));
                List<string> lines = new List<string>(json.GetLines());
                FileWorker.SaveToTextFile(AssetDatabase.GetAssetPath(winsScatterFile), lines.ToArray());
            }
            if (winsJPFile)
            {
                string json = JsonUtility.ToJson(new ListWrapper<SlotWinPosition>(winJPSlotWindows));
                List<string> lines = new List<string>(json.GetLines());
                FileWorker.SaveToTextFile(AssetDatabase.GetAssetPath(winsJPFile), lines.ToArray());
            }

            AssetDatabase.Refresh();
#endif
        }
        #endregion private
    }

    public class TestSlot
    {
        public TestReel[] testReels;                                // virtual slot reels
        public TestSlotLine[] testSlotLines;                        // virtual slot lines, needed lines must be enabled before using virtual test slot
        public List<PayLine> payTableFull;
        public List<ScatterPay> scatterPayTable;
        public List<bool> wildSubstitutet;

        private bool useWild = false;
        private int wildID;
        private bool useScatter = false;
        private int scatterID;

        private bool useMiniJackPot = false;
        private bool useMaxiJackPot = false;
        private bool useMegaJackPot = false;
        private int megaJackPotCount;
        private int maxiJackPotCount;
        private int miniJackPotCount;
        private int miniJackStartAmount;
        private int maxiJackStartAmount;
        private int megaJackStartAmount;

        private int jp_symb_id;
        private bool useAnyJackPot = false;
        private Dictionary<int, int> symbJPDict;    //  <id, count>
        private JackPotType jackPotType = JackPotType.None;

        public SlotIcon[] slotIcons;

        // slot window processing data
        public Dictionary<TestSlotLine, WinDataCalc> lineWinsDict;          // is filled when processing new slot window
        public List<TestSlotLine> winLines;                                 // is filled when processing new slot window
        public WinDataCalc scatterWinCalc;                                  // is filled when processing new slot window

        // complete result of processed windows slot machine
        public List<SlotWinPosition> winSlotWindows;                          // cached data of all processed slot windows
        public List<SlotWinPosition> winScatterSlotWindows;                   // cached data of all processed slot windows
        public List<SlotWinPosition> winJPSlotWindows;                        // cached data of all processed slot windows
        public List<SlotPosition> looseSlotWindows;
        public TestWindata CalcResult { get; private set; }
        public int Calcs { get; private set; }
        public Exception TException { get; private set; }

        private int maxLinesCount;

        #region public
        public TestSlot(SlotController slotController, LineBehavior[] lines, int maxLinesCount)
        {
            this.maxLinesCount = (int) MathF.Max (1, maxLinesCount);
            slotIcons = new List<SlotIcon> (slotController.slotIcons).ToArray();

            testReels = new TestReel[slotController.slotGroupsBeh.Length];
            wildSubstitutet = new List<bool>();

            useWild = slotController.useWild;
            wildID = slotController.wild_id;
            useScatter = slotController.useScatter;
            scatterID = slotController.scatter_id;

            megaJackPotCount = slotController.megaJackPotCount;
            maxiJackPotCount = slotController.maxiJackPotCount;
            miniJackPotCount = slotController.miniJackPotCount;
            useMiniJackPot = slotController.useMiniJacPot && (miniJackPotCount > 0);
            useMaxiJackPot = slotController.useMaxiJacPot && (maxiJackPotCount > 0);
            useMegaJackPot = slotController.useMegaJacPot && (megaJackPotCount > 0);
            useAnyJackPot = (useMiniJackPot || useMaxiJackPot || useMegaJackPot);
            miniJackStartAmount = slotController.controls.MiniJackPotStart;
            maxiJackStartAmount = slotController.controls.MaxiJackPotStart;
            megaJackStartAmount = slotController.controls.MegaJackPotStart;

            jp_symb_id = slotController.jp_symbol_id;

            lineWinsDict = new Dictionary<TestSlotLine, WinDataCalc>();

            for (int i = 0; i < slotController.slotGroupsBeh.Length; i++)
            {
                testReels[i] = new TestReel(slotController.slotGroupsBeh[i]);
            }

            if (lines != null && lines.Length > 0)
            {
                testSlotLines = new TestSlotLine[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    testSlotLines[i] = new TestSlotLine(lines[i], testReels);
                }
            }
            else  // create all possible lines 
            {
                CreateAllPossibleTestLines(slotController, testReels, maxLinesCount);
            }

            CreateFullPaytable(slotController);
            CreateScatterPaytable(slotController);

            foreach (var item in slotController.slotIcons)
            {
                wildSubstitutet.Add(item.useWildSubstitute);
            }
        }

        /// <summary>
        /// calculate win data for all array elements
        /// </summary>
        /// <param name="slotWindowsList"></param>
        public void CalcWin(List<List<ReelWindow>> slotWindowsList)
        {
            CalcPartWin(slotWindowsList, 0, slotWindowsList.Count - 1);
        }

        /// <summary>
        /// partial win data calculate, from start index to end index array of slot windows
        /// </summary>
        /// <param name="slotWindowsList"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        public void CalcPartWin(List<List<ReelWindow>> slotWindowsList, int startIndex, int endIndex)
        {
            TException = null;
            try
            {
                Calcs = 0;
                int wins = 0;
                double pOut = 0;
                int sumFreeSpins = 0;
                int swLength = slotWindowsList.Count;
                if (endIndex >= swLength) endIndex = swLength - 1;
                winSlotWindows = new List<SlotWinPosition>();
                looseSlotWindows = new List<SlotPosition>();
                winScatterSlotWindows = new List<SlotWinPosition>();
                winJPSlotWindows = new List<SlotWinPosition>();

                for (int i = startIndex; i <= endIndex; i++)
                {
                    Calcs++;
                    var item = slotWindowsList[i];
                    int freeSpins = 0;
                    int pay = 0;
                    int payMult = 1;

                    int freeSpinsScat = 0;
                    int payScat = 0;
                    int payMultScat = 1;

                    CalcSlotWindowWin(item, ref freeSpins, ref pay, ref payMult, ref freeSpinsScat, ref payScat, ref payMultScat);
                    pay += payScat;
                    freeSpins += freeSpinsScat;
                    payMult *= payMultScat;
                    pay *= payMult;

                    pOut += (double)pay;

                    if (winLines.Count > 0 || scatterWinCalc != null || jackPotType != JackPotType.None)
                    {
                        wins++;
                        winSlotWindows.Add(new SlotWinPosition(item, pay, freeSpins));
                        if (scatterWinCalc != null) winScatterSlotWindows.Add(new SlotWinPosition(item, pay, freeSpins));

                        if (jackPotType == JackPotType.Mini)
                        {
                            winJPSlotWindows.Add(new SlotWinPosition(item, miniJackStartAmount, 0));
                        }
                        else if (jackPotType == JackPotType.Maxi)
                        {
                            winJPSlotWindows.Add(new SlotWinPosition(item, maxiJackStartAmount, 0));
                        }
                        else if (jackPotType == JackPotType.Mega)
                        {
                            winJPSlotWindows.Add(new SlotWinPosition(item, megaJackStartAmount, 0));
                        }
                    }
                    else
                    {
                        looseSlotWindows.Add(new SlotPosition(item));
                    }
                    sumFreeSpins += freeSpins;
                }

                // Debug.Log("calcs: " + (endIndex - startIndex + 1) + " ; wins: " + wins + " ;payout %: " + (pOut * 100f) + " ;payout_1 %: " + (pOut_1 * 100f));
                CalcResult = new TestWindata(pOut, sumFreeSpins, wins, endIndex - startIndex + 1);
            }
            catch (Exception exc)
            {
                TException = exc;
                Debug.LogError(exc.Message + ";  " + exc.StackTrace);
            }
        }

        public void GetPayout(out double sumPayOut, out double sumPayoutFreeSpins)
        {
            sumPayOut = 0;
            sumPayoutFreeSpins = 0;

            PayLine pL;
            List<PayLine> freeSpinsPL = new List<PayLine>();  // paylines with free spins
            double anyWinProb = 0;

            for (int i = 0; i < payTableFull.Count; i++)
            {
                pL = payTableFull[i];
                double pOut = GetPayOutProb(pL);
                // Debug.Log(i + ") payout: " + pOut);
                sumPayOut += pOut;
                if (pL.freeSpins > 0) freeSpinsPL.Add(pL);
                anyWinProb += GetProbability(pL);
            }

            foreach (var item in freeSpinsPL)
            {
                sumPayoutFreeSpins += (GetProbability(item) * (double)item.freeSpins * 1.0);        // for 1 slot line 
            }

            sumPayoutFreeSpins = sumPayOut / (1.0 - sumPayoutFreeSpins * testSlotLines.Length);
        }
        #endregion public

        #region private
        /// <summary>
        /// calc win for appropriate slot window
        /// </summary>
        /// <param name="trList"></param>
        /// <param name="freeSpins"></param>
        /// <param name="pay"></param>
        /// <param name="payMult"></param>
        /// <param name="freeSpinsScat"></param>
        /// <param name="payScat"></param>
        /// <param name="payMultScat"></param>
        private void CalcSlotWindowWin(List<ReelWindow> slotWindow, ref int freeSpins, ref int pay, ref int payMult, ref int freeSpinsScat, ref int payScat, ref int payMultScat)
        {
            SetSlotWindow(slotWindow);
            winLines = new List<TestSlotLine>();
            SearchWin();

            pay = 0;
            freeSpins = 0;
            payMult = 1;

            int linePayMult;
            foreach (var item in winLines)
            {
                pay += lineWinsDict[item].Pay;
                freeSpins += lineWinsDict[item].FreeSpins;
                linePayMult = lineWinsDict[item].PayMult;
                if (linePayMult != 0) payMult *= linePayMult;
            }
            
            payScat = 0;
            freeSpinsScat = 0;
            payMultScat = 1;
            if (scatterWinCalc != null)
            {
                freeSpinsScat = scatterWinCalc.FreeSpins;
                payScat = scatterWinCalc.Pay;
                payMultScat = (scatterWinCalc.PayMult != 0) ? scatterWinCalc.PayMult : 1;
            }
        }

        private void SetSlotWindow(List<ReelWindow> slotWindowReels)
        {
            TestRayCaster[] rs;
            int reelWindowSize;
            for (int i = 0; i < testReels.Length; i++)
            {
                rs = testReels[i].testRayCasters;
                reelWindowSize = rs.Length;

                for (int j = 0; j < reelWindowSize; j++)
                {
                    rs[j].ID = slotWindowReels[i].revOrdering[j];
                }
            }
        }

        private WinDataCalc GetPayLineWin(TestSlotLine testSlotLine, PayLine payLine)
        {
            int winnSymbols = 0;
            int mainPS = payLine.line[0];                   // main payline symbol ID
            int ls;
            int ps;
            bool lineIsBroken = false;      // for broken slotline last symbols not affects

            bool _useWild = useWild && wildSubstitutet[mainPS];
            if (!_useWild)
            {
                for (int i = 0; i < testSlotLine.testRayCasters.Length; i++)
                {
                    ls = testSlotLine.testRayCasters[i].ID;     // symbol from slot line
                    ps = payLine.line[i];                       // symbol from pay line
                    if (ps >= 0)
                    {
                        if (ls != ps) { return null; }
                        else { winnSymbols++; }
                    }
                    else        // ps < 0 (any)
                    {
                        if (!lineIsBroken && ls == mainPS) { return null; }
                        lineIsBroken = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < testSlotLine.testRayCasters.Length; i++)
                {
                    ls = testSlotLine.testRayCasters[i].ID;     // symbol from slot line
                    ps = payLine.line[i];                       // symbol from pay line 
                    if (ps >= 0)
                    {
                        if (ls != ps) { return null; }
                        else { winnSymbols++; }
                    }
                    else        // ps < 0 (any)
                    {
                        if (!lineIsBroken && ls == mainPS) { return null; }
                        if (!lineIsBroken && ls == wildID) { return null; }
                        lineIsBroken = true;
                    }
                }
            }
            return new WinDataCalc(winnSymbols, payLine.freeSpins, payLine.pay, payLine.payMult);
        }

        private void CreateFullPaytable(SlotController slotController)
        {
            payTableFull = new List<PayLine>();
            for (int j = 0; j < slotController.payTable.Count; j++)
            {
                payTableFull.Add(slotController.payTable[j]);
                if (slotController.useWild) payTableFull.AddRange(slotController.payTable[j].GetWildLines(slotController));
            }
        }

        private void CreateScatterPaytable(SlotController slotController)
        {
            scatterPayTable = new List<ScatterPay>();

            foreach (var item in slotController.scatterPayTable)
            {
                ScatterPay scatterPay = new ScatterPay();
                scatterPay.pay = item.pay;
                scatterPay.payMult = item.payMult;
                scatterPay.freeSpins = item.freeSpins;
                scatterPay.freeSpinsMult = item.freeSpinsMult;
                scatterPay.scattersCount = item.scattersCount;
                scatterPayTable.Add(scatterPay);
            }
        }

        private void FindLineWin(TestSlotLine lineBehavior, List<PayLine> payTable)
        {
            lineWinsDict[lineBehavior] = null;
            WinDataCalc winTemp;
            foreach (var item in payTable)
            {
                winTemp = GetPayLineWin(lineBehavior, item);
                if (winTemp != null)
                {
                    lineWinsDict[lineBehavior] = winTemp;
                    winLines.Add(lineBehavior);
                    return;
                }
            }
        }

        /// <summary>
        /// search win for appropriate slot window
        /// </summary>
        private void SearchWin()
        {
            foreach (TestSlotLine tLine in testSlotLines)
            {
                FindLineWin(tLine, payTableFull);
            }

            // search scatters
            scatterWinCalc = null;
            if (useScatter)
            {
                int scatterWinS = 0;
                int scatterSymbolsTemp = 0;

                foreach (var item in testReels)
                {
                    scatterSymbolsTemp = 0;
                    for (int i = 0; i < item.testRayCasters.Length; i++)
                    {
                        if (item.testRayCasters[i].ID == scatterID)
                        {
                            scatterSymbolsTemp++;
                        }
                    }
                    scatterWinS += scatterSymbolsTemp;
                }

                foreach (var item in scatterPayTable)
                {
                    if (item.scattersCount > 0 && item.scattersCount == scatterWinS)
                    {
                        scatterWinCalc = new WinDataCalc(scatterWinS, item.freeSpins, item.pay, item.payMult);
                    }
                }
            }

            // search JP
            jackPotType = (useAnyJackPot) ? GetJackPotWin() : JackPotType.None;
        }

        private void CreateAllPossibleTestLines(SlotController slotController, TestReel[] testReels, int maxCount)
        {
            SlotGroupBehavior[] sGB = slotController.slotGroupsBeh;
            int[] rcCounts = new int[sGB.Length];       // raycasters counts by reel
            for (int i = 0; i < sGB.Length; i++)
            {
                rcCounts[i] = sGB[i].RayCasters.Length;
            }

            List<int[]> rcCombos = CreateRCCombos(rcCounts);

            maxCount = (rcCombos.Count < maxCount) ? rcCombos.Count : maxCount;
            testSlotLines = new TestSlotLine[maxCount];
            for (int i = 0; i < maxCount; i++)
            {
                int[] combo = rcCombos[i];
                testSlotLines[i] = new TestSlotLine(combo, testReels);
            }
        }

        /// <summary>
        /// Return all possible rc combos by rc number (from 1 to rc.length)
        /// </summary>
        /// <param name="counts"></param>
        /// <returns></returns>
        private List<int[]> CreateRCCombos(int[] counts)
        {
            List<int[]> res = new List<int[]>();
            int length = counts.Length;
            int decLength = length - 1;
            int[] counter = new int[length];
            for (int i = decLength; i >= 0; i--)
            {
                counter[i] = (counts[i] > 0) ? 1 : 0; // 0 - empty 
            }
            int[] copy = new int[length];//copy arr
            counter.CopyTo(copy, 0);
            res.Add(copy);

            bool result = true;
            while (result)
            {
                result = false;
                for (int i = decLength; i >= 0; i--)    // find new combo
                {
                    if (counter[i] < counts[i] && counter[i] > 0)
                    {
                        counter[i]++;
                        if (i != decLength) // reset low "bytes"
                        {
                            for (int j = i + 1; j < length; j++)
                            {
                                if (counter[j] > 0) counter[j] = 1;
                            }
                        }
                        result = true;
                        copy = new int[length];//copy arr
                        counter.CopyTo(copy, 0);
                        res.Add(copy);
                        break;
                    }
                }
            }
            return res;
        }

        private JackPotType GetJackPotWin()
        {
            symbJPDict = new Dictionary<int, int>(); // <id, count>
            TestRayCaster[] rCs;
            // create symbols dictionary
            foreach (var item in testReels)
            {
                rCs = item.testRayCasters;
                foreach (var rc in rCs)
                {
                    int sID = rc.ID;
                    if (symbJPDict.ContainsKey(sID))
                    {
                        symbJPDict[sID]++;
                    }
                    else
                    {
                        symbJPDict[sID] = 1;
                    }
                }
            }

            // search jackPot id if symbol is any
            if (jp_symb_id == -1)
            {
                int sCount = 0;
                int id = -1;
                foreach (var item in symbJPDict)
                {
                    if (item.Value > sCount)
                    {
                        sCount = item.Value;
                        id = item.Key;
                    }
                }

                if (useMegaJackPot && sCount >= megaJackPotCount)
                {
                    return JackPotType.Mega;
                }
                if (useMaxiJackPot && sCount >= maxiJackPotCount)
                {
                    return JackPotType.Maxi;
                }
                if (useMiniJackPot && sCount >= miniJackPotCount)
                {
                    return JackPotType.Mini;
                }
            }
            else
            {
                if (symbJPDict.ContainsKey(jp_symb_id))
                {
                    int count = symbJPDict[jp_symb_id];
                    if (useMegaJackPot && count >= megaJackPotCount)
                    {
                        return JackPotType.Mega;
                    }
                    if (useMaxiJackPot && count >= maxiJackPotCount)
                    {
                        return JackPotType.Maxi;
                    }
                    if (useMiniJackPot && count >= miniJackPotCount)
                    {
                        return JackPotType.Mini;
                    }
                }
            }
            return JackPotType.None;
        }

        public double GetProbability(PayLine pl)
        {
            double res = 0;
            int[] line = pl.line;

            if (line == null || testReels == null || testReels.Length > line.Length) return res;
            double[] rP = testReels[0].GetReelSymbHitPropabilities(slotIcons);

            //avoid "any" symbol error in first position
            res = (line[0] >= 0) ? rP[line[0]] : 1; //  res = rP[line[0]];
            int mainID = line[0];
            bool lineIsBroken = false;      // for broken slotline last symbols not affects
            bool _useWild = useWild && slotIcons[mainID].useWildSubstitute;

            for (int i = 1; i < testReels.Length; i++)
            {
                rP = testReels[i].GetReelSymbHitPropabilities(slotIcons);
                if (line[i] >= 0)                               // not any,  any.ID = -1
                {
                    res *= rP[line[i]];
                }
                else                                            // any.ID = -1, ANY != mainID, if (usewild) ANY != wildID
                {
                    if (!lineIsBroken)
                    {
                        if (_useWild && mainID != wildID) res *= (1.0 - rP[mainID] - rP[wildID]);
                        else if (!_useWild) res *= (1.0 - rP[mainID]);
                    }
                    else
                    {
                        res *= 1.0;
                    }
                    lineIsBroken = true;
                }
            }
            return res;
        }

        public double GetPayOutProb(PayLine pL)
        {
            return GetProbability(pL) * 100.0 * (double)pL.pay;
        }
        #endregion private

        #region classes
        public class TestSlotLine
        {
            public TestRayCaster[] testRayCasters;

            public TestSlotLine(LineBehavior lineBehavior, TestReel[] testReels)
            {
                testRayCasters = new TestRayCaster[testReels.Length];
                for (int i = 0; i < testReels.Length; i++)
                {
                    var tReel = testReels[i];
                    RayCaster rC = lineBehavior.rayCasters[i];
                    foreach (var tRcaster in tReel.testRayCasters)
                    {
                        if(rC == tRcaster.rayCaster)
                        {
                            testRayCasters[i] = tRcaster;
                        }
                    }
                } 
            }

            public TestSlotLine(int[] rcCombo, TestReel[] testReels)
            {
                testRayCasters = new TestRayCaster[testReels.Length];
                for (int i = 0; i < testReels.Length; i++)
                {
                    var tReel = testReels[i];
                    testRayCasters[i] = tReel.testRayCasters[rcCombo[i] - 1];
                }
            }
        }

        public class TestReel
        {
            public TestRayCaster[] testRayCasters;
            private List<int> symbOrder;

            public TestReel(SlotGroupBehavior slotGroupBehavior)
            {
                testRayCasters = new TestRayCaster[slotGroupBehavior.RayCasters.Length];
                symbOrder = new List<int> (slotGroupBehavior.symbOrder);

                for (int i = 0; i < slotGroupBehavior.RayCasters.Length; i++)
                {
                    testRayCasters[i] = new TestRayCaster(slotGroupBehavior.RayCasters[i]);
                }
            }

            /// <summary>
            /// Return probabilties for eac symbol according to symbOrder array 
            /// </summary>
            /// <returns></returns>
            internal double[] GetReelSymbHitPropabilities(SlotIcon[] symSprites)
            {
                if (symSprites == null || symSprites.Length == 0) return null;
                double[] probs = new double[symSprites.Length];
                int length = symbOrder.Count;
                for (int i = 0; i < length; i++)
                {
                    int n = symbOrder[i];
                    probs[n]++;
                }
                for (int i = 0; i < probs.Length; i++)
                {
                    probs[i] = probs[i] / (double)length;
                }
                return probs;
            }

            public string OrderToJsonString()
            {
                string res = "";
                ListWrapperStruct<int> lW = new ListWrapperStruct<int>(symbOrder);
                res = JsonUtility.ToJson(lW);
                return res;
            }

            public void AddSymbol(int slotIconID)
            {
                symbOrder.Add(slotIconID);
            }

            public void RemoveLastSymbol()
            {
                symbOrder.RemoveAt(symbOrder.Count-1);
            }
        }

        public class TestRayCaster
        {
            public RayCaster rayCaster;
            public int ID;

            public TestRayCaster(RayCaster rayCaster)
            {
                this.rayCaster = rayCaster;
            }
        }

        public class TestWindata
        {
            public int freeSpins = 0;
            public double pay = 0;
            public int winsCount = 0;
            public int calcsCount = 0;

            public TestWindata(double pay, int freeSpins, int winsCount, int calcsCount)
            {
                this.pay = pay;
                this.freeSpins = freeSpins;
                this.winsCount = winsCount;
                this.calcsCount = calcsCount;
            }
        }
        #endregion classes
    }

    /// <summary>
    /// helper clas to serialize win slot reels positions and win data
    /// </summary>
    [Serializable]
    public class SlotWinPosition
    {
        public int[] rP;            // reels positions
        public int p;               // pay
        public int fS;              // free spins

        public SlotWinPosition(List<ReelWindow> slotWindow, int pay, int freeSpins)
        {
            p = pay;
            fS = freeSpins;
            rP = new int[slotWindow.Count];
            for (int i = 0; i < slotWindow.Count; i++)
            {
                rP[i] = slotWindow[i].position;
            }
        }
    }

    /// <summary>
    /// helper clas to serialize slot reels positions
    /// </summary>
    [Serializable]
    public class SlotPosition
    {
        public int[] rP;          // reels positions

        public SlotPosition(List<ReelWindow> slotWindow)
        {
            rP = new int[slotWindow.Count];
            for (int i = 0; i < slotWindow.Count; i++)
            {
                rP[i] = slotWindow[i].position;
            }
        }
    }

    [Serializable]
    public class ReelWindow
    {
        public List<int> ordering;
        public List<int> revOrdering;
        public int position;        // reel position after spin

        public ReelWindow(List<int> ordering, int position)
        {
            this.ordering = new List<int>(ordering);
            revOrdering = new List<int>(ordering);
            revOrdering.Reverse();                  // use reverse ordering in ctor, 0 - raycaster - at TOP
            this.position = position;               // reel position after spin
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < ordering.Count; i++)
            {
                res += ordering[i];
                if (i < ordering.Count - 1) res += ", ";
            }
            return res;
        }
    }

    public class ComboCounterT
    {
        public List<int> combo;
        List<byte> counterSizes;

        public ComboCounterT(Dictionary<SlotGroupBehavior, List<ReelWindow>> sGD)
        {
            counterSizes = new List<byte>();
            foreach (var item in sGD)
            {
                counterSizes.Add((byte) item.Value.Count);
            }

            combo = new List<int>(counterSizes.Count);

            for (int i = 0; i < counterSizes.Count; i++) // create in counter first combination
            {
                combo.Add(0);
            }
        }

        private bool Next()
        {
            for (int i = counterSizes.Count - 1; i >= 0; i--)
            {
                if (combo[i] < counterSizes[i] - 1)
                {
                    combo[i]++;
                    if (i != counterSizes.Count - 1) // reset low "bytes"
                    {
                        for (int j = i + 1; j < counterSizes.Count; j++)
                        {
                            combo[j] = 0;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool NextCombo()
        {
            if (Next())
            {
                return true;
            }
            return false;
        }
    }

    public class WinDataCalc
    {
        int symbols;
        private int freeSpins = 0;
        private int pay = 0;
        private int payMult = 1;

        public int Pay
        {
            get { return pay; }
        }

        public int FreeSpins
        {
            get { return freeSpins; }
        }

        public int PayMult
        {
            get { return payMult; }
        }

        public WinDataCalc(int symbols, int freeSpins, int pay, int payMult)
        {
            this.symbols = symbols;
            this.freeSpins = freeSpins;
            this.pay = pay;
            this.payMult = payMult;
        }

        public override string ToString()
        {
            return "Pay: " + pay + " ; FreeSpin: " + freeSpins + " ; PayMult: " + payMult;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RTPCalc))]
    public class PCalcEditor : Editor
    {
        RTPCalc pCalc;
        public override void OnInspectorGUI()
        {
            pCalc = (RTPCalc)target;
            DrawDefaultInspector();

            #region calculate
            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Button("Calc winsTh"))
            {
                pCalc.CalcWinThr();
            }

            if (GUILayout.Button("Calc wins"))
            {
                pCalc.CalcWin();
            }

            EditorGUILayout.EndHorizontal();
            #endregion calculate
        }
    }
#endif
}

/*
         #region old
        /// <summary>
        /// use virtual slot machine with threads
        /// </summary>
        public void CalcWinThr_main()
        {
            if (threadsCount < 1) threadsCount = 1;
            if (started) return;
            started = true;

            GC.Collect();

            bool workComplete = false;

            lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);        // only active
            if (lbs.Length == 0)
            {
                Debug.LogError("Create slot lines before start....");
                return;
            }



            Debug.Log("available processors count: " + AvailableProcessors);

            // create virtual slots
            TestSlot[] testSlots = new TestSlot[threadsCount];
            winSlotWindows = new List<SlotWinPosition>();
            looseSlotWindows = new List<SlotPosition>();
            winScatterSlotWindows = new List<SlotWinPosition>();
            winJPSlotWindows = new List<SlotWinPosition>();

            for (int i = 0; i < threadsCount; i++)
            {
                testSlots[i] = new TestSlot(slotController, lbs);
            }

            // create test windows for slot machine
            CreateSlotWindows();
            Debug.Log("lines count: " + lbs.Length);

            int swLength = slotWindowsList.Count;
            int partLength = slotWindowsList.Count / threadsCount;

            Action<object> _calc = (o) =>
            {
                Measure("win calc time", () =>
                {
                    int procNumber = (int)o;
                    int startIndex = procNumber * partLength;
                    int endIndex = startIndex + partLength - 1;
                    testSlots[procNumber].CalcPartWin(slotWindowsList, startIndex, endIndex);
                });
            };


            bool[] threadComplete = new bool[threadsCount];
            Thread[] threads = new Thread[threadsCount];
            object lockOn = new object();

            // start all threads 
            for (int i = 0; i < threadsCount; i++)
            {
                threadComplete[i] = false;
                threads[i] = new Thread((object o) =>
                {
                    int tNumber = (int)o;
                    _calc.Invoke(o);
                    lock (lockOn)
                    {
                        
                        threadComplete[(int)o] = true;
                    }
                });
                threads[i].Start(i);
            }

            Thread managerT = new Thread(() =>
            {
                bool complete = false;
                while (!complete)
                {
                    bool check = true;
                    lock (lockOn)
                    {
                        for (int i = 0; i < threadsCount; i++)
                        {
                            if (threadComplete[i] == false) check = false;
                        }
                    }
                    complete = check;
                    Thread.Sleep(100);
                }
                Debug.Log("all threads complete");

                // calculate complete result for each test slot machine
                int wins = 0;
                double pOut = 0;
                int freeSpins = 0;
                int calcsCount = 0;
                for (int i = 0; i < threadsCount; i++)
                {
                    pOut += testSlots[i].CalcResult.pay;
                    wins += testSlots[i].CalcResult.winsCount;
                    freeSpins += testSlots[i].CalcResult.freeSpins;
                    calcsCount += testSlots[i].CalcResult.calcsCount;
                    winSlotWindows.AddRange(testSlots[i].winSlotWindows);
                    looseSlotWindows.AddRange(testSlots[i].looseSlotWindows);
                    winScatterSlotWindows.AddRange(testSlots[i].winScatterSlotWindows);
                    winJPSlotWindows.AddRange(testSlots[i].winJPSlotWindows);

                    testSlots[i] = null;
                }

                double sumBet = (slotWindowsList.Count - freeSpins) * lbs.Length * lineBet;
                pOut = pOut / sumBet;

                Debug.Log("calcs: " + calcsCount + " ; wins: " + wins + " ;payout %: " + (pOut * 100f));

                Debug.Log("Processed and cached slot windows count: " + winSlotWindows.Count);
                workComplete = true;
            });

            managerT.Start();
            float counter = 0;
            while (!workComplete)
            {
                counter++;
                bool cancel = false;
                if (EditorUtility.DisplayCancelableProgressBar("Cancelable", "Doing some work...", counter / 100.0f))
                {
                    cancel = true;
                }
                //  Debug.Log("wait threads");
                Thread.Sleep(100);
            }
            if (saveToFile) SaveToFile();
            started = false;

            winSlotWindows = null;
            looseSlotWindows = null;
            winJPSlotWindows = null;
            winScatterSlotWindows = null;
            GC.Collect();
            EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// use virtual slot machine with threads
        /// </summary>
        public void CalcWinTask()
        {
            if (threadsCount < 1) threadsCount = 1;
           // if (started) return;
            started = true;

            lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);        // only active
            if (lbs.Length == 0)
            {
                Debug.LogError("Create slot lines before start....");
                return;
            }

            Debug.Log("available processors count: " + AvailableProcessors);

            // create virtual slots
            TestSlot[] testSlots = new TestSlot[threadsCount];
            winSlotWindows = new List<SlotWinPosition>();
            looseSlotWindows = new List<SlotPosition>();
            winScatterSlotWindows = new List<SlotWinPosition>();
            winJPSlotWindows = new List<SlotWinPosition>();

            for (int i = 0; i < threadsCount; i++)
            {
                testSlots[i] = new TestSlot(slotController, lbs);
            }

            // create test windows for slot machine
            CreatSlotWindows();
            Debug.Log("lines count: " + lbs.Length);

            int swLength = slotWindowsList.Count;
            int partLength = slotWindowsList.Count / threadsCount;

            Task [] tasks = new Task[threadsCount];

            // start all tasks 
            for (int i = 0; i < threadsCount; i++)
            {
                tasks[i] = new Task(() =>
                {
                    int procNumber = i;
                    int startIndex = procNumber * partLength;
                    int endIndex = startIndex + partLength - 1;
                    Measure("win calc time", () =>
                    {
                        testSlots[procNumber].CalcPartWin(slotWindowsList, startIndex, endIndex);
                    });
                });
                tasks[i].Start();
               // tasks[i].Wait();
            }
            return;
            // wait all tasks
            //float counter = 0;
            //bool complete = false;
            //while (!complete)
            //{
            //    counter++;
            //    bool check = true;
            //    for (int i = 0; i < threadsCount; i++)
            //    {
            //        if (!tasks[i].IsCompletedSuccessfully) check = false;
            //    }
            //    complete = check;

            //    if (EditorUtility.DisplayCancelableProgressBar("Cancelable", "Doing some work...", counter / 1000.0f))
            //    {
            //        for (int i = 0; i < threadsCount; i++)
            //        {
            //           //  tasks[i].c;
            //        }
            //        complete = true;
            //        started = false;
            //        EditorUtility.ClearProgressBar();
            //        Debug.Log("work cancelled...");
            //        GC.Collect();
            //        return;
            //    }
            //    Thread.Sleep(100);
            //}
            Debug.Log("all threads complete");

            // calculate complete result for each test slot machine
            int wins = 0;
            double pOut = 0;
            int freeSpins = 0;
            int calcsCount = 0;
            for (int i = 0; i < threadsCount; i++)
            {
                pOut += testSlots[i].CalcResult.pay;
                wins += testSlots[i].CalcResult.winsCount;
                freeSpins += testSlots[i].CalcResult.freeSpins;
                calcsCount += testSlots[i].CalcResult.calcsCount;
                winSlotWindows.AddRange(testSlots[i].winSlotWindows);
                looseSlotWindows.AddRange(testSlots[i].looseSlotWindows);
                winScatterSlotWindows.AddRange(testSlots[i].winScatterSlotWindows);
                winJPSlotWindows.AddRange(testSlots[i].winJPSlotWindows);

                testSlots[i] = null;
            }

            double sumBet = (slotWindowsList.Count - freeSpins) * lbs.Length * lineBet;
            pOut = pOut / sumBet;

            Debug.Log("calcs: " + calcsCount + " ; wins: " + wins + " ;payout %: " + (pOut * 100f));

            Debug.Log("Processed and cached slot windows count: " + winSlotWindows.Count);

            if (saveToFile) SaveToFile();
            started = false;
            winSlotWindows = null;
            looseSlotWindows = null;
            winJPSlotWindows = null;
            winScatterSlotWindows = null;
            GC.Collect();
            EditorUtility.ClearProgressBar();
        }
        public void SetSlotWindow(List<ReelWindow> rwList)
        {
            RayCaster[] rs;
            int windowSize;
            for (int i = 0; i < slotController.slotGroupsBeh.Length; i++)
            {
                rs = slotController.slotGroupsBeh[i].RayCasters;
                windowSize = rs.Length;

                for (int j = 0; j < windowSize; j++)
                {
                    rs[j].ID = rwList[i].revOrdering[j];
                }
            }
        }

        /// <summary>
        /// Return line win spins + sctater win spins
        /// </summary>
        /// <returns></returns>
        private int GetLineWinSpins()
        {
            int res = 0;
            foreach (var item in winLines)
            {
                //if (IsWinningLine(item.Key))
                //{
                //    res += item.Value.FreeSpins;
                //}
                res += lbWinDict[item].FreeSpins;
            }
            return res;
        }

        /// <summary>
        /// Return line win coins + sctater win coins, without jackpot
        /// </summary>
        /// <returns></returns>
        private int GetLineWinCoinsCalc()
        {
            int res = 0;
            foreach (var item in winLines)
            {
                //if (IsWinningLine(item.Key))
                //{
                //    res += item.Value.Pay;
                //}

                res += lbWinDict[item].Pay;
            }
            return res;
        }

        /// <summary>
        /// Return product of lines payMultiplier, sctater payMultiplier
        /// </summary>
        /// <returns></returns>
        private int GetLinePayMultiplier()
        {
            int res = 1;
            int pMult;
            foreach (var item in winLines)
            {
                //if (IsWinningLine(item.Key) && item.Value.PayMult != 0)
                //{
                //    res *= item.Value.PayMult;
                //}
                pMult = lbWinDict[item].PayMult;
                if (pMult != 0) res *= pMult;
            }
            return res;
        }


        /// <summary>
        /// Calc win for triple
        /// </summary>
        /// <param name="trList"></param>
        public void TestWin()
        {
            double linebet = 0.004; // ???
            Measure("test time", () =>
            {
                double sumPayOUt = 0;
                int sumFreeSpins = 0;
                double sumBets = 0;
                lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);                                // only active
                //Debug.Log("lines count: " + lbs.Length);
                int linesCount = lbs.Length;
                int i = 0;
                int wins = 0;
                double totalBet = linesCount * linebet;
                Debug.Log("totalBet: " + totalBet);
                for (int w = 0; w < 1000000; w++)
                {
                    int r = UnityEngine.Random.Range(0, slotWindowsList.Count);
                    var item = slotWindowsList[r];
                    if (sumFreeSpins > 0) { sumFreeSpins--; }
                    else
                    {
                        sumBets += (totalBet);
                    }
                    int freeSpins = 0;
                    int pay = 0;
                    int payMult = 1;

                    int freeSpinsScat = 0;
                    int payScat = 0;
                    int payMultScat = 1;

                    CalcWin(item, ref freeSpins, ref pay, ref payMult, ref freeSpinsScat, ref payScat, ref payMultScat);
                    sumPayOUt += ((double)pay * linebet);
                    sumPayOUt += ((double)payScat * totalBet);
                    sumFreeSpins += freeSpins;
                    if (pay > 0 || payScat > 0 || freeSpins > 0) wins++;
                    i++;
                }
                Debug.Log("calcs: " + i + " ;payout: " + sumPayOUt + " ; sumBets: " + sumBets + "; wins: " + wins + " ;pOUt,%" + ((float)sumPayOUt / (float)sumBets * 100f));
            });
        }

        /// <summary>
        /// Calc win for triple
        /// </summary>
        /// <param name="trList"></param>
        public void CalcWin()
        {
            lbs = FindObjectsByType<LineBehavior>(FindObjectsSortMode.None);                                // only active
            if (lbs.Length == 0)
            {
                Debug.LogError("Create slot lines before start....");
                return;
            }
            lbWinDict = new Dictionary<LineBehavior, WinDataCalc>();
            ResetLinesWin();
            CreateFullPaytable(slotController);
            CreatSlotWindows();
            Debug.Log("lines count: " + lbs.Length);
            int linesCount = lbs.Length;
            int i = 0;
            int wins = 0;
            double pOut = 0;
            double pOut_1 = 0;
            double comboProb = (1f / (double)slotWindowsList.Count) / (double)linesCount;
            double comboProbScat = (1f / (double)slotWindowsList.Count);
            int swLength = slotWindowsList.Count;

            Action<object> _calc = (o) =>
            {
                Measure("win calc time", () =>
                {
                    for (i = 0; i < swLength; i++)
                    {
                        var item = slotWindowsList[i];
                        int freeSpins = 0;
                        int pay = 0;
                        int payMult = 1;

                        int freeSpinsScat = 0;
                        int payScat = 0;
                        int payMultScat = 1;

                        CalcWin(item, ref freeSpins, ref pay, ref payMult, ref freeSpinsScat, ref payScat, ref payMultScat);
                        payMult *= payMultScat;
                        pay *= payMult;

                        pOut += ((double)pay * comboProb + (double)payScat * comboProbScat);
                        // pOut_1 += ((double)pay + (double)payScat);


                        if (pay > 0) // || freeSpins > 0 || payScat > 0
                        {
                            wins++;
                            //Debug.Log("test thread: " + wins);
                        }
                    }

                    double sumBet = slotWindowsList.Count * lbs.Length;
                    pOut_1 = pOut_1 / sumBet;

                    Debug.Log("calcs: " + i + " ; wins: " + wins + " ;payout %: " + (pOut * 100f) + " ;payout_1 %: " + (pOut_1 * 100f));
                });
            };

            _calc.Invoke(1);
        }

        /// <summary>
        /// Calc win for SlotWindow
        /// </summary>
        /// <param name="trList"></param>
        public void CalcWin(List<ReelWindow> trList, ref int freeSpins, ref int pay, ref int payMult, ref int freeSpinsScat, ref int payScat, ref int payMultScat)
        {
            SetSlotWindow(trList);
            winLines = new List<LineBehavior>();
            SearchWin();

            pay = 0;
            freeSpins = 0;
            payMult = 1;
            int pMult;
            foreach (var item in winLines)
            {
                pay += lbWinDict[item].Pay;
                freeSpins += lbWinDict[item].FreeSpins;
                pMult = lbWinDict[item].PayMult;
                if (pMult != 0) payMult *= pMult;
            }
           
            freeSpinsScat = GetScatterWinSpins();
            payScat = GetScatterWinCoinsCalc();
            payMultScat = GetScatterPayMultiplier();
        }

        /// <summary>
        /// calc line wins and scatter wins
        /// </summary>
        /// 
        private void SearchWin()
        {
            foreach (LineBehavior lB in lbs)
            {
                FindLineWin(lB, payTableFull);
            }
            return;

            // search scatters
            //int scatterWinS = 0;
            //int scatterSymbolsTemp = 0;
            //scatterWinCalc = null;
            //foreach (var item in slotGroupsBeh)
            //{
            //    scatterSymbolsTemp = 0;
            //    for (int i = 0; i < item.RayCasters.Length; i++)
            //    {
            //        if (item.RayCasters[i].ID == scatter_id)
            //        {
            //            scatterSymbolsTemp++;
            //        }
            //    }
            //    scatterWinS += scatterSymbolsTemp;
            //}

            //if (useScatter)
            //    foreach (var item in scatterPayTable)
            //    {
            //        if (item.scattersCount > 0 && item.scattersCount == scatterWinS)
            //        {
            //            scatterWinCalc = new WinDataCalc(scatterWinS, item.freeSpins, item.pay, item.payMult);
            //            //Debug.Log("scatters: " + item.scattersCount);
            //        }
            //    }
        }

        private void CreateFullPaytable(SlotController slotController)
        {
            payTableFull = new List<PayLine>();
            for (int j = 0; j < slotController.payTable.Count; j++)
            {
                payTableFull.Add(slotController.payTable[j]);
                if (slotController.useWild) payTableFull.AddRange(slotController.payTable[j].GetWildLines(slotController));
            }
        }

        /// <summary>
        /// Return line win coins + sctater win coins, without jackpot
        /// </summary>
        /// <returns></returns>
        private int GetScatterWinCoinsCalc()
        {
            int res = 0;
            if (scatterWinCalc != null) res += scatterWinCalc.Pay;
            return res;
        }

        /// <summary>
        /// Return line win spins + sctater win spins
        /// </summary>
        /// <returns></returns>
        private int GetScatterWinSpins()
        {
            int res = 0;
            if (scatterWinCalc != null) res += scatterWinCalc.FreeSpins;
            return res;
        }

        /// <summary>
        /// Return product of lines payMultiplier, sctater payMultiplier
        /// </summary>
        /// <returns></returns>
        private int GetScatterPayMultiplier()
        {
            int res = 1;
            if (scatterWinCalc != null && scatterWinCalc.PayMult != 0) res *= scatterWinCalc.PayMult;
            return res;
        }

        private void ResetLinesWin()
        {
            foreach (var item in lbs)
            {
                lbWinDict[item] = null;
            }
        }


        private WinDataCalc scatterWinCalc;

        /// <summary>
        /// Find  and fill winning symbols list  from left to right, according pay lines
        /// </summary>
        private void FindLineWin(LineBehavior lineBehavior, List<PayLine> payTable)
        {
            lbWinDict[lineBehavior] = null;
            WinDataCalc winTemp;
            foreach (var item in payTable)
            {
                // find max win
                winTemp = GetPayLineWin(lineBehavior, item); // ищем совпадения в таблице выплат, могут быть одинаковые линии (ошибочные), выберем с большей выплатой
                if (winTemp != null)
                {
                    if (lbWinDict[lineBehavior] == null)
                    {
                        lbWinDict[lineBehavior] = winTemp;
                        winLines.Add(lineBehavior);
                        return;
                        // Debug.Log(item.ToString(slotController.slotIcons, slotController.slotGroupsBeh.Length));
                    }
                    //else
                    //{
                    //    Debug.LogError("Found identical line in paytable:" + item.ToString(slotController.slotIcons, slotController.slotGroupsBeh.Length));
                    //    return;
                    //    if (lbWins[lineBehavior].Pay < winTemp.Pay || lbWins[lineBehavior].FreeSpins < winTemp.FreeSpins)
                    //    {
                    //        lbWins[lineBehavior] = winTemp;
                    //    }
                    //}
                }
            }
        }

        /// <summary>
        /// Check if line is wonn, according payline (compare symbols of 2 lines)
        /// </summary>
        /// <param name="payLine"></param>
        /// <returns></returns>
        private WinDataCalc GetPayLineWin(LineBehavior lineBehavior, PayLine payLine)
        {
            int winnSymbols = 0;
            int mainPS = payLine.line[0];                   // main pailine symbol ID
            int wildS = slotController.wild_id;
            int ls;
            int ps;
            if (!slotController.useWild)
            {
                for (int i = 0; i < lineBehavior.rayCasters.Length; i++)
                {
                    ls = lineBehavior.rayCasters[i].ID;     // symbol from slot line
                    ps = payLine.line[i];                   // symbol from pay line
                    if(ps >= 0)
                    {
                        if (ls != ps) { return null; }
                        else { winnSymbols++; }
                    }
                    else        // ps < 0 (any)
                    {
                        if (ls == mainPS) { return null; }
                    }
                }
            }
            else
            {
                for (int i = 0; i < lineBehavior.rayCasters.Length; i++)
                {
                    ls = lineBehavior.rayCasters[i].ID;     // symbol from slot line
                    ps = payLine.line[i];                   // symbol from pay line 
                    if (ps >= 0)
                    {
                        if (ls != ps) { return null; }
                        else  { winnSymbols++; }
                    }
                    else        // ps < 0 (any)
                    {
                        if (ls == mainPS) { return null; }
                        if (ls == wildS) { return null; }
                    }
                }
            }
            return new WinDataCalc(winnSymbols, payLine.freeSpins, payLine.pay, payLine.payMult);
        }
        #endregion old
 */