using System.Collections.Generic;
using UnityEngine;

/* Offset scene objects according base resolution
    09.06.2020 - first
 */
namespace Mkey
{
    [ExecuteInEditMode]
    public class AutoOffset : MonoBehaviour {
        [SerializeField]
        private float baseRatio = 0.75f; // width/height
        [HideInInspector]
        [SerializeField]
        private float baseRatioOld = 0.0f;

        #region temp vars
        [SerializeField]
        private float currRatio;
        private int width = 0;
        private int height = 0;
        private bool debug = false;

        [SerializeField]
        private float lerpRatio = 1;
        [SerializeField]
        private Vector2 lerpOffset = Vector2.zero;

        private float lerpRatioOld = 1;
        private Vector2 lerpOffsetOld = Vector2.zero;
        private Vector2 currentLerpOffset;
        #endregion temp vars

        #region regular
        void Start()
        {
            SetOffset();
        }

        void Update()
        {
            SetOffset();
        }
        #endregion regular

        void SetOffset()
        {
            #region validate
            baseRatio = Mathf.Clamp(baseRatio, 0.3f, 2.5f);
            lerpRatio = Mathf.Clamp(lerpRatio, 0.3f, 2.5f);
            #endregion validate

            if (width != Screen.width || height != Screen.height || baseRatio != baseRatioOld || lerpRatio != lerpRatioOld || lerpOffset != lerpOffsetOld)
            {
                width = Screen.width;
                height = Screen.height;
                currRatio = width / (float)height;
                baseRatioOld = baseRatio;
                lerpRatioOld = lerpRatio;
                lerpOffsetOld = lerpOffset;
                currentLerpOffset = (currRatio - baseRatio) / (lerpRatio - baseRatio) * lerpOffset;
                gameObject.transform.localPosition = currentLerpOffset;
            }
        }
    }
}
