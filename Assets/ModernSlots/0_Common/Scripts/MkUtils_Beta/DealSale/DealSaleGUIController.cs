using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;


/*
    18.05.2021
        
 */

namespace Mkey
{
	public class DealSaleGUIController : MonoBehaviour
	{
        [SerializeField]
        private UnityEvent <bool> SaleEvent;
        [SerializeField]
        private UnityEvent<string> TimeUpdateEvent;

        #region temp vars
        private DealSaleController DSC { get { return DealSaleController.Instance; } }
        #endregion temp vars

        #region regular
        private IEnumerator Start()
		{
            while (!DSC) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            DSC.WorkingDealTickRestDaysHourMinSecEvent += WorkingDealTickRestDaysHourMinSecHandler;
            DSC.WorkingDealTimePassedEvent += WorkingDealTimePassedHandler;
            DSC.WorkingDealStartEvent += WorkingDealStartHandler;
            DSC.PausedDealStartEvent += PausedDealStartHandler;
            SaleEvent?.Invoke(DSC.IsDealTime);
        }
		
		private void OnDestroy()
        {
            if (DSC)
            {
                DSC.WorkingDealTickRestDaysHourMinSecEvent -= WorkingDealTickRestDaysHourMinSecHandler;
                DSC.WorkingDealTimePassedEvent -= WorkingDealTimePassedHandler;
                DSC.WorkingDealStartEvent -= WorkingDealStartHandler;
                DSC.PausedDealStartEvent -= PausedDealStartHandler;
            }
        }
        #endregion regular

        #region event handlers
        private void WorkingDealTickRestDaysHourMinSecHandler(int d, int h, int m, float s)
        {
            TimeUpdateEvent?.Invoke(String.Format("{0:00}:{1:00}:{2:00}", h, m, s));
        }

        private void WorkingDealTimePassedHandler(double initTime, double realyTime)
        {
            TimeUpdateEvent?.Invoke(String.Format("{0:00}:{1:00}:{2:00}", 0, 0, 0));
            SaleEvent?.Invoke(DSC.IsDealTime);
        }

        private void WorkingDealStartHandler()
        {
            SaleEvent?.Invoke(DSC.IsDealTime);
        }

        private void PausedDealStartHandler()
        {
            TimeUpdateEvent?.Invoke(String.Format("{0:00}:{1:00}:{2:00}", 0, 0, 0));
            SaleEvent?.Invoke(DSC.IsDealTime);
        }
        #endregion event handlers
    }
}
