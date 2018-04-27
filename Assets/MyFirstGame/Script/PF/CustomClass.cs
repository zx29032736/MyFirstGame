using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;

public class FG_ResourceData
{
    public int FireComponent { get; set; }
    public int WaterComponent { get; set; }
    public int GrassComponent { get; set; }
    public int DarkComponent { get; set; }
    public int LightComponent { get; set; }

    public FG_ResourceData() { }
}

public class FG_LevelData
{
    public string Description { get; set; }
    public string Icon { get; set; }
    public string StatsPrefix { get; set; }
    //public int MinEntryLevel { get; set; }
    //public bool IsLocked { get; set; }
    //public bool IsHidden { get; set; }
    public FG_LevelWinConditions WinConditions { get; set; }
    public Dictionary<string, FG_LevelAct> Acts { get; set; }

    public FG_LevelData() { }
}

public class FG_LevelAct
{
    public string Background { get; set; }
    public List<List<int>> PlayerSpawnPosition { get; set; }
    public Dictionary<string,List<int>> MapData { get; set; }
    public string IntroMonolog { get; set; }
    public string IntroBossMonolog { get; set; }
    public string OutroMonolog { get; set; }
    public string FailureMonolog { get; set; }
    //public FG_LevelActRewards RewardTable { get; set; }
    public bool IsActCompleted { get; set; }

    public FG_LevelEncounters Encounters { get; set; }

    public FG_LevelAct() { }
}

public class FG_LevelEncounters
{
    public float ChanceForAddedEncounters { get; set; }
    public List<string> SpawnSpecificEncountersByID { get; set; }
    public List<int> EncountersLevel { get; set; }
    public bool LimitProbabilityToAct { get; set; }
    public List<List<int>> EncounterSpawnPosition { get; set; }
    //ctor
    public FG_LevelEncounters() { }
}

public class FG_LevelWinConditions
{

}

[System.Serializable]
public class Equipment
{
    public string EquipmentName { get; set; }
    public string EquipmentId { get; set; }
    public Dictionary<string, string> stat { get; set; }
    public Equipment() { }
}

public class FG_CharacterData
{
    public FG_ClassDetail ClassDetails { get; set; }
    public int TotalExp { get; set; }
    public int ExpThisLevel { get; set; }

    public int Health { get; set; }
    public int Attack { get; set; }
    public int SpAttack { get; set; }
    public int Defense { get; set; }
    public int SpDefense { get; set; }
    public int Speed { get; set; }

    public int CharacterLevel { get; set; }

    public string Spell1 { get; set; }
    public string Spell2 { get; set; }
    public string Spell3 { get; set; }
    public string Spell4 { get; set; }

    public Equipment EquipedWeapon { get; set; }
    public Equipment EquipedArmor { get; set; }
    public Equipment EquipedJewelry { get; set; }

    public string Type { get; set; }
    //ctor
    public FG_CharacterData() { }
}

[System.Serializable]
public class FG_ClassDetail
{
    public string Description { get; set; }
    public string CatalogCode { get; set; }
    public string Icon { get; set; }
    public string Spell1 { get; set; }
    public string Spell2 { get; set; }
    public string Spell3 { get; set; }
    public string Spell4 { get; set; }

    public int BaseHP { get; set; }
    public int BaseAttack { get; set; }
    public int BaseSpAttack { get; set; }
    public int BaseDefense { get; set; }
    public int BaseSpDefense { get; set; }
    public int BaseSpeed { get; set; }

    public string Type { get; set; }
    //ctor
    public FG_ClassDetail() { }
}

[System.Serializable]
public class FG_SpellDetail
{
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Target { get; set; }
    public int BaseDmg { get; set; }
    public int ManaCost { get; set; }
    //public float UpgradePower { get; set; }
    //public int UpgradeLevels { get; set; }
    public string FX { get; set; }
    public int Cooldown { get; set; }
    public int LevelReq { get; set; }
    public List<int> RangeX { get; set; }
    public List<int> RangeY { get; set; }
    public FG_SpellStatus ApplyStatus { get; set; }

