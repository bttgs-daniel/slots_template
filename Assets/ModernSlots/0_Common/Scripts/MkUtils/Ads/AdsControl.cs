// #define ADDGADS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if ADDGADS
    using GoogleMobileAds.Api;
#endif

#if UNITY_EDITOR
    using UnityEditor;
#endif

/*
  test https://developers.google.com/admob/unity/test-ads
  Banner 	        ca-app-pub-3940256099942544/6300978111
  Interstitial 	    ca-app-pub-3940256099942544/1033173712
  Rewarded Video 	ca-app-pub-3940256099942544/5224354917
  Native Advanced 	ca-app-pub-3940256099942544/2247696110

  21.01.2020 
    - first release
    09.03.2020 
        - change  NOADS -> ADDGADS – symbol (from project settings)
    08.04.2020
        -destroy banner
    15.04.2020
        show banner, hide banner
    23.05.2020
        serialize ads ids
    14.07.2020 
        - fix showrewardedad (noads), add editor
    03.10.2021 
        - fix 6.1 admob issues, 
    05.10.2021
        - check interstitial != null
    13.05.2023
        - fix 8.0 admob issues
    12.02.2024
        - fix interstitial reload
	14.06.2024
        - fix 9.0 admob issues
	27.09.2024
        - get test reward in editor 
 */

namespace Mkey
{
    public class AdsControl : MonoBehaviour
    {
        private bool customBannerRequest = false;
        private bool customInterstitialRequest = false;
        private bool customRewardedAdRequest = false;

        [Header("Banner")]
#if ADDGADS
        [SerializeField]
        private bool requestBanner = true;
        [SerializeField]
        private AdPosition bannerPosition = AdPosition.Bottom;
#endif
        [SerializeField]
        private string bannerAdUnitIdAndroid = "ca-app-pub-3940256099942544/6300978111"; // test
        [SerializeField]
        private string bannerAdUnitIdIos = "ca-app-pub-3940256099942544/2934735716"; // test

        [Header("Interstitial")]
#if ADDGADS
        [SerializeField]
        private bool requestInterstitial = true;
#endif
        [SerializeField]
        private string interstitialAdUnitIdAndroid = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField]
        private string interstitialAdUnitIdIos = "ca-app-pub-3940256099942544/4411468910";

        [Header("Rewarded ads")]
#if ADDGADS
        [SerializeField]
        private bool requestRewardedAds = true;
#endif
        [SerializeField]
        private List<RewardAd> rewardAds;

#if ADDGADS
        [Space(16)]
        [SerializeField]
        private bool showTestGui = false;

        #region temp vars
        private InterstitialAd interstitial;
        private BannerView bannerView;
        private RewardedAd rewardedAd;
        private float deltaTime = 0.0f;
        private Action<bool, string, double> rewardCallBack; // <well, args.type or message, args.amount>
        private Action rewardedOpenedCallBack; // MSound.SetSound(false); 
        private Action rewardedClosedCallBack; //  MSound.SetSound(true);
        private Action interstitialOpenedCallBack; // MSound.SetSound(false); 
        private Action interstitialClosedCallBack; // MSound.SetSound(true);
        private static string outputMessage = string.Empty;
        #endregion temp vars

        public static AdsControl Instance;

        #region regular
        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            MobileAds.SetiOSAppPauseOnBackground(true);

            MobileAds.Initialize(initStatus => { }); // new

            if (requestRewardedAds) CreateAndLoadRewardedAd();

            if (requestInterstitial) RequestInterstitial();

