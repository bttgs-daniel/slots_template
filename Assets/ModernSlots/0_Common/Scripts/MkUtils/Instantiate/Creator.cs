using System;
using UnityEngine;

/*
    12.11.2020
    11.01.2021 - fix InstantiateAnimPrefabAtPosition, 
*/
namespace Mkey {
    public class Creator : MonoBehaviour {

        #region prefab
        /// <summary>
        /// Instantiate prefab at position, set parent, parent lossyScale
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="destroyTime"></param>
        internal static GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 position, int sortingLayer, int sortingOrder)
        {
            if (!prefab) return null;
            GameObject g = InstantiatePrefab(prefab, parent, position, 0);
            if (!g) return null;

            SpriteRenderer[] sRs = g.GetComponentsInChildren<SpriteRenderer>();
            if (sRs != null)
            {
                foreach (var sR in sRs)
                {
                    sR.sortingLayerID = sortingLayer;
                    sR.sortingOrder = sortingOrder;
                }
            }
            return g;
        }

        /// <summary>
        /// Instantiate prefab at position, set parent, parent lossyScale, and if (destroyTime>0) destroy result gameobject after destroytime.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="destroyTime"></param>
        internal static GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 position, float destroyTime)
        {
            if (!prefab) return null;
            GameObject g = Instantiate(prefab, position, Quaternion.identity);
            if (!g) return null;

            if (parent)
            {
                g.transform.localScale = parent.lossyScale;
                g.transform.parent = parent;
            }
            if (destroyTime > 0) Destroy(g, destroyTime);
            return g;
        }

        /// <summary>
        /// Instantiate prefab at position, set parent, parent lossyScale, and if (destroyTime>0) destroy result gameobject after destroytime.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="destroyTime"></param>
        internal static GameObject InstantiatePrefabAtPosition(GameObject prefab, Transform parent, Vector3 position, float destroyTime)
        {
            if (!prefab) return null;
            GameObject g = Instantiate(prefab, position, Quaternion.identity);
            if (!g) return null;

            if (parent)
            {
                g.transform.localScale = parent.lossyScale;
                g.transform.parent = parent;
            }
            if (destroyTime > 0) Destroy(g, destroyTime);
            return g;
        }

        /// <summary>
        /// Instantiate sprite anim prefab at position, set parent, parent lossyScale, setOrder
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="destroyTime"></param>
        internal static GameObject InstantiateAnimPrefabAtPosition(GameObject prefab, Transform parent, Vector3 position, int sortingOrder, bool destroy, Action completeCallback)
        {
            Debug.Log("anim");
            if (!prefab) { completeCallback?.Invoke(); return null; }
            GameObject g = Instantiate(prefab, position, Quaternion.identity);
            if (!g) { completeCallback?.Invoke(); return null; }

            if (parent)
            {
                g.transform.localScale = parent.lossyScale;
                g.transform.parent = parent;
            }
            SpriteRenderer srDot = g.GetComponent<SpriteRenderer>();
            if(srDot) srDot.sortingOrder = sortingOrder;

            AnimCallBack aC = g.GetComponent<AnimCallBack>();
            if (aC)
            {
                Debug.Log("ac");
                aC.SetEndCallBack( ()=> {if(destroy) Destroy(g); completeCallback?.Invoke(); });
            }
            else
            {
                completeCallback?.Invoke();
            }
            return g;
        }

        /// <summary>
        /// Instantiate sprite anim prefab at position, set parent, parent lossyScale, setOrder
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="destroyTime"></param>
        internal static GameObject InstantiateAnimPrefab(GameObject prefab, Transform parent, Vector3 position, int sortingOrder)
        {
            if (!prefab) return null;
            GameObject g = Instantiate(prefab);
            if (!g) return null;

            if (parent)
            {
                g.transform.localScale = parent.lossyScale;
                g.transform.parent = parent;
                g.transform.position = position;
            }
            SpriteRenderer[] srDot = g.GetComponentsInChildren<SpriteRenderer>();
            foreach (var item in srDot)
            {
                if (item) item.sortingOrder = sortingOrder;
            }
            return g;
        }
        #endregion prefab

        #region sprite
        /// <summary>
        /// Instantiate new 3D Sprite at position, and set parent (if parent !=null), set scale like parent lossyScale
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        /// <param name="renderLayer"></param>
        /// <param name="renderOrder"></param>
        /// <returns></returns>
        internal static SpriteRenderer CreateSpriteAtPosition(Transform parent, Sprite sprite, Vector3 position)
        {
            GameObject gO = new GameObject();

            if (parent)
            {
                gO.transform.parent = parent;
                gO.transform.localScale = Vector3.one;
            }

            gO.transform.position = position;
            SpriteRenderer sR = gO.AddComponent<SpriteRenderer>();
            sR.sprite = sprite;
            return sR;
        }

        /// <summary>
        /// Instantiate new 3D Sprite at position, and set parent (if parent !=null), set scale like parent lossyScale, set sortingLayerID, sortingOrder
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        /// <param name="renderLayer"></param>
        /// <param name="renderOrder"></param>
        /// <returns></returns>
        internal static SpriteRenderer CreateSpriteAtPosition(Transform parent, Sprite sprite, Vector3 position, int sortingOrder)
        {
            SpriteRenderer sR = CreateSpriteAtPosition(parent, sprite, position);
            if (sR) sR.sortingOrder = sortingOrder;
            return sR;
        }

        /// <summary>
        /// Instantiate new 3D Sprite at position, and set parent (if parent !=null), set scale like parent lossyScale, set sortingLayerID, sortingOrder
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        /// <param name="renderLayer"></param>
        /// <param name="renderOrder"></param>
        /// <returns></returns>
        internal static SpriteRenderer CreateSpriteAtPosition(Transform parent, Sprite sprite, Vector3 position, int sortingLayerID, int sortingOrder)
        {
            SpriteRenderer sR = CreateSpriteAtPosition(parent, sprite, position, sortingOrder);
            if (sR) sR.sortingLayerID = sortingLayerID;
            return sR;
        }

        /// <summary>
        /// Instantiate new 3D Sprite at position, and set parent (if parent !=null), set scale like parent lossyScale, set sortingLayerID, sortingOrder
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        /// <param name="renderLayer"></param>
        /// <param name="renderOrder"></param>
        /// <returns></returns>
        internal static SpriteRenderer CreateSpriteAtPosition(Transform parent, Sprite sprite, Material material, Vector3 position, int sortingLayerID, int sortingOrder)
        {
            SpriteRenderer sR = CreateSpriteAtPosition(parent, sprite, position, sortingLayerID, sortingOrder);
            if (sR && material) sR.material = material;
            return sR;
        }
        #endregion sprite
    }
}