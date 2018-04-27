using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the login with PlayFab account fields  
/// </summary>
public class PlayFabLoginController : MonoBehaviour
{

    // login fields
    public Transform loginGroup;
    public InputField User;
    public InputField Password;
    public Button Login;

    //registration fields
    public Transform registerGroup;
    public InputField user;
    public InputField email;
    public InputField pass1;
    public InputField pass2;
    public Button register;

    bool isRegisterObjActived = false;

    // toggle button
    public Button createAccountBtn;

    void OnEnable()
    {
        this.Password.text = string.Empty;
        PF_Authentication.OnLoginSuccess += HandleCallbackSuccess;
    }

    void OnDisable()
    {
        PF_Authentication.OnLoginSuccess -= HandleCallbackSuccess;
    }
    // Use this for initialization
    void Start()
    {
        Login.onClick.AddListener(() => LogIn());
        register.onClick.AddListener(() => RegisterNewAccount());
        createAccountBtn.onClick.AddListener(() => ToggleDevRegisterFields());
    }

    public void LogIn()
    {
        //PF_Authentication.RequestSpinner();
        GameController.Instance.soundManager.PlaySound(Vector3.zero, "Click_Standard_00");

        if (User.text.Contains("@"))
        {
            if (PF_Authentication.ValidateEmail(User.text))
            {
                PF_Authentication.LoginWithEmail(User.text, Password.text);
                Debug.Log("User.text.Contains @");
            }
        }
        else
        {
            PF_Authentication.LoginWithUsername(User.text, Password.text);
        }
    }

    public void ToggleDevRegisterFields()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, "Click_Standard_02");
        if (!isRegisterObjActived)
        {
            isRegisterObjActived = true;
            //this.loginGroup.gameObject.SetActive(false);
            //this.registerGroup.gameObject.SetActive(true);
            this.createAccountBtn.GetComponentInChildren<Text>().text = GlobalStrings.LOGOUT_BTN_TXT;
        }
        else
        {
            isRegisterObjActived = false;
            //this.loginGroup.gameObject.SetActive(true);
            //this.registerGroup.gameObject.SetActive(false);
            this.createAccountBtn.GetComponentInChildren<Text>().text = GlobalStrings.CREATE_BTN_TXT;
        }
    }

    public void RegisterNewAccount()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, "Click_Standard_01");
        //PlayFabLoginCalls.RequestSpinner();
        PF_Authentication.RegisterNewPlayfabAccount(user.text, pass1.text, pass2.text, email.text);
    }

    void HandleCallbackSuccess(string message,MessageDisplayStyle mg = MessageDisplayStyle.none)
    {
        if (message.Contains("New Account Registered"))
        {
            Debug.Log("Account Created, logging in with new account.");

            //System.Collections.Generic.Dictionary<string, string> updates = new System.Collections.Generic.Dictionary<string, string>();
            //updates.Add("FireComponent", "1000");
            //updates.Add("WaterComponent", "1000");
            //updates.Add("GrassComponent", "1000");
            //updates.Add("DarkComponent", "1000");
            //updates.Add("LightComponent", "1000");
            FG_ResourceData updates = new FG_ResourceData() { FireComponent = 1000, WaterComponent = 1000, DarkComponent = 1000, GrassComponent = 1000, LightComponent = 1000 };
            System.Collections.Generic.Dictionary<string, string> update2 = new System.Collections.Generic.Dictionary<string, string>();
            update2.Add( "Component", PlayFab.Json.PlayFabSimpleJson.SerializeObject(updates) );

            UnityEngine.Events.UnityAction<PlayFab.ClientModels.UpdateUserDataResult> action = (PlayFab.ClientModels.UpdateUserDataResult s) => {
                PF_Authentication.LoginWithUsername(user.text, pass1.text);

            };

            PF_PlayerData.UpdateUserData(update2, "Public", action);
        }

    }

    public void SettingButton()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);
        DialogCanvasController.RequestSettingPrompt();
    }
}

