using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PlayFab.ClientModels;

public class DialogCanvasController : Singleton<DialogCanvasController> {

    protected DialogCanvasController() { } // guarantee this will be always a singleton only - can't use the constructor!

    public Transform overlayTint;
    public ErrorPromptController errorPrompt;
    public LoadingPromptController loadingPrompt;
    public SettingPromptController settingPrompt;
    public ItemViewerController itemViewerPrompt;
    public FloatingStoreController floatingStorePrompt;
    public FloatingInventoryController floatingInvPrompt;
    public FloatingCharactersConrtroller floatingChaPrompt;

    public enum InventoryFilters { AllItems, UsableInCombat, Keys, Containers, Material }
    public enum CharacterFilters { AllCharacter, Water, Fire, Grass, Light, Dark, Name }

    public delegate void LoadingPromptHandler(PlayFabAPIMethods method);
    public static event LoadingPromptHandler RaiseLoadingPromptRequest;

    public delegate void SettingPromptHandler();
    public static event SettingPromptHandler RaiseSettingPromptRequest;

    public delegate void ItemViewRequestHandler(List<string> items, bool unpackToPlayer);
    public static event ItemViewRequestHandler RaiseItemViewRequest;

    public delegate void StoreRequestHandler(string storeID);
    public static event StoreRequestHandler RaiseStoreRequest;

    public delegate void InventoryPromptHandler(Action<string> responseCallback, InventoryFilters filter);
    public static event InventoryPromptHandler RaiseInventoryPromptRequest;

    public delegate void CharacterPromptHandler(Action<string> responseCallback, CharacterFilters filter);
    public static event CharacterPromptHandler RaiseCharacterPromptRequest;

    private List<OutgoingAPICounter> waitingOnRequests = new List<OutgoingAPICounter>();

    //Coroutine to manage the 10 second timeout.
    private Coroutine timeOutCallback;
    private float timeOutLength = 10f;

    void OnEnable()
    {
        PF_Bridge.OnPlayFabCallbackError += HandleCallbackError;
        PF_Bridge.OnPlayfabCallbackSuccess += HandleCallbackSuccess;

        PF_Authentication.OnLoginFail += HandleOnLoginFail;
        PF_Authentication.OnLoginSuccess += HandleOnLoginSuccess;

        RaiseLoadingPromptRequest += HandleLoadingPromptRequest;
        RaiseSettingPromptRequest += HandleSettingPromptRequest;
        RaiseItemViewRequest += HandleItemViewerRequest;
        RaiseStoreRequest += HandleStoreRequest;
        RaiseInventoryPromptRequest += HandleInventoryRequest;
        RaiseCharacterPromptRequest += HandleCharacterRequest;

    }

    void OnDisable()
    {
        PF_Bridge.OnPlayFabCallbackError -= HandleCallbackError;
        PF_Bridge.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;

        PF_Authentication.OnLoginFail -= HandleOnLoginFail;
        PF_Authentication.OnLoginSuccess -= HandleOnLoginSuccess;

        RaiseLoadingPromptRequest -= HandleLoadingPromptRequest;
        RaiseSettingPromptRequest -= HandleSettingPromptRequest;
        RaiseItemViewRequest -= HandleItemViewerRequest;
        RaiseStoreRequest -= HandleStoreRequest;
        RaiseInventoryPromptRequest -= HandleInventoryRequest;
        RaiseCharacterPromptRequest -= HandleCharacterRequest;

    }


    public static void RequestLoadingPrompt(PlayFabAPIMethods method)
    {
        if (RaiseLoadingPromptRequest != null)
        {
            RaiseLoadingPromptRequest(method);
        }
    }

