using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTeamController : MonoBehaviour {

    public FloatingCharactersConrtroller characterController;
    public List<CharacterMemberSlot> characterTeamMembers;
    public Button leaveTeamBtn;

    public bool isChangingMember = false;

    public void Init()
    {
        isChangingMember = false;

        foreach (var go in characterTeamMembers)
        {
            go.myBtn.onClick.RemoveAllListeners();
            go.myBtn.onClick.AddListener(() => 
            {
                if (!isChangingMember)
                    go.myImage.color = Color.blue;

                characterController.OnChangeTeamMember(go);
            });
        }

        leaveTeamBtn.onClick.RemoveAllListeners();
        leaveTeamBtn.onClick.AddListener(() => { characterController.RemoveMember(); });

        Refresh();
    }

    public void Refresh()
    {
        isChangingMember = false;

        foreach(var go in characterTeamMembers)
        {
            go.myImage.color = Color.white;
            go.SetButton("","", GameController.Instance.iconManager.defaultIcon);
        }

        for (int i = 0; i < PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].Count; i++)
        {
            characterTeamMembers[i].myBtn.interactable = true;
            string iconStr = PF_PlayerData.playerCharacterData[PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][i]].ClassDetails.Icon;
            string ChaName = PF_PlayerData.playerCharacterData[PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][i]].ClassDetails.CatalogCode;
            characterTeamMembers[i].SetButton(ChaName, PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][i], GameController.Instance.iconManager.GetIconById(iconStr));
        }
        characterController.OnCancelChangeMember();
    }
}
