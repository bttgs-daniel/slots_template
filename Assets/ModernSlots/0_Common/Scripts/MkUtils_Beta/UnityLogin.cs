using System;
using System.Threading.Tasks;

#if ADDUAUTH
    using Unity.Services.Core;
    using Unity.Services.Authentication;
#endif
using UnityEngine;

/*
 Why is this necessary?
    We get a PlayerID:  A PlayerID (a random alphanumeric string of 28 characters): used to identify returning and new players on different devices and external providers.
    We can use this identifier to work with our own backend.
*/

/*
    https://docs.unity.com/ugs/en-us/manual/authentication/manual/overview
    You can authenticate your players with anonymous, platform-specific, or custom sign-in solutions, 
    making it easy for games with custom identity solutions to unlock the full power of UGS.
        Platform specific providers include Google Play Games, Apple Game Center, Steam, and console-specific logins. 
        Platform agnostic providers include Username & Password, Facebook, Unity Player Accounts, and OpenID Connect.
   

    To use Authentication, you need to:  (https://docs.unity.com/ugs/en-us/manual/authentication/manual/get-started)
    -Sign up for UGS.
    -Link your dashboard to a Unity Editor project.
    -Install the SDK in your game code.  (Authentication package)
    -Initialize the SDK. (see Start method)
        - Register authentication events
        - If no profile is provided in the initialization options, the value default is used.


    When a player signs in to your app, Unity Authentication generates the following tokens and ID: (https://docs.unity.com/ugs/en-us/manual/authentication/manual/how-authentication-works)
     - A PlayerID (a random alphanumeric string of 28 characters): used to identify returning and new players on different devices and external providers.
     - A session token: used to re-authenticate the user after the session expires.
     - An access token: used to identify and authenticate the player to other Unity services.


    Anonymous authentication#   (https://docs.unity.com/ugs/en-us/manual/authentication/manual/approaches-to-authentication)
        Anonymous authentication is a platform agnostic and frictionless way to implement player authentication, 
        similar to a guest sign-in. It doesn't require players to enter credentials or create a player profile.
        On sign in the service creates a new player ID and returns the associated session token, or signs in a 
        returning player. Refer to How to use Anonymous Sign-in and Sign in a cached player for more information.
        However, anonymous authentication is not portable across devices because there is no way to re-authenticate 
        the player from another device. To sign in to the same game with the same player profile from a different device, 
        players must use an external identity provider.


    Signing in#   (https://docs.unity.com/ugs/en-us/manual/authentication/manual/session-management)
        When a player successfully logs in to a device for the first time, the Unity Authentication service grants 
        a long-lived session token. Based on this session token it also grants an access token which is valid for 1 hour. 
        The access token is used to authenticate the player when calling other Unity services. Both tokens are stored on 
        the device using Unity Player Prefs and are associated with a player profile. 
    Signing out#
        Calling AuthenticationService.Instance.SignOut signs the player out by clearing the access token. 
        By default the session token is retained. 
    Clearing session tokens#
        By default the SignOut method will retain the session token. This allows the player to re-authenticate 
        without needing to sign in again. If a different player signs in using an external provider, then their 
        tokens will replace both the session and access token from the previous player.
        Only in specific circumstances it is required to explicitly clear the session token. For example when you need 
        to sign in as a new anonymous player, since SignInAnonymouslyAsync will always try to use the existing persisted session token. 
        In that scenario you need to either clear the current player's session token, or use a different profile. 

    Token expiry and refreshing# https://docs.unity.com/ugs/en-us/manual/authentication/manual/session-management#sign-in-a-cached-player 
        In order to keep the player authorized beyond the 1 hour validity period of a single access token, the Unity Authentication 
        service automatically attempts to periodically refresh the access token. If the refresh attempts fail before the expiration time, 
        the access token expires and raises the Expired event. Developers must handle this case and retry signing in for the player.
        Use method: private void CheckStates()


    Manage profiles#    (https://docs.unity.com/ugs/en-us/manual/authentication/manual/profile-management)
        Players can use profiles to sign in to multiple accounts on a single device. Profiles add a level of isolation 
        to the values saved to the PlayerPrefs. Profiles are not automatically persisted; it’s up to developers to determine how to manage them.
            Switch profiles#
            Players must be signed out to switch the current profile. If a player is not signed out, SwitchProfile throws an AuthenticationException.
            If an invalid name is used, SwitchProfile throws an AuthenticationException.
            To view the current profile, use AuthenticationService.Instance.Profile
            If no profile is provided in the initialization options, the value default is used.


 */

namespace Mkey
{
    public class UnityLogin : MonoBehaviour
    {
        public string playerId;

        public static UnityLogin Instance { get; private set; }

        #region regular
        void Awake()
        {
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }

        async void Start()
        {
            await InitializeAndSignIn();
#if ADDUAUTH
            Debug.Log("AuthenticationService.Instance.IsSignedIn: " + AuthenticationService.Instance.IsSignedIn);
            PlayerPrefsLog();
#endif
        }
        #endregion regular

        #region Initialize, Profile
        private async Task InitializeAndSignIn()
        {
#if ADDUAUTH
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                try
                {
                    await UnityServices.InitializeAsync();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (!IsInternetReachable())
            {
                Debug.Log("No internet connection");
                return;
            }

            await SignInAnonymously();

#else
            // just STUB
            await Task.Run(() => Debug.Log("Add ADDUAUTH - Unity Auth scripting symbol"));
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task InitializeUnityServices()
        {
#if ADDUAUTH
            var options = new InitializationOptions();
            options.SetProfile("test_profile");
            await UnityServices.InitializeAsync(options);
#else
            // just STUB
            await Task.Run(() => Debug.Log("Add ADDUAUTH - Unity Auth scripting symbol"));
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        private void SwitchProfile(string profile)
        {
#if ADDUAUTH
            try
            {
                AuthenticationService.Instance.SwitchProfile(profile);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            Debug.Log($"Current Profile: {AuthenticationService.Instance.Profile}");
            PlayerPrefsLog();
#endif
        }
        #endregion Initialize, Profile

        #region  sign in, out
#if ADDUAUTH
        /// <summary>
        /// Register authentication events and SignInAnonymously
        /// </summary>
        /// <returns></returns>
        private async Task SignInAnonymously()
        {
            // Register authentication events
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("PlayerID, signed in as: " + AuthenticationService.Instance.PlayerId);
                // Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

                playerId = AuthenticationService.Instance.PlayerId;
            };

            //You can listen to events to display custom messages
            AuthenticationService.Instance.SignInFailed += errorResponse =>
            {
                Debug.LogError($"Sign in anonymously failed with error code: {errorResponse.ErrorCode}");
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                Debug.Log("Player signed out.");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("Player session could not be refreshed and expired.");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void SimpleSignOut()
        {
            // The session token will remain but the player will not be authenticated
            AuthenticationService.Instance.SignOut();
        }

        /// <summary>
        /// Sign out. The session token will be deleted immediately, allowing for a new anonymous player to be created
        /// </summary>
        private void SignOutAndClearSession()
        {
            AuthenticationService.Instance.SignOut(true);
        }

        private void SignOutThenClearSession()
        {
            AuthenticationService.Instance.SignOut();
            // Do something else...
            // Now clear the session token to allow a new anonymous player to be created
            AuthenticationService.Instance.ClearSessionToken();
        }

        /// <summary>
        /// Check access token expiry
        /// </summary>
        private void CheckStates()
        {
            /*
              In order to keep the player authorized beyond the 1 hour validity period of a single access token, 
              the Unity Authentication service automatically attempts to periodically refresh the access token.
              If the refresh attempts fail before the expiration time, the access token expires and raises the Expired event. 
              Developers must handle this case and retry signing in for the player.
             */
            // this is true if the access token exists, but it can be expired or refreshing
            Debug.Log($"Is SignedIn: {AuthenticationService.Instance.IsSignedIn}");

            // this is true if the access token exists and is valid/has not expired
            Debug.Log($"Is Authorized: {AuthenticationService.Instance.IsAuthorized}");

            // this is true if the access token exists but has expired
            Debug.Log($"Is Expired: {AuthenticationService.Instance.IsExpired}");
        }

        /// <summary>
        /// https://docs.unity.com/ugs/en-us/manual/authentication/manual/session-management#sign-in-a-cached-player
        /// If the session token exists, then the SignInAnonymouslyAsync() method recovers the existing credentials of a player, 
        /// regardless of whether they signed in anonymously or through a platform account.
        /// </summary>
        /// <returns></returns>
        private async Task SignInCachedUserAsync()
        {
            // Check if a cached player already exists by checking if the session token exists
            if (!AuthenticationService.Instance.SessionTokenExists)
            {
                // if not, then do nothing
                return;
            }

            // Sign in Anonymously
            // This call will sign in the cached player.
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");

                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Log stored data on device (session token)
        /// </summary>
        private void PlayerPrefsLog()
        {
            var sessionToken = PlayerPrefs.GetString($"{Application.cloudProjectId}.{AuthenticationService.Instance.Profile}.unity.services.authentication.session_token");
            var playerPrefsMessageResult = string.IsNullOrEmpty(sessionToken) ? "No session token for this profile" : $"Session token: {sessionToken}";
            Debug.Log(playerPrefsMessageResult);
        }
#endif
        #endregion sign in

        public static bool IsInternetReachable()
        {
            // https://www.codespeedy.com/how-to-check-internet-connection-in-unity/
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Error. Please Check your internet connection!");
                return false;
            }
            else
            {
                Debug.Log("Connected to Network");
                return true;
            }
        }
    }
}
