using Mkey;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 14.05.2021
 */
namespace Mkey
{
    public class WinText : WinSectorBehavior
    {
        [SerializeField]
        private Font winFont;
        [SerializeField]
        private Material winFontMaterial;
        [SerializeField]
        private Color winTextColor = Color.white;

        private TextMesh text;
        private Font sourceFont;
        private Material sourceMaterial;
        private Color sourceColor;
        private ColorFlasher cF;
        private MeshRenderer mesh;
        #region override

        protected override void PlayWin()
        {
            base.PlayWin();
            text = WinSector.Text;
            if (!text) return;
            mesh = text.GetComponent<MeshRenderer>();
            if (!mesh) return;

            sourceFont = text.font;
            sourceMaterial = mesh.material;
            sourceColor = text.color;

            text.font = winFont;
            text.color = winTextColor;
            mesh.material = winFontMaterial;
            FlashAlpha();
        }

        protected override void Cancel()
        {
            base.Cancel();
            if (!this) return;
            if (cF != null) cF.Cancel();
            SimpleTween.Cancel(gameObject, false);
            if (text)
            {
                text.font = sourceFont;
                text.color = sourceColor;
            }
            if (mesh) mesh.material = sourceMaterial;
        }
        #endregion override

        private void FlashAlpha()
        {
            cF = new ColorFlasher(gameObject, new TextMesh[] { text }, null, null, null, 1);
            cF.FlashingAlpha();
        }
    }

}