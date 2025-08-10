using UnityEngine;
using Mkey;
using System;
using UnityEngine.Events;

namespace MkeyFW // mkey fortune wheel
{
    enum RotDir { Counter, ClockWise }
    public class WheelController : MonoBehaviour
    {
        [Header("Main references")]
        [Space(16, order = 0)]
        [SerializeField]
        private Transform Reel;
        [SerializeField]
        private Animator pointerAnimator;
        [SerializeField]
        private LampsController lampsController;
        [SerializeField]
        private SceneButton spinButton;
        [SerializeField]
        private ArrowBeviour arrowBeviour;
        [SerializeField]
        private WinSectorBehavior winSectorPrefab;
        [SerializeField]
        private Transform winSectorParent;

        [Header("Spin options")]
        [Space(16, order = 0)]
        [SerializeField]
        private float inRotTime = 0.2f;
        [SerializeField]
        private float inRotAngle = 5;
        [SerializeField]
        private float mainRotTime = 1.0f;
        [SerializeField]
        private EaseAnim mainRotEase = EaseAnim.EaseLinear;
        [SerializeField]
        private float outRotTime = 0.2f;
        [SerializeField]
        private float outRotAngle = 5;
        [SerializeField]
        private float spinStartDelay = 0;
        [SerializeField]
        private int spinSpeedMultiplier = 1;
        [SerializeField]
        private RotDir rotDir = RotDir.Counter;

        [Header("Lamps control")]
        [Space(16, order = 0)]
        [Tooltip("Before spin")]
        [SerializeField]
        private LampsFlash lampsFlashAtStart = LampsFlash.Random;
        [Tooltip("During spin")]
        [SerializeField]
        private LampsFlash lampsFlashDuringSpin = LampsFlash.Sequence;
        [Tooltip("After spin")]
        [SerializeField]
        private LampsFlash lampsFlashEnd = LampsFlash.All;

        [Header("Additional options")]
        [Space(16, order = 0)]
        [Tooltip("Sector light")]
        [SerializeField]
        private int lightBlinkCount = 4;
        [Tooltip("Help arrow")]
        [SerializeField]
        private int arrowBlinkCount = 2;
        [SerializeField]
        private AudioClip spinSound;

        [Header("Simulation, only for test")]
        [Space(32, order = 0)]
        [SerializeField]
        private bool simulate = false;
        [SerializeField]
        private int simPos = 0;
        [SerializeField]
        private bool debug = false;

        [SerializeField]
        private SceneButton backGroundButton;

        public SceneButton BackGroundButton { get { return backGroundButton; } }

        #region events
        public Action<int, bool> SpinResultEvent; // spin result event <coins, isBigWin>
        [Space(8)]
        public UnityEvent StartSpinEvent;
        public UnityEvent EndSpinEvent;
        #endregion events

        [SerializeField]
        private bool blocked;// { get; private set; }
        public Sector WinSector { get; private set; }

        #region temp vars
        private Sector[] sectors;
        private int rand = 0;
        private int sectorsCount = 0;
        private float angleSpeed = 0;
        private float sectorAngleRad;
        private float sectorAngleDeg;
        private int currSector = 0;
        private int nextSector = 0;
        private TweenSeq tS;
        private AudioSource audioSource;
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private float rotDirF = -0;
        private WinSectorBehavior winSectorBehavior;
        #endregion temp vars

        #region regular
        void OnValidate()
        {
            Validate();
        }

        void Start()
        {
            sectors = GetComponentsInChildren<Sector>();
            sectorsCount = (sectors != null) ? sectors.Length : 0;
            if (debug) Debug.Log("sectorsCount: " + sectorsCount);
            if (sectorsCount > 0)
            {
                sectorAngleDeg = 360f / sectorsCount;
                sectorAngleRad = 360f / sectorsCount * Mathf.Deg2Rad;
            }
            if (pointerAnimator)
            {
                pointerAnimator.enabled = false;
                pointerAnimator.speed = 0;
                pointerAnimator.transform.localEulerAngles = Vector3.zero;
            }
            if (lampsController) lampsController.lampFlash = lampsFlashAtStart;
            UpdateRand();
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            UpdateRand();
        }

        void OnDestroy()
        {
            CancelSpin();
        }
        #endregion regular

