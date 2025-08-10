using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
    10012019
    - add Action<float> progressDel
    - remove public  Action LoadingCallBack
    13032019
    - add method  ReLoadCurrentScene()
    11112019
     - use LoadGroupPrefab popup
    18.11.2019
     - add GetCurrentSceneName();
    21.01.2020
    - improve AsyncLoadBeaty
    13.05.2020 
    - PSlider
    18.05.2020
    - GetCurrentSceneBuildIndex()
    30.06.2020 remove reverence to =GuiController
    04.04.2021 - LoadGroup.GetComponent<PSlider>() -> LoadGroup.GetComponentInChildren<PSlider>()
    12.04.2021 - fix beaty load
    29.04.2021 - add control object
    18.05.2021 - OnDestroy
    21.12.2023 - add public void ReLoadCurrentScene(bool withLoaderPopup)
	12.03.2024 - update loader 
*/

namespace Mkey
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        private PopUpsController LoadGroupPrefab;

        private float loadProgress;

        public static SceneLoader Instance;

        #region temp vars
        private GuiController mGUI;
        private GuiController MGUI { get { if (!mGUI) mGUI = FindObjectOfType<GuiController>(); return mGUI; } }
        private PopUpsController LoadGroup;
        private PSlider simpleSlider;
        private bool loading = false;
        private AsyncOperation asyncLoad;
        #endregion temp vars

        #region regular
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); }
            else
            {
                Instance = this;
            }
        }
        private void OnDestroy()
        {
            if (loading && asyncLoad != null && !asyncLoad.allowSceneActivation)
            {
                asyncLoad.allowSceneActivation = true;
            }
        }
        #endregion regular

        public void LoadScene(int scene)
        {
            StartCoroutine(AsyncLoadBeaty(scene, null, null));
        }

        public void LoadScene(int scene, Action completeCallBack)
        {
            StartCoroutine(AsyncLoadBeaty(scene, null, completeCallBack));
        }

        public void LoadScene(int scene, Action<float> progresUpdate, Action completeCallBack)
        {
            StartCoroutine(AsyncLoadBeaty(scene, progresUpdate, completeCallBack));
        }

        public void LoadScene(string sceneName)
        {
            int scene = SceneManager.GetSceneByName(sceneName).buildIndex;
            StartCoroutine(AsyncLoadBeaty(scene, null, null));
        }

        public void ReLoadCurrentScene()
        {
            int scene = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(AsyncLoadBeaty(scene, null, null));
        }

        public void ReLoadCurrentScene(bool withLoaderPopup)
        {
            int scene = SceneManager.GetActiveScene().buildIndex;
            if (withLoaderPopup)
            {
                StartCoroutine(AsyncLoadBeaty(scene, null, null));
            }
            else
            {
                SceneManager.LoadScene(scene);
            }
        }

        private IEnumerator AsyncLoadBeaty(int scene, Action<float> progresUpdate, Action completeCallBack)
        {
            GameObject loadController = new GameObject("LoadController");
            loading = true;
            float apprLoadTime = 0.25f;
            float apprLoadTimeMin = 0.2f;
            float apprLoadTimeMax = 3f;

            float steps = 25f;
            float iStep = 1f / steps;
            float loadTime = 0.0f;
            loadProgress = 0;
            bool fin = false; // check fade in

            if (LoadGroupPrefab)
            {
                LoadGroup = MGUI.ShowPopUp(LoadGroupPrefab);
                if (LoadGroup) simpleSlider = LoadGroup.GetComponentInChildren<PSlider>();
                if (simpleSlider) simpleSlider.SetFillAmount(loadProgress);
                yield return new WaitForSeconds(0.2f);
                fin = true;
            }
            else
            {
                fin = true;
            }

            asyncLoad = SceneManager.LoadSceneAsync(scene);
            asyncLoad.allowSceneActivation = false;
            float lastTime = Time.time;

            while (loadProgress < 0.99f || asyncLoad.progress < 0.90f)
            {
                loadTime += (Time.time - lastTime);
                lastTime = Time.time;
                loadProgress = Mathf.Clamp01(loadProgress + iStep);
                if (simpleSlider) simpleSlider.SetFillAmount(loadProgress);

                if (loadTime >= 0.5f * apprLoadTime && (asyncLoad.progress < 0.5f))
                {
                    apprLoadTime *= 1.1f;
                    apprLoadTime = Mathf.Min(apprLoadTimeMax, apprLoadTime);
                }
                else if (loadTime >= 0.5f * apprLoadTime && (asyncLoad.progress > 0.5f))
                {
                    apprLoadTime /= 1.1f;
                    apprLoadTime = Mathf.Max(apprLoadTimeMin, apprLoadTime);
                }

                progresUpdate?.Invoke(loadProgress);
                // Debug.Log("waite scene: " + loadTime + "; ao.progress : " + ao.progress + " ;loadProgress" + loadProgress);
                yield return new WaitForSeconds(apprLoadTime / steps);
            }
           
            yield return new WaitForSeconds(1f);
            Debug.Log("------------->SceneActivation -  Load time: " + (loadTime));
            asyncLoad.allowSceneActivation = true;
            yield return new WaitWhile(() => { return loadController; }); // wait while gameobject exist
            if (LoadGroup) LoadGroup.CloseWindow();
            completeCallBack?.Invoke();
            loading = false;
        }

        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public static int GetCurrentSceneBuildIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }
    }
}