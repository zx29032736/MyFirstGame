using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;

public class CharacterDisplayItem : MonoBehaviour {

    public Button myBtn;
    public Image icon;

    public FloatingCharactersConrtroller controller;

    public FG_SavedCharacter saved = null;

    public void Init()
    {
        myBtn.onClick.RemoveAllListeners();

        myBtn.onClick.AddListener
        (
             () =>
             {
                 this.controller.OnCharacterClicked(this);
             }
            
        );
    }

    public void SetButton(FG_CharacterData data, CharacterResult cha ,Sprite sprite)
    {
        icon.sprite = sprite;
        myBtn.GetComponentInChildren<Text>().text = data.ClassDetails.CatalogCode;

        //Fill slot 
        if (PF_GameData.Classes.ContainsKey(cha.CharacterType))
        {
            saved = (new FG_SavedCharacter()
            {
                baseClass = PF_GameData.Classes[cha.CharacterType],
                characterDetails = cha,
                characterData = PF_PlayerData.playerCharacterData.ContainsKey(cha.CharacterId) ? PF_PlayerData.playerCharacterData[cha.CharacterId] : null
            });
        }   
    }

    public void OnMemberChanging()
    {
        icon.color = Color.red;
    }

    public void OnCancelChangeMember()
    {
        icon.color = Color.white;
    }
}