        /// <summary>
        /// Start spin, call completeCallBack(int result, bool isBigWin) 
        /// </summary>
        /// <param name="completeCallBack"></param>
        public void StartSpin(Action completeCallBack)
        {
            WinSector = null;
            if (arrowBeviour) arrowBeviour.CancelTween();
            if (tS != null) return;
            if (debug) Debug.Log("rand: " + rand);
            nextSector = rand;
            SetBlocked(true, true);
            CancelSectorWin();

            bool soundOn = (MSound) ? MSound.SoundOn : true;
            float volume = (MSound) ? MSound.Volume : 1;

            // spin sound
            if (audioSource)
            {
                audioSource.volume = volume;
                audioSource.Stop(); // stop spin sound
            }

            if (audioSource && spinSound && soundOn)
            {
                audioSource.volume = (MSound) ? MSound.Volume : 1;
                audioSource.clip = spinSound;
                audioSource.Play();
                audioSource.loop = true;
            }
            StartSpinEvent?.Invoke();
            RotateWheel(() =>
            {
                WinSector = sectors[currSector];

                ShowSectorWin();

                if (audioSource) audioSource.Stop();  // stop spin sound

                if (audioSource && sectors[currSector] && sectors[currSector].hitSound && soundOn) // play hit sound
                {
                    audioSource.volume = (MSound) ? MSound.Volume : 1;
                    audioSource.clip = sectors[currSector].hitSound;
                    audioSource.Play();
                    audioSource.loop = false;
                }
                bool isBigWin = false;
                int res = GetWin(ref isBigWin);
                EndSpinEvent?.Invoke();
                SpinResultEvent?.Invoke(res, isBigWin);
                completeCallBack?.Invoke();
            });
        }

        /// <summary>
        /// Start spin
        /// </summary>
        public void StartSpin()
        {
            StartSpin(null);
        }

        /// <summary>
        /// Async rotate wheel to next sector
        /// </summary>
        private void RotateWheel(Action rotCallBack)
        {
            rotDirF = (rotDir == RotDir.ClockWise) ? -1f : 1f;

            // validate input
            Validate();

            //change lamps state
            if (lampsController) lampsController.lampFlash = lampsFlashDuringSpin;

            // get next reel position
            nextSector = (!simulate) ? nextSector : simPos;
            if (debug) Debug.Log("next: " + nextSector + " ;angle: " + GetAngleToNextSector(nextSector));

            // create reel rotation sequence - 4 parts  in - (continuous) - main - out
            float oldVal = 0f;
            tS = new TweenSeq();
            float angleZ = 0;

            tS.Add((callBack) => // in rotation part
            {
                SimpleTween.Value(gameObject, 0f, inRotAngle, inRotTime)
                                  .SetOnUpdate((float val) =>
                                  {
                                      if (Reel) Reel.Rotate(0, 0, (-val + oldVal) * rotDirF);
                                      oldVal = val;
                                  })
                                  .AddCompleteCallBack(() =>
                                  {
                                      callBack?.Invoke();
                                  }).SetDelay(spinStartDelay);
            });

            tS.Add((callBack) =>  // main rotation part
            {
                oldVal = 0f;
                pointerAnimator.enabled = true;
                spinSpeedMultiplier = Mathf.Max(0, spinSpeedMultiplier);
                angleZ = GetAngleToNextSector(nextSector) + 360.0f * spinSpeedMultiplier;
                SimpleTween.Value(gameObject, 0, -(angleZ + outRotAngle + inRotAngle), mainRotTime)
                                  .SetOnUpdate((float val) =>
                                  {
                                      angleSpeed = (-val + oldVal) * rotDirF;
                                      if (Reel) Reel.Rotate(0, 0, angleSpeed);
                                      oldVal = val;
                                      if (pointerAnimator)
                                      {
                                          pointerAnimator.speed = Mathf.Abs(angleSpeed);
                                      }
                                  })
                                  .SetEase(mainRotEase)
                                  .AddCompleteCallBack(() =>
                                  {
                                      if (pointerAnimator)
                                      {
                                          pointerAnimator.enabled = false;
                                          pointerAnimator.speed = 0;
                                          pointerAnimator.transform.localEulerAngles = Vector3.zero;
                                      }
                                      if (lampsController) lampsController.lampFlash = lampsFlashEnd;
                                      callBack?.Invoke();
                                  });
            });

            tS.Add((callBack) =>  // out rotation part
            {
                oldVal = 0f;
                SimpleTween.Value(gameObject, 0, outRotAngle, outRotTime)
                                  .SetOnUpdate((float val) =>
                                  {
                                      if (Reel) Reel.Rotate(0, 0, (-val + oldVal) * rotDirF);
                                      oldVal = val;
                                  })
                                  .AddCompleteCallBack(() =>
                                  {
                                      if (pointerAnimator)
                                      {
                                          pointerAnimator.transform.localEulerAngles = Vector3.zero;
                                      }
                                      currSector = nextSector;
                                      CheckResult();
                                      callBack?.Invoke();
                                  });
            });

            tS.Add((callBack) =>
            {
                rotCallBack?.Invoke();
                tS = null;
                callBack?.Invoke();
            });

            tS.Start();
        }

