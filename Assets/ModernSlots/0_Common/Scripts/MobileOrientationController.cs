using UnityEngine;
using System.Runtime.InteropServices;

namespace Mkey
{
    /// <summary>
    /// Controls screen orientation for mobile browsers and PWA
    /// Forces landscape orientation and handles orientation changes
    /// </summary>
    public class MobileOrientationController : MonoBehaviour
    {
        private static MobileOrientationController instance;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void RequestLandscapeOrientation();
        
        [DllImport("__Internal")]
        private static extern bool IsPortraitOrientation();
        
        [DllImport("__Internal")]
        private static extern void ShowOrientationWarning();
        
        [DllImport("__Internal")]
        private static extern void HideOrientationWarning();
#endif
        
        [Header("Orientation Settings")]
        [SerializeField] private bool forceLandscape = true;
        [SerializeField] private float orientationCheckInterval = 0.5f;
        
        [Header("Game Pause Settings")]
        [SerializeField] private bool pauseInPortrait = true;
        [SerializeField] private GameObject[] objectsToHideInPortrait;
        
        private float lastOrientationCheck;
        private bool isCurrentlyPortrait;
        private bool gamePausedByOrientation;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeOrientation();
        }
        
        private void InitializeOrientation()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (forceLandscape)
            {
                // Request landscape orientation on start
                RequestLandscapeOrientation();
            }
#endif
            
#if UNITY_ANDROID || UNITY_IOS
            // For native mobile builds
            if (forceLandscape)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
            }
#endif
        }
        
        private void Update()
        {
            if (!forceLandscape)
                return;
                
            // Check orientation periodically
            if (Time.time - lastOrientationCheck > orientationCheckInterval)
            {
                lastOrientationCheck = Time.time;
                CheckOrientation();
            }
        }
        
        private void CheckOrientation()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            bool isPortrait = IsPortraitOrientation();
            
            if (isPortrait != isCurrentlyPortrait)
            {
                isCurrentlyPortrait = isPortrait;
                OnOrientationChanged(isPortrait);
            }
#else
            // For editor testing or other platforms
            bool isPortrait = Screen.height > Screen.width;
            
            if (isPortrait != isCurrentlyPortrait)
            {
                isCurrentlyPortrait = isPortrait;
                OnOrientationChanged(isPortrait);
            }
#endif
        }
        
        private void OnOrientationChanged(bool isPortrait)
        {
            Debug.Log($"Orientation changed - Portrait: {isPortrait}");
            
            if (isPortrait)
            {
                HandlePortraitMode();
            }
            else
            {
                HandleLandscapeMode();
            }
        }
        
        private void HandlePortraitMode()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ShowOrientationWarning();
#endif
            
            if (pauseInPortrait && !gamePausedByOrientation)
            {
                gamePausedByOrientation = true;
                Time.timeScale = 0f;
                AudioListener.pause = true;
                
                // Hide specified objects
                if (objectsToHideInPortrait != null)
                {
                    foreach (var obj in objectsToHideInPortrait)
                    {
                        if (obj != null)
                            obj.SetActive(false);
                    }
                }
                
                // Send pause event to slot controller
                var slotController = FindObjectOfType<SlotController>();
                if (slotController != null)
                {
                    slotController.SendMessage("OnApplicationPause", true, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        
        private void HandleLandscapeMode()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            HideOrientationWarning();
#endif
            
            if (gamePausedByOrientation)
            {
                gamePausedByOrientation = false;
                Time.timeScale = 1f;
                AudioListener.pause = false;
                
                // Show hidden objects
                if (objectsToHideInPortrait != null)
                {
                    foreach (var obj in objectsToHideInPortrait)
                    {
                        if (obj != null)
                            obj.SetActive(true);
                    }
                }
                
                // Send resume event to slot controller
                var slotController = FindObjectOfType<SlotController>();
                if (slotController != null)
                {
                    slotController.SendMessage("OnApplicationPause", false, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Handle app pause/resume
            if (!pauseStatus && forceLandscape)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                RequestLandscapeOrientation();
#endif
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // Handle app focus
            if (hasFocus && forceLandscape)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                RequestLandscapeOrientation();
#endif
            }
        }
        
        /// <summary>
        /// Public method to manually request landscape orientation
        /// </summary>
        public static void ForceLandscape()
        {
            if (instance != null)
            {
                instance.InitializeOrientation();
            }
        }
        
        /// <summary>
        /// Check if currently in portrait mode
        /// </summary>
        public static bool IsInPortraitMode()
        {
            if (instance != null)
            {
                return instance.isCurrentlyPortrait;
            }
            return false;
        }
    }
}