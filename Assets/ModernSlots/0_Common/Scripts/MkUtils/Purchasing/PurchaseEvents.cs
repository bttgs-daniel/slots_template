using UnityEngine;

/*
    31.10.2020 
    18.04.2021 
        - add  failedMessagePrefab
    21.01.2022 (test) - remove player reference
    
 */
namespace Mkey
{
    public class PurchaseEvents : MonoBehaviour
    {
        [SerializeField]
        private WarningMessController goodMessagePrefab;
        [SerializeField]
        private WarningMessController failedMessagePrefab;

        [SerializeField]
        private bool showGoodMessage = true;
        [SerializeField]
        private bool showFailedMessage = true;

        private GuiController MGui => GuiController.Instance;
        private Purchaser MPurchaser => Purchaser.Instance;

        #region regular
        private void Start()
        {
            if (MPurchaser)
            {
               if(showGoodMessage) MPurchaser.GoodPurchaseEvent += GoodPurchaseMessage;
               if(showFailedMessage) MPurchaser.FailedPurchaseEvent += FailedPurchaseMessage;
            }
        }

        private void OnDestroy()
        {
            if (MPurchaser)
            {
                MPurchaser.GoodPurchaseEvent -= GoodPurchaseMessage;
                MPurchaser.FailedPurchaseEvent -= FailedPurchaseMessage;
            }
        }
        #endregion regular

        internal void GoodPurchaseMessage(string prodId, string prodName)
        {
            if (MGui && goodMessagePrefab)
            {
                MGui.ShowMessageWithYesNoCloseButton(goodMessagePrefab, null, prodName + " purchased successfull.", () => { }, null, null);
            }
            else if (MGui)
            {
                MGui.ShowMessageWithYesNoCloseButton("Succesfull!!!", prodName + " purchased successfull.", () => { }, null, null);
            }
        }

        internal void FailedPurchaseMessage(string prodId, string prodName)
        {
            if (MGui && failedMessagePrefab)
            {
                MGui.ShowMessageWithYesNoCloseButton(failedMessagePrefab, null, prodName + " - purchase failed.", () => { }, null, null);
            }
            else if (MGui)
            {
                MGui.ShowMessageWithYesNoCloseButton("Sorry.", prodName + " - purchase failed.", () => { }, null, null);
            }
        }

        internal void GoodPurchaseMessage(string message)
        {
            if (MGui)
            {
                MGui.ShowMessage("Succesfull!!!", message, 3, null);
            }
        }

        internal void FailedPurchaseMessage(string message)
        {
            if (MGui)
            {
                MGui.ShowMessage("Sorry.", message, 3, null);
            }
        }
    }
}