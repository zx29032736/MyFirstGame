using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using PlayFab.ClientModels;
using PlayFab;

public class PF_Authentication {

    private static string _playFabPlayerIdCache;

    // used for device ID
    public static string android_id = string.Empty; // device ID to use with PlayFab login
    public static string ios_id = string.Empty; // device ID to use with PlayFab login
    public static string custom_id = string.Empty; // custom id for other platforms

    private const string emailPattern = @"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

    //tracked actions
    public static bool isLoggedOut = false;
    //public static bool hasLoggedInOnce = false;

    /* Communication is diseminated across these 4 events */
    //called after a successful login 
    public delegate void SuccessfulLoginHandler(string details, MessageDisplayStyle style);
    public static event SuccessfulLoginHandler OnLoginSuccess;

    //called after a login error or when logging out
    public delegate void FailedLoginHandler(string details, MessageDisplayStyle style);
    public static event FailedLoginHandler OnLoginFail;

    /// <summary>
	/// Informs lisenters when a successful login occurs
	/// </summary>
	/// <param name="details">Details - general string for additional details </param>
	/// <param name="style">Style - controls how the message should be handled </param>
	public static void RaiseLoginSuccessEvent(string details, MessageDisplayStyle style)
    {
        if (OnLoginSuccess != null)
        {
            OnLoginSuccess(details, style);
        }
    }

    /// <summary>
    /// Informs lisenters when a successful login occurs
    /// </summary>
    /// <param name="details">Details - general string for additional details </param>
    /// <param name="style">Style - controls how the message should be handled </param>
    public static void RaiseLoginFailedEvent(string details, MessageDisplayStyle style)
    {
        if (OnLoginFail != null)
        {
            OnLoginFail(details, style);
        }
    }

