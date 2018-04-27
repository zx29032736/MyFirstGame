using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CharacterMemberSlot : MonoBehaviour {

    public CharacterTeamController controller;
    public Image myImage;
    public Button myBtn;
    public Text myId;
    public string myName;

    public void SetButton( string chaNmae, string instanceId, Sprite sprite)
    {
        myName = chaNmae;
        myId.text = instanceId;
        myImage.overrideSprite = sprite;
    }
}
