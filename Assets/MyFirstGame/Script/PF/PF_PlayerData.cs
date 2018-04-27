using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using PlayFab.Json;
using PlayFab;

public class PF_PlayerData {

    // Player Level Data:
    public static string PlayerId = string.Empty;
    public static bool showAccountOptionsOnLogin = true;
    public static bool isRegisteredForPush = false;

    public static UserAccountInfo accountInfo;
    public static Dictionary<string, UserDataRecord> UserData = new Dictionary<string, UserDataRecord>();
    // this is a sorted, collated structure built from playerInventory. By default, this will only grab items that are in the primary catalog
    public static Dictionary<string, InventoryCategory> inventoryByCategory = new Dictionary<string, InventoryCategory>();
    public static Dictionary<string, int> virtualCurrency;
    public static List<ItemInstance> playerInventory = new List<ItemInstance>();
    public static Dictionary<string, int> userStatistics = new Dictionary<string, int>();

    //aggregation of player characters
    public static List<PlayFab.ClientModels.CharacterResult> playerCharacters = new List<PlayFab.ClientModels.CharacterResult>();
    public static Dictionary<string, FG_CharacterData> playerCharacterData = new Dictionary<string, FG_CharacterData>();
    public static List<string> characterEquipedItem = new List<string>();

    public static List< FG_SavedCharacter> SavedTeam = null;

    public static Dictionary<string, List<string>> MyTeamsCharacterId = new Dictionary<string, List<string>>();

