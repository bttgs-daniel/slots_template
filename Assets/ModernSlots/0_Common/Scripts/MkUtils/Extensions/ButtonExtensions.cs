using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
  20.05.2020 - first
  24.12.2020 - add cashe
*/

namespace Mkey
{
    public static class ButtonExtensions
    {
        public static void Release(this Button b)
        {
            SourceButtonImages source = CasheSourceImages(b);
            if (!source) return;

            Image im = b.GetComponent<Image>();
            im.sprite = source.normalSprite;// normal;
            SpriteState bST = b.spriteState;
            bST.pressedSprite = source.pressedSprite;// pressed;
            b.spriteState = bST;
        }

        public static void SetPressed(this Button b)
        {
            SourceButtonImages source = CasheSourceImages(b);
            if (!source) return;

            Image im = b.GetComponent<Image>();
            im.sprite = source.pressedSprite;// pressed;
            SpriteState bST = b.spriteState;
            bST.pressedSprite = source.normalSprite;//normal;
            b.spriteState = bST;
        }

        private static SourceButtonImages CasheSourceImages(Button b)
        {
            SourceButtonImages source = null;
            if (b)
            {
                source = b.GetComponent<SourceButtonImages>();
                if (source) return source;
                source = b.GetOrAddComponent<SourceButtonImages>();
                source.normalSprite = b.GetComponent<Image>().sprite;
                source.pressedSprite = b.spriteState.pressedSprite;
            }
            return source;
        }
    }

    public class SourceButtonImages : MonoBehaviour
    {
        public Sprite normalSprite;
        public Sprite pressedSprite;
    }

}