using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Mkey
{
    public class SlotGroupBehavior : MonoBehaviour
    {
        public List<int> symbOrder;

        [SerializeField]
        [Tooltip("Symbol windows, from top to bottom")]
        private RayCaster[] rayCasters;

        [Space(16, order = 0)]
        [SerializeField]
        [Tooltip("sec, additional rotation time")]
        private float addRotateTime = 0f;
        [SerializeField]
        [Tooltip("sec, delay time for spin")]
        private float spinStartDelay = 0f;
        [Tooltip("min 0% - max 20%, change spinStartDelay")]
        [SerializeField]
        private int spinStartRandomize = 0;
        [SerializeField]
        private int spinSpeedMultiplier = 1;

        [Space(16, order = 0)]
        [SerializeField]
        [Tooltip("If true - reel set to random position at start")]
        private bool randomStartPosition = false;
        [Space(16, order = 0)]
        [SerializeField]
        [Tooltip("Tile size by Y")]
        private float tileSizeY = 3.13f;

        #region simulate
        [SerializeField]
        private bool simulate =false;
        [SerializeField]
        public  int simPos = 0;
        #endregion simulate

        public Action EndSpinAction;

        #region const
        private static double PI = MathF.PI;
        private static double PI2 = MathF.PI * 2f;
        #endregion const

        #region temp vars
        private double anglePerTileRad = 0;
        private double anglePerTileDeg = 0;
        private TweenSeq tS;
        private Transform TilesGroup;
        private SlotSymbol[] slotSymbols;
        private SlotIcon[] sprites;

        private bool debugreel=false;
        private int windowSize;

        private double reelAngleX = 0;
        public double reelWindowAngleBot;
        public double reelWindowAngleTop;

        private bool spinDirDown = true; // false
        private double tape = 0;            // spent tape in tiles 
        private List<SlotSymbol> windSymbList;
        private bool forceStopFlag = false; // force stop infinite spin
        private double forceStopAngle = 0;

        private Vector3 sPos0;
        private Vector3 sPosOld0;
        private float lineSpeed = 0;
        private bool canUpdate = false;
        private float nudgeSpinTime = 0.2f;

        private List<int> cachedOrder;
        #endregion temp vars

        #region properties 
        public int NextOrderPosition { get; private set; }
        public int CurrOrderPosition; // { get; private set; }
        public RayCaster[] RayCasters { get { return rayCasters; } }
        public double[] SymbProbabilities
        {
            get; private set;
        }

        public bool IsSpinDirDown => spinDirDown;
        #endregion properties 

        #region dev
        public string orderJsonString;
        #endregion dev

        #region regular
        private void Awake()
        {
            cachedOrder = new List<int>(symbOrder);
        }

        private void Update()
        {
            if (!canUpdate) return;

            // update linear speed
            sPos0 = slotSymbols[0].transform.position;
            lineSpeed = (sPos0 - sPosOld0).magnitude / Time.deltaTime; // linear circular speed
            sPosOld0 = sPos0;

            // update symbols 
            for (int i = 0; i < slotSymbols.Length; i++)
            {
                if (slotSymbols[i])
                {
                    slotSymbols[i].InitUpdate(lineSpeed > 1);
                    slotSymbols[i].isInWindow = SymbolInWindow(slotSymbols[i]);
                }
            }
        }

        private void OnValidate()
        {
            spinStartRandomize = (int)Mathf.Clamp(spinStartRandomize, 0, 20);
            spinStartDelay = Mathf.Max(0,spinStartDelay);
            spinSpeedMultiplier = Mathf.Max(0, spinSpeedMultiplier);
            addRotateTime = Mathf.Max(0, addRotateTime);
        }

        private void OnDestroy()
        {
            CancelRotation();
        }

        private void OnDisable()
        {
            CancelRotation();
        }
        #endregion regular

        /// <summary>
        /// Instantiate slot tiles 
        /// </summary>
        internal void CreateSlotCylinder(SlotIcon[] sprites, int tileCount, GameObject tilePrefab)
        {
            CurrOrderPosition = 0;
            this.sprites = sprites;
            slotSymbols = new SlotSymbol[tileCount];

            // create Reel transform
            TilesGroup = (new GameObject()).transform;
            TilesGroup.localScale = transform.lossyScale;
            TilesGroup.parent = transform;
            TilesGroup.localPosition = Vector3.zero;
            TilesGroup.name = "Reel(" + name + ")";

            // calculate reel geometry
            float distTileY = tileSizeY;

            anglePerTileDeg = 360.0f / (float)tileCount;
            anglePerTileRad = anglePerTileDeg * Mathf.Deg2Rad;
            double radius = (distTileY / 2.0) / Math.Tan(anglePerTileRad / 2.0); 

            windowSize = rayCasters.Length;

            bool isEvenRayCastersCount = (windowSize % 2 == 0);
            int dCount = (isEvenRayCastersCount) ? windowSize / 2 - 1 : windowSize / 2;
            double addAnglePerTileDeg = (float) NormalizeAngleDeg((isEvenRayCastersCount) ? -anglePerTileDeg*dCount - anglePerTileDeg /2f : -anglePerTileDeg * dCount);

            TilesGroup.localPosition = new Vector3(TilesGroup.localPosition.x, TilesGroup.localPosition.y, (float)radius); // offset reel position by z-coordinat

            //create reel tiles
            for (int i = 0; i < tileCount; i++)
            {
                double n = (double)i;
                double tileAngleDeg = NormalizeAngleDeg(n * anglePerTileDeg + addAnglePerTileDeg);
                double tileAngleRad = tileAngleDeg * Mathf.Deg2Rad;

                slotSymbols[i] = Instantiate(tilePrefab, transform.position, Quaternion.identity).GetComponent<SlotSymbol>();
                slotSymbols[i].transform.parent = TilesGroup;
                slotSymbols[i].transform.localPosition = new Vector3(0, (float)(radius * Math.Sin(tileAngleRad)), (float)(-radius * Math.Cos(tileAngleRad)));
                slotSymbols[i].transform.localScale = Vector3.one;
                slotSymbols[i].transform.localEulerAngles = new Vector3((float)tileAngleDeg, 0, 0);
                slotSymbols[i].name = "SlotSymbol: " + String.Format("{0:00}", i);
                slotSymbols[i].initialIndex = n;
                slotSymbols[i].initialAngle = tileAngleDeg;
                slotSymbols[i].slotGroup = this;
                if (i > 0)
                {
                    slotSymbols[i].Prev = slotSymbols[i - 1];
                    slotSymbols[i - 1].Next = slotSymbols[i];
                    if(i == tileCount - 1)
                    {
                        slotSymbols[i].Next = slotSymbols[0];
                        slotSymbols[0].Prev = slotSymbols[i];
                    }
                }
            }

            //set symbols         
            for (int i = 0; i < tileCount/2; i++)
            {
                int _nextSymbol = (int)Mathf.Repeat(i, symbOrder.Count);
                int symNumber = symbOrder[_nextSymbol];
                slotSymbols[i].tape = i;
                slotSymbols[i].SetIcon(sprites[symNumber], symNumber);
            }

            for (int i = tileCount - 1; i >= tileCount/2; i--)
            {
                int _nextSymbol = (int)Mathf.Repeat(i - tileCount, symbOrder.Count);
                int symNumber = symbOrder[_nextSymbol];
                slotSymbols[i].tape = i - tileCount;
                slotSymbols[i].SetIcon(sprites[symNumber], symNumber);
            }

            SymbProbabilities = GetReelSymbHitPropabilities(sprites);
            CurrOrderPosition = 0; // offset  '- anglePerTileRad' - 

            // check whether the normalized angle is within the window
            reelWindowAngleBot = NormalizeAngleDeg(-anglePerTileDeg * (0.5 + windowSize / 2.0));
            reelWindowAngleTop = NormalizeAngleDeg(anglePerTileDeg * (0.5 + windowSize / 2.0));

            // set random start position
            if (randomStartPosition)
            {
                NextOrderPosition = (spinDirDown) ? UnityEngine.Random.Range(1, 5) : UnityEngine.Random.Range(symbOrder.Count - 1, symbOrder.Count - 1 - 5);
                float angleX = GetAngleToNextSymb(NextOrderPosition);
                CurrOrderPosition = NextOrderPosition;
                Debug.Log(name + ": start pos: " + NextOrderPosition + "; angle: " + angleX);
                RotateReelStep(spinDirDown ? -angleX : angleX, true);        // rotation greater than 90 degrees is incorrect
            }
            sPos0 = slotSymbols[0].transform.position;
            sPosOld0 = sPos0;
            canUpdate = true;
        }

        #region rotate, cancel, stop
        /// <summary>
        /// Async rotate cylinder
        /// </summary>
        internal void NextRotateCylinderEase(EaseAnim mainRotType, EaseAnim inRotType, EaseAnim outRotType,
                                        float mainRotTime, float mainRotateTimeRandomize,
                                        float inRotTime, float outRotTime,
                                        float inRotAngle, float outRotAngle,
                                        int nextOrderPosition,  Action rotCallBack)

        {
            NextOrderPosition = (!simulate)? nextOrderPosition : simPos;
            if (nextOrderPosition == -1) NextOrderPosition = -1; // avoid simulation 

            // Debug.Log(name + ": " + NextOrderPosition);
            // start spin delay
            spinStartDelay = Mathf.Max(0, spinStartDelay);
            float spinStartRandomizeF = Mathf.Clamp(spinStartRandomize / 100f, 0f, 0.2f);
            float startDelay = UnityEngine.Random.Range(spinStartDelay * (1.0f - spinStartRandomizeF), spinStartDelay * (1.0f + spinStartRandomizeF));

            // check range before start
            inRotTime = Mathf.Clamp(inRotTime, 0, 1f);
            inRotAngle = Mathf.Clamp(inRotAngle, 0, 10);

            outRotTime = Mathf.Clamp(outRotTime, 0, 1f);
            outRotAngle = Mathf.Clamp(outRotAngle, 0, 10);

            // create reel rotation sequence - 3 parts  in --> continuous or main --> out
            tS = new TweenSeq();
            float angleX = 0;
            float dirAngle = spinDirDown ? -1 : 1;

            // main rotation time 
            addRotateTime = Mathf.Max(0, addRotateTime);
            mainRotateTimeRandomize = Mathf.Clamp(mainRotateTimeRandomize, 0f, 0.2f);
            mainRotTime = addRotateTime + UnityEngine.Random.Range(mainRotTime * (1.0f - mainRotateTimeRandomize), mainRotTime * (1.0f + mainRotateTimeRandomize));
            spinSpeedMultiplier = Mathf.Max(0, spinSpeedMultiplier);
            

            tS.Add((callBack) => // in rotation part
            {
                RotateReelTween(-dirAngle * inRotAngle, startDelay, inRotTime, inRotType, false, false, callBack);
            });

            if (NextOrderPosition == -1)
            {
                tS.Add((callBack) => // continuous rotation
                {
                    forceStopFlag = false;
                    double angleSpeed = (anglePerTileDeg * (symbOrder.Count / 2.0f) + anglePerTileDeg * symbOrder.Count * spinSpeedMultiplier) / mainRotTime;
                    StartCoroutine(RecurRotationC(angleSpeed, inRotAngle, outRotAngle, callBack));
                });
            }
            else
            {
                tS.Add((callBack) =>  // main rotation part
                {
                    angleX = GetAngleToNextSymb(NextOrderPosition) + (float)(anglePerTileDeg * symbOrder.Count * spinSpeedMultiplier);
                    if (debugreel) Debug.Log(name + ", angleX : " + angleX);
                    StartCoroutine(MainRotationC(dirAngle * (angleX + inRotAngle), inRotAngle, outRotAngle, mainRotTime, callBack));
                });
            }

            tS.Add((callBack) =>  // out rotation part
            {
                RotateReelTween(-dirAngle * outRotAngle, 0, outRotTime, outRotType, true, false, ()=> 
                { 
                    CurrOrderPosition = NextOrderPosition; 
                    forceStopFlag = false;
                    rotCallBack?.Invoke();
                    EndSpinAction?.Invoke();
                    callBack();
                });
            });

            tS.Start();
        }


        /// <summary>
        /// Async rotate cylinder
        /// </summary>
        internal void ReelNudge(EaseAnim mainRotType, bool raise, Action rotCallBack)

        {
            NextOrderPosition = (raise) ? NextOrderPosition -  1 : NextOrderPosition + 1;

            tS = new TweenSeq();
            tS.Add((callBack) =>  // main rotation part
            {
                RotateReelTween((raise) ? (float)anglePerTileDeg : (float)-anglePerTileDeg, 0, nudgeSpinTime, mainRotType, false, true, ()=> { rotCallBack?.Invoke(); EndSpinAction?.Invoke(); callBack(); });
            });
            tS.Start();
        }

        private IEnumerator RecurRotationC(double angleSpeed, float inRotAngle, float outRotAngle, Action completeCallBack)
        {
            double sumAngle = 0;
            // reason for stopping - manual stop (forceStopFlag = true), or receiving a new position from the backend (NextOrderPosition != -1 )
            while (NextOrderPosition == -1) 
            {
                if (forceStopFlag && (Math.Abs(sumAngle) >= Mathf.Abs(inRotAngle)))
                {
                    SetStopPosition();
                    break;
                }

                double dA = angleSpeed * Time.deltaTime;
                dA = (spinDirDown) ? -dA : dA;
                sumAngle += dA;
                RotateReelStep((float)dA, true);
                yield return new WaitForEndOfFrame();
            }
            // we finish the spin
            float angle = (float)forceStopAngle + outRotAngle;
            float time =(float) (angle / angleSpeed);
            RotateReelTween(spinDirDown ? - angle : angle, 0, time, EaseAnim.EaseLinear, false, true, completeCallBack);
            forceStopFlag = false;
        }

        private IEnumerator MainRotationC(float mAngle, float inRotAngle, float outRotAngle, float tTime, Action completeCallBack)
        {
            float oldAngle = 0;
            float sTime = 0;
            float angleSpeed = Math.Abs(mAngle) / tTime;
            Func<float, float, float, float> easeLinear = (elapsedTime, val, tweenTime) => { return val * elapsedTime / tweenTime; };
            bool mainSpinComplete = false;

            // reason for stopping - manual stop (forceStopFlag = true)
            while (sTime < tTime)
            {
                if (forceStopFlag && (Mathf.Abs(oldAngle) >= Mathf.Abs(inRotAngle))) 
                {
                    SetStopPosition();
                    break; 
                }
                sTime += Time.deltaTime;
                float nAngle = 0;
                if(sTime > tTime)
                {
                    nAngle = easeLinear(tTime, mAngle, tTime); 
                    mainSpinComplete = true;
                }
                else
                {
                    nAngle = easeLinear(sTime, mAngle, tTime);
                }
                float dA = nAngle - oldAngle;
                oldAngle = nAngle;
                RotateReelStep(dA, true);
                yield return new WaitForEndOfFrame();
            }
            // we finish the spin
            float fsAngle = mainSpinComplete ? outRotAngle : (float)forceStopAngle + outRotAngle;
            float fsTime = (float)(fsAngle / angleSpeed);
            RotateReelTween(spinDirDown ? -fsAngle : fsAngle, 0, fsTime, EaseAnim.EaseLinear, false, true, completeCallBack);
            forceStopFlag = false;
        }

        /// <summary>
        /// elementary rotation during one update, max dA = 90 degree
        /// </summary>
        /// <param name="dA"></param>
        private void RotateReelStep(float dA, bool wrap)
        {
            // 1) we start the rotation, it will be done after calling the Update method 
            TilesGroup.Rotate((MathF.Abs(dA) > 360) ? dA % 360f : dA, 0, 0); // possible - TilesGroup.Rotate(Vector3.right, dA);

            //2) wrap
            double nTiles = Math.Abs(dA / anglePerTileDeg);   // determine how many tiles will come off the tape
            bool down = dA < 0;
            tape += ((down) ? nTiles : -nTiles);                 // for top-down rotation, tape sign "+"
            windSymbList = GetWindowSymbols(false); // we get the symbols in the window, starting from the bottom. before rotation angle increment
            reelAngleX = NormalizeAngleDeg((reelAngleX + dA));  // after that we increase the current angle of the reel to quickly calculate the position (0-360 degrees), needed to receive symbols from the window
            if (!wrap) return;
            if (windSymbList.Count >= 0)
            {
                if (down)
                {
                    WrapNextReelSymbols_B(windSymbList[0], (int)(nTiles), false);
                }
                else
                {
                    WrapPrevSymbols_B(windSymbList[0], (int)(nTiles), false);
                }
            }
            else
            {
                Debug.LogError("!!!Incorrect window position!!!");
            }
        }

        private void RotateReelTween(float tAngle, float tDelay, float tTime, EaseAnim tEase, bool align, bool wrap, Action completeCallBack)
        {
            float oldVal = 0;
            SimpleTween.Value(gameObject, 0f, tAngle, tTime)
                                 .SetOnUpdate((float val) =>
                                 {
                                     RotateReelStep(val - oldVal, wrap);
                                     oldVal = val;
                                 })
                                 .AddCompleteCallBack(() =>
                                 {
                                     if (align)
                                     {
                                         StartCoroutine(AlignReel(completeCallBack));
                                     }
                                     else completeCallBack?.Invoke();
                                 }).SetEase(tEase).SetDelay(tDelay);
        }

        internal void CancelRotation()
        {
            SimpleTween.Cancel(gameObject, false);
            if (tS != null) tS.Break();
        }

        /// <summary>
        /// Return angle in degree to next symbol position in symbOrder array
        /// </summary>
        /// <param name="nextOrderPosition"></param>
        /// <returns></returns>
        private float GetAngleToNextSymb(int nextOrderPosition)
        {
            if (spinDirDown)
            {
                if (CurrOrderPosition < nextOrderPosition)
                {
                    return (float)((nextOrderPosition - CurrOrderPosition) * anglePerTileDeg);
                }
                return (float)((symbOrder.Count - CurrOrderPosition + nextOrderPosition) * anglePerTileDeg);
            }
            else
            {
                if (CurrOrderPosition > nextOrderPosition)
                {
                    return (float)((CurrOrderPosition - nextOrderPosition) * anglePerTileDeg);
                }
                return (float)((symbOrder.Count + CurrOrderPosition - nextOrderPosition) * anglePerTileDeg);
            }
        }
 
        public void ForceStopInfiniteSpin()
        {  
            forceStopFlag = true;
        }

        private void SetStopPosition()
        {
            NextOrderPosition = GetCurrentOrderPosition() + ((spinDirDown) ? 2 : -3);
            NextOrderPosition = (int)Mathf.Repeat(NextOrderPosition, symbOrder.Count);
            double tape_1 = Math.Ceiling(Math.Abs(tape) + 2);
            double dTape = tape_1 - Math.Abs(tape);
            forceStopAngle = dTape * anglePerTileDeg;
        }

        /// <summary>
        /// get a rounded position from the tape.
        /// </summary>
        /// <returns></returns>
        private int GetCurrentOrderPosition()
        {
            double symbTape = Math.Ceiling(tape);
            int symbTapeRep = (int)Mathf.Repeat((long)(symbTape), symbOrder.Count);
            return symbTapeRep;
        }

        private IEnumerator AlignReel(Action completeCallBack )
        {
            yield return new WaitForEndOfFrame();
            float dA = (float)(Math.Round(TilesGroup.eulerAngles.x) - TilesGroup.eulerAngles.x);
            //   Debug.Log(name + "; TilesGroup.eulerAngles.x: " + TilesGroup.eulerAngles.x + " align: " + dA);
            RotateReelStep(spinDirDown ? -dA  : dA, false);
            yield return new WaitForEndOfFrame();
            TilesGroup.eulerAngles = new Vector3(Mathf.Round(TilesGroup.eulerAngles.x), TilesGroup.eulerAngles.y, TilesGroup.eulerAngles.z);
            yield return new WaitForEndOfFrame();
            completeCallBack?.Invoke();
        }
        #endregion rotate, cancel, stop

        /// <summary>
        /// Set next reel order while continuous rotation
        /// </summary>
        /// <param name="r"></param>
        internal void SetNextOrder(int r)
        {
            if (NextOrderPosition == -1) NextOrderPosition = r;
        }

        #region get window data
        /// <summary>
        /// Return true if top, middle or bottom raycaster has symbol with ID == symbID
        /// </summary>
        /// <param name="symbID"></param>
        /// <returns></returns>
        public bool HasSymbolInAnyRayCaster(int symbID, ref List<SlotSymbol> slotSymbols)
        {
            slotSymbols = new List<SlotSymbol>();
            bool res = false;
            SlotSymbol sS;

            for (int i = 0; i < rayCasters.Length; i++)
            {
                sS = rayCasters[i].GetSymbol();
                if (sS.IconID == symbID)
                {
                    res = true;
                    slotSymbols.Add(sS);
                }
            }

            return res;
        }

        public List<SlotSymbol> GetWindowSymbols(bool tb)
        {
            List<SlotSymbol> res = new List<SlotSymbol>();
            for (int i = 0; i < slotSymbols.Length; i++)
            {
                if (slotSymbols[i] && SymbolInWindow(slotSymbols[i]))
                {
                    res.Add(slotSymbols[i]);
                }
            }

            // sort from top to bottom
            if (tb)
                res.Sort((a, b) => { return b.transform.position.y.CompareTo(a.transform.position.y); });
            else
                res.Sort((a, b) => { return a.transform.position.y.CompareTo(b.transform.position.y); });
            return res;
        }

        public List<SlotSymbol> GetRCSymbols(int symbolID)
        {
            List<SlotSymbol> _slotSymbols = new();
            foreach (var item in rayCasters)
            {
               if( item.GetSymbol().IconID == symbolID) _slotSymbols.Add(item.GetSymbol());
            } 
            return _slotSymbols;
        }
        #endregion get window data

        #region change order, restore order
        public void ReplaceOrder(SlotSymbol slotSymbol,int newSymbolId)
        {
            Debug.Log("replace order");
            int symbTapeRep = (int)Mathf.Repeat((long)(slotSymbol.tape), symbOrder.Count);
            symbOrder[symbTapeRep] = newSymbolId;                   // replace old id in current order
            slotSymbol.SetIcon(sprites[newSymbolId], newSymbolId);
        }

        public void RestoreOrder()
        {
            symbOrder = new List<int>(cachedOrder);
            windSymbList = GetWindowSymbols(false);
            WrapNextReelSymbols_B(windSymbList[0], slotSymbols.Length / 2 - 1, true);
            WrapPrevSymbols_B(windSymbList[0], slotSymbols.Length / 2 - 2, true);
        }
        #endregion change order, restore order

        #region wrap
        private void WrapNextReelSymbols_B(SlotSymbol startSlotSymbol, int count, bool force)
        {
            SlotSymbol currSymbol = startSlotSymbol.Next; // startSlotSymbol - this is the bottom symbol in the window, it is always correct - the baseline
            int symbTapeRep;
            double nTape;
            // here you need to go up the slot reel and update all the symbols according to the offset
            for (int i = 0; i < count + windowSize + 1; i++)
            {
                nTape = currSymbol.Prev.tape + 1.0;
                if (currSymbol.tape != nTape || force)
                {
                    currSymbol.tape = nTape;
                    symbTapeRep = (int)Mathf.Repeat((long)(currSymbol.tape), symbOrder.Count);
                    int symID = symbOrder[symbTapeRep];
                    currSymbol.SetIcon(sprites[symID], symID);
                }
                currSymbol = currSymbol.Next;
            }

            if(count > symbOrder.Count/2) return;
            // update the bottom of the reel, starting from the index under the window
            currSymbol = startSlotSymbol.Prev;
            for (int i = 0; i < windowSize + 2; i++)
            {
                nTape = currSymbol.Next.tape - 1.0;
                if (currSymbol.tape != nTape || force)
                {
                    currSymbol.tape = currSymbol.Next.tape - 1.0;
                    symbTapeRep = (int)Mathf.Repeat((long)(currSymbol.tape), symbOrder.Count);
                    int symID = symbOrder[symbTapeRep];
                    currSymbol.SetIcon(sprites[symID], symID);
                }
                currSymbol = currSymbol.Prev;
            }
        }

        private void WrapPrevSymbols_B(SlotSymbol startSlotSymbol, int count, bool force)
        {
            SlotSymbol currSymbol = startSlotSymbol.Prev; // startSlotSymbol - this is the bottom symbol in the window, it is always correct - the baseline
            int symbTapeRep;
            double nTape;

            // here you need to go down the slot reel (against the array order) and change all the symbols according to the offset
            for (int i = 0; i < count + windowSize + 1; i++)
            {
                nTape = currSymbol.Next.tape - 1.0;
                if (currSymbol.tape != nTape || force)
                {
                    currSymbol.tape = nTape;
                    symbTapeRep = (int)Mathf.Repeat((long)(currSymbol.tape), symbOrder.Count);
                    int symID = symbOrder[symbTapeRep];
                    currSymbol.SetIcon(sprites[symID], symID);
                }
                currSymbol = currSymbol.Prev;
            }

            if (count > symbOrder.Count / 2) return;
            currSymbol = startSlotSymbol.Next;
            for (int i = 0; i < windowSize + 2; i++)
            {
                nTape = currSymbol.Prev.tape + 1.0;
                if (currSymbol.tape != nTape || force)
                {
                    currSymbol.tape = nTape;
                    symbTapeRep = (int)Mathf.Repeat((long)(currSymbol.tape), symbOrder.Count);
                    int symID = symbOrder[symbTapeRep];
                    currSymbol.SetIcon(sprites[symID], symID);
                }
                currSymbol = currSymbol.Next;
            }
        }
        #endregion wrap

        #region utils
        /// <summary>
        /// check whether the normalized angle is within the window
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private bool SymbolInWindow(SlotSymbol slotSymbol)
        {
            slotSymbol.currAngle = (spinDirDown) ?  NormalizeAngleDeg(slotSymbol.initialAngle - (360.0 - reelAngleX)) : NormalizeAngleDeg(slotSymbol.initialAngle + reelAngleX);
            return AngleInWindow(slotSymbol.currAngle);
        }

        /// <summary>
        /// check whether the normalized angle is within the window
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private bool AngleInWindow(double angle)
        {
            if(angle >= 90 && angle <= 270) 
            {
                return false;
            }
            if (angle < 90 && angle <= reelWindowAngleTop) return true;
            if (angle > 270 && angle > reelWindowAngleBot) return true;
            return false;
        }

        /// <summary>
        /// returns an angle in the range from 0 to 360 degrees
        /// </summary>
        /// <param name="_angle"></param>
        /// <returns></returns>
        private static double NormalizeAngleDeg(double _angle)
        {
            double res = _angle;
            if (_angle > 360 || _angle < -360) res = _angle % 360.0;
            if (res < 0) res += 360.0;
            return res;
        }

        /// <summary>
        /// returns an angle in the range from 0 to 2pi radians
        /// </summary>
        /// <param name="_angle"></param>
        /// <returns></returns>
        private static double NormalizeAngleRad(double _angle)
        {
            double res = _angle;
            if (_angle > PI2|| _angle < -PI2) res = _angle % PI2;
            if (res < 0) res += PI2;
            return res;
        }
        #endregion utils

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

        #region dev
        public void OrderTostring()
        {
            string res = "";
            for (int i = 0; i < symbOrder.Count; i++)
            {
                res += (i + ") ");
                res += symbOrder[i];
                if (i < symbOrder.Count - 1) res += "; ";
            }

            Debug.Log(res);
        }

        private void SignTopSymbol(int top)
        {
            for (int i = 0; i < slotSymbols.Length; i++)
            {
                if (slotSymbols[i].name.IndexOf("Top")!=-1) slotSymbols[i].name = "SlotSymbol: " + String.Format("{0:00}", i);
            }

            slotSymbols[top].name = "Top - " + slotSymbols[top].name;
        }

        public int GetRaycasterIndex(RayCaster rC)
        {
            int res = -1;
            if (!rC) return res;
            for (int i = 0; i < RayCasters.Length; i++)
            {
                if (RayCasters[i] == rC) return i;
            }
            return res;
        }

        public string CheckRaycasters()
        {
            string res = "";
            if (RayCasters == null || RayCasters.Length == 0) return "need to setup raycasters";

            for (int i = 0; i < RayCasters.Length; i++)
            {
                if (!RayCasters[i]) res += (i + ")raycaster - null; ");
                else { res += (i + ")" + RayCasters[i].name + "; "); }
            }
            return res;
        }

        public void SetDefaultChildRaycasters()
        {
            RayCaster[] rcs = GetComponentsInChildren<RayCaster>(true);
            rayCasters = rcs;

        }

        public string OrderToJsonString()
        {
            string res = "";
            ListWrapperStruct<int> lW = new ListWrapperStruct<int>(symbOrder);
            res = JsonUtility.ToJson(lW);
            return res;
        }

        public void SetOrderFromJson()
        {
            Debug.Log("Json viewer - " + "http://jsonviewer.stack.hu/");
            Debug.Log("old reel symborder json: " + OrderToJsonString());

            SetOrderFromJson(orderJsonString);
        }

        public void SetOrderFromJson(string js)
        {
            if (string.IsNullOrEmpty(js))
            {
                Debug.LogError("json string : empty");
                return;
            }

            ListWrapperStruct<int> lWPB = JsonUtility.FromJson<ListWrapperStruct<int>>(js);
            if (lWPB != null && lWPB.list != null && lWPB.list.Count > 0)
            {
                symbOrder = lWPB.list;
            }
        }
        #endregion dev
    }
}
