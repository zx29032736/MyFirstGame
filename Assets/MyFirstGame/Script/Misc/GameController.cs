﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using PlayFab.Internal;

public class GameController : Singleton<GameController>
{
    protected GameController() { } // guarantee this will be always a singleton only - can't use the constructor!

    public IconManager iconManager;
    public soundManager soundManager;

    public SceneController sceneController;
    public List<string> TitleDataKeys = new List<string>();
    public Transform sceneControllerPrefab;

    private string pausedOnScene = string.Empty;
    private string lostFocusedOnScene = string.Empty;

    void OnLevelWasLoaded(int index)
    {
        if (sceneController != null)
        {
            string levelName = SceneManager.GetActiveScene().name;
            //Debug.Log ("GAME CONTROLLER ON LEVEL LOADED: " +  Application.loadedLevelName);

            if (levelName.Contains("Authentication") && (sceneController.previousScene != SceneController.GameScenes.Null || sceneController.previousScene != SceneController.GameScenes.Splash))
            {
                //Debug.Log("MADE IT THROUGH THE IF STATEMENT");
                //DialogCanvasController.RequestInterstitial();
                soundManager.PlayMusic(0);

            }
            else if (levelName.Contains("Profile"))
            {
                soundManager.PlayMusic(1);
                Debug.Log("Profile loaded! ");
                //CharacterProfileDataRefresh();
                CharacterSelectDataRefresh();
            }
            else if (levelName.Contains("GamePlay"))
            {
                soundManager.PlayMusic(3);

                //DialogCanvasController.RequestInterstitial();
            }
        }
    }

    public void OnOnApplicationPause(bool status)
    {
        if (status == true && SceneManager.GetActiveScene().buildIndex != 0)
        {
            // application just got paused
            Debug.Log("application just got paused");
            this.pausedOnScene = SceneManager.GetActiveScene().name;
        }
        else
        {
            // application just resumed, go back to the previously used scene.
            Debug.Log("application just resumed, go back to the previously used scene: " + this.pausedOnScene);

            if (SceneManager.GetActiveScene().name != this.pausedOnScene)
            {
                SceneController.Instance.RequestSceneChange((SceneController.GameScenes)System.Enum.Parse(typeof(SceneController.GameScenes), this.pausedOnScene));
            }

        }

    }

    void OnApplicationFocus(bool status)
    {
        if (status == false)
        {
            // application just got paused
            Debug.Log("application just lost focus");
            this.lostFocusedOnScene = SceneManager.GetActiveScene().name;
        }
        else if (status == true)
        {
            // application just resumed, go back to the previously used scene.
            Debug.Log("application just regained focus, go back to the previously used scene: " + this.lostFocusedOnScene);

            if (!string.IsNullOrEmpty(this.lostFocusedOnScene) && SceneManager.GetActiveScene().name != this.lostFocusedOnScene)
            {
                SceneController.Instance.RequestSceneChange((SceneController.GameScenes)System.Enum.Parse(typeof(SceneController.GameScenes), this.pausedOnScene));
            }
        }
    }

    public void ProcessPush()
    {
        //Will contain the code for processing push notifications.
    }

    public static void CharacterSelectDataRefresh()
    {
        //Debug.Log("Ran CharacterSelectDataRefresh");
        PF_PlayerData.GetUserAccountInfo();
        PF_GameData.GetTitleData();
        //PF_GameData.GetTitleNews();
        PF_GameData.GetCatalogInfo();
        //PF_GameData.GetOffersCatalog();

        System.Action action = () =>
        {
            PF_PlayerData.GetCharacterData();
        };

        PF_PlayerData.GetPlayerCharacters(action);
        //PF_PlayerData.GetCharacterData();
        //PF_PlayerData.GetUserStatistics();
    }

    public void CharacterProfileDataRefresh()
    {
        //PF_PlayerData.GetCharacterDataById(PF_PlayerData.activeCharacter.characterDetails.CharacterId);
        //PF_PlayerData.GetCharacterInventory(PF_PlayerData.activeCharacter.characterDetails.CharacterId);
    }

    void Start()
    {

        DontDestroyOnLoad(this.transform.parent.gameObject);

        //base.Awake();
        GameController[] go = GameObject.FindObjectsOfType<GameController>();
        if (go.Length == 1)
        {
            this.gameObject.tag = "GameController";
            //DontDestroyOnLoad(this.gameObject);

            Transform sceneCont = Instantiate(this.sceneControllerPrefab);
            sceneCont.SetParent(this.transform, false);
            this.sceneController = sceneCont.GetComponent<SceneController>();
        }

    }
}