    #region user data
    public static void GetUserDatas(List<string> keys, UnityAction<GetUserDataResult> callback = null)
    {
        GetUserDataRequest request = new GetUserDataRequest();
        request.Keys = keys;
        request.PlayFabId = PF_PlayerData.PlayerId;

        //DialogCanvasController.RequestLoadingPrompt (PlayFabAPIMethods.GetUserData);
        PlayFabClientAPI.GetUserReadOnlyData(request, (GetUserDataResult result) =>
        {
            if (callback != null)
            {
                callback(result);
            }
            PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.GetUserData, MessageDisplayStyle.none);

        }, PF_Bridge.PlayFabErrorCallback);

    }

    public static void UpdateUserData(Dictionary<string, string> updates, string permission = "Public", UnityAction<UpdateUserDataResult> callback = null)
    {
        UpdateUserDataRequest request = new UpdateUserDataRequest();
        request.Data = updates;
        request.Permission = (UserDataPermission)Enum.Parse(typeof(UserDataPermission), permission);


        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.UpdateUserData);
        PlayFabClientAPI.UpdateUserData(request, (UpdateUserDataResult result) =>
        {
            if (callback != null)
            {
                callback(result);
            }
            PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.UpdateUserData, MessageDisplayStyle.none);

        }, PF_Bridge.PlayFabErrorCallback);
    }

    public static void GetUserInventory(Action callback = null)
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GetUserInventory);
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), (GetUserInventoryResult result) =>
        {
            virtualCurrency = result.VirtualCurrency;
            playerInventory = result.Inventory;
            inventoryByCategory.Clear();

            if (PF_GameData.catalogItems.Count > 0)
            {
                foreach (var item in playerInventory)
                {
                    //if (item.CatalogVersion == "Offers")
                    //{
                    //    OfferContainers.Add(item);
                    //    continue;
                    //}

                    if (!inventoryByCategory.ContainsKey(item.ItemId))
                    {
                        CatalogItem catalog = PF_GameData.catalogItems.Find((x) => { return x.ItemId.Contains(item.ItemId); });
                        List<ItemInstance> items = new List<ItemInstance>(playerInventory.FindAll((x) => { return x.ItemId.Equals(item.ItemId); }));

                        try
                        {
                            if (catalog != null)
                            {
                                Dictionary<string, string> customAttributes = new Dictionary<string, string>();
                                string customIcon = "Defaut";
                                // here we can process the custom data and apply the propper treatment (eg assign icons)
                                if (catalog.CustomData != null && catalog.CustomData != "null") //TODO update once the bug is fixed on the null value
                                {
                                    customAttributes = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(catalog.CustomData);
                                    if (customAttributes.ContainsKey("icon"))
                                    {
                                        customIcon = customAttributes["icon"];
                                    }
                                }

                                Sprite icon = GameController.Instance.iconManager.GetIconById(customIcon);

                                if (catalog.Consumable.UsageCount > 0)
                                {
                                    inventoryByCategory.Add(item.ItemId, new InventoryCategory(item.ItemId, catalog, items, icon, true, customAttributes));
                                }
                                else
                                {
                                    inventoryByCategory.Add(item.ItemId, new InventoryCategory(item.ItemId, catalog, items, icon, customAttributes));
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning(item.ItemId + " -- " + e.Message);
                            continue;
                        }
                    }
                }

                //if (OfferContainers.Count > 0)
                //{
                //    DialogCanvasController.RequestOfferPrompt();
                //}
            }

            if (callback != null)
            {
                callback();
            }

            PF_Bridge.RaiseCallbackSuccess("", PlayFabAPIMethods.GetUserInventory, MessageDisplayStyle.none);
        }, PF_Bridge.PlayFabErrorCallback);
    }

    #endregion

    #region User Statistics
    public static void GetPlayerStatistics()
    {
        GetPlayerStatisticsRequest request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, OnGetPlayerStatisticsSuccess, OnGetPlayerStatisticsError);
    }

    private static void OnGetPlayerStatisticsSuccess(GetPlayerStatisticsResult result)
    {
        //TODO update to use new 
        PF_Bridge.RaiseCallbackSuccess("", PlayFabAPIMethods.GetUserStatistics, MessageDisplayStyle.none);
        foreach (var each in result.Statistics)
            userStatistics[each.StatisticName] = each.Value;
    }

    private static void OnGetPlayerStatisticsError(PlayFabError error)
    {
        PF_Bridge.RaiseCallbackError(error.ErrorMessage, PlayFabAPIMethods.GetUserStatistics, MessageDisplayStyle.error);
    }

    public static void UpdatPlayerStatistics(Dictionary<string, int> updates)
    {
        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
        request.Statistics = new List<StatisticUpdate>();

        foreach (var eachUpdate in updates) // Copy the stats from the inputs to the request
        {
            int eachStat;
            userStatistics.TryGetValue(eachUpdate.Key, out eachStat);
            request.Statistics.Add(new StatisticUpdate() { StatisticName = eachUpdate.Key, Value = eachUpdate.Value }); // Send the value to the server
            userStatistics[eachUpdate.Key] = eachStat + eachUpdate.Value; // Update the local cache so that future updates are using correct values
        }

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdatePlayerStatisticsSuccess, OnUpdateUserStatisticsError);
    }

    private static void OnUpdatePlayerStatisticsSuccess(UpdatePlayerStatisticsResult result)
    {
        PF_Bridge.RaiseCallbackSuccess("User Statistics Loaded", PlayFabAPIMethods.UpdateUserStatistics, MessageDisplayStyle.none);
        GetPlayerStatistics(); // Refresh stats that we just updated
    }

    private static void OnUpdateUserStatisticsError(PlayFabError error)
    {
        PF_Bridge.RaiseCallbackError(error.ErrorMessage, PlayFabAPIMethods.UpdateUserStatistics, MessageDisplayStyle.error);
    }
    #endregion

    #region User Account APIs
    public static void GetUserAccountInfo()
    {
        GetPlayerCombinedInfoRequest request = new GetPlayerCombinedInfoRequest()
        {
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserData = true , GetUserVirtualCurrency = true, GetUserInventory = true, GetUserAccountInfo = true}
        };
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GetAccountInfo);
        PlayFabClientAPI.GetPlayerCombinedInfo(request, OnGetPlayerAccountInfoSuccess, PF_Bridge.PlayFabErrorCallback);
    }

    public static void OnGetPlayerAccountInfoSuccess(GetPlayerCombinedInfoResult result)
    {
        playerInventory = result.InfoResultPayload.UserInventory;
        accountInfo = result.InfoResultPayload.AccountInfo;
        //UserData = result.InfoResultPayload.UserData;

        if (result.InfoResultPayload.UserData.ContainsKey("IsRegisteredForPush"))
        {
            if (result.InfoResultPayload.UserData["IsRegisteredForPush"].Value == "1")
            {
                PF_PlayerData.isRegisteredForPush = true;
            }
            else
            {
                PF_PlayerData.isRegisteredForPush = false;
            }
        }
        else
        {
            PF_PlayerData.isRegisteredForPush = false;
        }

        if (result.InfoResultPayload.UserData.ContainsKey("ShowAccountOptionsOnLogin") && result.InfoResultPayload.UserData["ShowAccountOptionsOnLogin"].Value == "0")
        {
            PF_PlayerData.showAccountOptionsOnLogin = false;
        }
        else //if (PF_Authentication.hasLoggedInOnce == false) 
        {

            //DialogCanvasController.RequestAccountSettings();//modifing
        }

        inventoryByCategory.Clear();

        if (PF_GameData.catalogItems.Count > 0)
        {
            foreach (var item in playerInventory)
            {
                //if (item.CatalogVersion == "Offers")
                //{
                //    OfferContainers.Add(item);
                //    continue;
                //}

                if (!inventoryByCategory.ContainsKey(item.ItemId))
                {
                    CatalogItem catalog = PF_GameData.catalogItems.Find((x) => { return x.ItemId.Contains(item.ItemId); });
                    List<ItemInstance> items = new List<ItemInstance>(playerInventory.FindAll((x) => { return x.ItemId.Equals(item.ItemId); }));

                    try
                    {
                        if (catalog != null)
                        {
                            string customIcon = "Defaut";
                            Dictionary<string, string> customAttributes = new Dictionary<string, string>();
                            // here we can process the custom data and apply the propper treatment (eg assign icons)
                            if (catalog.CustomData != null && catalog.CustomData != "null") //TODO update once the bug is fixed on the null value
                            {
                                //Dictionary<string, string> customAttributes = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(catalog.CustomData);
                                customAttributes = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(catalog.CustomData);
                                if (customAttributes.ContainsKey("icon"))
                                {
                                    customIcon = customAttributes["icon"];
                                }
                            }

                            Sprite icon = GameController.Instance.iconManager.GetIconById(customIcon);

                            if (catalog.Consumable.UsageCount > 0)
                            {
                                inventoryByCategory.Add(item.ItemId, new InventoryCategory(item.ItemId, catalog, items, icon, true, customAttributes));
                            }
                            else
                            {
                                inventoryByCategory.Add(item.ItemId, new InventoryCategory(item.ItemId, catalog, items, icon, customAttributes));
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(item.ItemId + " -- " + e.Message);
                        continue;
                    }
                }
            }
        }


        if (PF_Authentication.GetDeviceId(true))
        {
            //Debug.Log("Mobile Device ID Found!");

            string deviceID = string.IsNullOrEmpty(PF_Authentication.android_id) ? PF_Authentication.ios_id : PF_Authentication.android_id;
            PlayerPrefs.SetString("LastDeviceIdUsed", deviceID);
        }
        else
        {
            //Debug.Log("Custom Device ID Found!");

            if (string.IsNullOrEmpty(PF_Authentication.custom_id))
            {
                PlayerPrefs.SetString("LastDeviceIdUsed", PF_Authentication.custom_id);
            }
        }

        virtualCurrency = result.InfoResultPayload.UserVirtualCurrency;

        if (result.InfoResultPayload.UserData.ContainsKey("Teams"))
        {
            PF_PlayerData.MyTeamsCharacterId = PlayFabSimpleJson.DeserializeObject<Dictionary<string, List<string>>>(result.InfoResultPayload.UserData["Teams"].Value);
        }


        PF_Bridge.RaiseCallbackSuccess("Player Account Info Loaded", PlayFabAPIMethods.GetAccountInfo, MessageDisplayStyle.none);
    }
    #endregion

    #region Character APIs

    public static void GetCharacterData(Action callback = null)
    {
        playerCharacterData.Clear();
        characterEquipedItem.Clear();
        //characterAchievements.Clear();

        int remainingCallbacks = playerCharacters.Count;

        if (remainingCallbacks == 0)
        {
            PF_Bridge.RaiseCallbackSuccess("", PlayFabAPIMethods.GetCharacterReadOnlyData, MessageDisplayStyle.none);
            return;
        }
        else
        {
            DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GetCharacterReadOnlyData);
        }


        foreach (var character in playerCharacters)
        {
            GetCharacterDataRequest request = new GetCharacterDataRequest();
            //request.PlayFabId = PlayFabSettings.PlayerId;
            request.CharacterId = character.CharacterId;
            request.Keys = new List<string>() { "CharacterData"};

            PlayFabClientAPI.GetCharacterReadOnlyData(request, (result) => {
                // OFFERS

                //if (result.Data.ContainsKey("Achievements"))
                //{
                //    characterAchievements.Add(result.CharacterId, PlayFab.SimpleJson.DeserializeObject<List<string>>(result.Data["Achievements"].Value));
                //}


                if (result.Data.ContainsKey("CharacterData"))
                {
                    playerCharacterData.Add(result.CharacterId, PlayFabSimpleJson.DeserializeObject<FG_CharacterData>(result.Data["CharacterData"].Value));

                    #region setup equipment
                    if(playerCharacterData[result.CharacterId].EquipedWeapon != null)
                    {
                        if (!characterEquipedItem.Contains(playerCharacterData[result.CharacterId].EquipedWeapon.EquipmentId))
                            characterEquipedItem.Add(playerCharacterData[result.CharacterId].EquipedWeapon.EquipmentId);

                    }
                    if (playerCharacterData[result.CharacterId].EquipedArmor != null)
                    {
                        if (!characterEquipedItem.Contains(playerCharacterData[result.CharacterId].EquipedArmor.EquipmentId))
                            characterEquipedItem.Add(playerCharacterData[result.CharacterId].EquipedArmor.EquipmentId);
                    }
                    if (playerCharacterData[result.CharacterId].EquipedJewelry != null)
                    {
                        if (!characterEquipedItem.Contains(playerCharacterData[result.CharacterId].EquipedJewelry.EquipmentId))
                            characterEquipedItem.Add(playerCharacterData[result.CharacterId].EquipedJewelry.EquipmentId);
                    }
                    #endregion  

                    remainingCallbacks--;
                    if (remainingCallbacks == 0)
                    {
                        if (callback != null)
                            callback();

                        PF_Bridge.RaiseCallbackSuccess("", PlayFabAPIMethods.GetCharacterReadOnlyData, MessageDisplayStyle.none);
                    }
                }
                //Debug.Log(result.Data["CharacterData"].Value);
                
            }, PF_Bridge.PlayFabErrorCallback);

        }
    }

    public static void GetCharacterDataById(string characterId)
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GetCharacterReadOnlyData);

        GetCharacterDataRequest request = new GetCharacterDataRequest();
        //request.PlayFabId = PlayFabSettings.PlayerId;
        request.CharacterId = characterId;
        request.Keys = new List<string>() { "CharacterData" };

        PlayFabClientAPI.GetCharacterReadOnlyData(request, (result) =>
        {
            if (result.Data.ContainsKey("CharacterData"))
            {
                playerCharacterData[result.CharacterId] = PlayFabSimpleJson.DeserializeObject<FG_CharacterData>(result.Data["CharacterData"].Value);

                PF_Bridge.RaiseCallbackSuccess("", PlayFabAPIMethods.GetCharacterReadOnlyData, MessageDisplayStyle.none);
            }

        }, PF_Bridge.PlayFabErrorCallback);
    }

    public static void GetPlayerCharacters(Action callback = null)
    {
        ListUsersCharactersRequest request = new ListUsersCharactersRequest();

        //PlayFabClientAPI.GetAllUsersCharacters(request, OnGetPlayerCharactersSuccess, PF_Bridge.PlayFabErrorCallback);
        PlayFabClientAPI.GetAllUsersCharacters(request, (result) => 
        {
            playerCharacters = result.Characters;

            if (callback != null)
                callback();

            PF_Bridge.RaiseCallbackSuccess("Player Characters Retrieved", PlayFabAPIMethods.GetAllUsersCharacters, MessageDisplayStyle.none);
        }
        , PF_Bridge.PlayFabErrorCallback);
    }

    public static void OnGetPlayerCharactersSuccess(ListUsersCharactersResult result)
    {
        playerCharacters = result.Characters;
        PF_Bridge.RaiseCallbackSuccess("Player Characters Retrieved", PlayFabAPIMethods.GetAllUsersCharacters, MessageDisplayStyle.none);
    }

    public static void CreateNewCharacter(string name, FG_ClassDetail details)
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.GrantCharacterToUser);
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
        request.FunctionName = "CreateCharacter";
        request.FunctionParameter = new { catalogCode = details.CatalogCode, characterName = name };
        PlayFabClientAPI.ExecuteCloudScript(request, OnCreateNewCharacterSuccess, PF_Bridge.PlayFabErrorCallback);
    }

    public static void OnCreateNewCharacterSuccess(ExecuteCloudScriptResult result)
    {
        if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
            return;

        if ((bool)result.FunctionResult)
        {
            PF_Bridge.RaiseCallbackSuccess("New Character Added", PlayFabAPIMethods.GrantCharacterToUser, MessageDisplayStyle.none);
        }
        else
        {
            PF_Bridge.RaiseCallbackError("Error Creating Character" + result.Logs.ToString(), PlayFabAPIMethods.GrantCharacterToUser, MessageDisplayStyle.error);
        }
    }

    public static void DrawCharactersToUser()
    {
        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.DrawCharacterToUser);
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
        request.FunctionName = "DrawCharacters";
        request.FunctionParameter = new { times = 2 };
        PlayFabClientAPI.ExecuteCloudScript(request, OnDrawCharactersSuccess, PF_Bridge.PlayFabErrorCallback);
    }

    public static void OnDrawCharactersSuccess(ExecuteCloudScriptResult result)
    {
        if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
            return;
        if(result.FunctionResult.ToString() != "false")
        {
            List<string> grantedName = PlayFabSimpleJson.DeserializeObject<List<string>>(result.FunctionResult.ToString());
            PF_Bridge.RaiseCallbackSuccess(grantedName[0], PlayFabAPIMethods.DrawCharacterToUser, MessageDisplayStyle.none);
        }
        else
        {
            PF_Bridge.RaiseCallbackSuccess("you dont have enough money to draw", PlayFabAPIMethods.DrawCharacterToUser, MessageDisplayStyle.none);
        }

    }

    #endregion

    #region misc
    public static void TransferItemToPlayer(string sourceId, string instanceId, Action callback = null)
    {
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
        request.FunctionName = "TransferItemToPlayer";
        request.FunctionParameter = new { sourceId = sourceId, instanceId = instanceId };
        PlayFabClientAPI.ExecuteCloudScript(request, (ExecuteCloudScriptResult result) => {
            if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
                return;

            if (callback != null)
            {
                callback();
            }
        }, PF_Bridge.PlayFabErrorCallback);
    }

    public static void TransferItemToCharacter(string sourceId, string sourceType, string instanceId, string destId, Action callback = null)
    {
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
        request.FunctionName = "TransferItemToCharacter";
        request.FunctionParameter = new { sourceId = sourceId, sourceType = sourceType, destId = destId, instanceId = instanceId };
        PlayFabClientAPI.ExecuteCloudScript(request, (ExecuteCloudScriptResult result) => {
            if (!PF_Bridge.VerifyErrorFreeCloudScriptResult(result))
                return;

            if (callback != null)
            {
                callback();
            }
        }, PF_Bridge.PlayFabErrorCallback);
    }
    #endregion
}