            if (requestBanner) RequestBanner();
        }

        private void Update()
        {
            // Calculate simple moving average for time to render screen. 0.1 factor used as smoothing value.
            deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
        }

        private void OnGUI() // https://github.com/googleads/googleads-mobile-unity/blob/master/samples/HelloWorld/Assets/Scripts/GoogleMobileAdsDemoScript.cs
        {
            if (!showTestGui) return;
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            style.alignment = TextAnchor.LowerRight;
            style.fontSize = (int)(Screen.height * 0.06);
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            float fps = 1.0f / this.deltaTime;
            string text = string.Format("{0:0.} fps", fps);
            GUI.Label(rect, text, style);

            // Puts some basic buttons onto the screen.
            GUI.skin.button.fontSize = (int)(0.035f * Screen.width);
            float buttonWidth = 0.35f * Screen.width;
            float buttonHeight = 0.15f * Screen.height;
            float columnOnePosition = 0.1f * Screen.width;
            float columnTwoPosition = 0.55f * Screen.width;

            Rect requestBannerRect = new Rect(
                columnOnePosition,
                0.05f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(requestBannerRect, "Request\nBanner"))
            {
                this.RequestBanner();
            }

            Rect destroyBannerRect = new Rect(
                columnOnePosition,
                0.225f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(destroyBannerRect, "Destroy\nBanner"))
            {
                this.bannerView.Destroy();
            }

            Rect requestInterstitialRect = new Rect(
                columnOnePosition,
                0.4f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(requestInterstitialRect, "Request\nInterstitial"))
            {
                this.RequestInterstitial();
            }

            Rect showInterstitialRect = new Rect(
                columnOnePosition,
                0.575f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(showInterstitialRect, "Show\nInterstitial"))
            {
                this.ShowInterstitial(null, null);
            }

            Rect destroyInterstitialRect = new Rect(
                columnOnePosition,
                0.75f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(destroyInterstitialRect, "Destroy\nInterstitial"))
            {
                this.interstitial.Destroy();
            }

            Rect requestRewardedRect = new Rect(
                columnTwoPosition,
                0.05f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(requestRewardedRect, "Request\nRewarded Ad"))
            {
                this.CreateAndLoadRewardedAd();
            }

            Rect showRewardedRect = new Rect(
                columnTwoPosition,
                0.225f * Screen.height,
                buttonWidth,
                buttonHeight);
            if (GUI.Button(showRewardedRect, "Show\nRewarded Ad"))
            {
                if (rewardAds != null && rewardAds.Count > 0 && rewardAds[0] != null)
                {
                    this.ShowRewardedAd(rewardAds[0].Name, null, null, null);
                }
            }

            Rect textOutputRect = new Rect(
                columnTwoPosition,
                0.925f * Screen.height,
                buttonWidth,
                0.05f * Screen.height);
            GUI.Label(textOutputRect, outputMessage);
        }

        private void OnDestroy()
        {
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            if (bannerView != null)
            {
                bannerView.Destroy();
            }
        }
        #endregion regular

        #region banner
        public void RequestBanner(AdPosition bannerPosition)
        {
            this.bannerPosition = bannerPosition;
            RequestBanner();
        }

        public void RequestBanner()
        {
#if UNITY_ANDROID
            string adUnitId = bannerAdUnitIdAndroid;
#elif UNITY_IPHONE
        string adUnitId = bannerAdUnitIdIos;
#else
        string adUnitId ="unexpected_platform";
#endif
            // Clean up banner ad before creating a new one.
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }

            bannerView = new BannerView(adUnitId, AdSize.Banner, bannerPosition);

            // create our request used to load the ad.
            AdRequest adRequest = new AdRequest();// Builder().Build();
            adRequest.Keywords.Add("unity-admob-sample");
            // send the request to load the ad.
            Debug.Log("Loading banner ad.");
            bannerView.LoadAd(adRequest);

    // Raised when an ad is loaded into the banner view.
    bannerView.OnBannerAdLoaded += () =>
    {
        Debug.Log("Banner view loaded an ad with response : "
            + bannerView.GetResponseInfo());
    };
    // Raised when an ad fails to load into the banner view.
    bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
    {
        Debug.LogError("Banner view failed to load an ad with error : "
            + error);
    };
    // Raised when the ad is estimated to have earned money.
    bannerView.OnAdPaid += (AdValue adValue) =>
    {
        Debug.Log(String.Format("Banner view paid {0} {1}.",
            adValue.Value,
            adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    bannerView.OnAdImpressionRecorded += () =>
    {
        Debug.Log("Banner view recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    bannerView.OnAdClicked += () =>
    {
        Debug.Log("Banner view was clicked.");
    };
    // Raised when an ad opened full screen content.
    bannerView.OnAdFullScreenContentOpened += () =>
    {
        Debug.Log("Banner view full screen content opened.");
    };
    // Raised when the ad closed full screen content.
    bannerView.OnAdFullScreenContentClosed += () =>
    {
        Debug.Log("Banner view full screen content closed.");
    };

        }

        public void HideBanner()
        {
            if (bannerView != null) bannerView.Hide();
        }

        public void ShowBanner()
        {
            if (bannerView != null) bannerView.Show();
        }
        #endregion banner

        #region interstitial
        private void RequestInterstitial()
        {
#if UNITY_ANDROID
            string adUnitId = interstitialAdUnitIdAndroid;
#elif UNITY_IPHONE
            string adUnitId = interstitialAdUnitIdIos;
#else
            string adUnitId = "unexpected_platform";
#endif
           // Clean up the old ad before loading a new one.
      if (interstitial != null)
      {
            interstitial.Destroy();
            interstitial = null;
      }

      Debug.Log("Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest(); //Builder().AddKeyword("unity-admob-sample").Build();

      // send the request to load the ad.
      InterstitialAd.Load(adUnitId, adRequest,
          (InterstitialAd ad, LoadAdError error) =>
          {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
              {
                  Debug.LogError("interstitial ad failed to load an ad " +
                                 "with error : " + error);
                  return;
              }

              Debug.Log("Interstitial ad loaded with response : "
                        + ad.GetResponseInfo());

              interstitial = ad;
          });
     
        }

      private void RegisterEventHandlers(InterstitialAd ad)
{
    // Raised when the ad is estimated to have earned money.
    ad.OnAdPaid += (AdValue adValue) =>
    {
        Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
            adValue.Value,
            adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    ad.OnAdImpressionRecorded += () =>
    {
        Debug.Log("Interstitial ad recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    ad.OnAdClicked += () =>
    {
        Debug.Log("Interstitial ad was clicked.");
    };
    // Raised when an ad opened full screen content.
    ad.OnAdFullScreenContentOpened += () =>
    {
        Debug.Log("Interstitial ad full screen content opened.");
    };
    // Raised when the ad closed full screen content.
    ad.OnAdFullScreenContentClosed += () =>
    {
        Debug.Log("Interstitial ad full screen content closed.");
    };
    // Raised when the ad failed to open full screen content.
    ad.OnAdFullScreenContentFailed += (AdError error) =>
    {
        Debug.LogError("Interstitial ad failed to open full screen content " +
                       "with error : " + error);
    };
}

        bool ireqStarted = false;
        private IEnumerator NewInterstitialRequest()
        {
            if (!ireqStarted)
            {
                ireqStarted = true;
                yield return new WaitForSeconds(3f);
                RequestInterstitial();
                ireqStarted = false;
            }
        }

        public void ShowInterstitial(Action interstitialOpenedCallBack, Action interstitialClosedCallBack)
        {
            this.interstitialOpenedCallBack = interstitialOpenedCallBack;
            this.interstitialClosedCallBack = interstitialClosedCallBack;
            if (interstitial != null && interstitial.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                interstitial.Show();
                StartCoroutine(NewInterstitialRequest());
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
                RequestInterstitial();
            }
        }
        #endregion interstitial

        #region rewarded ad
        private void CreateAndLoadRewardedAd()
        {
            if (rewardAds == null || rewardAds.Count == 0) return;
            foreach (var item in rewardAds)
            {
                if (item != null) item.CreateAndLoadRewardedAd();
            }
        }

        public void ShowRewardedAd(string adName, Action rewardedOpenedCallBack, Action rewardedClosedCallBack, Action<bool, string, double> rewardCallBack)
        {
            if (rewardAds == null || rewardAds.Count == 0) return;

            string defaultAdName = "default";
            if (string.IsNullOrEmpty(adName)) adName = defaultAdName;

            RewardAd rA = null;
            foreach (var item in rewardAds)
            {
                if (String.Equals(item.Name, adName, StringComparison.Ordinal))
                    rA = item;
            }

            if (rA == null && String.Equals(adName, defaultAdName, StringComparison.Ordinal))
            {
                rA = rewardAds[0];
            }

            if (rA == null)
            {
                Debug.Log("Rearded ad: " + adName + " not exist");
                return;
            }
            rA.ShowRewardedAd(rewardedOpenedCallBack, rewardedClosedCallBack, rewardCallBack);
        }
        #endregion rewarded ad

#else
        public static AdsControl Instance;

        #region regular
        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }
        #endregion regular

        public void ShowInterstitial(Action interstitialOpenedCallBack, Action interstitialClosedCallBack)
        {
            Debug.Log("ADD ADDGADS sripting symbol in project settings");
        }

        public void ShowRewardedAd(string adName, Action rewardedOpenedCallBack, Action rewardedClosedCallBack, Action<bool, string, double> rewardCallBack)
        {
            Debug.Log("ADD ADDGADS sripting symbol in project settings");
        }

        public void HideBanner()
        {
            Debug.Log("ADD ADDGADS sripting symbol in project settings");
        }

        public void ShowBanner()
        {
            Debug.Log("ADD ADDGADS sripting symbol in project settings");
        }
#endif
    }

    [Serializable]
    public class RewardAd
    {
        [SerializeField]
        private string name = "rewardedad";
        [SerializeField]
        private string adUnitIdAndroid = "ca-app-pub-3940256099942544/5224354917";  // test
        [SerializeField]
        private string adUnitIdIOS = "ca-app-pub-3940256099942544/1712485313";      // test

#if ADDGADS
        public string Name { get { return name; } }
        private Action<bool, string, double> rewardCallBack;    //  <well, args.type or message, args.amount>
        private Action rewardedOpenedCallBack;                  //  MSound.SetSound(false); 
        private Action rewardedClosedCallBack;                  //  MSound.SetSound(true);

        private RewardedAd rewardedAd;

        private RewardAd()
        {
            name = "rewardedad";
            adUnitIdAndroid = "ca-app-pub-3940256099942544/5224354917";
            adUnitIdIOS = "ca-app-pub-3940256099942544/5224354917";
        }

        private string adUnitId = "";

        public void CreateAndLoadRewardedAd() //https://developers.google.com/admob/unity/rewarded-ads
        {
#if UNITY_ANDROID
            adUnitId = adUnitIdAndroid;
#elif UNITY_IPHONE
             adUnitId = adUnitIdIOS;
#else
             adUnitId = "unexpected_platform";
#endif
            Debug.Log("RequestRewardBasedVideo (adUnitId): " + adUnitId);

            // Clean up the old ad before loading a new one.
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }


            // create our request used to load the ad.
            var adRequest = new AdRequest(); // Builder().Build();

            // send the request to load the ad.
            RewardedAd.Load(adUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    rewardedAd = ad;
                    RegisterEventHandlers(rewardedAd);
                    RegisterReloadHandler(ad);
                });

        }

        public void ShowRewardedAd(Action rewardedOpenedCallBack, Action rewardedClosedCallBack, Action<bool, string, double> rewardCallBack)
        {
            this.rewardedOpenedCallBack = rewardedOpenedCallBack;
            this.rewardedClosedCallBack = rewardedClosedCallBack;
            this.rewardCallBack = rewardCallBack;

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                Debug.Log("show loaded video");
                rewardedAd.Show((rew) => {

#if UNITY_EDITOR
                    // only for test in unity editor,  prevent double callback
                    Debug.Log("end show loaded test video");
                    rewardCallBack?.Invoke(true, "test reward receiwed", 1);
#endif
                });
            }
            else
            {
                Debug.Log("Rewarded ad: " + ToString() + " -  not loaded");
                rewardCallBack?.Invoke(false, "Rewarded ad: " + ToString() + " -  not loaded", 0);
            }
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
                rewardCallBack?.Invoke(true, adValue.CurrencyCode, adValue.Value);
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                rewardedOpenedCallBack?.Invoke();
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                rewardedClosedCallBack?.Invoke();
                Debug.Log("Rewarded ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        /// <summary>
        /// To prepare a rewarded ad for the next impression opportunity, preload the rewarded ad once the OnAdFullScreenContentClosed or OnAdFullScreenContentFailed ad event is raised.
        /// </summary>
        /// <param name="ad"></param>
        private void RegisterReloadHandler(RewardedAd ad)
        {
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded Ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                CreateAndLoadRewardedAd();
            };

            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                CreateAndLoadRewardedAd();
            };
        }

        public override string ToString()
        {
            return "rewarde ad name: " + name + ", (adUnitId): " + adUnitId;
        }
#else
        public string Name { get { return ""; } }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AdsControl))]
    public class AdsControlEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUIStyle sl = new GUIStyle(EditorStyles.label);
            sl.margin = new RectOffset(0,0,-15,-15);


            GUILayout.Space(16);
            GUILayout.Label("Android test ad units IDs ", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel("Banner:         ca-app-pub-3940256099942544/6300978111", sl);
            EditorGUILayout.SelectableLabel("Interstitial:   ca-app-pub-3940256099942544/1033173712", sl);
            EditorGUILayout.SelectableLabel("Rewarded Video: ca-app-pub-3940256099942544/5224354917", sl);
            GUILayout.Space(8);
            GUILayout.Label("IOS test ad units IDs ", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel("Banner:         ca-app-pub-3940256099942544/2934735716", sl);
            EditorGUILayout.SelectableLabel("Interstitial:   ca-app-pub-3940256099942544/4411468910", sl);
            EditorGUILayout.SelectableLabel("Rewarded Video: ca-app-pub-3940256099942544/1712485313", sl);


            GUILayout.Space(8);
            GUILayout.Label("Admob links:", EditorStyles.boldLabel);
            if (LinkLabel(new GUIContent("Get Started")))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/quick-start");
            }
            if (LinkLabel(new GUIContent("Banner Ads")))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/banner");
            }
            if (LinkLabel(new GUIContent("Interstitial Ads")))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/interstitial");
            }
            if (LinkLabel(new GUIContent("Rewarded Ads")))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/rewarded-ads");
            }
            if (LinkLabel(new GUIContent("Test Ads")))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/test-ads");
            }
        }

       private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, EditorStyles.linkLabel, options);

            Handles.BeginGUI();
            Handles.color = EditorStyles.linkLabel.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, label, EditorStyles.linkLabel);
        }
    }
#endif
}
/*
 * https://github.com/googleads/googleads-mobile-unity/blob/master/samples/HelloWorld/Assets/Scripts/GoogleMobileAdsDemoScript.cs
 * https://developers.google.com/admob/unity/interstitial
 * https://developers.google.com/admob/unity/banner
 * https://developers.google.com/admob/unity/rewarded-video
 */
