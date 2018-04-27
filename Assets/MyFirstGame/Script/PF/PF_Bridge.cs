﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using PlayFab;
using System.Linq;

public static class PF_Bridge
{

    // signature & callback for rasing errors, the enum can be useful for tracking what API call threw the error. 
    public delegate void PlayFabErrorHandler(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle);
    public static event PlayFabErrorHandler OnPlayFabCallbackError;

    // called after a successful API callback (useful for stopping the spinner)
    public delegate void CallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle);
    public static event CallbackSuccess OnPlayfabCallbackSuccess;

    /// <summary>
    ///  The standard way to notify listeners that a PF call has completed successfully
    /// </summary>
    /// <param name="details"> 	a string that can be used so send any additional custom information </param>
    /// <param name="method"> enum that maps to the call that just completed successfully </param>
    /// <param name="style"> error will throw the standard error box, none will eat the message and output to console </param>
    public static void RaiseCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle style)
    {
        if (OnPlayfabCallbackSuccess != null)
        {
            OnPlayfabCallbackSuccess(details, method, style);
        }
    }

    /// <summary>
    /// The standard way to notify listeners that a PF call has failed
    /// </summary>
    /// <param name="details"> a string that can be used so send any additional custom information </param>
    /// <param name="method"> enum that maps to the call that just completed successfully </param>
    /// <param name="style"> error will throw the standard error box, none will eat the message and output to console </param>
    public static void RaiseCallbackError(string details, PlayFabAPIMethods method, MessageDisplayStyle style)
    {
        if (OnPlayFabCallbackError != null)
        {
            OnPlayFabCallbackError(details, method, style);
        }
    }

    /// <summary>
    /// Standard, reusable error callback
    /// </summary>
    /// <param name="error">Error.</param>
    public static void PlayFabErrorCallback(PlayFab.PlayFabError error)
    {
        if (OnPlayFabCallbackError != null)
        {

            OnPlayFabCallbackError(error.ErrorMessage, PlayFabAPIMethods.Generic, MessageDisplayStyle.error);
        }
    }


    public static bool VerifyErrorFreeCloudScriptResult(ExecuteCloudScriptResult result)
    {
        if (result.Error != null)
        {
            for (int i = 0; i < result.Logs.Count; i++)
            {
                Debug.LogError(result.Logs[i].Data);
            }
            OnPlayFabCallbackError(string.Format("{0}: ERROR: [{1}] -- {2}", result.FunctionName, result.Error.Error, result.Error.Message), PlayFabAPIMethods.ExecuteCloudScript, MessageDisplayStyle.error);
            return false;
        }
        return true;
    }

    //	/// <summary>
    //	/// Makes sure we have a proper API endpoint to access Cloud Script
    //	/// </summary>
    //	/// <returns><c>true</c>, if cloud script was setup, <c>false</c> otherwise.</returns>
    //	/// <param name="cb">callback to run after check is completed</param>
    //	public static bool SetupCloudScript(Action cb = null)
    //	{
    //		if(string.IsNullOrEmpty(PlayFabSettings.LogicServerURL))
    //		{
    //			PlayFabClientAPI.GetCloudScriptUrl(new GetCloudScriptUrlRequest(), (s) => 
    //			{ 
    //				PF_Bridge.RaiseCallbackSuccess("CloudScript URL Loaded", PlayFabAPIMethods.GetCloudScriptUrl, MessageDisplayStyle.none);
    //				if(cb != null)
    //				{
    //					cb();
    //				}
    //			}, 
    //			PlayFabErrorCallback);
    //			return false;
    //		} 
    //		else
    //		{
    //			return true;
    //		}
    //	}

    /// <summary>
    /// Validates the android IAP completed through the GooglePlay store.
    /// </summary>
    /// <param name="json">Json.</param>
    /// <param name="sig">Sig.</param>
    public static void ValidateAndroidPurcahse(string json, string sig)
    {
        ValidateGooglePlayPurchaseRequest request = new ValidateGooglePlayPurchaseRequest();
        request.Signature = sig;
        request.ReceiptJson = json;

        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.ValidateIAP);
        PlayFabClientAPI.ValidateGooglePlayPurchase(request, (ValidateGooglePlayPurchaseResult result) =>
        {
            Debug.Log("Reciept Validated!!");
            PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.ValidateIAP, MessageDisplayStyle.none);
        }, PF_Bridge.PlayFabErrorCallback);


    }

    public static void ValidateIosPurchase(string receipt)
    {
        ValidateIOSReceiptRequest request = new ValidateIOSReceiptRequest();
        request.ReceiptData = receipt;

        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.ValidateIAP);
        PlayFabClientAPI.ValidateIOSReceipt(request, (ValidateIOSReceiptResult result) =>
        {
            Debug.Log("Reciept Validated!!");
            PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.ValidateIAP, MessageDisplayStyle.none);
        }, PF_Bridge.PlayFabErrorCallback);
    }


    //	Used for sending analytics data back into PlayFab
    public enum CustomEventTypes
    {
        none,
        Client_LevelUp,
        Client_LevelComplete,
        Client_PlayerDied,
        Client_BossKill,
        Client_UnicornFreed,
        Client_StoreVisit,
        Client_SaleClicked,
        Client_BattleAborted,
        Client_RegisteredAccount,
        Client_FriendAdded
    }

    /// <summary>
    /// Logs the custom event.
    /// </summary>
    /// <param name="eventName">Event name.</param>
    /// <param name="eventData">Event data.</param>
    public static void LogCustomEvent(CustomEventTypes eventName, Dictionary<string, object> eventData)
    {
        WriteClientPlayerEventRequest request = new WriteClientPlayerEventRequest();
        request.Body = eventData;
        request.EventName = eventName.ToString();
        PlayFabClientAPI.WritePlayerEvent(request, null, PlayFabErrorCallback);

        /* EXAMPLE OF eventData
			new Dictionary<string,object >() 
			{
				{"monsters_killed", obj.kills},
				{"gold_won", obj.currency},
				{"result", "win" }  
			};
		*/
    }
}

