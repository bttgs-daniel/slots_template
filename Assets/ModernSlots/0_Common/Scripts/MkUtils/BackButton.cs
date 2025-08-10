using UnityEngine.SceneManagement;
using UnityEngine;

// android back button behavior
/*
 22.10.2020
 */
namespace Mkey
{
    public class BackButton : MonoBehaviour
    {
        [SerializeField]
        private bool defaultBehavior = false;

        public static BackButton Instance;

        void Awake()
        {
#if !UNITY_ANDROID
            Destroy(gameObject);
            return;
#endif
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            if(defaultBehavior)  Input.backButtonLeavesApp = true; // default behavior
        }

        void Update()
        {
            if (!defaultBehavior)
            {
                // Make sure user is on Android platform
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        if (SceneManager.GetActiveScene().buildIndex == 0)
                            Application.Quit();
                        else
                            SceneManager.LoadScene(0);
                    }
                }
            }
        }
    }
}