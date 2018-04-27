using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using PlayFab.ClientModels;

public class ProfileController : MonoBehaviour {

    public CharacterTeamUIController teamUiController;
    public ComponentDisplay goldDisplay;

    public GamePlayUIController gamePlayUiController;
    public CharacterDrawerController characterDrawerController;
    //public Image chaDisplay;
    public AudioClip pressButton;

    bool isAccountInfoLoaded = false;
    bool isCharacterDataLoaded = false;
    bool isUserDataUpdated = false;

    public void OpenStore()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);

        DialogCanvasController.RequestStore("Profile Store");
    }

    public void OpenInventory()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);

        DialogCanvasController.RequestInventoryPrompt(null, DialogCanvasController.InventoryFilters.AllItems);
    }

    public void OpenChaPanel()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);

        DialogCanvasController.RequestCharacterPrompt(null, DialogCanvasController.CharacterFilters.AllCharacter);
    }

    public void OpenGamePlayUI()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);

        gamePlayUiController.Init();
    }

    public void OpenCharacterDrawer()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);
        characterDrawerController.Init();
    }

    public void OpenSettingPanel()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_CLICK_SOUND_EFFECT);
        DialogCanvasController.RequestSettingPrompt();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            int rng = UnityEngine.Random.Range(0, PF_GameData.Classes.Count);
            PF_PlayerData.CreateNewCharacter(PF_GameData.Classes.ToList()[rng].Value.CatalogCode, PF_GameData.Classes.ToList()[rng].Value);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            PF_PlayerData.DrawCharactersToUser();
        }
    }

    void OnEnable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
    }

    void OnDisable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
    }

    void CheckToContinue()
    {
        if (isAccountInfoLoaded)
        {
            if (PF_PlayerData.virtualCurrency != null)
            {
                goldDisplay.Init(GameController.Instance.iconManager.GetIconById("Gold"), PF_PlayerData.virtualCurrency["NT"]);
            }
        }

        if (isCharacterDataLoaded && isAccountInfoLoaded)
        {
            teamUiController.Init();
            SetSavedTeamData();
            CheckIsNewPlayer();
            ResetCheck();
        }
    }

    private void HandleCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle)
    {
        switch (method)
        {
            case PlayFabAPIMethods.GetTitleData:
                break;

            case PlayFabAPIMethods.GetAccountInfo:
                isAccountInfoLoaded = true;
                break;

            case PlayFabAPIMethods.GetCharacterReadOnlyData:
                isCharacterDataLoaded = true;
                SetSavedTeamData();

                break;

            case PlayFabAPIMethods.UpdateUserData:
                isUserDataUpdated = true;
                SetSavedTeamData();
                teamUiController.Init();

                break;
            case PlayFabAPIMethods.GrantCharacterToUser:

                PF_PlayerData.GetPlayerCharacters( ()=> 
                {
                    PF_PlayerData.GetCharacterData( () => 
                    {
                        CheckIsNewPlayer();
                        
                    } );
                } );
                break;
        }

        CheckToContinue();
    }

    void SetSavedTeamData()
    {
        if (PF_PlayerData.MyTeamsCharacterId == null || PF_PlayerData.MyTeamsCharacterId.Count == 0)
            return;

        PF_PlayerData.SavedTeam = new List<FG_SavedCharacter>();
        PF_GamePlay.AllSavedCharacterUnitData = new Dictionary<string, FG_SavedCharacter>();
        int count = 1;
        foreach (var id in PF_PlayerData.MyTeamsCharacterId["CurrentTeam"])
        {
            CharacterResult cha = PF_PlayerData.playerCharacters.Find(x => x.CharacterId.Contains(id));
            FG_SavedCharacter saved = new FG_SavedCharacter()
            {
                characterDetails = cha,
                baseClass = PF_GameData.Classes[cha.CharacterType],
                characterData = PF_PlayerData.playerCharacterData[id]
            };
            PF_PlayerData.SavedTeam.Add(saved);
            PF_GamePlay.AllSavedCharacterUnitData.Add(saved.baseClass.CatalogCode + "_"+count, saved);
            count++;
        }
    }

    void CheckIsNewPlayer()
    {
        if(PF_PlayerData.SavedTeam == null)
        {
            if(PF_PlayerData.playerCharacters.Count != 0)
            {
                int rng = Random.Range(0, PF_PlayerData.playerCharacters.Count-1);
                PF_PlayerData.MyTeamsCharacterId = new Dictionary<string, List<string>>();
                PF_PlayerData.MyTeamsCharacterId.Add("CurrentTeam", new List<string>() { PF_PlayerData.playerCharacters[rng].CharacterId });
                string json = PlayFab.Json.PlayFabSimpleJson.SerializeObject(PF_PlayerData.MyTeamsCharacterId);
                PF_PlayerData.UpdateUserData(new Dictionary<string, string>() { { "Teams", json } });
            }

            if(PF_PlayerData.playerCharacters.Count == 0)
            {
                PF_PlayerData.CreateNewCharacter(PF_GameData.Classes.ToList()[0].Value.CatalogCode, PF_GameData.Classes.ToList()[0].Value);
            }
        }
    }

    void ResetCheck()
    {
        isCharacterDataLoaded = false;
        isAccountInfoLoaded = false;
    }

    public void ChangeCharacterDisplay(Sprite sp)
    {
        //chaDisplay.overrideSprite = sp;
    }
}
