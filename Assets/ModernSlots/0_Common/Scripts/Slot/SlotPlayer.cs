using UnityEngine;
using System;

namespace Mkey
{
    public class SlotPlayer : MonoBehaviour
    {
        #region default data
        [Space(10, order = 0)]
        [Header("Default data", order = 1)]
        [Tooltip("Default coins at start")]
        [SerializeField]
        private long defCoinsCount = 500; 

        [Tooltip("Default facebook coins")]
        [SerializeField]
        private long defFBCoinsCount = 100;

        [Tooltip("Check if you want to add level up reward")]
        [SerializeField]
        private bool useLevelUpReward = true;

        [Tooltip("Default level up reward")]
        [SerializeField]
        private long levelUpReward = 3000;

        [Tooltip("Check if you want to show big win congratulation")]
        [SerializeField]
        private bool useBigWinCongratulation = true;

        [Tooltip("Min win to show big win congratulation")]
        [SerializeField]
        private long minWin = 5000;

        [Space(8)]
        [Tooltip("Check if you want to save coins, level, progress, facebook gift flag, sound settings")]
        [SerializeField]
        private bool saveData = false;
        #endregion default data

        #region keys
        private string saveCoinsKey = "mk_slot_coins"; // current coins
        private string saveFbCoinsKey = "mk_slot_fbcoins"; // facebook coins
        private string saveLevelKey = "mk_slot_level"; // current level
        private string saveLevelProgressKey = "mk_slot_level_progress"; // progress to next level %
        #endregion keys

        #region events
        public Action <long> ChangeCoinsEvent;
        public Action<long> LoadCoinsEvent;
        public Action <float> ChangeLevelProgressEvent;
        public Action<float> LoadLevelProgressEvent;
        public Action <int, long, bool> ChangeLevelEvent;
        public Action<int> LoadLevelEvent;
        public Action<long> ChangeWinCoinsEvent;
        #endregion events

        #region properties
        public bool SaveData
        {
            get { return saveData; }
        }

        public long MinWin
        {
            get { return minWin; }
        }

        public bool UseBigWinCongratulation
        {
            get { return useBigWinCongratulation; }
        }

        public long WinCoins
        {
            get; private set;
        }

        public long LevelUpReward => levelUpReward;

        public bool UseLevelUpReward => useLevelUpReward;
        #endregion properties

        #region saved properties
        public long Coins
        {
            get; private set;
        }

        public int Level
        {
            get; private set;
        }

        public float LevelProgress
        {
            get; private set;
        }
        #endregion saved properties

        public static SlotPlayer Instance { get; private set; }

        #region regular
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            Validate();
            LoadCoins();
            LoadLevel();
            LoadLevelProgress();
        }

        private void OnValidate()
        {
            Validate();
        }

        private void Validate()
        {
            defCoinsCount = Math.Max(0, defCoinsCount);
            defFBCoinsCount = Math.Max(0, defFBCoinsCount);
            levelUpReward = Math.Max(0, levelUpReward);
        }
        #endregion regular

        #region coins
        /// <summary>
        /// Add coins and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddCoins(long count)
        {
            SetCoinsCount(Coins + count);
        }

        /// <summary>
        /// Set coins and save result 
        /// </summary>
        /// <param name="count"></param>
        public void SetCoinsCount(long count)
        {
            count = Math.Max(0, count);
            bool changed = (Coins != count);
            Coins = count;
            if (SaveData && changed)
            {
                string key = saveCoinsKey;
                PlayerPrefsExtension.SetLong(key, Coins);
            }
            if (changed) ChangeCoinsEvent?.Invoke(Coins);
        }

        /// <summary>
        /// Add facebook gift (only once), and save flag.
        /// </summary>
        public void AddFbCoins()
        {
            if (!PlayerPrefs.HasKey(saveFbCoinsKey) || PlayerPrefs.GetInt(saveFbCoinsKey) == 0)
            {
                PlayerPrefs.SetInt(saveFbCoinsKey, 1);
                AddCoins(defFBCoinsCount);
            }
        }

