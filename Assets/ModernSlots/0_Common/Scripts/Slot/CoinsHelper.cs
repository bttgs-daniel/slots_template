using UnityEngine;

namespace Mkey
{
    public class CoinsHelper : MonoBehaviour
    {
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }

        public void AddCoins(int coins)
        {
            if (MPlayer) MPlayer.AddCoins(coins);
        }
    }
}