using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpellActions : MonoBehaviour {

    public List<GameObject> FX_Resources;
    public static Dictionary<string, GameObject> FX_lookUp;//( fx_name, fx gameobject )
    Dictionary<string, List<GameObject>> spellFX_preload = new Dictionary<string, List<GameObject>>();//( fx_name, fx gameobject )
    Dictionary<string, List<GameObject>> fx_container = new Dictionary<string, List<GameObject>>();//( fx_name, fx gameobject )

    public List<AudioClip> audioClip_Resources;
    Dictionary<string, AudioClip> audio_lookup;

    private void Start()
    {
        FX_lookUp = new Dictionary<string, GameObject>();
        foreach (var go in FX_Resources)
        {
            string FXname = go.name;

            if (!FX_lookUp.ContainsKey(FXname))
                FX_lookUp.Add(FXname, go);
        }

        int cnt = 0;
        audio_lookup = new Dictionary<string, AudioClip>();
        foreach(var spell in PF_GameData.Spells)
        {
            if (cnt > audioClip_Resources.Count)
            {
                Debug.Log("cnt > audioClip_Resources.Count");
                return;
            }
            audio_lookup.Add(spell.Key, audioClip_Resources[cnt]);
            cnt++;
        }
    }

    public void SetupSpellFX(FG_Spell spell)
    {
        if (!spellFX_preload.ContainsKey(spell.SpellName))
        {

            if (!fx_container.ContainsKey(spell.FX))
            {
                fx_container.Add(spell.FX, new List<GameObject>() { Instantiate(FX_lookUp[spell.FX]) });
            }

            while(fx_container[spell.FX].Count < spell.RangeX.Count)
            {
                fx_container[spell.FX].Add(Instantiate(FX_lookUp[spell.FX]));
            }

            spellFX_preload.Add(spell.SpellName, new List<GameObject>());
            for (int i = 0; i < spell.RangeX.Count; i++)
            {
                fx_container[spell.FX][i].SetActive(false);
                spellFX_preload[spell.SpellName].Add(fx_container[spell.FX][i]);
            }
        }
    }

    public void ShowFX(Unit attacker, string FX_name = null, FG_Spell spell = null)
    {
        
        for(int i = 0; i < spell.RangeX.Count; i++)
        {

            spellFX_preload[spell.SpellName][i].transform.position = new Vector2(attacker.tileX + spell.RangeX[i], attacker.tileY + spell.RangeY[i]);
            spellFX_preload[spell.SpellName][i].SetActive(true);
        }
        if (audio_lookup.ContainsKey(spell.SpellName))
            GameController.Instance.soundManager.PlaySound(attacker.transform.position, audio_lookup[spell.SpellName]);
    }
    public void ShowFX(GameObject objectToEffect, FG_Spell spell = null)
    {

        if(spell != null && spellFX_preload.ContainsKey(spell.SpellName))
        {
            GameObject go = spellFX_preload[spell.SpellName][0];
            go.transform.position = objectToEffect.transform.position;
            go.SetActive(true);

            if (audio_lookup.ContainsKey(spell.SpellName))
                GameController.Instance.soundManager.PlaySound(objectToEffect.transform.position, audio_lookup[spell.SpellName]);
        }
        else
        {
            GameObject go = fx_container["CFX2_RockHit"][0];//Instantiate(FX_lookUp["CFX_Virus"]);
            go.transform.position = objectToEffect.transform.position;
            go.SetActive(true);

            //if (audio_lookup.ContainsKey(spell.SpellName))
                GameController.Instance.soundManager.PlaySound(objectToEffect.transform.position, audioClip_Resources[0]);
        }
    }

    public void TriggerStatus(Unit unit)
    {
        //onTurnStart|under attacking|onTurnEnd
        for (int i = 0; i < unit.savedCharacter.PlayerVitals.ActiveStati.Count; i++)
        {
            FG_SpellStatus sta = unit.savedCharacter.PlayerVitals.ActiveStati[i];

            sta.Turns--;
            Debug.Log(unit.name + " is under status effect and left turn is : " + sta.Turns);
            if (sta.Turns < 0)
            {
                if (sta.StatModifierCode.Contains("Attack"))
                {
                    unit.savedCharacter.PlayerVitals.Attack += (int)(unit.savedCharacter.PlayerVitals.MaxAttack * sta.ModifyAmount) * -1;
                }
                else if (sta.StatModifierCode.Contains("SpAttack"))
                {
                    unit.savedCharacter.PlayerVitals.SpAttack += (int)(unit.savedCharacter.PlayerVitals.MaxSpAttack * sta.ModifyAmount) * -1;
                }
                else if (sta.StatModifierCode.Contains("Defense"))
                {
                    unit.savedCharacter.PlayerVitals.Defense += (int)(unit.savedCharacter.PlayerVitals.MaxDefense * sta.ModifyAmount) * -1;
                }
                else if (sta.StatModifierCode.Contains("SpDefense"))
                {
                    unit.savedCharacter.PlayerVitals.SpDefense += (int)(unit.savedCharacter.PlayerVitals.MaxSpDefense * sta.ModifyAmount) * -1;
                }
                else if (sta.StatModifierCode.Contains("Speed"))
                {
                    unit.savedCharacter.PlayerVitals.Speed += (int)(unit.savedCharacter.PlayerVitals.MaxSpeed * sta.ModifyAmount) * -1;
                }

                Debug.Log(unit.name + " is break the status : " + sta.StatusName);
                unit.savedCharacter.PlayerVitals.ActiveStati.Remove(sta);

                if (sta.Target.Contains("Player"))
                {
                    unit.DisableBuff(true);
                }
                else if (sta.Target.Contains("Enemy"))
                {
                    unit.DisableBuff(false);
                }

            }
            else
            {
                if (sta.StatModifierCode.Contains("HP"))
                {
                    if (sta.ModifyAmount > 0)
                    {
                        unit.savedCharacter.PlayerVitals.Health += 20;
                        if (unit.savedCharacter.PlayerVitals.Health > unit.savedCharacter.PlayerVitals.MaxHealth)
                            unit.savedCharacter.PlayerVitals.Health = unit.savedCharacter.PlayerVitals.MaxHealth;
                    }
                    else
                    {
                        unit.TakeDamage(1);
                        ShowFX(unit.gameObject);
                    }
                }
            }

        }

    }
    //施放完技能直接觸發，只用在降低Stat方面，扣HP等回合開始時觸發。
    public void TriggerImmediately(Unit unit,int index)
    {
        FG_SpellStatus sta = unit.savedCharacter.PlayerVitals.ActiveStati[index];

        if (sta.StatModifierCode.Contains("Attack"))
        {
            unit.savedCharacter.PlayerVitals.Attack += (int)(unit.savedCharacter.PlayerVitals.MaxAttack * sta.ModifyAmount);
        }
        else if (sta.StatModifierCode.Contains("SpAttack"))
        {
            unit.savedCharacter.PlayerVitals.SpAttack += (int)(unit.savedCharacter.PlayerVitals.MaxSpAttack * sta.ModifyAmount);
        }
        else if (sta.StatModifierCode.Contains("Defense"))
        {
            unit.savedCharacter.PlayerVitals.Defense += (int)(unit.savedCharacter.PlayerVitals.MaxDefense * sta.ModifyAmount);
        }
        else if (sta.StatModifierCode.Contains("SpDefense"))
        {
            unit.savedCharacter.PlayerVitals.SpDefense += (int)(unit.savedCharacter.PlayerVitals.MaxSpDefense * sta.ModifyAmount);
        }
        else if (sta.StatModifierCode.Contains("Speed"))
        {
            unit.savedCharacter.PlayerVitals.Speed += (int)(unit.savedCharacter.PlayerVitals.MaxSpeed * sta.ModifyAmount);
        }

    }

    public GameObject RandomFX()
    {
        int rng = Random.Range(0, FX_Resources.Count);
        return FX_Resources[rng];
    }

    public int DmgCalculator(FG_SavedCharacter Attacker, FG_SavedCharacter target, FG_Spell spell)
    {
        int dmg = 0;
        int spellPower = spell.Dmg;
        int AtkPoint = Attacker.PlayerVitals.Attack;
        int DefPoint = target.PlayerVitals.Defense;

        if (spellPower <= 0)
            return 0;
        else
            dmg = (int)(0.5f * (float)spellPower * (float)AtkPoint / (float)DefPoint) + 1;

        Debug.Log(spellPower + "|" + AtkPoint + "|" + DefPoint + "|" + dmg);
        return dmg;
    }

    public IEnumerator AttackMovement(Unit attacker, Unit target, string FX_name = null)
    {
        if (target == null)
        {
            GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.TurnEnd);
            yield break;
        }

        target.GetComponent<SpriteRenderer>().color = Color.blue;

        if (attacker.tag.Contains("Player"))
        {
            target.TakeDamage(attacker.selectedSpell.Dmg);
        }
        else
        {
            target.TakeDamage(1);
        }

        yield return new WaitForSeconds(0.7f);

        target.GetComponent<SpriteRenderer>().color = Color.white;

        if (PhotonNetwork.isMasterClient)
            GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.TurnEnd);
    }
}
