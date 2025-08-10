using UnityEngine;
using System.Collections.Generic;

/*
    01.02.2021 
 */

namespace Mkey
{
	public class ShowRandomGuiPopUp : MonoBehaviour
	{
        [SerializeField]
        private List<PopUpsController> popUps;

        #region temp vars
        protected static GuiController mGui;
        #endregion temp vars

        public void ShowRandomPopUp()
        {
            if (mGui == null) mGui = FindObjectOfType<GuiController>();
            if (mGui && popUps != null && popUps.Count > 0)
            {
                PopUpsController rP = popUps.GetRandomPos();
                if(rP) mGui.ShowPopUp(rP);
            }
        }
    }
}
