using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionCanvasController : MonoBehaviour {

    public GamePlayManager playManager;
    public ActIntroController actIntroObject;
    public ActOutroController actOutroObject;

    private void OnEnable()
    {
        GamePlayManager.OnGameplayEvent += OnGameplayEventReceive;
    }

    private void OnDisable()
    {
        GamePlayManager.OnGameplayEvent -= OnGameplayEventReceive;
    }

    private void OnGameplayEventReceive(string message, GamePlayManager.GameplayEvent type)
    {
        if (type == GamePlayManager.GameplayEvent.IntroQuest)
        {
            //PF_GamePlay.IntroPane(actIntroObject.gameObject, 0);
            actIntroObject.ShowInfo();
        }

        if(type == GamePlayManager.GameplayEvent.OnAllPlayerReady)
        {
            actIntroObject.RefreshConfirmPanel();
        }

        if(type == GamePlayManager.GameplayEvent.OutroAct)
        {
            actOutroObject.UpdateQuestStats();
            PF_GamePlay.IntroPane(actOutroObject.gameObject,0);
        }
    }

}
