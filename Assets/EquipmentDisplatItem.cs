using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDisplatItem : MonoBehaviour {

    public Button myBtn;
    public Image equipmentImage;
    public Text equipmentNameText;
    public Text stat1NameText;
    public Text stat1VlaueText;
    public Text stat2NameText;
    public Text stat2VlaueText;
    public Text stat3NameText;
    public Text stat3VlaueText;
    public CharacterEquipController controller;

    public InventoryCategory myCategory;
    public string instanceId;
    public Dictionary<string, string> detail = new Dictionary<string, string>();

    public void Init(CharacterEquipController cont)
    {
        controller = cont;

        myBtn.onClick.RemoveAllListeners();
        myBtn.onClick.AddListener( () => { controller.onItemClicked(this); } );

        gameObject.SetActive(true);
    }

    public void SetUpButton(InventoryCategory category, string id)
    {
        myCategory = category;
        instanceId = id;
        detail = myCategory.customData;//PF_PlayerData.inventoryByCategory[myCategory.catalogRef.DisplayName].customData;//PlayFab.Json.PlayFabSimpleJson.DeserializeObject<Dictionary<string,string>>(myCategory.catalogRef.CustomData);

        equipmentImage.overrideSprite = myCategory.icon;
        equipmentNameText.text = myCategory.catalogRef.DisplayName;

        int count = 0;
        foreach(var stat in detail)
        {
            if (stat.Key.Contains("icon"))
                continue;

            if(count == 0)
            {
                stat1NameText.text = stat.Key.ToString();
                stat1VlaueText.text = stat.Value.ToString();
                stat1NameText.gameObject.SetActive(true);
                stat1VlaueText.gameObject.SetActive(true);
                count++;
            }
            else if(count == 1)
            {
                stat2NameText.text = stat.Key.ToString();
                stat2VlaueText.text = stat.Value.ToString();
                stat2NameText.gameObject.SetActive(true);
                stat2VlaueText.gameObject.SetActive(true);
                count++;
            }
            else if (count == 2)
            {
                stat3NameText.text = stat.Key.ToString();
                stat3VlaueText.text = stat.Value.ToString();
                stat3NameText.gameObject.SetActive(true);
                stat3VlaueText.gameObject.SetActive(true);
                count++;
            }
        }
        
    }

}
