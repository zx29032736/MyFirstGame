using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using PlayFab.ClientModels;
using PlayFab;
using PlayFab.Json;
using UnityEngine.Events;

public class FloatingCharactersConrtroller : MonoBehaviour {

    public CharacterTeamController teamController;
    public CharacterDetailController chaDetail;

    public DialogCanvasController.CharacterFilters activeFilter;
    private Action<string> callbackAfterUse;

    private Dictionary<string, FG_CharacterData> characterData = new Dictionary<string, FG_CharacterData>();
    private List<CharacterResult> characterToDisplay = new List<CharacterResult>();

    public CharacterDisplayItem selectedCharacter;
    public CharacterMemberSlot selectedMember;

    public const int MAX_ROW = 6;
    private int maxStorageCount = 50;

    public Transform prefabsParent;
    public List<CharacterDisplayItem> slots;
    List<CharacterDisplayItem> activedSlots;

    public void Init(Action<string> callback = null, DialogCanvasController.CharacterFilters filter = DialogCanvasController.CharacterFilters.AllCharacter)
    {
        teamController.Init();
        activedSlots = new List<CharacterDisplayItem>();
        chaDetail.gameObject.SetActive(false);
        
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        this.activeFilter = filter;

        if (callback != null)
        {
            this.callbackAfterUse = callback;
        }

        this.characterToDisplay = PF_PlayerData.playerCharacters;
        this.characterData = PF_PlayerData.playerCharacterData;

        //if (PF_PlayerData.playerCharacters != null)
        //{

        //    //this.Currencies.Init(PF_PlayerData.virtualCurrency);
        //}
        //else
        //{
        //    return;
        //}

        int count = 0;
        foreach (var kvp in this.characterToDisplay)
        {
            bool addCharacter = false;
            if(filter == DialogCanvasController.CharacterFilters.Fire)
            {
                if (string.Equals(characterData[kvp.CharacterId].Type, filter.ToString()))
                {
                    addCharacter = true;
                }
                else
                    continue;
            }
            if (filter == DialogCanvasController.CharacterFilters.Grass)
            {
                if (string.Equals(characterData[kvp.CharacterId].Type, filter.ToString()))
                {
                    addCharacter = true;
                }
                else
                    continue;
            }
            else if (filter == DialogCanvasController.CharacterFilters.AllCharacter)
            {
                addCharacter = true;
            }

            if (addCharacter == true)
            {
                Sprite icon = GameController.Instance.iconManager.GetIconById(characterData[kvp.CharacterId].ClassDetails.Icon);
                slots[count].gameObject.SetActive(true);
                slots[count].Init();
                slots[count].SetButton(characterData[kvp.CharacterId], kvp, icon);
                activedSlots.Add(slots[count]);
                count++;
            }
        }
        this.gameObject.SetActive(true);
    }

    public void OnCharacterClicked(CharacterDisplayItem cha)
    {
        selectedCharacter = cha;

        if (chaDetail != null && !teamController.isChangingMember)
        {
            chaDetail.Init(cha);
        }
        else if (teamController.isChangingMember)
        {
            if (CheckIsSameCharacterInTeam(cha.saved.baseClass.CatalogCode))
            {
                Debug.LogError("same character is in team already");
                PF_Bridge.RaiseCallbackError("same character is in the team already", PlayFabAPIMethods.Generic, MessageDisplayStyle.context);
                teamController.Refresh();
                return;
            }

            if (string.IsNullOrEmpty(selectedMember.myId.text))
            {
                PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].Add(cha.saved.characterDetails.CharacterId);
            }
            else
            {
                int index = PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].FindIndex(x => x.Contains(selectedMember.myId.text));
                PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][index] = cha.saved.characterDetails.CharacterId;
            }
            UpdateTeam();
            teamController.Refresh();
        }
    }

    public void OnChangeTeamMember(CharacterMemberSlot selectedSlot)
    {
        if (!teamController.isChangingMember)
        {
            selectedMember = selectedSlot;
            teamController.isChangingMember = true;
            teamController.leaveTeamBtn.interactable = true;

            // find character itself and interact btn
            foreach(var id in PF_PlayerData.MyTeamsCharacterId["CurrentTeam"])
            {
                CharacterDisplayItem slot = activedSlots.Find(x => x.saved.characterDetails.CharacterId.Contains(id));
                if (slot != null)
                {
                    slot.myBtn.interactable = false;
                }
            }
            // change slot state
            foreach (var go in activedSlots)
            {
                if(go.saved != null)
                {
                    go.OnMemberChanging();
                }
            }
        }
        else if(teamController.isChangingMember)
        {
            if (string.IsNullOrEmpty(selectedSlot.myId.text))
            {
                teamController.Refresh();
                return;
            }
            //to do : Swap order
            int index1 = PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].FindIndex(x => x.Contains(selectedMember.myId.text));
            int index2 = PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].FindIndex(x => x.Contains(selectedSlot.myId.text));
            string temp = PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][index1];
            PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][index1] = selectedSlot.myId.text;
            PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][index2] = temp;
            UpdateTeam();
            teamController.Refresh();
        }
    }

    public void OnCancelChangeMember()
    {
        teamController.leaveTeamBtn.interactable = false;

        foreach (var go in slots)
        {
            go.OnCancelChangeMember();
            go.myBtn.interactable = true;
        }

    }

    public void RemoveMember()
    {
        if (PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].Count > 1)
        {
            PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].Remove(selectedMember.myId.text);
            UpdateTeam();
            teamController.Refresh();
        }
        else
        {
            Debug.Log(" this is the last character , it cant be removed");
        }
    }

    void UpdateTeam()
    {
        string jsonToUpdate = PlayFabSimpleJson.SerializeObject(PF_PlayerData.MyTeamsCharacterId);
        PF_PlayerData.UpdateUserData(new Dictionary<string, string>() { { "Teams", jsonToUpdate } }, "Public", (UpdateUserDataResult) => { Debug.Log(" teams updated"); });
    }

    bool CheckIsSameCharacterInTeam(string chaName)
    {
        foreach(var go in teamController.characterTeamMembers)
        {
            if (go.myName.Contains(chaName))
            {
                return true;
            }
        }
        return false;
    }

    public void Type_Fire_Filter()
    {
        Init(null, DialogCanvasController.CharacterFilters.Fire);
    }
    public void Type_Grass_Filter()
    {
        Init(null, DialogCanvasController.CharacterFilters.Grass);
    }
    public void Type_All_Filter()
    {
        Init(null, DialogCanvasController.CharacterFilters.AllCharacter);
    }

    public void CloseCharacterPanel()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_LEAVE_SOUND_EFFECT);
        // get this to close down and also close the tint.	
        // get a confirmation here
        this.gameObject.SetActive(false);
    }
}
