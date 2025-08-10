using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
    30.06.2020 - first
    29.10.2020 - remove shop type, prefabs
*/

namespace Mkey
{
    [System.Serializable]
    public class ShopThingData 
    {
        public string name;

        public string kProductID;
        [Space(8, order = 0)]
        [Header("Purchase Event: ", order = 1)]
        public UnityEvent PurchaseEvent;

        [HideInInspector]
        public Button.ButtonClickedEvent clickEvent;

        public ShopThingData(ShopThingData prod)
        {
            if (prod == null) return;
            name = prod.name;
            clickEvent = prod.clickEvent;
            kProductID = prod.kProductID;
            PurchaseEvent = prod.PurchaseEvent;
        }
    }
}
