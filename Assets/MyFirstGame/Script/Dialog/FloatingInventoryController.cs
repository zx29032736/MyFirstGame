using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using PlayFab.ClientModels;
using PlayFab;
using PlayFab.Json;
using UnityEngine.Events;

public class FloatingInventoryController : MonoBehaviour {

    public Text StoreName;
    //public List<InventoryDisplayItem> inventory = new List<InventoryDisplayItem>();
    public DialogCanvasController.InventoryFilters activeFilter;

    public List<string> currenciesInUse;
    private Dictionary<string, InventoryCategory> itemsToDisplay = new Dictionary<string, InventoryCategory>();
    private Action<string> callbackAfterUse;

    public InventoryDisplayItem selectedItem;

    public InventoryCurrencyBarController Currencies;

    public int MaxStorageCount = 10;
    public InventoryDisplayItem[] itemSlots;

    public void Init(Action<string> callback = null, DialogCanvasController.InventoryFilters filter = DialogCanvasController.InventoryFilters.AllItems)
    {
        foreach (var slot in itemSlots)
            slot.gameObject.SetActive(false);

        if (selectedItem != null)
        {
            selectedItem.Deselect();
            selectedItem = null;
        }

        this.activeFilter = filter;

        if (callback != null)
        {
            this.callbackAfterUse = callback;
        }

        //only displaying the main currencies (Gold & Gems) for now
        this.currenciesInUse.Clear();
        this.currenciesInUse.Add("NT");

        if (PF_PlayerData.inventoryByCategory != null && PF_PlayerData.virtualCurrency != null)
        {
            this.itemsToDisplay = PF_PlayerData.inventoryByCategory;
            //this.Currencies.Init(PF_PlayerData.virtualCurrency);
        }
        else
        {
            return;
        }

        string filterName;
        // GlobalStrings.INV_FILTER_DISPLAY_NAMES.TryGetValue(filter, out filterName);
        //this.StoreName.text = string.Format(GlobalStrings.INV_WINDOW_TITLE, filterName);
        int count = 0;
        foreach (var kvp in this.itemsToDisplay)
        {

            bool addItem = false;
            if (filter == DialogCanvasController.InventoryFilters.Containers)
            {
                if (string.Equals(kvp.Value.catalogRef.ItemClass, filter.ToString()))
                {
                    addItem = true;
                }
                else
                {
                    continue;
                }
            }
            else if (filter == DialogCanvasController.InventoryFilters.Keys)
            {
                if (string.Equals(kvp.Value.catalogRef.ItemClass, filter.ToString()))
                {
                    addItem = true;
                }
                else
                {
                    continue;
                }
            }
            else if (filter == DialogCanvasController.InventoryFilters.UsableInCombat)
            {
                if (string.Equals(kvp.Value.catalogRef.ItemClass, filter.ToString()))
                {
                    addItem = true;
                }
                else
                {
                    continue;
                }
            }
            else if (filter == DialogCanvasController.InventoryFilters.Material)
            {
                if (string.Equals(kvp.Value.catalogRef.ItemClass, filter.ToString()))
                {
                    addItem = true;
                }
                else
                {
                    continue;
                }
            }
            else if (filter == DialogCanvasController.InventoryFilters.AllItems)
            {
                addItem = true;
            }
            if (addItem == true)
            {
                itemSlots[count].gameObject.SetActive(true);
                itemSlots[count].Init();
                itemSlots[count].SetButton(kvp.Value.icon, kvp.Value);

                count++;
            }
        }
        this.gameObject.SetActive(true);
    }

    public void CloseInventory()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_LEAVE_SOUND_EFFECT);
        // get this to close down and also close the tint.	
        // get a confirmation here
        //DeselectButtons();
        if (selectedItem != null)
        {
            selectedItem.Deselect();
            selectedItem = null;
        }
        this.gameObject.SetActive(false);
    }

    public void ItemClicked(InventoryDisplayItem item)
    {
        //if (this.selectedItem.itemData != null)
        //{
        //    this.selectedItem.itemData.Deselect();
        //}

        //this.selectedItem.RefreshSelected(item);
        //ShowSelectedItem();
        if (this.selectedItem != null)
        {
            this.selectedItem.Deselect();
        }
        this.selectedItem = item;
        this.selectedItem.CheckIsUseable();
        Debug.Log(item.itemName.text + " Clicked");
    }

    public void UseItem() // not possible outside of battle
    {
        //NEED TO KNOW WHICH PURCHASE FLOW TO USE
        //Debug.Log ("Using Item: " + selectedItem.itemData.category.catalogRef.ItemId);
        //PF_GamePlay.StartBuyStoreItem(this.selectedItem.itemData.catalogItem, this.StoreName.text);

        // CALL CS to decrement item
        //PF_GameData

        if (this.callbackAfterUse != null)
        {
            this.callbackAfterUse(selectedItem.itemData.category.catalogRef.ItemId);
            CloseInventory();
        }
    }

    public void HideSelectedItem()
    {
        DeselectButtons();

        if (this.selectedItem.itemData != null)
        {
            this.selectedItem.itemData = null;
        }

        //this.selectedItem.gameObject.SetActive(false);

    }

    public void DeselectButtons()
    {
        foreach (var item in this.itemSlots)
        {
            if (item != null)
            {
                item.Deselect();
            }
        }
    }

    public void UnlockContainer()
    {
        DialogCanvasController.RequestItemViewer(new List<string>() { selectedItem.itemData.category.catalogRef.ItemId }, true);
        HideSelectedItem();
    }

    public void RefreshInventory()
    {
            PF_PlayerData.GetUserInventory();
            DialogCanvasController.RequestInventoryPrompt(null, this.activeFilter);
    }


    #region Filters
    public void All_Filter()
    {
        this.Init(null, DialogCanvasController.InventoryFilters.AllItems);
    }

    public void Container_Filter()
    {
        this.Init(null, DialogCanvasController.InventoryFilters.Containers);
    }

    public void Combat_Filter()
    {
        this.Init(null, DialogCanvasController.InventoryFilters.UsableInCombat);
    }

    public void Material_Filter()
    {
        this.Init(null, DialogCanvasController.InventoryFilters.Material);
    }
    #endregion
}
