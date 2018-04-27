using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab.Json;

public class CharacterDetailController : MonoBehaviour {

    public CharacterEquipController equipController;
    public Button closeBtn;

    public Image characterImage;
    //public Text characterStat;
#region stat text
    public Text nameText;
    public Text levelText;
    public Text expText;
    public Text hpText;
    public Text attackText;
    public Text spAttackText;
    public Text defenseText;
    public Text spDefenseText;
    public Text speedText;

    public Image skill1Img;
    public Image skill2Img;
    public Image skill3Img;
    public Image skill4Img;

    public Text skill1Text;
    public Text skill2Text;
    public Text skill3Text;
    public Text skill4Text;

    public Button equipedWeaponBtn;
    public Button equipedArmorBtn;
    public Button equipedJewelryBtn;

    public Text equipedWeaponText;
    public Text equipedArmorText;
    public Text equipedJewelryText;
    //
#endregion

    CharacterDisplayItem selectedSlot = null;

    private void Awake()
    {
        equipedWeaponBtn.onClick.RemoveAllListeners();
        equipedWeaponBtn.onClick.AddListener(() => equipController.Init(EquipmentType.Weapon));

        equipedArmorBtn.onClick.RemoveAllListeners();
        equipedArmorBtn.onClick.AddListener(() => equipController.Init(EquipmentType.Armor));

        equipedJewelryBtn.onClick.RemoveAllListeners();
        equipedJewelryBtn.onClick.AddListener(() => equipController.Init(EquipmentType.Jewelry));
    }