    #region PlayFab API calls
    /// <summary>
	/// Logins the with device identifier (iOS & Android only).
	/// </summary>
	public static void LoginWithDeviceId(bool createAcount, UnityAction errCallback)
    {
        Action<bool> processResponse = (bool response) => {
            if (response == true && GetDeviceId())
            {
                if (!string.IsNullOrEmpty(android_id))
                {
                    Debug.Log("Using Android Device ID: " + android_id);
                    LoginWithAndroidDeviceIDRequest request = new LoginWithAndroidDeviceIDRequest();
                    request.AndroidDeviceId = android_id;
                    request.TitleId = PlayFabSettings.TitleId;
                    request.CreateAccount = createAcount;

                    //DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GenericLogin);
                    PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginResult, (PlayFabError error) =>
                    {
                        if (errCallback != null && error.Error == PlayFabErrorCode.AccountNotFound)
                        {
                            errCallback();
                            PF_Bridge.RaiseCallbackError("Account not found, please select a login method to continue.", PlayFabAPIMethods.GenericLogin, MessageDisplayStyle.none);
                        }
                        else
                        {
                            OnLoginError(error);
                        }

                    });
                }
                else if (!string.IsNullOrEmpty(ios_id))
                {
                    Debug.Log("Using IOS Device ID: " + ios_id);
                    LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest();
                    request.DeviceId = ios_id;
                    request.TitleId = PlayFabSettings.TitleId;
                    request.CreateAccount = createAcount;

                    //DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GenericLogin);
                    PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginResult, (PlayFabError error) =>
                    {
                        if (errCallback != null && error.Error == PlayFabErrorCode.AccountNotFound)
                        {
                            errCallback();
                            PF_Bridge.RaiseCallbackError("Account not found, please select a login method to continue.", PlayFabAPIMethods.GenericLogin, MessageDisplayStyle.none);
                        }
                        else
                        {
                            OnLoginError(error);
                        }
                    });
                }
            }
            else
            {
                Debug.Log("Using custom device ID: " + custom_id);
                LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
                request.CustomId = custom_id;
                request.TitleId = PlayFabSettings.TitleId;
                request.CreateAccount = createAcount;

                //DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GenericLogin);
                PlayFabClientAPI.LoginWithCustomID(request, OnLoginResult, (PlayFabError error) =>
                {
                    if (errCallback != null && error.Error == PlayFabErrorCode.AccountNotFound)
                    {
                        errCallback();
                        PF_Bridge.RaiseCallbackError("Account not found, please select a login method to continue.", PlayFabAPIMethods.GenericLogin, MessageDisplayStyle.none);
                    }
                    else
                    {
                        OnLoginError(error);
                    }
                });
            }
        };

        processResponse(true);
        //DialogCanvasController.RequestConfirmationPrompt("Login With Device ID", "Logging in with device ID has some issue. Are you sure you want to contine?", processResponse);


    }

    /// <summary>
	/// Registers the new PlayFab account.
	/// </summary>
	public static void RegisterNewPlayfabAccount(string user, string pass1, string pass2, string email)
    {
        if (user.Length == 0 || pass1.Length == 0 || pass2.Length == 0 || email.Length == 0)
        {
            if (OnLoginFail != null)
            {
                OnLoginFail("All fields are required.", MessageDisplayStyle.error);
            }
            return;
        }

        bool passwordCheck = ValidatePassword(pass1, pass2);
        bool emailCheck = ValidateEmail(email);

        if (!passwordCheck)
        {
            if (OnLoginFail != null)
            {
                OnLoginFail("Passwords must match and be longer than 5 characters.", MessageDisplayStyle.error);
            }
            return;

        }
        else if (!emailCheck)
        {
            if (OnLoginFail != null)
            {
                OnLoginFail("Invalid Email format.", MessageDisplayStyle.error);
            }
            return;
        }
        else
        {
            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest();
            request.TitleId = PlayFabSettings.TitleId;
            request.DisplayName = user;
            request.Username = user;
            request.Email = email;
            request.Password = pass1;

            DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GenericLogin);
            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterResult, OnLoginError);
        }
    }

    /// <summary>
	/// Login with PlayFab username.
	/// </summary>
	/// <param name="user">Username to use</param>
	/// <param name="pass">Password to use</param>
	public static void LoginWithUsername(string user, string password)
    {
        if (user.Length > 0 && password.Length > 0)
        {
            //LoginMethodUsed = LoginPathways.pf_username;
            LoginWithPlayFabRequest request = new LoginWithPlayFabRequest();
            request.Username = user;
            request.Password = password;
            request.TitleId = PlayFabSettings.TitleId;


            DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GenericLogin);
            PlayFabClientAPI.LoginWithPlayFab(request, RequestPhotonToken, OnLoginError);
        }
        else
        {
            if (OnLoginFail != null)
            {
                OnLoginFail("User Name and Password cannot be blank.", MessageDisplayStyle.error);
            }
        }
    }

    /// <summary>
	/// Login using the email associated with a PlayFab account.
	/// </summary>
	/// <param name="user">User.</param>
	/// <param name="password">Password.</param>
	public static void LoginWithEmail(string user, string password)
    {
        if (user.Length > 0 && password.Length > 0 && ValidateEmail(user))
        {
            //LoginMethodUsed = LoginPathways.pf_email;
            LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest();
            request.Email = user;
            request.Password = password;
            request.TitleId = PlayFabSettings.TitleId;

            DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GenericLogin);
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginResult, OnLoginError);

        }
        else
        {
            if (OnLoginFail != null)
            {
                OnLoginFail("Username or Password is invalid. Check credentails and try again", MessageDisplayStyle.error);
            }
        }

    }

    /// <summary>
	/// Gets the device identifier and updates the static variables
	/// </summary>
	/// <returns><c>true</c>, if device identifier was obtained, <c>false</c> otherwise.</returns>
	public static bool GetDeviceId(bool silent = false) // silent suppresses the error
    {
        if (CheckForSupportedMobilePlatform())
        {
#if UNITY_ANDROID
			//http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
			AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
			AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
			android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
#endif

#if UNITY_IPHONE
			ios_id = UnityEngine.iOS.Device.vendorIdentifier;
#endif
            return true;
        }
        else
        {
            custom_id = SystemInfo.deviceUniqueIdentifier;
            //LoginWithCustomIDRequest
            //			if(OnLoginFail != null && silent == false)
            //			{
            //				OnLoginFail("Must be using android or ios platforms to use deveice id.", MessageDisplayStyle.error);
            //			}
            return false;
        }
    }

    /// <summary>
    /// Check to see if our current platform is supported (iOS & Android)
    /// </summary>
    /// <returns><c>true</c>, for supported mobile platform, <c>false</c> otherwise.</returns>
    public static bool CheckForSupportedMobilePlatform()
    {
        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    public static void Logout()
    {
        if (OnLoginFail != null)
        {
            OnLoginFail("Logout", MessageDisplayStyle.none);
        }
        android_id = string.Empty;
        ios_id = string.Empty;
        custom_id = string.Empty;

        //if (FB.IsInitialized == true && FB.IsLoggedIn == true)
        //{
           // CallFBLogout();
        //}

        //TODO maybe not OK to delete all, but if it works out this is easy
        // hack, manually deleteing keys to work across android devices.
        PlayerPrefs.DeleteKey("LastDeviceIdUsed");
        PlayerPrefs.DeleteKey("TitleId");



        PF_Authentication.isLoggedOut = true;

        //TODO make sure the delay here is long enough to shut down the active game systems
        SceneController.Instance.RequestSceneChange(SceneController.GameScenes.Authentication, .333f);
    }

    /// <summary>
    /// Called on a successful login attempt
    /// </summary>
    /// <param name="result">Result object returned from PlayFab server</param>
    private static void OnLoginResult(LoginResult result) //LoginResult
    {

        //PlayFabSettings.PlayerId = result.PlayFabId; //modifying
        //PF_PlayerData.PlayerId = result.PlayFabId; //modifying

#if UNITY_ANDROID && !UNITY_EDITOR
		string senderID = "295754311231";
		//PlayFabGoogleCloudMessaging._RegistrationReadyCallback += AccountStatusController.OnGCMReady;
		//PlayFabGoogleCloudMessaging._RegistrationCallback += AccountStatusController.OnGCMRegistration;
		//PlayFabAndroidPlugin.Init(senderID);
#endif

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.isEditor)
        {
            //if (FB.IsInitialized == false)
            //{
                //FB.Init(OnInitComplete, OnHideUnity);
               // FB.Init(null);
            //}

            //if (PF_Authentication.usedManualFacebook == true)
            //{
                //LinkDeviceId();
                //PF_Authentication.usedManualFacebook = false;
            //}
        }

        Debug.Log("Session Ticket: " + result.SessionTicket);

        PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.GenericLogin, MessageDisplayStyle.none);
        if (OnLoginSuccess != null)
        {
            OnLoginSuccess(string.Format("SUCCESS: {0}", result.SessionTicket), MessageDisplayStyle.error);
        }
    }

    /// <summary>
    /// Raises the login error event.
    /// </summary>
    /// <param name="error">Error.</param>
    private static void OnLoginError(PlayFabError error) //PlayFabError
    {
        string errorMessage = string.Empty;
        if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Password"))
        {
            errorMessage = "Invalid Password";
        }
        else if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Username") || (error.Error == PlayFabErrorCode.InvalidUsername))
        {
            errorMessage = "Invalid Username";
        }
        else if (error.Error == PlayFabErrorCode.AccountNotFound)
        {
            errorMessage = "Account Not Found, you must have a linked PlayFab account. Start by registering a new account or using your device id";
        }
        else if (error.Error == PlayFabErrorCode.AccountBanned)
        {
            errorMessage = "Account Banned";
        }
        else if (error.Error == PlayFabErrorCode.InvalidUsernameOrPassword)
        {
            errorMessage = "Invalid Username or Password";
        }
        else
        {
            errorMessage = string.Format("Error {0}: {1}", error.HttpCode, error.ErrorMessage);
        }

        if (OnLoginFail != null)
        {
            OnLoginFail(errorMessage, MessageDisplayStyle.error);
        }

        // reset these IDs (a hack for properly detecting if a device is claimed or not, we will have an API call for this soon)
        //PlayFabLoginCalls.android_id = string.Empty;
        //PlayFabLoginCalls.ios_id = string.Empty;

        //clear the token if we had a fb login fail
        //if (FB.IsLoggedIn)
        //{
            //CallFBLogout();
        //}
    }
    #endregion

    private static void RequestPhotonToken(LoginResult obj)
    {
        LogMessage("PlayFab authenticated. Requesting photon token...");

        //We can player PlayFabId. This will come in handy during next step
        _playFabPlayerIdCache = obj.PlayFabId;

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppID
        }, AuthenticateWithPhoton, OnPlayFabError);

        Debug.Log("Session Ticket: " + obj.SessionTicket);
    }

    /*
     * Step 3
     * This is the final and the simplest step. We create new AuthenticationValues instance.
     * This class describes how to authenticate a players inside Photon environment.
     */
    private static void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        LogMessage("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

        //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
        var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

        //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
        customAuth.AddAuthParameter("username", _playFabPlayerIdCache);    // expected by PlayFab custom auth service

        //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
        customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

        //We finally tell Photon to use this authentication parameters throughout the entire application.
        PhotonNetwork.AuthValues = customAuth;
        //
        PhotonNetwork.AuthValues.UserId = _playFabPlayerIdCache;
        //
        //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.GenericLogin, MessageDisplayStyle.none);
        if (OnLoginSuccess != null)
        {
            OnLoginSuccess(string.Format("SUCCESS: {0}", "result.SessionTicket"), MessageDisplayStyle.error);
        }

        Debug.Log("AuthenticateWithPhoton");
    }

    #region pf_callbacks

    /// <summary>
    /// Called on a successful registration result
    /// </summary>
    /// <param name="result">Result object returned from the PlayFab server</param>
    private static void OnRegisterResult(RegisterPlayFabUserResult result)
    {
        if (OnLoginSuccess != null)
        {
            OnLoginSuccess("New Account Registered", MessageDisplayStyle.none);
        }
    }

    private static void OnPlayFabError(PlayFabError obj)
    {
        LogMessage(obj.GenerateErrorReport());
    }

    public static void LogMessage(string message)
    {
        Debug.Log("PlayFab + Photon Example: " + message);
    }

    #endregion

    #region helperfunctions
    /// <summary>
    /// Validates the email.
    /// </summary>
    /// <returns><c>true</c>, if email was validated, <c>false</c> otherwise.</returns>
    /// <param name="em">Email address</param>
    public static bool ValidateEmail(string em)
    {
        return Regex.IsMatch(em, PF_Authentication.emailPattern);
    }

    /// <summary>
    /// Validates the password.
    /// </summary>
    /// <returns><c>true</c>, if password was validated, <c>false</c> otherwise.</returns>
    /// <param name="p1">P1, text from password field one</param>
    /// <param name="p2">P2, text from password field two</param>
    public static bool ValidatePassword(string p1, string p2)
    {
        return ((p1 == p2) && p1.Length > 5);
    }

    #endregion
}
