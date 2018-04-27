using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab.ClientModels;
using PlayFab;
using PlayFab.Json;

public class PF_GamePlay : MonoBehaviour {

    public static QuestTracker QuestProgress = new QuestTracker();
    public static FG_LevelData ActiveQuest;
    public static Dictionary<string,FG_SavedCharacter> AllSavedCharacterUnitData;

    #region Ui Animation
    /// <summary>
    /// Intros the pane.
    /// </summary>
    /// <param name="obj">Object - object to animate in</param>
    /// <param name="duration">Duration -   how long is the transition</param>
    /// <param name="callback">Callback - method to call after the animation is complete </param>
    public static void IntroPane(GameObject obj, float duration, UnityAction callback = null)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true;
            //TweenCGAlpha.Tween(obj, duration, 1, callback);
            cg.alpha = 1;
            
            //callback();
        }
        else
        {
            // will add a cg automatically
            //TweenCGAlpha.Tween(obj, duration, 1, callback);
        }
    }

    /// <summary>
    /// Outros the pane.
    /// </summary>
    /// <param name="obj">Object - object to animate out</param>
    /// <param name="duration">Duration -   how long is the transition</param>
    /// <param name="callback">Callback - method to call after the animation is complete </param>
    public static void OutroPane(GameObject obj, float duration, UnityAction callback = null)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
            //TweenCGAlpha.Tween(obj, duration, 0, callback);
            cg.alpha = 0;
            //callback();
        }
        else
        {
            // will add a cg automatically
            //TweenCGAlpha.Tween(obj, duration, 0, callback);
        }
    }

    #endregion
    /// <summary>
    /// Retrives the quest items.
    /// </summary>
    public static void RetriveQuestItems()
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.RetriveQuestItems);

        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();

        request.FunctionName = "RetriveQuestItems";
        request.FunctionParameter = new {ItemIds = PF_GamePlay.QuestProgress.ItemsFound };
        PlayFabClientAPI.ExecuteCloudScript(request, OnRetriveQuestItemsSuccess, PF_Bridge.PlayFabErrorCallback);
    }

    /// <summary>
    /// Raises the retrive quest items success event.
    /// </summary>
    /// <param name="result">Result.</param>
    public static void OnRetriveQuestItemsSuccess(ExecuteCloudScriptResult result)
    {
        if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
            return;

        Debug.Log(result.ToString());
        QuestProgress.ItemsGranted = PlayFabSimpleJson.DeserializeObject<List<ItemGrantResult>>(result.FunctionResult.ToString());

        PF_GamePlay.QuestProgress.areItemsAwarded = true;

        //PF_PlayerData.GetCharacterInventory(PF_PlayerData.activeCharacter.characterDetails.CharacterId);
        //PF_PlayerData.GetUserInventory();
        PF_Bridge.RaiseCallbackSuccess("Items granted", PlayFabAPIMethods.RetriveQuestItems, MessageDisplayStyle.none);
    }

    /// <summary>
    /// Retrives the store items.
    /// </summary>
    /// <param name="storeName">Store name.</param>
    /// <param name="callback">Callback.</param>
    public static void RetriveStoreItems(string storeName, UnityAction<List<StoreItem>> callback = null)
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GetStoreItems);
        GetStoreItemsRequest request = new GetStoreItemsRequest();
        request.StoreId = storeName;
        request.CatalogVersion = "GameItems";
        PlayFabClientAPI.GetStoreItems(request, (GetStoreItemsResult result) =>
        {
            OnRetriveStoreItemsSuccess(result);
            if (callback != null)
            {
                callback(result.Store);
            }

        }, PF_Bridge.PlayFabErrorCallback);

    }

    /// <summary>
    /// Raises the retrive store items success event.
    /// </summary>
    /// <param name="result">Result.</param>
    public static void OnRetriveStoreItemsSuccess(GetStoreItemsResult result)
    {
        //mostRecentStore = result.Store;
        PF_Bridge.RaiseCallbackSuccess("Store Retrieved", PlayFabAPIMethods.GetStoreItems, MessageDisplayStyle.none);
    }

    /// <summary>
	/// Starts the buy store item.
	/// </summary>
	/// <param name="item">Item.</param>
	/// <param name="storeID">Store I.</param>
	public static void StartBuyStoreItem(CatalogItem item, string storeID)
    {

        string characterId = null;//PF_PlayerData.activeCharacter == null ? null : PF_PlayerData.activeCharacter.characterDetails.CharacterId;
        var vcKVP = item.VirtualCurrencyPrices.First();

        if (characterId != null)
        {
            DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.MakePurchase);

            ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
            request.FunctionName = "PurchaseItem";
            //request.FunctionParameter = new { ItemPrice = (int)vcKVP.Value, CurrencyCode = vcKVP.Key, CharacterId = PF_PlayerData.activeCharacter.characterDetails.CharacterId, ItemId = item.ItemId };

            PlayFabClientAPI.ExecuteCloudScript(request, (ExecuteCloudScriptResult result) =>
            {
                if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
                    return;

                if ((bool)result.FunctionResult == true)
                {
                    PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.MakePurchase, MessageDisplayStyle.none);
                }
                else
                {
                    PF_Bridge.RaiseCallbackError("Could not process request due to insufficient VC.", PlayFabAPIMethods.MakePurchase, MessageDisplayStyle.error);
                }

            }, PF_Bridge.PlayFabErrorCallback);
        }
        else if (characterId == null)
        {
            // normal purchase item flow
            PurchaseItemRequest request = new PurchaseItemRequest();
            request.ItemId = item.ItemId;
            request.VirtualCurrency = vcKVP.Key;
            request.Price = (int)vcKVP.Value;
            request.StoreId = storeID;
            DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.MakePurchase);
            PlayFabClientAPI.PurchaseItem(request, OnBuyStoreItemSuccess, PF_Bridge.PlayFabErrorCallback);
        }
        else
        {
            Debug.LogWarning("Store purchase failed: " + characterId);
        }
    }

    /// <summary>
    /// Raises the buy store item success event.
    /// </summary>
    /// <param name="result">Result.</param>
    public static void OnBuyStoreItemSuccess(PurchaseItemResult result)
    {
        PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.MakePurchase, MessageDisplayStyle.none);
        Debug.Log(string.Format("{0} Items Purchased!", result.Items.Count));
       //GameController.CharacterSelectDataRefresh();
    }

    /// <summary>
	/// write back the quest progress to playfab
	/// </summary>
	public static void SavePlayerData(FG_SavedCharacter savedCha, QuestTracker questProgress)
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.SavePlayerInfo);

        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
        request.FunctionName = "SaveProgress";
        request.FunctionParameter = new { CurrentPlayerData = savedCha,  LevelRamp = PF_GameData.CharacterLevelRamp , QuestProgress = questProgress };


        PlayFabClientAPI.ExecuteCloudScript(request, OnSavePlayerDataSuccess, PF_Bridge.PlayFabErrorCallback);

    }
    /// <summary>
    /// Callback after 
    /// </summary>
    /// <param name="result">Result.</param>
    public static void OnSavePlayerDataSuccess(ExecuteCloudScriptResult result)
    {
        if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
            return;

        PF_Bridge.RaiseCallbackSuccess("Player Info Saved", PlayFabAPIMethods.SavePlayerInfo, MessageDisplayStyle.none);
    }
}
