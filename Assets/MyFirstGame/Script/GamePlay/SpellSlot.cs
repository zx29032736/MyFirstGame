using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlot : MonoBehaviour {

    public GamePlayActionBar actionBar;
    public FG_Spell mySpell;
    public Button btn;
    public Text text;

    public bool isLocked;
    public bool isOnCD;
    private Dictionary<string,int> cdTurns = new Dictionary<string, int>();

    public void Init(string spellName, FG_Spell spell, GamePlayActionBar bar)
    {
        actionBar = bar;
        AddSpell(spell);
        text.text = spellName;

        if (!cdTurns.ContainsKey(actionBar.controlledPlayer.name))
            cdTurns.Add(actionBar.controlledPlayer.name, 0);

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { actionBar.OnSpellSlotClicked(this); });
    }

    void AddSpell(FG_Spell spell = null,string spellName = null)
    {
        /*
        if (PF_GameData.Spells.ContainsKey(spellName))
        {
            FG_SpellDetail spellData = PF_GameData.Spells[spellName];
            mySpell = new FG_Spell()
            {
                SpellName = spellName,
                Target = spellData.Target,
                ApplyStatus = spellData.ApplyStatus,
                Cooldown = spellData.Cooldown,
                Description = spellData.Description,
                Dmg = spellData.BaseDmg,
                Icon = spellData.Icon,
                FX = spellData.FX,
                RangeX = spellData.RangeX,
                RangeY = spellData.RangeY,
                LevelReq = spellData.LevelReq,
            };

            //Debug.LogError(string.Format("character Level : {0} | Spell Level : {1} ", actionBar.controlledPlayer.savedCharacter.characterData.CharacterLevel, mySpell.LevelReq));

        }

        btn.interactable = false;
        */
        mySpell = spell;
        btn.interactable = false;
    }

    public void CheckInterAct()
    {
        if (mySpell == null)
            return;

        if (actionBar.controlledPlayer.savedCharacter.characterData.CharacterLevel < mySpell.LevelReq)
        {
            Lock();
        }
        else
        {
            UnLock();
        }

        DeterminCdTurns();
    }

    void Lock()
    {
        this.btn.interactable = false;
        this.isLocked = true;
        //lock icon ; tint
    }

    void UnLock()
    {
        this.btn.interactable = true;
        this.isLocked = false;
    }

    public void EnableCD(int turns)
    {
        this.btn.interactable = false;
        this.isOnCD = true;
        this.cdTurns[actionBar.controlledPlayer.name] = turns;
    }

    void DisableCD()
    {
        this.btn.interactable = true;
        this.isOnCD = false;
    }

    public void DeterminCdTurns()
    {
        if(this.cdTurns[actionBar.controlledPlayer.name] > 0)
        {
            this.cdTurns[actionBar.controlledPlayer.name]--;
        }

        if(this.cdTurns[actionBar.controlledPlayer.name] <= 0 && this.isOnCD == true)
        {
            DisableCD();
            GetComponentInChildren<Text>().text = string.Format("{0}", mySpell.SpellName);
        }
        else if(this.isOnCD == true)
        {
            GetComponentInChildren<Text>().text = string.Format("{0} {1}", mySpell.SpellName, cdTurns[actionBar.controlledPlayer.name]);
            EnableCD(cdTurns[actionBar.controlledPlayer.name]);
        }
    }
}