    //ctor
    public FG_SpellDetail() { }

    //copy ctor
    public FG_SpellDetail(FG_SpellDetail prius)
    {
        if (prius != null)
        {
            this.Description = prius.Description;
            this.Icon = prius.Icon;
            this.Target = prius.Target;
            this.BaseDmg = prius.BaseDmg;
            this.ManaCost = prius.ManaCost;
            this.BaseDmg = prius.BaseDmg;
            //this.UpgradePower = prius.UpgradePower;
            //this.UpgradeLevels = prius.UpgradeLevels;
            this.FX = prius.FX;
            this.Cooldown = prius.Cooldown;
            this.LevelReq = prius.LevelReq;
            this.ApplyStatus = new FG_SpellStatus(prius.ApplyStatus);
        }
    }
}

public enum StatModifierCode { HP, MP, Attack, SpAttack, Defense, SpDefense, Speed }
[System.Serializable]
public class FG_SpellStatus
{
    public string StatusName { get; set; }
    public string Target { get; set; }
    //public string UpgradeReq { get; set; }
    public string StatusDescription { get; set; }
    public string StatModifierCode { get; set; } // prbably need to map to an enum 
    public float ModifyAmount { get; set; }
    public float ChanceToApply { get; set; }
    public int Turns { get; set; }
    public string Icon { get; set; }
    public string FX { get; set; }

    //ctor
    public FG_SpellStatus() { }

    // copy ctor
    public FG_SpellStatus(FG_SpellStatus prius)
    {
        if (prius != null)
        {
            this.StatusName = prius.StatusName;
            this.Target = prius.Target;
            //this.UpgradeReq = prius.UpgradeReq;
            this.StatusDescription = prius.StatusDescription;
            this.StatModifierCode = prius.StatModifierCode;
            this.ModifyAmount = prius.ModifyAmount;
            this.ChanceToApply = prius.ChanceToApply;
            this.Turns = prius.Turns;
            this.Icon = prius.Icon;
            this.FX = prius.FX;
        }
    }
}

public class FG_Spell
{
    public string SpellName { get; set; }
    public string Target { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int Dmg { get; set; }
    public int Level { get; set; }
    //public int UpgradeLevels { get; set; }
    public string FX { get; set; }
    public int Cooldown { get; set; }
    public int LevelReq { get; set; }

    public List<int> RangeX { get; set; }
    public List<int> RangeY { get; set; }

    public FG_SpellStatus ApplyStatus { get; set; }

    //ctor
    public FG_Spell() { }
}

public class FG_SavedCharacter
{
    public FG_ClassDetail baseClass { get; set; }
    public PlayFab.ClientModels.CharacterResult characterDetails { get; set; }
    public FG_CharacterData characterData { get; set; }
    public PlayerVitals PlayerVitals { get; set; }

    public void SetMaxVitals()
    {
        this.PlayerVitals.MaxHealth = SetUpStat(characterData.CharacterLevel, this.baseClass.BaseHP, 0);
        this.PlayerVitals.MaxAttack = SetUpStat(characterData.CharacterLevel, this.baseClass.BaseAttack, 1);
        this.PlayerVitals.MaxSpAttack = SetUpStat(characterData.CharacterLevel, this.baseClass.BaseSpAttack, 2);
        this.PlayerVitals.MaxDefense = SetUpStat(characterData.CharacterLevel, this.baseClass.BaseDefense, 1);
        this.PlayerVitals.MaxSpDefense = SetUpStat(characterData.CharacterLevel, this.baseClass.BaseSpDefense, 2);
        this.PlayerVitals.MaxSpeed = SetUpStat(characterData.CharacterLevel, this.baseClass.BaseSpeed, 3);

        SetUpEquipAtt();

        //this.PlayerVitals.Health = this.characterData.Health;
        //this.PlayerVitals.Attack = this.characterData.Attack;
        //this.PlayerVitals.SpAttack = this.characterData.SpAttack;
        //this.PlayerVitals.Defense = this.characterData.Defense;
        //this.PlayerVitals.SpDefense = this.characterData.SpDefense;
        //this.PlayerVitals.Speed = this.characterData.Speed;

        this.PlayerVitals.ActiveStati.Clear();
        this.PlayerVitals.didLevelUp = false;
        this.PlayerVitals.skillSelected = 0;
        this.PlayerVitals.levelIncrease = 0;
    }

