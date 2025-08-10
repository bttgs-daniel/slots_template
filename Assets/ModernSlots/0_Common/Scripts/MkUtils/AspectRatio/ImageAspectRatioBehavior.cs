using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
  08.09.2020 - first
 */
namespace Mkey
{
    [ExecuteInEditMode]
    public class ImageAspectRatioBehavior : MonoBehaviour
    {
        [SerializeField]
        private List<RatioToImage> ratios;

        [SerializeField]
        private bool updateSprites;

        [ReadOnly]
        [SerializeField]
        private float currRatio;


        #region temp vars
        private int width = 0;
        private int height = 0;
        private bool debug = false;

        private Image image;
        private SpriteRenderer spriteRenderer;

        [HideInInspector]
        [SerializeField]
        private List<RatioToImage> ratiosOld;

        [HideInInspector]
        [SerializeField]
        private bool updateSpritesOld;
        private bool dirty = false;
        private RatioToImage p;
        private RatioToImage n;
        private RatioToImage c;
        #endregion temp vars

        #region regular
        void OnEnable()
        {
            CheckRatio();
            if (dirty) SetByRatio();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            CheckRatio();
            if (dirty) SetByRatio();
        }
        #endregion regular
     
        private void CheckRatio()
        {
            dirty = false;
            if (updateSprites != updateSpritesOld)
            {
                updateSpritesOld = updateSprites;
                dirty = true;
                return;
            }

#if UNITY_EDITOR
            if (ratios == null)
            {
                ratios = new List<RatioToImage>();
                ratiosOld = new List<RatioToImage>();
                dirty = true;
                return;
            }

            if (ratiosOld == null || ratios.Count != ratiosOld.Count)
            {
                ratiosOld = new List<RatioToImage>();
                foreach (var item in ratios)
                {
                    ratiosOld.Add(new RatioToImage(item));
                }
                dirty = true;
                return;
            }

            #region validate
            for (int i = 0; i < ratios.Count; i++)
            {
                RatioToImage item = ratios[i];
                if (item == null) item = new RatioToImage(0.3f, null);

                item.ratio = Mathf.Clamp(item.ratio, 0.3f, 2.5f);
            }
            #endregion validate
#endif

            #region search changes in list
            for (int i = 0; i < ratios.Count; i++)
            {
                if (!ratios[i].IsEqual(ratiosOld[i]))
                {
                    dirty = true;
                    ratiosOld[i] = new RatioToImage(ratios[i]);
                }
            }
            if (dirty) return;
            #endregion search changes in list

            #region search changes in screen resolution
            if (width != Screen.width || height != Screen.height)
            {
                width = Screen.width;
                height = Screen.height;
                currRatio = width / (float)height;
                dirty = true;
                return;
            }
            #endregion search changes in screen resolution
        }

        private void SetSprite(Sprite sprite)
        { 
            if (!image) image = GetComponent<Image>();
            if (image) { image.sprite = sprite; image.SetNativeSize(); return; }
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
            if(spriteRenderer) { spriteRenderer.sprite = sprite; }

        }

        private void SetByRatio() // Lerp
        {
            if (ratios == null || ratios.Count == 0) return;

            List<RatioToImage> tL = new List<RatioToImage>(ratios);
            tL.Sort((p, n) => { if (n == null) return 1; if (p == null) return 0; return p.ratio.CompareTo(n.ratio); });
            Sprite s = null;
            Vector3 lPos = Vector3.zero;
            float lScale = 1;

            if (tL.Count == 0) return;
            else if (tL.Count == 1)
            {
                s = tL[0].sprite;
                lPos = tL[0].lPosition;
                lScale = tL[0].lScale;
            }
            else if (currRatio <= tL[0].ratio)
            {
                p = tL[0];
                n = tL[1];
                c = RLerp(currRatio, p, n);
                if (c != null)
                {
                    s = c.sprite;
                    lPos = c.lPosition;
                    lScale = c.lScale;
                   // Debug.Log(c);
                }
            }
            else if (currRatio >= tL[tL.Count - 1].ratio)
            {
                p = tL[tL.Count - 2];
                n = tL[tL.Count - 1];
                c = RLerp(currRatio, p, n);
                if (c != null)
                {
                    s = c.sprite;
                    lPos = c.lPosition;
                    lScale = c.lScale;
                  //  Debug.Log(c);
                }
            }
            else
            {
                for (int i = 0; i < tL.Count - 1; i++)
                {
                    p = tL[i];
                    n = tL[i + 1];
                    if (currRatio >= p.ratio && currRatio < n.ratio)
                    {
                        c = RLerp(currRatio, p, n);
                        if (c != null)
                        {
                            s = c.sprite;
                            lPos = c.lPosition;
                            lScale = c.lScale;
                           // Debug.Log(c);
                        }
                    }
                }
            }
            if(updateSprites)   SetSprite(s);
            transform.localPosition = lPos;
            transform.localScale = new Vector3(lScale, lScale, lScale);
            dirty = false;

        }

        private RatioToImage RLerp(float ratio, RatioToImage r1, RatioToImage r2)
        {
            if (r1 == null && r2 == null) return null;
            if (r1 == null) return r2;
            if (r2 == null) return r1;
            if (r1.ratio == r2.ratio) return r1;

            RatioToImage rMin = r1;
            RatioToImage rMax = r2;
            if(r1.ratio > r2.ratio)
            {
                rMin = r2;
                rMax = r1;
            }

            float dr = rMax.ratio - rMin.ratio;
            float drc = ratio - rMin.ratio;
            float t = drc / dr;
            return new RatioToImage(ratio, Mathf.LerpUnclamped(rMin.lScale, rMax.lScale, t), Vector3.LerpUnclamped( rMin.lPosition, rMax.lPosition, t), t>=0.5 ? rMax.sprite: rMin.sprite);
        }
    }

    [Serializable]
    public class RatioToImage
    {
        public float ratio;
        public float lScale;
        public Vector3 lPosition;
        public Sprite sprite;

        public RatioToImage(float ratio, float lScale, Vector3 lPosition, Sprite sprite)
        {
           this.ratio = ratio;
           this.sprite = sprite;
           this.lScale = lScale;
           this.lPosition = lPosition;
        }

        public RatioToImage(float ratio, Sprite sprite)
        {
            this.ratio = ratio;
            this.sprite = sprite;
            lPosition = Vector3.one;
            lScale = 1;
        }

        public RatioToImage(RatioToImage rTI)
        {
            ratio = rTI.ratio;
            sprite = rTI.sprite;
            lPosition = rTI.lPosition;
            lScale = rTI.lScale;
        }

        public bool IsEqual(RatioToImage rTI)
        {
            return (rTI != null && rTI.sprite == sprite && rTI.ratio == ratio && rTI.lScale == lScale && rTI.lPosition == lPosition);
        }

        public override string ToString()
        {
            return "ratio: " + ratio + " ;sprite: "+ ((sprite)? sprite.name : "none sprite" + " ;scale: " + lScale + " ;position" + lPosition);
        }
    }

}