using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

public class PF_GameData  {

    public static Dictionary<string, FG_SpellDetail> Spells = new Dictionary<string, FG_SpellDetail>();
    public static Dictionary<string, FG_ClassDetail> Classes = new Dictionary<string, FG_ClassDetail>();
    public static Dictionary<string, FG_LevelData> Levels = new Dictionary<string, FG_LevelData>();

    public static Dictionary<string, int> CharacterLevelRamp = new Dictionary<string, int>();

    public static float MinimumInterstitialWait;

    public static List<string> StandardStores = new List<string>();


    // all the items in our primary (GameItems) catalog
    public static List<CatalogItem> catalogItems = new List<CatalogItem>();

    public static void GetTitleData()
    {
        GetTitleDataRequest request = new GetTitleDataRequest()
        {
            Keys = new List<string>() { "Classes", "Spells", "MinimumInterstitialWait", "StandardStores", "CharacterLevelRamp","Levels" }
        };
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GetTitleData);
        PlayFabClientAPI.GetTitleData(request, OnGetTitleDataSuccess, PF_Bridge.PlayFabErrorCallback);
    }

    public static void OnGetTitleDataSuccess(GetTitleDataResult result)
    {
        Debug.Log("OnGetTitleDataSuccess");

        Debug.Log("OnGetTitleDataSuccess -- Spells");
        if (result.Data.ContainsKey("Spells")) 
        {
            Spells = PlayFabSimpleJson.DeserializeObject<Dictionary<string, FG_SpellDetail>>(result.Data["Spells"]);
            Debug.Log(string.Format("{0} Spell count found", Spells.Count));
        }

        Debug.Log("OnGetTitleDataSuccess -- Classes");
        if (result.Data.ContainsKey("Classes"))
        {
            Classes = PlayFabSimpleJson.DeserializeObject<Dictionary<string, FG_ClassDetail>>(result.Data["Classes"]);
            Debug.Log(string.Format("{0} Classes count found", Classes.Count));
        }

        Debug.Log("OnGetTitleDataSuccess -- Levels");
        if (result.Data.ContainsKey("Levels"))
        {
            Levels = PlayFabSimpleJson.DeserializeObject<Dictionary<string, FG_LevelData>>(result.Data["Levels"]);
            Debug.Log(Levels.Count + " sbdaibdaiubdiabba");
        }

        Debug.Log("OnGetTitleDataSuccess -- MinimumInterstitialWait");
        if (result.Data.ContainsKey("MinimumInterstitialWait"))
        {
            MinimumInterstitialWait = float.Parse(result.Data["MinimumInterstitialWait"]);
        }

        Debug.Log("OnGetTitleDataSuccess -- CharacterLevelRamp");
        if (result.Data.ContainsKey("CharacterLevelRamp"))
        {
            CharacterLevelRamp = PlayFabSimpleJson.DeserializeObject<Dictionary<string, int>>(result.Data["CharacterLevelRamp"]);
        }

        if (result.Data.ContainsKey("StandardStores"))
        {
            //StandardStores = PlayFabSimpleJson.DeserializeObject<List<string>>(result.Data["StandardStores"]);
            Debug.Log("Standard Stores Retrieved");
        }

            PF_Bridge.RaiseCallbackSuccess("Title Data Loaded", PlayFabAPIMethods.GetTitleData, MessageDisplayStyle.none);
    }

    public static void TryOpenContainer(string containerId, string characterId = null, UnityAction<UnlockContainerItemResult> callback = null)
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.UnlockContainerItem);
        UnlockContainerItemRequest request = new UnlockContainerItemRequest();
        request.ContainerItemId = containerId;

        if (characterId != null)
        {
            request.CharacterId = characterId;
        }

        PlayFabClientAPI.UnlockContainerItem(request, (UnlockContainerItemResult result) =>
        {
            if (callback != null)
            {
                callback(result);
            }

            PF_Bridge.RaiseCallbackSuccess("Container Unlocked", PlayFabAPIMethods.UnlockContainerItem, MessageDisplayStyle.none);
        }, PF_Bridge.PlayFabErrorCallback);

    }

    public static void GetCatalogInfo()
    {
        GetCatalogItemsRequest request = new GetCatalogItemsRequest();
        request.CatalogVersion = "GameItems";
        PlayFabClientAPI.GetCatalogItems(request, OnGetCatalogSuccess, PF_Bridge.PlayFabErrorCallback);
    }

    public static void OnGetCatalogSuccess(GetCatalogItemsResult result)
    {
        catalogItems = result.Catalog;
        PF_PlayerData.GetUserAccountInfo();
    }

    public static CatalogItem GetCatalogItemById(string id)
    {
        return PF_GameData.catalogItems.Find((item) => { return item.ItemId == id; });
    }

    public static CatalogItem ConvertStoreItemToCatalogItem(StoreItem si)
    {
        CatalogItem ci = new CatalogItem();
        CatalogItem reference = PF_GameData.catalogItems.Find((item) => { return item.ItemId == si.ItemId; });

        if (reference == null)
        {
            return new CatalogItem()
            {
                ItemId = "ITEM ERROR",
                DisplayName = "ITEM ERROR",
                VirtualCurrencyPrices = new Dictionary<string, uint>()
            };
        }

        ci.Bundle = reference.Bundle;
        ci.CanBecomeCharacter = reference.CanBecomeCharacter;
        ci.CatalogVersion = reference.CatalogVersion;
        ci.Consumable = reference.Consumable;
        ci.Container = reference.Container;
        ci.CustomData = reference.CustomData;
        ci.Description = reference.Description;
        ci.DisplayName = reference.DisplayName;
        ci.IsStackable = reference.IsStackable;
        ci.ItemClass = reference.ItemClass;
        ci.Tags = reference.Tags;

        ci.RealCurrencyPrices = si.RealCurrencyPrices;
        ci.VirtualCurrencyPrices = si.VirtualCurrencyPrices;
        ci.ItemId = si.ItemId;

        return ci;
    }
}
