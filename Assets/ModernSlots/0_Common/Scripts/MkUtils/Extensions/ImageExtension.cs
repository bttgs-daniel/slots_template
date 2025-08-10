using UnityEngine;
using UnityEngine.UI;

/*
  10.06.2020 - first
*/

namespace Mkey
{
    public static class ImageExtension
    {
        public static void SetImageSprite(Image image, Sprite sprite, bool setNativeSize)
        {
            if (image)
            {
                image.sprite = sprite;
                if (sprite && setNativeSize) image.SetNativeSize();
            }
        }
    }
}