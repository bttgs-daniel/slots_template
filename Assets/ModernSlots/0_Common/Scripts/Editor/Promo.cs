#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mkey
{
    [InitializeOnLoad]
    internal static class Promo
    {
        private const int loadsToShow = 3;
        const string showKey = "promo_shown";
        const string loadsCounterKey = "promo_loads_counter";

        static Promo()
        {
            // EditorApplication.delayCall += ShowMessage; return;  // test
            if (EditorPrefs.GetBool(showKey)) return;       
            int loads = EditorPrefs.GetInt(loadsCounterKey, 0);
            if (loads < loadsToShow)
            {
                loads++;
                EditorPrefs.SetInt(loadsCounterKey, loads);
                return;
            }
            else if(loads == loadsToShow)
            {
                loads++;
                EditorPrefs.SetInt(loadsCounterKey, loads);
                EditorApplication.delayCall += ShowMessage;    
            }

          
        }

        static void ShowMessage()
        {
            if (EditorUtility.DisplayDialog(
                    "Enjoying MK – Slot Engine?",
                    "Your feedback shapes our next update! \n   • If the asset saved you time, a quick ⭐️⭐️⭐️⭐️⭐️ helps other devs discover it. \n   • Got an idea, bug report, or dream feature? Email us at putchkov1975@gmail.com — we prioritise the most-wanted improvements. \n\nThanks for helping us make MK – Slot Engine even better!",
                    "Rate & Suggest", "Later"))
            {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/templates/packs/mk-modern-suits-slot-asset-115588#reviews");        
            }
            EditorPrefs.SetBool(showKey, true);              
        }
    }
}
#endif
