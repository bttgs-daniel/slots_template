using UnityEngine;


/*
  20.05.2021
 */
namespace Mkey
{
    public class WinSpriteSeqAnimBehavior : WinSymbolBehavior
    {
        [SerializeField]
        private bool hideSymbol = true;

        #region override
        protected override void PlayWin()
        {
            if(Symbol && hideSymbol) Symbol.HideSymbol();
            SpriteRenderer sR = GetComponent<SpriteRenderer>();
            if (sR)
            {
                sR.sortingOrder = SymbolSortingOrder + GetNextAddSortingOrder();
            }
        }

        protected override void Cancel()
        {

        }
        #endregion override
    }
}