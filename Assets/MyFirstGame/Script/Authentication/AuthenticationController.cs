
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using PlayFab;


public class AuthenticationController : MonoBehaviour {

    public bool useDevLogin = false;
    public Transform developerLogin;
    public Image BackGround;

    public Text DeviceIdDisplay;

    //unused
    public Text Banner;
    public Text Status;
    public Text debugs;
    public Transform PlayIcon;
    //unused

    #region Monobehaviour Methods
    void OnEnable()
    {
        PF_Authentication.OnLoginFail += HandleOnLoginFail;
        PF_Authentication.OnLoginSuccess += HandleOnLoginSuccess;
    }

    void OnDisable()
    {
        PF_Authentication.OnLoginFail -= HandleOnLoginFail;
        PF_Authentication.OnLoginSuccess -= HandleOnLoginSuccess;
    }
    // Use this for initialization
    void Start()
    {
        if (PlayerPrefs.HasKey("TitleId"))
        {
            PlayFabSettings.TitleId = PlayerPrefs.GetString("TitleId");
        }
        else
        {
            PlayFabSettings.TitleId = GlobalStrings.FG_TITLE_ID;
        }

        if (this.useDevLogin == true)
        {
            EnableDeveloperMode();
        }
        else if (PF_Authentication.isLoggedOut == true)
        {
            EnableDeveloperMode();
        }

        if (PF_Authentication.GetDeviceId())
        {
            DeviceIdDisplay.text = "Devide ID :" + PF_Authentication.android_id == null ? PF_Authentication.ios_id : PF_Authentication.android_id;
        }
        else
        {
            DeviceIdDisplay.text = "Devide ID :" + PF_Authentication.custom_id;
        }
    }
    #endregion

    #region UI Control
    public void EnableDeveloperMode()
    {
        this.developerLogin.gameObject.SetActive(true);
        this.Banner.text = GlobalStrings.TEST_LOGIN_PROMPT;
    }

    public void DisableDeveloperMode()
    {
        this.developerLogin.gameObject.SetActive(false);
        this.Banner.text = "";
    }

    #endregion

    #region start login methods & callbacks
    void HandleOnLoginSuccess(string message, MessageDisplayStyle style)
    {
        debugs.text = message;
        if (message.Contains("SUCCESS"))
        {
            Debug.Log("login success ! ready  to load scene");
            SceneController.Instance.RequestSceneChange(SceneController.GameScenes.Profile);
        }
    }

    void HandleOnLoginFail(string message, MessageDisplayStyle style)
    {
        Debug.Log(message);
//#if UNITY_ANDRIOD || UNITY_IPHONE
        //EnableUserSelectMode();
//#else
        EnableDeveloperMode();
//#endif

    }
#endregion
}