    public void RefillVitals()
    {
        this.PlayerVitals.ActiveStati.Clear();
        this.PlayerVitals.didLevelUp = false;
        this.PlayerVitals.skillSelected = 0;
        this.PlayerVitals.levelIncrease = 0;

        this.PlayerVitals.Health = this.PlayerVitals.MaxHealth;
        //this.PlayerVitals.Mana = this.PlayerVitals.MaxMana;
        this.PlayerVitals.Attack = this.PlayerVitals.MaxAttack;
        this.PlayerVitals.SpAttack = this.PlayerVitals.MaxSpAttack;
        this.PlayerVitals.SpDefense = this.PlayerVitals.MaxSpDefense;
        this.PlayerVitals.Speed = this.PlayerVitals.MaxSpeed;
        this.PlayerVitals.Defense = this.PlayerVitals.MaxDefense;


    }

    public void LevelUpCharacterStats()
    {
        //TODO add in this -- needs to have a level up table from title data
    }

    int SetUpStat(float level, float baseValue,int type)
    {
        var EV = 25;
        var IV = 100;
        var finalValue = 0;
        // 0 for health; 1 for normal; 2 for special; 3 = speed;
        if (type == 0)
        {
            finalValue = (int)(((2 * baseValue + IV + EV) * level / 100) + level + 10);
        }
        else if (type == 1)
        {
            finalValue = (int)((((2 * baseValue + IV + EV) * level / 100) + 5) * 1.1f);
        }
        else if (type == 2)
        {
            finalValue = (int)((((2 * baseValue + IV + EV) * level / 100) + 5) * 0.9f);
        }
        else
        {
            finalValue = (int)(((2 * baseValue + IV + EV) * level / 100) + 5);
        }

        return finalValue;

    }

    void SetUpEquipAtt()
    {
        for (int i = 0; i < 3; i++)
        {
            Equipment equip = null;
            if (i == 0)
                equip = characterData.EquipedWeapon;
            else if (i == 1)
                equip = characterData.EquipedArmor;
            else
                equip = characterData.EquipedJewelry;

            if (equip != null)
            {
                foreach (var stat in equip.stat)
                {
                    if (stat.Key.Contains("attack"))
                    {
                        //attackText.text += string.Format(" ({0} {1})", "+", stat.Value);
                        this.PlayerVitals.MaxAttack += int.Parse(stat.Value);
                    }
                    else if (stat.Key.Contains("spAttack"))
                    {
                        //spAttackText.text += string.Format(" ({0} {1})", "+", stat.Value);
                        this.PlayerVitals.MaxSpAttack += int.Parse(stat.Value);
                    }
                    else if (stat.Key.Contains("defense"))
                    {
                        //defenseText.text += string.Format(" ({0} {1})", "+", stat.Value);
                        this.PlayerVitals.MaxDefense += int.Parse(stat.Value);
                    }
                    else if (stat.Key.Contains("spDefense"))
                    {
                        //spDefenseText.text += string.Format(" ({0} {1})", "+", stat.Value);
                        this.PlayerVitals.MaxSpDefense += int.Parse(stat.Value);
                    }
                    else if (stat.Key.Contains("speed"))
                    {
                        //speedText.text += string.Format(" ({0} {1})", "+", stat.Value);
                        this.PlayerVitals.MaxSpeed += int.Parse(stat.Value);
                    }
                    else if (stat.Key.Contains("hp"))
                    {
                        //hpText.text += string.Format(" ({0} {1})", "+", stat.Value);
                        this.PlayerVitals.MaxHealth += int.Parse(stat.Value);
                    }
                }
            }
        }

    }