    private void OnEnable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess += OnPlayfabCallbackSuccess;
    }

    private void OnDisable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess -= OnPlayfabCallbackSuccess;
    }

    private void OnPlayfabCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle)
    {
        if(method == PlayFabAPIMethods.SavePlayerInfo)
        {
            //PF_PlayerData.GetCharacterDataById(selectedSlot.saved.characterDetails.CharacterId);
            PF_PlayerData.GetCharacterData();
        }
        else if(method == PlayFabAPIMethods.GetCharacterReadOnlyData)
        {
            RefreshData();

            equipController.Refresh();
        }
    }

    public void Init(CharacterDisplayItem selected)
    {
        selectedSlot = selected;
        SetUpCharacterStats();

        characterImage.sprite = GameController.Instance.iconManager.GetIconById(selected.saved.baseClass.Icon + "_Card");
        this.gameObject.SetActive(true);
    }

    public void CloseDetail()
    {
        this.gameObject.SetActive(false);
    }

    public void LevelUpCharacter()
    {
        if(selectedSlot != null)
        {
            QuestTracker quest = new QuestTracker { XpCollected = PF_GameData.CharacterLevelRamp[(selectedSlot.saved.characterData.CharacterLevel).ToString()] };
            EvaluateLevelUp(quest);
            PF_GamePlay.SavePlayerData(selectedSlot.saved, quest);
        }
    }

    public void RefreshData()
    {
        if(selectedSlot != null)
        {
            //FG_SavedCharacter saved = new FG_SavedCharacter()
            //{
            //    baseClass = PF_GameData.Classes[selectedSlot.saved.characterDetails.CharacterType],
            //    characterDetails = selectedSlot.saved.characterDetails,
            //    characterData = PF_PlayerData.playerCharacterData.ContainsKey(selectedSlot.character.CharacterId) ? PF_PlayerData.playerCharacterData[selectedSlot.character.CharacterId] : null
            //};
            //selectedSlot.saved = saved;
            //selectedSlot.my_data = saved.characterData;
            //Init(selectedSlot);
            selectedSlot.saved.characterData = PF_PlayerData.playerCharacterData.ContainsKey(selectedSlot.saved.characterDetails.CharacterId) ? PF_PlayerData.playerCharacterData[selectedSlot.saved.characterDetails.CharacterId] : null;
            selectedSlot.saved.RefillVitals();
            selectedSlot.saved.SetMaxVitals();
            //Init(selectedSlot);
            SetUpCharacterStats();
        }
    }

    void EvaluateLevelUp(QuestTracker exp)
    {
        string level = selectedSlot.saved.characterData.CharacterLevel.ToString();
        if (PF_GameData.CharacterLevelRamp[level] <= selectedSlot.saved.characterData.ExpThisLevel + exp.XpCollected)
        {
            selectedSlot.saved.characterData.TotalExp = selectedSlot.saved.characterData.ExpThisLevel;
            selectedSlot.saved.PlayerVitals.levelIncrease++;
        }
    }

    void SetUpCharacterStats()
    {
        totalAtk = 0; totalSpAtk = 0; totalDef = 0; totalSpDef = 0; totalSpd = 0; totalHp = 0;
        this.skill1Img.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.Spell1);
        this.skill1Text.text = selectedSlot.saved.characterData.Spell1;

        this.skill2Img.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.Spell2);
        this.skill2Text.text = selectedSlot.saved.characterData.Spell2;

        this.skill3Img.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.Spell3);
        this.skill3Text.text = selectedSlot.saved.characterData.Spell3;

        this.skill4Img.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.Spell4);
        this.skill4Text.text = selectedSlot.saved.characterData.Spell4;

        if (selectedSlot.saved.characterData.EquipedWeapon != null)
        {
            this.equipedWeaponBtn.image.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.EquipedWeapon.EquipmentName);
            this.equipedWeaponText.text = selectedSlot.saved.characterData.EquipedWeapon.EquipmentName;

            EquipmentIncreaseAtt(selectedSlot.saved.characterData.EquipedWeapon.EquipmentName);
        }
        else
        {
            this.equipedWeaponBtn.image.overrideSprite = null;
            this.equipedWeaponText.text = "null";
        }
        if(selectedSlot.saved.characterData.EquipedArmor != null)
        {
            this.equipedArmorBtn.image.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.EquipedArmor.EquipmentName);
            this.equipedArmorText.text = selectedSlot.saved.characterData.EquipedArmor.EquipmentName;
            EquipmentIncreaseAtt(selectedSlot.saved.characterData.EquipedArmor.EquipmentName);
        }
        else
        {
            this.equipedArmorBtn.image.overrideSprite = null;
            this.equipedArmorText.text = "null";
        }
        if (selectedSlot.saved.characterData.EquipedJewelry != null)
        {

            this.equipedJewelryBtn.image.overrideSprite = GameController.Instance.iconManager.GetIconById(selectedSlot.saved.characterData.EquipedJewelry.EquipmentName);
            this.equipedJewelryText.text = selectedSlot.saved.characterData.EquipedJewelry.EquipmentName;
            EquipmentIncreaseAtt(selectedSlot.saved.characterData.EquipedJewelry.EquipmentName);
        }
        else
        {
            this.equipedJewelryBtn.image.overrideSprite = null;
            this.equipedJewelryText.text = "null";
        }

        this.nameText.text = selectedSlot.saved.baseClass.CatalogCode;
        this.levelText.text = selectedSlot.saved.characterData.CharacterLevel.ToString();
        this.expText.text = selectedSlot.saved.characterData.ExpThisLevel + " / " + PF_GameData.CharacterLevelRamp[selectedSlot.saved.characterData.CharacterLevel.ToString()];
        this.hpText.text = string.Format("{0} + ({1})", selectedSlot.saved.characterData.Health.ToString(), totalHp);
        this.attackText.text = string.Format("{0} + ({1})", selectedSlot.saved.characterData.Attack.ToString(), totalAtk);
        this.spAttackText.text = string.Format("{0} + ({1})", selectedSlot.saved.characterData.SpAttack.ToString(),totalSpAtk);
        this.defenseText.text = string.Format("{0} + ({1})", selectedSlot.saved.characterData.Defense.ToString(),totalDef);
        this.spDefenseText.text = string.Format("{0} + ({1})", selectedSlot.saved.characterData.SpDefense.ToString(),totalSpDef);
        this.speedText.text = string.Format("{0} + ({1})", selectedSlot.saved.characterData.Speed.ToString(),totalSpd);


    }
    int totalAtk, totalSpAtk, totalDef, totalSpDef, totalSpd, totalHp;

    void EquipmentIncreaseAtt(string name)
    {

        foreach (var stat in PF_PlayerData.inventoryByCategory[name].customData)
        {
            if (stat.Key.Contains("attack"))
            {
                //attackText.text += string.Format(" ({0} {1})", "+", stat.Value);
                totalAtk += int.Parse(stat.Value);
            }
            else if (stat.Key.Contains("spAttack"))
            {
                //spAttackText.text += string.Format(" ({0} {1})", "+", stat.Value);
                totalSpAtk += int.Parse(stat.Value);
            }
            else if (stat.Key.Contains("defense"))
            {
                //defenseText.text += string.Format(" ({0} {1})", "+", stat.Value);
                totalDef += int.Parse(stat.Value);
            }
            else if (stat.Key.Contains("spDefense"))
            {
                //spDefenseText.text += string.Format(" ({0} {1})", "+", stat.Value);
                totalSpDef += int.Parse(stat.Value);
            }
            else if (stat.Key.Contains("speed"))
            {
                //speedText.text += string.Format(" ({0} {1})", "+", stat.Value);
                totalSpd += int.Parse(stat.Value);
            }
            else if (stat.Key.Contains("hp"))
            {
                //hpText.text += string.Format(" ({0} {1})", "+", stat.Value);
                totalHp += int.Parse(stat.Value);
            }

        }
    }
}
