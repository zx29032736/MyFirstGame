using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePlayActionBar : MonoBehaviour {

    public Unit controlledPlayer;
    public UIManager uiManager;
    public Button endBtn;
    public List<SpellSlot> spells = new List<SpellSlot>();
    public SpellSlot selectedSpellSlot;

    private void Awake()
    {
        endBtn.onClick.RemoveAllListeners();
        endBtn.onClick.AddListener(() => EndMoving());
    }

    public void Init(Unit controlled)
    {
        //GetComponent<CanvasGroup>().alpha = 1;

        endBtn.GetComponentInChildren<Text>().text = "End Moving";

        if(controlledPlayer != controlled)
        {
            controlledPlayer = controlled;

            //spells[0].Init(controlledPlayer.savedCharacter.characterData.Spell1, this);
            //spells[1].Init(controlledPlayer.savedCharacter.characterData.Spell2, this);
            //spells[2].Init(controlledPlayer.savedCharacter.characterData.Spell3, this);
            //spells[3].Init(controlledPlayer.savedCharacter.characterData.Spell4, this);
            spells[0].Init(controlledPlayer.savedCharacter.characterData.Spell1, controlledPlayer.mySpells[controlledPlayer.savedCharacter.characterData.Spell1], this);
            spells[1].Init(controlledPlayer.savedCharacter.characterData.Spell2, controlledPlayer.mySpells[controlledPlayer.savedCharacter.characterData.Spell2], this);
            spells[2].Init(controlledPlayer.savedCharacter.characterData.Spell3, controlledPlayer.mySpells[controlledPlayer.savedCharacter.characterData.Spell3], this);
            spells[3].Init(controlledPlayer.savedCharacter.characterData.Spell4, controlledPlayer.mySpells[controlledPlayer.savedCharacter.characterData.Spell4], this);

        }
        GetComponent<CanvasGroup>().alpha = 1;
        //gameObject.SetActive(true);
    }


    public void EndMoving()
    {
        //if (!PhotonNetwork.isMasterClient)
        //{
        //    if (controlledPlayer.moveState == Unit.MoveState.Move || controlledPlayer.moveState == Unit.MoveState.Init)
        //    {
        //        controlledPlayer.moveState = Unit.MoveState.End;
        //        //GamePlayManager.instances.SwitchState(GamePlayManager.States.MoveState);
        //    }
        //    else if (controlledPlayer.attState == Unit.AttackState.Init && controlledPlayer.moveState == Unit.MoveState.End)
        //    {
        //        controlledPlayer.attState = Unit.AttackState.End;
        //        //GamePlayManager.instances.SwitchState(GamePlayManager.States.AttackState);
        //    }
        //}
        uiManager.playManager.map.DisableTileMask();
        uiManager.playManager.DisableTargetMask();
        uiManager.playManager.OnEndMovingClicked();
    }

    public void EnableSkills()
    {
        endBtn.GetComponentInChildren<Text>().text = "End Attacking";

        foreach (var slot in spells)
            slot.CheckInterAct();

    }

    public void OnSpellSlotClicked(SpellSlot slot)
    {
        if(this.selectedSpellSlot != slot)
        {
            DisableSelectedSlot();
            this.selectedSpellSlot = slot;
        }
        //GamePlayManager.instances.ShowTargetBySpellRange(slot.spell);
        controlledPlayer.selectedSpell = slot.mySpell;//RandomSpell(slot.mySpell.SpellName);
        uiManager.playManager.ShowTargetBySpellRange(slot.mySpell);
    }
    public void DisableSelectedSlot()
    {
        uiManager.playManager.DisableTargetMask();
    }

    public void Close()
    {
        foreach (var go in spells)
            go.btn.interactable = false;

        GetComponent<CanvasGroup>().alpha = 0;
        //gameObject.SetActive(false);
    }

}
