using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class BalanceGUIController : MonoBehaviour
	{
        [SerializeField]
        private Text balanceAmountText;

        #region temp vars
        private TweenLongValue balanceTween;
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        private GuiController MGui { get { return GuiController.Instance; } }
        private string coinsFormat = "0,0";
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            while (!MPlayer)
            {
                yield return new WaitForEndOfFrame();
            }

            // set player event handlers
            MPlayer.ChangeCoinsEvent += ChangeBalanceHandler;
            MPlayer.LoadCoinsEvent += LoadBalanceHandler;
            if (balanceAmountText) balanceTween = new TweenLongValue(balanceAmountText.gameObject, MPlayer.Coins, 1, 3, true, (b) => { if (this && balanceAmountText) balanceAmountText.text = (b > 0) ? b.ToString(coinsFormat) : "0"; });
            Refresh();
        }

        private void OnDestroy()
        {
            if (MPlayer)
            {
                // remove player event handlers
                MPlayer.ChangeCoinsEvent -= ChangeBalanceHandler;
                MPlayer.LoadCoinsEvent -= LoadBalanceHandler;
            }
        }
        #endregion regular

        /// <summary>
        /// Refresh gui balance
        /// </summary>
        private void Refresh()
        {
            if (balanceAmountText && MPlayer) balanceAmountText.text = (MPlayer.Coins > 0) ? MPlayer.Coins.ToString(coinsFormat) : "0";
        }

        #region eventhandlers
        private void ChangeBalanceHandler(long newBalance)
        {
            if (balanceTween != null) balanceTween.Tween(newBalance, 100);
            else
            {
                if (balanceAmountText) balanceAmountText.text = (newBalance > 0) ? newBalance.ToString(coinsFormat) : "0";
            }
        }

        private void LoadBalanceHandler(long newBalance)
        {
            if (balanceAmountText) balanceAmountText.text = (newBalance > 0) ? newBalance.ToString(coinsFormat) : "0";
        }
        #endregion eventhandlers
    }
}
