using UnityEngine;
using MkeyFW;

namespace Mkey
{
    public class SlotEvents : MonoBehaviour
    {
        [SerializeField]
        private PopUpsController chestsPrefab;
        public FortuneWheelInstantiator Instantiator;
        public bool autoStartMiniGame = true;
        [SerializeField]
        private AudioClip winCoinsSound;
        [SerializeField]
        private AudioClip bonusSound;

        public static SlotEvents Instance;

        public bool MiniGameStarted { get { return (Instantiator && Instantiator.MiniGame); } }

        #region temp vars
        private Mkey.SlotPlayer MPlayer { get { return Mkey.SlotPlayer.Instance; } }
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private GuiController MGUI { get { return GuiController.Instance; } }
        #endregion temp vars

        private void Awake()
        {
            Instance = this; 
        }

        public void AddLevelProgress_100()
        {
            MPlayer.AddLevelProgress(100f);
            MSound.PlayClip(0, winCoinsSound);
        }

        public void AddLevelProgress_50()
        {
            MPlayer.AddLevelProgress(50f);
            MSound.PlayClip(0, winCoinsSound);
        }

        public void ShowChestMiniGame()
        {
            MGUI.ShowPopUp(chestsPrefab);
        }

        public void ShowFortuneWheel()
        {
            MSound.PlayClip(0, bonusSound);
            Instantiator.Create(autoStartMiniGame);
            if (Instantiator.MiniGame)
            {
                Instantiator.MiniGame.SetBlocked(autoStartMiniGame, autoStartMiniGame);
                Instantiator.SpinResultEvent -= FortuneWheelWinHandler;
                Instantiator.SpinResultEvent += FortuneWheelWinHandler;
            }
        }

        public void ShowFortuneWheel(bool autoStart)
        {
            MSound.PlayClip(0, bonusSound);
            Instantiator.Create(autoStartMiniGame);
            if (Instantiator.MiniGame)
            {
                Instantiator.MiniGame.SetBlocked(autoStartMiniGame, autoStartMiniGame);
                Instantiator.SpinResultEvent -= FortuneWheelWinHandler;
                Instantiator.SpinResultEvent += FortuneWheelWinHandler;
            }
        }

        private void FortuneWheelWinHandler(int coins, bool isBigWin)
        {
            MPlayer.AddCoins(coins);
        }
    }
}