        /// <summary>
        /// Load serialized coins or set defaults
        /// </summary>
        private void LoadCoins()
        {
            if (SaveData)
            {
                string key = saveCoinsKey;
                Coins = Math.Max(0, PlayerPrefsExtension.GetLong(key, defCoinsCount));
            }
            else
            {
                Coins = defCoinsCount;
            }

            LoadCoinsEvent?.Invoke(Coins);
        }

        #endregion coins

        #region wincoins
        /// <summary>
        /// Add coins and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddWinCoins(long count)
        {
            SetWinCoinsCount(WinCoins + count);
        }

        /// <summary>
        /// Set coins and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetWinCoinsCount(long count)
        {
            count = Math.Max(0, count);
            bool changed = (WinCoins != count);
            WinCoins = count;
            if (changed) ChangeWinCoinsEvent?.Invoke(WinCoins);
        }

        public void TakeWin()
        {
            AddCoins(WinCoins);
            SetWinCoinsCount(0);
        }
        #endregion wincoins

        #region Level
        /// <summary>
        /// Change level and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddLevel(int count)
        {
            SetLevel(Level + count);
        }

        /// <summary>
        /// Set level, save result and raise event ChangeLevelEvent
        /// </summary>
        /// <param name="count"></param>
        public void SetLevel(int count)
        {
            count = Mathf.Max(0, count);
            bool changed = (Level != count);
            int addLevels = count - Level;
            Level = count;
            if (SaveData && changed)
            {
                string key = saveLevelKey;
                PlayerPrefs.SetInt(key, Level);
            }
            if (changed) ChangeLevelEvent?.Invoke(Level, Math.Max(0, addLevels * levelUpReward), useLevelUpReward);
        }

        /// <summary>
        /// Load serialized level or set 0
        /// </summary>
        private void LoadLevel()
        {
            if (SaveData)
            {
                string key = saveLevelKey;
                Level = Math.Max(0, PlayerPrefs.GetInt(key, 0));
            }
            else
            {
                Level = 1;
            }
            LoadLevelEvent?.Invoke(Level);
        }
        #endregion Level

        #region LevelProgress
        /// <summary>
        /// Change level and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddLevelProgress(float count)
        {
            SetLevelProgress(LevelProgress + count);
        }

        /// <summary>
        /// Set level, save result and raise ChangeLevelProgressEvent
        /// </summary>
        /// <param name="count"></param>
        public void SetLevelProgress(float count)
        {
            SetLevelProgress(count, true);
        }

        /// <summary>
        /// Set level, save result and raise ChangeLevelProgressEvent
        /// </summary>
        /// <param name="count"></param>
        private void SetLevelProgress(float count, bool raiseEvent)
        {
            count = Mathf.Max(0, count);
            if (count >= 100)
            {
                int addLevels = (int)count / 100;
                AddLevel(addLevels);
                count = 0;
            }

            bool changed = (LevelProgress != count);
            LevelProgress = count;
            if (SaveData && changed)
            {
                string key = saveLevelProgressKey;
                PlayerPrefs.SetFloat(key, LevelProgress);
            }
            if (changed && raiseEvent) ChangeLevelProgressEvent?.Invoke(LevelProgress);
        }

        /// <summary>
        /// Load serialized levelprogress or set 0
        /// </summary>
        private void LoadLevelProgress()
        {
            if (SaveData)
            {
                string key = saveLevelProgressKey;
                SetLevelProgress(PlayerPrefs.GetFloat(key, 0),false);
            }
            else
            {
                SetLevelProgress(0, false);
            }
            LoadLevelProgressEvent?.Invoke(LevelProgress);
        }
        #endregion LevelProgress

        public void SetDefaultData()
        {
            SetCoinsCount(defCoinsCount);
            PlayerPrefs.SetInt(saveFbCoinsKey, 0); // reset facebook gift
           
            SetLevel(0);
            SetLevelProgress(0);
        }

        public bool HasMoneyForBet (int totalBet)
        {
             return totalBet <= Coins; 
        }
    }
}