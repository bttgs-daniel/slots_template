using UnityEngine;
using UnityEngine.Events;

/*
   29.04.2021
*/
namespace Mkey
{
    public class DailyRewardLine : MonoBehaviour
    {
        [SerializeField]
        private int gameDayNumber = 0;
        [SerializeField]
        private int controlledDaysCount = 7;

        public static DailyRewardLine CurrentLine => currentLine;

        #region events
        [SerializeField]
        private UnityEvent StartCurrentRewardDayEvent;
        [SerializeField]
        private UnityEvent StartOldRewardDayEvent;
        [SerializeField]
        private UnityEvent StartNextRewardDayEvent;
        [SerializeField]
        private UnityEvent ApplyRewardEvent;
        [SerializeField]
        private UnityEvent <string> UpdateDayNumberText;
        #endregion events

        #region temp vars
        private DailyRewardController DRC => DailyRewardController.Instance;
        private int rewDay = -1;
        private static DailyRewardLine currentLine;
        #endregion temp vars

        private void Start()
        {
            rewDay = DRC.RewardDay;
            if (rewDay < 0) return;
            int rewDayCl = DRC.RepeatingReward ? rewDay % controlledDaysCount : Mathf.Clamp(rewDay, 0, controlledDaysCount - 1);

            // raise events
            if (gameDayNumber == rewDayCl)
            {
                currentLine = this;
                StartCurrentRewardDayEvent?.Invoke();
            }
            else if (gameDayNumber < rewDayCl)
            {
                StartOldRewardDayEvent?.Invoke();
            }
            else if (gameDayNumber > rewDayCl)
            {
                StartNextRewardDayEvent?.Invoke();
            }

            if (DRC.RepeatingReward) UpdateDayNumberText?.Invoke((rewDay + gameDayNumber - rewDayCl + 1).ToString());
        }

        public void Apply()
        {
            DRC.ApplyReward();
            ApplyRewardEvent?.Invoke();
        }

        public void CurrentRewardApply()
        {
           if(CurrentLine) CurrentLine.Apply();
        }
    }
}

/*mkutils*/