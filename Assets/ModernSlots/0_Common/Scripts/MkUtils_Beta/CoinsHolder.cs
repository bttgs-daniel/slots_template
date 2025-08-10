using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

/*
  player game coins holder
  04.05.2021
  27.05.2021
 */
namespace Mkey
{
    [CreateAssetMenu(menuName = "ScriptableObjects/CoinsHolder")]
    public class CoinsHolder : ScriptableObject
    {
        #region default data
        [Space(10, order = 0)]
        [Header("Default data", order = 1)]
        [Tooltip("Default coins at start")]
        [SerializeField]
        private int defCoinsCount = 500;

        [Tooltip("Default facebook coins")]
        [SerializeField]
        private int defFBCoinsCount = 100;
        #endregion default data

        #region keys
        private string saveCoinsKey = "mk_slot_coins"; // current coins
        private string saveFbCoinsKey = "mk_slot_fbcoins"; // facebook coins
        #endregion keys

        public int Coins
        {
            get; private set;
        }

        public int DefaultCoins => defCoinsCount;

        public UnityEvent <int> ChangeCoinsEvent;
        public UnityEvent <int> LoadCoinsEvent;

        private void Awake()
        {
            LoadCoins();
            Debug.Log("Awake: " + this + " ;coins: " + Coins);
        }

        /// <summary>
        /// Add coins and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddCoins(int count)
        {
            SetCoinsCount(Coins + count);
        }

        /// <summary>
        /// Set coins and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetCoinsCount(int count)
        {
            SetCoinsCount(count, true);
        }

        /// <summary>
        /// Set coins, save result and raise ChangeCoinsEvent
        /// </summary>
        /// <param name="count"></param>
        private void SetCoinsCount(int count, bool raiseEvent)
        {
            count = Mathf.Max(0, count);
            bool changed = (Coins != count);
            Coins = count;
            if (changed)
            {
                string key = saveCoinsKey;
                PlayerPrefs.SetInt(key, Coins);
            }
            if (changed && raiseEvent) ChangeCoinsEvent?.Invoke(Coins);
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
        public void LoadCoins()
        {
            string key = saveCoinsKey;
            Coins = PlayerPrefs.GetInt(key, defCoinsCount);
            LoadCoinsEvent?.Invoke(Coins);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CoinsHolder))]
    public class CoinsHolderEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            #region test
            if (EditorApplication.isPlaying)
            {
                if (test = EditorGUILayout.Foldout(test, "Test"))
                {
                    #region coins
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Add 500 coins"))
                    {
                        CoinsHolder cH = (CoinsHolder)target;
                        if (cH)
                            cH.AddCoins(500);
                    }
                    if (GUILayout.Button("Set 500 coins"))
                    {
                        CoinsHolder cH = (CoinsHolder)target;
                        if (cH)
                            cH.SetCoinsCount(500);
                    }
                    if (GUILayout.Button("Clear coins"))
                    {
                        CoinsHolder cH = (CoinsHolder)target;
                        if (cH)
                            cH.SetCoinsCount(0);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion coins

                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Reset to default"))
                    {
                        CoinsHolder cH = (CoinsHolder)target;
                        if (cH)
                            cH.SetCoinsCount(0);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Goto play mode for test");
                if (GUILayout.Button("Log coins"))
                {
                    CoinsHolder cH = (CoinsHolder)target;
                    if (cH) Debug.Log("Coins: " + cH.Coins);

                }
                if (GUILayout.Button("Load saved coins"))
                {
                    CoinsHolder cH = (CoinsHolder)target;
                    if (cH) cH.LoadCoins();

                }
            }
            #endregion test
        }
    }
#endif
}