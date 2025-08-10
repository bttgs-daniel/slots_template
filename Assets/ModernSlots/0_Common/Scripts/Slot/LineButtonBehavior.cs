using UnityEngine;
using System;

namespace Mkey
{
    public class LineButtonBehavior : TouchPadMessageTarget
    {
        [SerializeField]
        private Sprite normalSprite;
        [SerializeField]
        private Sprite pressedSprite;
        private SpriteRenderer spriteRenderer;

        public bool interactable = true;

        #region regular
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        #endregion regular

        internal void Refresh(bool lineSelected)
        {
            if (spriteRenderer) spriteRenderer.sprite = (lineSelected) ? pressedSprite : normalSprite;
        }

        internal void SetSprites(Sprite normalSprite, Sprite pressedSprite)
        {
            this.normalSprite = normalSprite;
            this.pressedSprite = pressedSprite;
            SpriteRenderer sR = GetComponent<SpriteRenderer>();
            if (sR) sR.sprite = normalSprite;
        }

    }
}

