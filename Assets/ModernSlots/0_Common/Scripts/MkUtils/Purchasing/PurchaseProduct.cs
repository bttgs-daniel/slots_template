using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    16.09.2020 - first
*/

namespace Mkey
{
    public class PurchaseProduct : MonoBehaviour
    {
        private Purchaser P { get { return Purchaser.Instance; } }

        public void PurchaseByID(string productID)
        {
            if (P) P.BuyProductID(productID);
        }
    }
}