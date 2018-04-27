using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTeamUIController : MonoBehaviour {

    public List<Image> icons;
    public List<Image> fullImage;

    public void Init()
    {


        for(int i = 1; i < icons.Count; i++)
        {
            icons[i].transform.parent.gameObject.SetActive(false);
            fullImage[i].gameObject.SetActive(false);
        }

        if (PF_PlayerData.MyTeamsCharacterId == null || PF_PlayerData.MyTeamsCharacterId.Count == 0)
            return;

        for(int i = 0;i < PF_PlayerData.MyTeamsCharacterId["CurrentTeam"].Count; i++)
        {
            string iconID = PF_PlayerData.playerCharacterData[PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][i]].ClassDetails.Icon;
            string ImageID = PF_PlayerData.playerCharacterData[PF_PlayerData.MyTeamsCharacterId["CurrentTeam"][i]].ClassDetails.Icon;
            icons[i].overrideSprite = GameController.Instance.iconManager.GetIconById(iconID);
            fullImage[i].overrideSprite = GameController.Instance.iconManager.GetFullCharacterById(ImageID);

            icons[i].transform.parent.gameObject.SetActive(true);
            fullImage[i].gameObject.SetActive(true);
        }
    }
}
