using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
   helper object for displaying the rewards popup
   28.04.2021
*/
namespace Mkey
{
    public class DailyRewardGUIController : MonoBehaviour
    {
        [SerializeField]
        private PopUpsController dailyRewardPUPrefab;
        [SerializeField]
        private float delay;
        [SerializeField]
        private bool showOnlyAtStart = true;

        #region temp vars
        private GuiController MGui => GuiController.Instance;
        private DailyRewardController DRC => DailyRewardController.Instance;
        private int rewDay = -1;
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            while (!DRC) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            DRC.TimePassEvent.AddListener(TimePassedEventHandler);
            rewDay = DRC.RewardDay;
            if (rewDay >= 0)
            {
                StartCoroutine(ShowRewardPopup());
            }
        }

        private void OnDestroy()
        {
            if(DRC) DRC.TimePassEvent.RemoveListener(TimePassedEventHandler);
        }
        #endregion regular

        private IEnumerator ShowRewardPopup()
        {
            yield return new WaitForSeconds(delay);
            MGui.ShowPopUp(dailyRewardPUPrefab);
        }

        private void TimePassedEventHandler()
        {
            rewDay = DRC.RewardDay;
            if (rewDay >= 0)
            {
                if (!showOnlyAtStart) StartCoroutine(ShowRewardPopup());
            }
        }
    }
}

/*mkutils*/
