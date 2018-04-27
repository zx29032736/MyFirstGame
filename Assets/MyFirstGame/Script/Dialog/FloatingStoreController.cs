using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FloatingStoreController : MonoBehaviour {

    public Text StoreName;

    public List<string> currenciesInUse;

    private StorePicker sPicker;
    private List<StoreItem> itemsToDisplay;

    //public StoreDisplayItem selectedItem
    public StoreDisplayItem selectedItem;
    public StoreCurrencyBarController Currencies;

    public Transform itemParent;
    //public StoreDisplayItem itemPrefab;
    public StoreDisplayItem[] itemInventory;

    //TODO solve the confusion with what VC balances are checked player VS character
    // close also needs to fire a callback to the calling area of the code
    public void InitiateStore(string name, List<StoreItem> items)
    {
        foreach (var slot in itemInventory)
            slot.gameObject.SetActive(false);

        //Dictionary<string, object> eventData = new Dictionary<string, object>()
        //{
        //    { "store_name", name }
        //};
        //PF_Bridge.LogCustomEvent(PF_Bridge.CustomEventTypes.Client_StoreVisit, eventData);

        //reset
        //itemInventory = new StoreDisplayItem[items.Count];

        this.currenciesInUse.Clear();

        //foreach (var item in this.itemInventory)
        //{
        //    item.ClearButton();
        //}


        this.itemsToDisplay = items;
        this.StoreName.text = name;

        for (int z = 0; z < items.Count; z++)
        {
            //todo : instance the item buttom and initialize

            itemInventory[z].gameObject.SetActive(true);
            itemInventory[z].ClearButton();
            //
            CatalogItem CI = PF_GameData.ConvertStoreItemToCatalogItem(items[z]);


            string iconName = "Default";
            if (CI.CustomData != null && !string.Equals(CI.CustomData, "null"))
            {
                try
                {
                    Dictionary<string, string> kvps = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(CI.CustomData);
                    kvps.TryGetValue("icon", out iconName);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            Sprite icon = GameController.Instance.iconManager.GetIconById(iconName);

            this.itemInventory[z].Init();
            this.itemInventory[z].SetButton(icon, CI);

            // keep track of what currencies are being used in the store.				
            List<string> VCs = items[z].VirtualCurrencyPrices.Keys.ToList();
            foreach (var vc in VCs)
            {
                int index = this.currenciesInUse.FindIndex((key) => { return string.Equals(key, vc); });
                // make sure not already in the list.
                if (index < 0)
                {
                    this.currenciesInUse.Add(vc);
                }
            }
        }

        //hide selected
        //this.Currencies.Init();
        this.gameObject.SetActive(true);

    }

    public void HideSelectedItem()
    {
        DeselectButtons();

        if (this.selectedItem != null)
        {
            this.selectedItem = null;
        }

        //this.selectedItem.gameObject.SetActive(false);

    }

    public void DeselectButtons()
    {
        foreach (var item in this.itemInventory)
        {
            if (item.catalogItem != null)
            {
                item.Deselect();
            }
        }
    }

    public void CloseStore()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_LEAVE_SOUND_EFFECT);
        // get this to close down and also close the tint.	
        // get a confirmation here
        this.gameObject.SetActive(false);
    }

    public void ItemClicked(StoreDisplayItem item)
    {
        if (this.selectedItem != null)
        {
            this.selectedItem.Deselect();
        }
        this.selectedItem = item;
        //this.selectedItem.RefreshSelected (item);
        //ShowSelectedItem();
        //item.bg.color = Color.green;
    }

    public void InitiatePurchase()
    {
        //NEED TO KNOW WHICH PURCHASE FLOW TO USE
        //Debug.Log ("Starting purchase of " + selectedItem.catalogItem.ItemId);
        PF_GamePlay.StartBuyStoreItem(this.selectedItem.catalogItem, this.StoreName.text);
        HideSelectedItem();


    }

    void OnEnable()
    {
        PF_Bridge.OnPlayFabCallbackError += HandleCallbackError;
        PF_Bridge.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
    }

    void OnDisable()
    {
        PF_Bridge.OnPlayFabCallbackError -= HandleCallbackError;
        PF_Bridge.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
    }

    public void HandleCallbackError(string details, PlayFabAPIMethods method, MessageDisplayStyle style)
    {

    }

    public void HandleCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle style)
    {
        switch (method)
        {
            case PlayFabAPIMethods.MakePurchase:
                // refresh after purchase.
                //if (PF_PlayerData.activeCharacter == null)
                //{
                    PF_PlayerData.GetUserAccountInfo();
                //}
                //else
                //{
                    //PF_PlayerData.GetCharacterInventory(PF_PlayerData.activeCharacter.characterDetails.CharacterId);
               // }
                break;

            case PlayFabAPIMethods.GetCharacterInventory:
                DialogCanvasController.RequestStore(this.StoreName.text);
                break;

            case PlayFabAPIMethods.GetAccountInfo:
                DialogCanvasController.RequestStore(this.StoreName.text);
                break;


        }
    }
}