public class UpdatedVCBalance
{
    public string VirtualCurrency { get; set; }
    public int Balance { get; set; }
}

public class OutgoingAPICounter
{
    public float outgoingGameTime;
    public PlayFabAPIMethods method;
}

//[Flags] // need to add in Po2 numbers
//http://stackoverflow.com/questions/1030090/how-do-you-pass-multiple-enum-values-in-c
// think about how to log to console / ui log maybe another enum for this
// could aslo add something like errorWithRetry,
public enum MessageDisplayStyle
{
    none,
    success,
    context,
    error
}


//TODO uncomment this out when we  
// An enum that maps to the PlayFab calls for tracking messages passed around the app
//#region API Enum
public enum PlayFabAPIMethods
{
    Null,
    Generic,
    GenericLogin,
    GenericCloudScript,
    ExecuteCloudScript,
    RegisterPlayFabUser,
    LoginWithPlayFab,
    LoginWithDeviceId,
    LoginWithFacebook,
    GetAccountInfo,
    GetCDNConent,
    GetTitleData,
    GetTitleNews,
    GetCloudScriptUrl,
    GetAllUsersCharacters,
    GetCharacterData,
    GetCharacterReadOnlyData,
    GetCharacterInventory,
    GetUserStatistics,
    GetCharacterStatistics,
    GetPlayerLeaderboard,
    GetFriendsLeaderboard,
    GetMyPlayerRank,
    GetUserData,
    GetUserInventory,
    GetOffersCatalog,
    UpdateUserStatistics,
    UpdateCharacterStatistics,
    GetStoreItems,
    GrantCharacterToUser,
    DeleteCharacter,
    UpdateDisplayName,
    SendAccountRecoveryEmail,
    SavePlayerInfo,
    RetriveQuestItems,
    RegisterForPush,
    AddUsernamePassword,
    LinkDeviceID,
    LinkFacebookId,
    LinkGameCenterId,
    UnlinkAndroidDeviceID,
    UnlinkIOSDeviceID,
    UnlinkFacebookId,
    UnlinkGameCenterId,
    UnlockContainerItem,
    UpdateUserData,
    AddFriend,
    RemoveFriend,
    RedeemCoupon,
    SetFriendTags,
    GetFriendList,
    GetCharacterLeaderboard,
    GetLeaderboardAroundCharacter,
    ConsumeOffer,
    ValidateIAP,
    ConsumeItemUse,
    UnlockContainer,
    MakePurchase,
    DrawCharacterToUser
}
//#endregion
