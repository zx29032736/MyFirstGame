using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;


public class InventoryDisplayItem : MonoBehaviour
{

    public Image bg;
    public Image image;
    public Button btn;
    public FloatingInventoryController controller;
    public InventoryCategory category;

    public InventoryDisplayItem itemData;
    public Image icon;
    public Text itemName;
    public Text itemDescription;
    public Text annotation;
    public Text totalUses;
    public Image usesIcon;

    public Button UseAction;
    public Button UnlockAction;

    public void Init()
    {   
        this.btn.onClick.RemoveAllListeners();
        this.UseAction.onClick.RemoveAllListeners();
        this.UnlockAction.onClick.RemoveAllListeners();

        this.btn.onClick.AddListener(() =>
        {
            this.controller.ItemClicked(this);
            this.bg.color = Color.green;
            this.image.color = new Color(255, 255, 255, 255);
        });

        this.UseAction.onClick.AddListener(() =>
        {
            this.controller.UseItem();
        });

        this.UnlockAction.onClick.AddListener(() =>
        {
            this.controller.UnlockContainer();
        });

    }


    public void SetButton(Sprite icon, InventoryCategory cItem)
    {
        ActivateButton();
        this.image.overrideSprite = icon;
        this.category = cItem;
        RefreshSelected();
    }

    public void Deselect()
    {
        this.bg.color = Color.white;
        this.image.color = new Color(255, 255, 255, 190);
        this.UseAction.gameObject.SetActive(false);
        this.UnlockAction.gameObject.SetActive(false);
    }


    public void ClearButton()
    {
        this.image.overrideSprite = null;
        this.image.color = Color.clear;
        this.category = null;
        this.btn.interactable = false;
        this.bg.color = Color.white;
    }

    public void ActivateButton()
    {
        this.image.color = new Color(255, 255, 255, 190);
        this.image.overrideSprite = null;
        this.image.color = Color.white;
        this.category = null;
        this.btn.interactable = true;
    }

    public void RefreshSelected()
    {
        this.itemData = this;
        this.icon.overrideSprite = this.category.icon;
        this.itemName.text = this.category.catalogRef.DisplayName;
        this.itemDescription.text = this.category.catalogRef.Description;
        this.annotation.text = this.category.inventory[0].Annotation;

        //		var kvp = item.category.catalogRef.VirtualCurrencyPrices.First();
        //		this.itemCost.text = string.Format(" x{0}", kvp.Value);
        //		this.currencyIcon.overrideSprite = GameController.Instance.iconManager.GetIconById(kvp.Key);

        if (this.category.catalogRef.Container != null && this.category.catalogRef.Container.ResultTableContents != null)
        {
            this.totalUses.gameObject.SetActive(true);
            this.totalUses.text = string.Format(" x{0}", this.category.count);

        }
        else if (this.category.isConsumable || this.category.totalUses > 0 || this.category.count > 1)
        {
            if (controller.activeFilter == DialogCanvasController.InventoryFilters.UsableInCombat)
            {
                //this.UseAction.gameObject.SetActive(true);
            }
            
            this.totalUses.text = string.Format(" x{0}", this.category.totalUses > this.category.count ? this.category.totalUses : this.category.count);
            this.totalUses.gameObject.SetActive(true);
        }
        else
        {
            this.totalUses.gameObject.SetActive(false);
        }
    }

    public void CheckIsUseable()
    {
        if (this.category.catalogRef.Container != null && this.category.catalogRef.Container.ResultTableContents != null)
        {
            this.UseAction.gameObject.SetActive(false);
            this.UnlockAction.gameObject.SetActive(true);
        }
        else if (this.category.isConsumable || this.category.totalUses > 0 || this.category.count > 1)
        {
            this.UnlockAction.gameObject.SetActive(false);
        }
        else
        {
            this.UseAction.gameObject.SetActive(false);
            this.UnlockAction.gameObject.SetActive(false);
        }
    }
}