        private void Validate()
        {
            mainRotTime = Mathf.Max(0.1f, mainRotTime);

            inRotTime = Mathf.Clamp(inRotTime, 0, 1f);
            inRotAngle = Mathf.Clamp(inRotAngle, 0, 10);

            outRotTime = Mathf.Clamp(outRotTime, 0, 1f);
            outRotAngle = Mathf.Clamp(outRotAngle, 0, 10);
            spinSpeedMultiplier = Mathf.Max(1, spinSpeedMultiplier);
            spinStartDelay = Mathf.Max(0, spinStartDelay);

            lightBlinkCount = Mathf.Max(0, lightBlinkCount);

            if (simulate)
            {
                sectors = GetComponentsInChildren<Sector>();
                sectorsCount = (sectors != null) ? sectors.Length : 0;
                simPos = Mathf.Clamp(simPos, 0, sectorsCount - 1);
            }
        }

        /// <summary>
        /// Return angle in degree to next symbol position in symbOrder array
        /// </summary>
        /// <param name="nextOrderPosition"></param>
        /// <returns></returns>
        private float GetAngleToNextSector(int nextOrderPosition)
        {
            rotDirF = (rotDir == RotDir.ClockWise) ? -1f : 1f;
            return (currSector < nextOrderPosition) ? rotDirF * (nextOrderPosition - currSector) * sectorAngleDeg : (sectors.Length - rotDirF * (currSector - nextOrderPosition)) * sectorAngleDeg;
        }

        /// <summary>
        /// Upadate random value rand
        /// </summary>
        private void UpdateRand()
        {
            rand = UnityEngine.Random.Range(0, sectorsCount);
        }

        public void CancelSpin()
        {
            if (this)
            {
                CancelSectorWin();

                if (tS != null) tS.Break();
                tS = null;

                SimpleTween.Cancel(gameObject, false);
                if (pointerAnimator)
                {
                    pointerAnimator.enabled = false;
                    pointerAnimator.speed = 0;
                    pointerAnimator.transform.localEulerAngles = Vector3.zero;
                }
            }
        }

        #region win
        public void CancelSectorWin()
        {
            if (this && winSectorBehavior)
            {
                Destroy(winSectorBehavior);
            }
        }

        private void ShowSectorWin()
        {
            if (winSectorPrefab && winSectorParent) winSectorBehavior = Instantiate(winSectorPrefab, winSectorParent);
        }
        #endregion win

        /// <summary>
        /// Check result and invoke sector hit event
        /// </summary>
        private void CheckResult()
        {
            int coins = 0;
            bool isBigWin = false;

            if (sectors != null && currSector >= 0 && currSector < sectors.Length)
            {
                Sector s = sectors[currSector];
                if (s != null)
                {
                    isBigWin = s.BigWin;
                    coins = s.Coins;
                    s.PlayHit(Reel.position);
                }
            }
            if (debug) Debug.Log("Coins: " + coins + " ;IsBigWin: " + isBigWin);
        }

        /// <summary>
        /// Return spin result, coins
        /// </summary>
        /// <param name="isBigWin"></param>
        /// <returns></returns>
        public int GetWin(ref bool isBigWin)
        {
            int res = 0;
            isBigWin = false;
            if (sectors != null && currSector >= 0 && currSector < sectors.Length)
            {
                isBigWin = sectors[currSector].BigWin;
                return sectors[currSector].Coins;
            }
            return res;
        }

        public void SetBlocked(bool blockSpin, bool blockBkgButton)
        {
            if (blocked != blockSpin)
            {
                blocked = blockSpin;
                if (!blocked && arrowBeviour) arrowBeviour.Show(arrowBlinkCount, 0.1f);
                else if(arrowBeviour) arrowBeviour.CancelTween();
            }
            SetTouchActivity(!blockSpin, !blockBkgButton);
        }

        private void SetTouchActivity(bool spinButtonActive, bool active)
        {
            Debug.Log("SetTouchActivity(bool spinButtonActive, bool active)" + spinButtonActive + " : " + active);
            if (spinButton) spinButton.interactable = spinButtonActive;
            if (backGroundButton) backGroundButton.interactable = active;
        }
    }
}