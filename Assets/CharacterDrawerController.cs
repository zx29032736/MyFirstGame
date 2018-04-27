using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDrawerController : MonoBehaviour {

    public Button drawButton;
    public Image grantedCharacter;
    public Text debugText;
    public Text vcText;

    public EasyTween uiAnimation;

    private void OnEnable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess += HandlePlayfabCallbackSuccess;
    }
    private void OnDisable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess -= HandlePlayfabCallbackSuccess;

    }
    private void HandlePlayfabCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle)
    {
        if(method == PlayFabAPIMethods.DrawCharacterToUser)
        {
            ShowGrantedCharacterImage(details);
            debugText.text = details;
            PF_PlayerData.GetUserAccountInfo();
        }
        if(method == PlayFabAPIMethods.GetAccountInfo)
        {
            vcText.text = PF_PlayerData.virtualCurrency["NT"].ToString();
            CanPlayerDraw();
        }
    }
    public void Init()
    {
        gameObject.SetActive(true);
        CanPlayerDraw();
        vcText.text = PF_PlayerData.virtualCurrency["NT"].ToString();
    }

    public void DrawCard()
    {
        PF_PlayerData.DrawCharactersToUser();
        drawButton.interactable = false;
    }

    void ShowGrantedCharacterImage(string grantedName)
    {
        grantedCharacter.overrideSprite = GameController.Instance.iconManager.GetIconById(grantedName + "_Card");
        //grantedCharacter.gameObject.SetActive(true);
        uiAnimation.OpenCloseObjectAnimation();
    }

    public void CanPlayerDraw()
    {
        if(PF_PlayerData.virtualCurrency["NT"] >= 100)
        {
            drawButton.interactable = true;
        }
        else
        {
            drawButton.interactable = false;
        }
    }

    public void Leave()
    {
        gameObject.SetActive(false);
    }
}
