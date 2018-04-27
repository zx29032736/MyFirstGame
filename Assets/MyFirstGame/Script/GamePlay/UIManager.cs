using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIManager : MonoBehaviour {
    public enum UiEvent { EnableCharacterStat, DisableCharacterStat}
    public delegate void OnUiEvent(string messege, UiEvent uiEvent);
    public static event OnUiEvent onUiEventCalled;

    public RectTransform myRectTransform;

    public GamePlayActionBar ActionBar;

    public Transform healthBarParent;
    public CharacterHealthBar healBarPrefab;

    public CharacterUnitController unitController;
    public GamePlayManager playManager;
    public GameStatUIController StateUiController;
    public OrderBarController orderBarController;
    public CharacterStatDisplayer statDisplayer;

    private void Awake()
    {
        ActionBar.uiManager = this;
        myRectTransform = GetComponent<RectTransform>();
    }

    public static void RaiseUiEvent(string messege, UiEvent uiEvent)
    {
        if (onUiEventCalled != null)
            onUiEventCalled(messege, uiEvent);
    }

    private void OnEnable()
    {
        GamePlayManager.OnGameplayEvent += OnGameplayEventReceive;
        UIManager.onUiEventCalled += UIManager_onUiEventCalled;
    }

    private void OnDisable()
    {
        GamePlayManager.OnGameplayEvent -= OnGameplayEventReceive;
        UIManager.onUiEventCalled -= UIManager_onUiEventCalled;

    }

    private void UIManager_onUiEventCalled(string messege, UiEvent uiEvent)
    {
        if(uiEvent == UiEvent.EnableCharacterStat)
        {
            statDisplayer.Init(unitController.allCharacterDictionary[messege]);
        }
        if (uiEvent == UiEvent.DisableCharacterStat)
        {
            statDisplayer.Close();
        }
    }

    private void OnGameplayEventReceive(string message, GamePlayManager.GameplayEvent type)
    {
        if(type == GamePlayManager.GameplayEvent.IntroQuest)
        {
            //string levelname = (string)PhotonNetwork.room.CustomProperties["Level"];
            //OpenConfirmPanel(PF_GameData.Levels[levelname].Acts.Values.ToList()[0].IntroMonolog);
        }
        if(type == GamePlayManager.GameplayEvent.StartQuest)
        {
            GenerateHealthBar();
            orderBarController.Init(this.unitController);
            Debug.Log(" Geenerate Health Bar !!!");
        }

        if(type == GamePlayManager.GameplayEvent.MyPlayerPreMove)
        {
            if (unitController.currentUnit == null)
            {
                return;
            }
            if (GamePlayManager.IsMultiPlayer)
            {

                if (playManager.IsPhotonPlayersTurn(unitController.currentUnit.name))
                    ActionBar.Init(unitController.currentUnit);
            }
            else
            {
                ActionBar.Init(unitController.currentUnit);
            }
        }

        if(type == GamePlayManager.GameplayEvent.MyPlayerPreAttack)
        {
            if (unitController.currentUnit == null)
                return;
            if (GamePlayManager.IsMultiPlayer)
            {
                if (unitController.currentUnit.name.Contains(PhotonNetwork.player.UserId))
                {
                    ActionBar.EnableSkills();
                }
            }
            else
            {
                ActionBar.EnableSkills();
            }
                
        }

        if(type == GamePlayManager.GameplayEvent.TurnEnd)
        {
            ActionBar.Close();
            orderBarController.RefreshBar();
        }

        if(type == GamePlayManager.GameplayEvent.OutroAct)
        {
            PF_GamePlay.OutroPane(healthBarParent.gameObject, 0);
        }

    }


    public void GenerateHealthBar()
    {
        foreach(var characterUnit in unitController.allCharacterUnit)
        {
            CharacterHealthBar go = Instantiate(healBarPrefab, Vector3.zero, Quaternion.identity, healthBarParent);
            go.Init(characterUnit, myRectTransform);
            go.GetComponent<RectTransform>().anchoredPosition = WorldToUI(myRectTransform, new Vector3(go.transform.position.x - 0.5f, go.transform.position.y - 0.5f, go.transform.position.z));
            if (characterUnit.tag.Contains("Player"))
                StateUiController.InitTeamHealthUI(characterUnit);
        }
    }

    void ShowCharacterStatUI(string characterName)
    {
        Unit unit = unitController.allCharacterDictionary[characterName];
    }

    public void RaiseSetting()
    {
        DialogCanvasController.RequestSettingPrompt();
    }

    static public Vector2 WorldToUI(RectTransform r, Vector3 pos)
    {
        Vector2 screenPos = Camera.main.WorldToViewportPoint(pos); //世界物件在螢幕上的座標，螢幕左下角為(0,0)，右上角為(1,1)
        Vector2 viewPos = (screenPos - r.pivot) * 2; //世界物件在螢幕上轉換為UI的座標，UI的Pivot point預設是(0.5, 0.5)，這邊把座標原點置中，並讓一個單位從0.5改為1
        float width = r.rect.width / 2; //UI一半的寬，因為原點在中心
        float height = r.rect.height / 2; //UI一半的高
        return new Vector2(viewPos.x * width, viewPos.y * height); //回傳UI座標
    }

    static public Vector3 UIToWorld(RectTransform r, Vector3 uiPos)
    {
        float width = r.rect.width / 2; //UI一半的寬
        float height = r.rect.height / 2; //UI一半的高
        Vector3 screenPos = new Vector3(((uiPos.x / width) + 1f) / 2, ((uiPos.y / height) + 1f) / 2, uiPos.z); //須小心Z座標的位置
        return Camera.main.ViewportToWorldPoint(screenPos);
    }
}
