using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 25.06.2025
// real-time RTP adjustments
namespace Mkey
{
	public enum ADJUSTMODE { FULLRANDOM, SCATTERWINS, WINS, LOSSES, JPWINS, AUTO }

	public class RTPAdj : MonoBehaviour
	{
		public ADJUSTMODE adjMode = ADJUSTMODE.FULLRANDOM;

		public float payOut = 80;	// percentage

		#region temp vars
		private SlotController slotController;
		private List<SlotWinPosition> winSlotWindows;
		private List<SlotWinPosition> winScatterSlotWindows;
		private List<SlotWinPosition> winJPSlotWindows;
		private List<SlotPosition> looseSlotWindows;
		private float lastPayOut = 0;
		int[] fromController;
		#endregion temp vars

		#region regular
		private void Start()
		{
			RTPCalc pCalc = GetComponent<RTPCalc>();
			if (!pCalc) return;
			if (!pCalc.isActiveAndEnabled) return;
			SlotController slotController = GetComponent<SlotController>();
			CreateWinBase();
			slotController.EndCalcPayoutEvent += (pOut) => { lastPayOut = pOut; };
#if UNITY_EDITOR
			pCalc.RebuildAction += CreateWinBase;
#endif
		}

        private void OnDestroy()
        {
			RTPCalc pCalc = GetComponent<RTPCalc>();
#if UNITY_EDITOR
			if (pCalc) pCalc.RebuildAction -= CreateWinBase;
#endif
		}
        #endregion regular

        /// <summary>
        /// return next reels positions
        /// </summary>
        /// <param name="fromController"></param>
        /// <returns></returns>
        public int[] GetNextPositions(int[] fromController)
		{
			this.fromController = fromController;
			switch (adjMode)
			{
				case ADJUSTMODE.FULLRANDOM:    
					return fromController;								// return source data  from controller
				case ADJUSTMODE.SCATTERWINS:							
					return GetScatterWinReelsPositions();				// return random data from scatter wins file
				case ADJUSTMODE.WINS:                                   // return random data from wins file (any win: line win, scatter win, jp win)
					return GetWinReelsPositions();
				case ADJUSTMODE.LOSSES:                                 // return random data from looses file
					return GetLooseReelsPositions();
				case ADJUSTMODE.JPWINS:                                 // return random data from jackpot wins file
					return GetJPWinReelsPositions();
				case ADJUSTMODE.AUTO:
					return GetAutoPayoutPositions();
				default:
					return fromController;                              // return source data  from controller
			}
		}

		/// <summary>
		/// retun reels position from win file or random fromController 
		/// </summary>
		/// <returns></returns>
		public int [] GetWinReelsPositions()
		{
			SlotWinPosition winW = GetRandomWin();
			if (winW == null) return fromController;
			List<int> res = new List<int>(winW.rP);
			return res.ToArray();
		}

		/// <summary>
		/// return reels positions from loose file or random fromController 
		/// </summary>
		/// <returns></returns>
		public int [] GetLooseReelsPositions()
		{
			SlotPosition looseW = GetRandomLoose();
			if (looseW == null) return fromController;
			List<int> res = new List<int>(looseW.rP);
			return res.ToArray();
		}

		/// <summary>
		/// retun reels position from scatter win file or random fromController 
		/// </summary>
		/// <returns></returns>
		public int[] GetScatterWinReelsPositions()
		{
			SlotWinPosition winW = GetRandomScatterWin();
			if (winW == null) return fromController;
			List<int> res = new List<int>(winW.rP);
			return res.ToArray();
		}

		/// <summary>
		/// retun reels position from jackpot win file or random fromController 
		/// </summary>
		/// <returns></returns>
		public int[] GetJPWinReelsPositions()
		{
			SlotWinPosition winW = GetRandomJPWin();
			if (winW == null) return fromController;
			List<int> res = new List<int>(winW.rP);
			return res.ToArray();
		}

		#region private
		private SlotPosition GetRandomLoose()
		{
			return (looseSlotWindows != null && looseSlotWindows.Count > 0) ? looseSlotWindows.GetRandomPos() : null;
		}

		private SlotWinPosition GetRandomWin()
		{
			return (winSlotWindows != null && winSlotWindows.Count > 0) ? winSlotWindows.GetRandomPos() : null;
		}

		private SlotWinPosition GetRandomScatterWin()
		{
			return (winScatterSlotWindows != null && winScatterSlotWindows.Count > 0) ?  winScatterSlotWindows.GetRandomPos() : null;
		}

		private SlotWinPosition GetRandomJPWin()
		{
			return (winJPSlotWindows != null && winJPSlotWindows.Count > 0) ? winJPSlotWindows.GetRandomPos() : null;
		}

		private int [] GetAutoPayoutPositions()
        {
			if(lastPayOut < payOut)
            {
				return GetWinReelsPositions();
			}
			return GetLooseReelsPositions();

		}

		private void CreateWinBase()
        {
			RTPCalc pCalc = GetComponent<RTPCalc>();
			string json;

			if (pCalc.loosesFile)
			{
				json = pCalc.loosesFile.text;
				ListWrapper<SlotPosition> lWLooses = JsonUtility.FromJson<ListWrapper<SlotPosition>>(json);
				looseSlotWindows = lWLooses.list;
				Debug.Log("looseSlotWindows.Count: " + looseSlotWindows.Count);
			}
			else Debug.LogError("pCalc.loosesJson failed");

			if (pCalc.winsFile)
			{
				json = pCalc.winsFile.text;
				ListWrapper<SlotWinPosition> lWins = JsonUtility.FromJson<ListWrapper<SlotWinPosition>>(json);
				winSlotWindows = lWins.list;
				Debug.Log("winSlotWindows.Count: " + winSlotWindows.Count);
			}
			else Debug.LogError("pCalc.winsJson failed");

			if (pCalc.winsScatterFile)
			{
				json = pCalc.winsScatterFile.text;
				ListWrapper<SlotWinPosition> lScWins = JsonUtility.FromJson<ListWrapper<SlotWinPosition>>(json);
				winScatterSlotWindows = lScWins.list;
				Debug.Log("winScatterSlotWindows.Count: " + winScatterSlotWindows.Count);
			}

			if (pCalc.winsJPFile)
			{
				json = pCalc.winsJPFile.text;
				ListWrapper<SlotWinPosition> lScWins = JsonUtility.FromJson<ListWrapper<SlotWinPosition>>(json);
				winJPSlotWindows = lScWins.list;
				Debug.Log("winJPSlotWindows.Count: " + winJPSlotWindows.Count);
			}
		}
		#endregion private
	}
}
