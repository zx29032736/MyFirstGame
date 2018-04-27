using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentType { Weapon, Armor, Jewelry }
public class CharacterEquipController : MonoBehaviour {

    // public GameObject equipmentSlotField;
    CharacterDisplayItem selectedCha;
    public FloatingCharactersConrtroller chaController;
    public EquipmentDisplatItem[] itemSlots;

    //equiped stat
    public Image equipedImg;
    public Text equipedNameText;
    public Text equipedStatText;

    public Image readyToEquipedImg;
    public Text readyToEquipedNameText;
    public Text readyToEquipedStatText;
    //
    public EquipmentDisplatItem selectedItem;
    public EquipmentType activedType;

    InventoryCategory myEquip = null;

    public void Init(EquipmentType type)
    {
        activedType = type;
        Refresh();
        gameObject.SetActive(true);
    }

    public void Refresh()
    {
        foreach (var go in itemSlots)
            go.gameObject.SetActive(false);

        selectedCha = chaController.selectedCharacter;
        selectedItem = null;
        SetUpChaEquipment(activedType);

        int count = 0;
        foreach (var kvp in PF_PlayerData.inventoryByCategory)
        {

            foreach (var id in kvp.Value.inventory)
            {
                bool addItem = false;

                if (string.Equals(kvp.Value.catalogRef.ItemClass, activedType.ToString()) && !PF_PlayerData.characterEquipedItem.Contains(id.ItemInstanceId))
                {
                    addItem = true;
                }
                else
                    continue;

                if (addItem == true)
                {
                    itemSlots[count].gameObject.SetActive(true);
                    itemSlots[count].Init(this);
                    itemSlots[count].SetUpButton(kvp.Value, id.ItemInstanceId);

                    count++;
                }
            }

        }
    }

    void SetUpChaEquipment(EquipmentType type)
    {
        readyToEquipedImg.overrideSprite = null;
        readyToEquipedNameText.text = "";
        equipedStatText.text = "";

        myEquip = null;
        if (type == EquipmentType.Weapon && selectedCha.saved.characterData.EquipedWeapon != null)
        {
            myEquip = PF_PlayerData.inventoryByCategory[selectedCha.saved.characterData.EquipedWeapon.EquipmentName];
        }
        else if(type == EquipmentType.Armor && selectedCha.saved.characterData.EquipedArmor != null)
        {
            myEquip = PF_PlayerData.inventoryByCategory[selectedCha.saved.characterData.EquipedArmor.EquipmentName];

            //foreach (var st in selectedCha.saved.characterData.EquipedArmor.stat)
            //{
            //    if (st.Key.Contains("icon"))
            //        continue;
            //    equipedStatText.text += string.Format("{0} : {1} \n", st.Key, st.Value);

            //}
        }
        else if(type == EquipmentType.Jewelry && selectedCha.saved.characterData.EquipedJewelry != null)
        {
            myEquip = PF_PlayerData.inventoryByCategory[selectedCha.saved.characterData.EquipedJewelry.EquipmentName];

            //foreach (var st in selectedCha.saved.characterData.EquipedJewelry.stat)
            //{
            //    if (st.Key.Contains("icon"))
            //        continue;
            //    equipedStatText.text += string.Format("{0} : {1} \n", st.Key, st.Value);
            //}
        }
        else
        {
            equipedNameText.text = "null";
            return;
        }

        equipedImg.overrideSprite = myEquip.icon;
        equipedNameText.text = myEquip.catalogRef.DisplayName;

        foreach (var st in myEquip.customData)
        {
            if (st.Key.Contains("icon"))
                continue;
            equipedStatText.text += string.Format("{0} : {1} \n", st.Key, st.Value);

        }

    }

    public void onItemClicked(EquipmentDisplatItem it)
    {
        if (selectedItem == it)
            return;

        selectedItem = it;
        SetUpToEquipment();
    }

    public void SetUpToEquipment()
    {
        readyToEquipedImg.overrideSprite = selectedItem.equipmentImage.overrideSprite;
        readyToEquipedNameText.text = selectedItem.equipmentNameText.text;
        readyToEquipedStatText.text = "";
        foreach(var st in selectedItem.detail)
        {
            readyToEquipedStatText.text += string.Format("{0} : {1} \n", st.Key, st.Value);
        }
    }

    public void ConfirmToEquip()
    {
        if (selectedItem == null)
            return;

        if (selectedItem.myCategory.catalogRef.ItemClass == activedType.ToString())
        {
            if(activedType == EquipmentType.Weapon)
            {
                selectedCha.saved.characterData.EquipedWeapon = new Equipment()
                {
                    EquipmentId = selectedItem.instanceId,
                    EquipmentName = selectedItem.myCategory.catalogRef.DisplayName,
                    stat = selectedItem.detail
                };
            }
            else if(activedType == EquipmentType.Armor)
            {
                selectedCha.saved.characterData.EquipedArmor = new Equipment()
                {
                    EquipmentId = selectedItem.instanceId,
                    EquipmentName = selectedItem.myCategory.catalogRef.DisplayName,
                    stat = selectedItem.detail
                };
            }
            else if(activedType == EquipmentType.Jewelry)
            {
                return; // data storage is full
                selectedCha.saved.characterData.EquipedJewelry = new Equipment()
                {
                    EquipmentId = selectedItem.instanceId,
                    EquipmentName = selectedItem.myCategory.catalogRef.DisplayName,
                    stat = selectedItem.detail
                };
            }

        }
        else
            Debug.LogError("type error");

        UpdateToCharatcterData();
    }

    public void UnEquip()
    {
        if (myEquip == null)
            return;

        if(activedType == EquipmentType.Weapon)
        {
            selectedCha.saved.characterData.EquipedWeapon = null;
        }
        else if(activedType == EquipmentType.Armor)
        {
            selectedCha.saved.characterData.EquipedArmor = null;
        }
        else if (activedType == EquipmentType.Jewelry)
        {
            selectedCha.saved.characterData.EquipedJewelry = null;
        }

        UpdateToCharatcterData();
    }

    void UpdateToCharatcterData()
    {
        PF_GamePlay.SavePlayerData(selectedCha.saved, new QuestTracker() {  });
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