    public void HandleLoadingPromptRequest(PlayFabAPIMethods method)
    {
        if (this.waitingOnRequests.Count == 0)
        {
            //ShowTint();
            this.loadingPrompt.RaiseLoadingPrompt();
        }
        this.waitingOnRequests.Add(new OutgoingAPICounter() { method = method, outgoingGameTime = Time.time });

        if (this.timeOutCallback == null)
        {
            this.timeOutCallback = StartCoroutine(OutgoingApiTimeoutCallback());
        }
    }

    public void CloseLoadingPrompt(PlayFabAPIMethods method)
    {
        List<OutgoingAPICounter> waiting = this.waitingOnRequests.FindAll((i) => { return i.method == method; });

        OutgoingAPICounter itemToRemove = null;

        for (int z = 0; z < waiting.Count; z++)
        {
            // in absence of a true GUID request system, we will get the oldest requests to prevent timeouts
            if (itemToRemove != null && waiting[z].outgoingGameTime > itemToRemove.outgoingGameTime)
            {
                // shouldnt be too many times where there are multiple requests of the same type.
                itemToRemove = waiting[z];
            }
            else if (itemToRemove == null)
            {
                //first and likly only match
                itemToRemove = waiting[z];
            }
        }

        if (itemToRemove != null)
        {
            this.waitingOnRequests.Remove(itemToRemove);
            HideTint();
            this.loadingPrompt.CloseLoadingPrompt();
        }
    }

    public void CloseLoadingPromptAfterError()
    {
        this.waitingOnRequests.Clear();
        this.loadingPrompt.CloseLoadingPrompt();
    }

    public static void RequestSettingPrompt()
    {
        if (RaiseSettingPromptRequest != null)
            RaiseSettingPromptRequest();
    }

    public void HandleSettingPromptRequest()
    {
        this.settingPrompt.Init();
    }

    public static void RequestItemViewer(List<string> items, bool unpackToPlayer = false)
    {
        if (RaiseItemViewRequest != null)
        {
            RaiseItemViewRequest(items, unpackToPlayer);
        }
    }

    void HandleItemViewerRequest(List<string> items, bool unpackToPlayer)
    {
        //		UnityAction<List<StoreItem>> afterGetStoreItems = (List<StoreItem> resultSet) => 
        //		{
        //			ShowTint();
        //			this.floatingStorePrompt.InitiateStore(storeID, resultSet);
        //		};
        //		PF_GamePlay.RetriveStoreItems (storeID, afterGetStoreItems);

        this.itemViewerPrompt.InitiateViewer(items, unpackToPlayer);

    }

    public static void RequestStore(string storeID)
    {
        if (RaiseStoreRequest != null)
        {
            RaiseStoreRequest(storeID);
        }
    }

    void HandleStoreRequest(string storeID)
    {
        UnityAction<List<StoreItem>> afterGetStoreItems = (List<StoreItem> resultSet) =>
        {
            // ENABLE THIS AFTER WE HAVE A CONSISTENT WAY TO HIDE TINTS
            //ShowTint();
            this.floatingStorePrompt.InitiateStore(storeID, resultSet);
        };
        PF_GamePlay.RetriveStoreItems(storeID, afterGetStoreItems);

    }

    public static void RequestInventoryPrompt(Action<string> callback = null, InventoryFilters filter = InventoryFilters.AllItems)
    {
        if (RaiseInventoryPromptRequest != null)
        {
            RaiseInventoryPromptRequest(callback, filter);
        }
    }

    void HandleInventoryRequest(Action<string> callback = null, InventoryFilters filter = InventoryFilters.AllItems)
    {

        Action afterGetInventory = () =>
        {
            // ENABLE THIS AFTER WE HAVE A CONSISTENT WAY TO HIDE TINTS
            //ShowTint();
            this.floatingInvPrompt.Init(callback, filter);
        };


        PF_PlayerData.GetUserInventory(afterGetInventory);
       

    }

    public static void RequestCharacterPrompt(Action<string> callback = null, CharacterFilters filter = CharacterFilters.AllCharacter)
    {
        if (RaiseCharacterPromptRequest != null)
        {
            RaiseCharacterPromptRequest(callback, filter);
        }
    }