    //ctor
    public FG_SavedCharacter()
    {
        this.PlayerVitals = new PlayerVitals();
        this.PlayerVitals.ActiveStati = new List<FG_SpellStatus>();
        //TODO can initialize an ingame character tracker.
        //^^^ this will be what will need to get leveled up to match the stats
        //Debug.LogError("UB_SavedCharacter RAN!!!!");
    }

}

public class PlayerVitals
{
    public int Health { get; set; }
    //public int Mana { get; set; }
    public int Attack { get; set; }
    public int SpAttack { get; set; }
    public int SpDefense { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }

    public List<FG_SpellStatus> ActiveStati { get; set; }

    public int MaxHealth { get; set; }
    //public int MaxMana { get; set; }
    public int MaxAttack { get; set; }
    public int MaxSpAttack { get; set; }
    public int MaxSpDefense { get; set; }
    public int MaxDefense { get; set; }
    public int MaxSpeed { get; set; }

    public bool didLevelUp { get; set; }
    public int skillSelected { get; set; }
    public int levelIncrease { get; set; }

    //ctor
    public PlayerVitals() { }
}

[System.Serializable]
public class GameConditions
{
    public GamePlayManager.GameplayEvent State { get; set; }
    public string CurrentChaName { get; set; }
    public int MoveToTileX { get; set; }
    public int MoveToTileY { get; set; }
    public float RemainingMovement { get; set; }

    public string TargetChaName { get; set; }
    public string SelectedSpell { get; set; }
    public Dictionary<string, FG_SavedCharacter> AllSavedChaharacterData { get; set; }
    public Dictionary<string, CharacterPos> AllCharacterPos { get; set; }

    public GameConditions()
    {

    }

}

public class CharacterPos
{
    public int TileX { get; set; }
    public int TileY { get; set; }
}

public class QuestTracker
{
    public int XpCollected { get; set; }
    public int GoldCollected { get; set; }
    public List<string> ItemsFound { get; set; }
    public List<ItemGrantResult> ItemsGranted { get; set; }

    public bool isQuestWon = false;
    public bool areItemsAwarded = false;

    public QuestTracker()
    {
        this.ItemsFound = new List<string>();
    }
}

public class ItemGrantResult
{
    public string PlayFabId { get; set; }
    public string ItemId { get; set; }
    public string ItemInstanceId { get; set; }
    public bool Result { get; set; }

    //ctor
    public ItemGrantResult() { }
}

public class InventoryCategory
{
    public string itemId = string.Empty;
    public CatalogItem catalogRef;
    public List<ItemInstance> inventory;
    public Sprite icon;
    public bool isConsumable = false;
    public int totalUses = 0;
    public int count { get { return this.inventory.Count; } }
    public Dictionary<string, string> customData;

    //ctor
    public InventoryCategory() { }

    //ctor
    public InventoryCategory(string id, CatalogItem cat, List<ItemInstance> inv, Sprite icon, Dictionary<string, string> data)
    {
        this.itemId = id;
        this.catalogRef = cat;
        this.inventory = inv;
        this.icon = icon;
        this.customData = data;
    }

    //ctor
    public InventoryCategory(string id, CatalogItem cat, List<ItemInstance> inv, Sprite icon, bool consumable, Dictionary<string, string> data)
    {
        this.itemId = id;
        this.catalogRef = cat;
        this.inventory = inv;
        this.icon = icon;
        this.isConsumable = consumable;
        this.customData = data;
        CalcTotalUses();
    }

    public void CalcTotalUses()
    {
        if (this.isConsumable == true)
        {
            this.totalUses = 0;
            foreach (var item in inventory)
            {
                if (item.RemainingUses != null)
                {
                    totalUses += (int)item.RemainingUses;
                }
            }
        }
        else
        {
            totalUses = 0;
        }
    }
}