using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class ChestMiniGamePU : PopUpsController
	{
        [SerializeField]
        private GameObject touchBlocker;
        [SerializeField]
        private CoinProcAnim coinsFountain;
        [Header ("Coins random amount in chest")]
        [SerializeField]
        private int minCoinsInChest;
        [SerializeField]
        private int maxCoinsInChest;

        #region temp vars
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        private GuiController MGui { get { return GuiController.Instance; } }
        private List<ChestBehavior> chests;
        #endregion temp vars


        #region regular
        public override void RefreshWindow()
        {
            OnValidate();
            touchBlocker.SetActive(true);
            ParallelTween pt = new ParallelTween();
            float d = 0;
            chests = new List<ChestBehavior>( GetComponentsInChildren<ChestBehavior>());

            foreach (var item in chests)
            {
                float f = d;
                pt.Add(callBack=> { item.ScaleOut(callBack, f); });
                d += 0.35f;
                item.Coins = UnityEngine.Random.Range(minCoinsInChest, maxCoinsInChest + 1);
                item.CoinsFountain = coinsFountain;

                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    touchBlocker.SetActive(true);
                    MPlayer.AddCoins(item.Coins);
                    Invoke("CloseWindow", 2f);
                });
            }
            pt.Start(()=> { touchBlocker.SetActive(false); }); 
            base.RefreshWindow();
        }

        private void OnValidate()
        {
            minCoinsInChest = Mathf.Max(0, minCoinsInChest);
            maxCoinsInChest = Math.Max(minCoinsInChest, maxCoinsInChest);
        }
		#endregion regular
	}
}