    void HandleCharacterRequest(Action<string> callback = null, CharacterFilters filter = CharacterFilters.AllCharacter)
    {

        Action afterGetCharacter = () =>
        {
            // ENABLE THIS AFTER WE HAVE A CONSISTENT WAY TO HIDE TINTS
            //ShowTint();

            Action afterGetData = () =>
            {
                this.floatingChaPrompt.Init(callback, filter);
            };

             PF_PlayerData.GetCharacterData(afterGetData);
        };

        PF_PlayerData.GetPlayerCharacters(afterGetCharacter);
    }

    void HandleOnLoginSuccess(string message, MessageDisplayStyle style)
    {
        HandleCallbackSuccess(message, PlayFabAPIMethods.GenericLogin, style);
    }

    void HandleOnLoginFail(string message, MessageDisplayStyle style)
    {
        HandleCallbackError(message, PlayFabAPIMethods.GenericLogin, style);
    }

    public void HandleCallbackError(string details, PlayFabAPIMethods method, MessageDisplayStyle style)
    {
        switch (style)
        {
            case MessageDisplayStyle.error:
                string errorMessage = string.Format("CALLBACK ERROR: {0}: {1}", method, details);
                ShowTint();
                this.errorPrompt.RaiseErrorDialog(errorMessage);
                CloseLoadingPromptAfterError();
                Debug.LogError(" whats wrong");
                break;
            case MessageDisplayStyle.context:
                ShowTint();
                this.errorPrompt.RaiseErrorDialog(details);
                break;
            default:
                CloseLoadingPrompt(method);
                //Debug.Log(string.Format("CALLBACK ERROR: {0}: {1}", method, details));
                break;

        }

    }

    public void HandleCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle style)
    {
        CloseLoadingPrompt(method);
        //Debug.Log(string.Format("{0} completed successfully.", method.ToString()));
    }

    public void ShowTint()
    {
        overlayTint.gameObject.SetActive(true);
    }

    public void HideTint()
    {
        overlayTint.gameObject.SetActive(false);
    }

    public void CloseErrorDialog()
    {
        this.errorPrompt.CloseErrorDialog();
        HideTint();

    }

    private IEnumerator OutgoingApiTimeoutCallback()
    {
        while (this.waitingOnRequests.Count > 0)
        {
            for (var z = 0; z < this.waitingOnRequests.Count; z++)
            {
                if (Time.time > (this.waitingOnRequests[z].outgoingGameTime + this.timeOutLength))
                {
                    // time has elapsed for this request, until we can handle this more specifically, we can only reload the scene, and hope for the best.
                    PlayFabAPIMethods capturedDetails = this.waitingOnRequests[z].method;
                    PF_Bridge.RaiseCallbackError(string.Format("API Call: {0} Timed out after {1} seconds.", capturedDetails, this.timeOutLength), this.waitingOnRequests[z].method, MessageDisplayStyle.error);

                    Action<bool> afterConfirmation = (bool response) => {
                        if (response == false)
                        {
                            // user clicked cancel (to reload);
                            Debug.LogErrorFormat("Reloading scene due {0} API timing out.", capturedDetails);
                            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
                        }
                    };
                    //modifing
                    //DialogCanvasController.RequestConfirmationPrompt("Caution! Bravery Required!", string.Format("API Call: {0} Timed out. \n\tACCEPT: To proceed, may cause client instability. \n\tCANCEL: To reload this scene and hope for the best.", capturedDetails), afterConfirmation);
                }
            }

            // tick once per second while we have outbound requests. (keep enabled while debugging this feature)
            Debug.Log(string.Format("{0}", (int)Time.time % 2 == 0 ? "Tick" : "Tock"));
            yield return new WaitForSeconds(1f);
        }

        // outgoing request queue empty
        this.timeOutCallback = null;
        yield break;
    }
}
