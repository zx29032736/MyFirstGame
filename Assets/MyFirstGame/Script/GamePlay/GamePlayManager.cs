using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using DG.Tweening;
using System.Linq;

public class GamePlayManager :Photon.PunBehaviour
{
    public enum GameplayEvent { IntroQuest, StartQuest, IntroAct, OutroAct, PreMoveState, MoveState, PreAttackState, AttackState, TurnEnd, PlayerDied, EnemyDied, MyPlayerPreMove, MyPlayerPreAttack, OnAllPlayerReady, ShowStatUI }

    public static bool IsMultiPlayer { get { return PhotonNetwork.connected; } }
    public int currentTurn = 1;
    public bool isOver;

    public TileMap map;

    public GameplayEvent myEvent;
    public Hashtable datas = new Hashtable();

    public GameConditions conditions = new GameConditions();

    public delegate void GameplayEventHandller(string message, GameplayEvent type);
    public static event GameplayEventHandller OnGameplayEvent;

    public UIManager uiManager;
    public CharacterUnitController unitControlller;


    private void OnEnable()
    {
        OnGameplayEvent += OnGameplayEventReceived;
    }

    private void OnDisable()
    {
        OnGameplayEvent -= OnGameplayEventReceived;
    }

    private void Start()
    {
        Init();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
             RaiseGameplayEvent("", GameplayEvent.PreMoveState);
            //GamePlayManager.RaiseGameplayEvent("Intro Quest", GameplayEvent.IntroQuest);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log(PF_GameData.Levels["The Cave"].Acts["One Way Out..."].MapData.Count);
        }
    }
    void OnGameplayEventReceived(string message, GameplayEvent type)
    {
        if(type == GameplayEvent.IntroQuest)
        {
            //RaiseGameplayEvent("intro act", GameplayEvent.IntroAct);
            myEvent = GameplayEvent.IntroQuest;
            //string levelname = (string)PhotonNetwork.room.CustomProperties["Level"];
            //uiManager.OpenConfirmPanel(PF_GameData.Levels[levelname].Acts.Values.ToList()[0].IntroMonolog);
            //initialize all
        }

        if (type == GameplayEvent.StartQuest)
        {
            myEvent = GameplayEvent.StartQuest;
            RaiseGameplayEvent("", GameplayEvent.PreMoveState);
        }

        if(type == GameplayEvent.PreMoveState)
        {
            myEvent = GameplayEvent.PreMoveState;
        }

        if(type == GameplayEvent.MoveState)
        {
            myEvent = GameplayEvent.MoveState;

        }

        if (type == GameplayEvent.PreAttackState)
        {
            myEvent = GameplayEvent.PreAttackState;

        }

        if (type == GameplayEvent.AttackState)
        {
            myEvent = GameplayEvent.AttackState;
        }

        if(type == GameplayEvent.TurnEnd)
        {
            myEvent = GameplayEvent.TurnEnd;
        }

        if(type == GameplayEvent.MyPlayerPreMove)
        {
            map.ShowTilesCanBereached();
        }

        if(type == GameplayEvent.PreAttackState)
        {
            map.DisableTileMask();
        }
    }

    public static void RaiseGameplayEvent(string message, GameplayEvent type)
    {
        if(OnGameplayEvent != null)
        {
            OnGameplayEvent(message, type);
        }
    }

    public void PhotonInitialize()
    {
        PhotonNetwork.OnEventCall += this.OnEvent;//不能Start Awake呼叫
    }

    void Init()
    {

        //if (PhotonNetwork.connected)
        //    GamePlayManager.IsMultiPlayer = true;
        //else
        //    GamePlayManager.IsMultiPlayer = false;

        GetLevelData();
        GetAllPlayerUnitData();
        if (PF_GamePlay.ActiveQuest != null && PF_GamePlay.AllSavedCharacterUnitData != null)
            GamePlayManager.RaiseGameplayEvent("Intro Quest", GameplayEvent.IntroQuest);
    }

    #region custom room properties
     
    public void RefreshRoomProperties(GameplayEvent state)
    {


        var chaPos = new Dictionary<string, CharacterPos>();
        var allSaved = new Dictionary<string, FG_SavedCharacter>();
        foreach (var go in unitControlller.allCharacterUnit)
        {
            chaPos.Add(go.name, new CharacterPos() { TileX = go.tileX, TileY = go.tileY});
            allSaved.Add(go.name,go.savedCharacter);
        }
        GameConditions cc = new GameConditions()
        {
            State = state,
            CurrentChaName = unitControlller.allCharacterUnit[unitControlller.currentId].name,
            TargetChaName = unitControlller.allCharacterUnit[unitControlller.currentId].attackTarget == null ? "" : unitControlller.allCharacterUnit[unitControlller.currentId].attackTarget.name,
            MoveToTileX = unitControlller.allCharacterUnit[unitControlller.currentId].currentPath == null ? unitControlller.allCharacterUnit[unitControlller.currentId].tileX : unitControlller.allCharacterUnit[unitControlller.currentId].currentPath[1].x,
            MoveToTileY = unitControlller.allCharacterUnit[unitControlller.currentId].currentPath == null ? unitControlller.allCharacterUnit[unitControlller.currentId].tileY : unitControlller.allCharacterUnit[unitControlller.currentId].currentPath[1].y,
            RemainingMovement = unitControlller.allCharacterUnit[unitControlller.currentId].remainingMovement,
            SelectedSpell = unitControlller.allCharacterUnit[unitControlller.currentId].selectedSpell == null ? "" : unitControlller.allCharacterUnit[unitControlller.currentId].selectedSpell.SpellName,
            AllSavedChaharacterData = allSaved,
            AllCharacterPos = chaPos,
        };

        if (GamePlayManager.IsMultiPlayer)
        {
            Hashtable hs = new Hashtable();
            hs.Add("Conditions", PlayFab.Json.PlayFabSimpleJson.SerializeObject(cc));
            PhotonNetwork.room.SetCustomProperties(hs, null, false);
        }
        else
        {
            this.conditions = cc;

            if(state == GameplayEvent.PreMoveState)
            {
                RaiseGameplayEvent("", GameplayEvent.MoveState);
            }
            else if(state == GameplayEvent.PreAttackState)
            {
                RaiseGameplayEvent("", GameplayEvent.AttackState);
            }
            else if(state == GameplayEvent.TurnEnd)
            {
                System.Action action = () => { RaiseGameplayEvent("", GameplayEvent.PreMoveState); };
                StartCoroutine(Wait(action, 1));
            }
            else
            {
                if (myEvent != conditions.State)
                    RaiseGameplayEvent("", state);
            }
        }

    }

    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        datas = propertiesThatChanged;
        
        if (datas["Conditions"] != null)
        {
            conditions = new GameConditions();
            conditions = (GameConditions)PlayFab.Json.PlayFabSimpleJson.DeserializeObject<GameConditions>((string)datas["Conditions"]);
        }

        if (myEvent != conditions.State)
        {
            myEvent = conditions.State;
            RaiseGameplayEvent(conditions.CurrentChaName, myEvent);
        }
        switch (myEvent)
        {
            case GameplayEvent.IntroQuest:
                Debug.Log("Game Staet State form room properties");
                break;
            case GameplayEvent.PreMoveState:
                if (PhotonNetwork.isMasterClient && !unitControlller.currentUnit.tag.Contains("Player"))
                {
                    System.Action action = () => { RefreshRoomProperties( GameplayEvent.MoveState); };
                    StartCoroutine(Wait(action, 0.25f));
                }
                break;
            case GameplayEvent.MoveState:
                break;

            case GameplayEvent.PreAttackState:

                if (PhotonNetwork.isMasterClient && !unitControlller.currentUnit.tag.Contains("Player"))
                {
                    System.Action action = () => { RefreshRoomProperties(GameplayEvent.AttackState); };
                    StartCoroutine(Wait(action, 0.25f));
                }

                if (PhotonNetwork.isMasterClient)
                {
                    if (unitControlller.currentUnit.tag.Contains("Player") && unitControlller.allCharacterUnit[unitControlller.currentId].attackTarget != null)
                    {
                        System.Action action = () => { RefreshRoomProperties(GameplayEvent.AttackState); };
                        StartCoroutine(Wait(action, 0.25f));
                    }
                }
                break;

            case GameplayEvent.AttackState:

                break;
            case GameplayEvent.TurnEnd:
                if (PhotonNetwork.isMasterClient)
                {
                    System.Action action = () => { RefreshRoomProperties(GameplayEvent.PreMoveState); };
                    StartCoroutine(Wait(action, 1));
                }
                break;
            case GameplayEvent.OutroAct:

                //PhotonNetwork.Disconnect();
                Debug.Log("Game Over and disconnected photon");
                break;
            //case States.GameWin:

            //    Debug.LogError("game win !!!");
            //    PhotonNetwork.Disconnect();
            //    SceneController.Instance.RequestSceneChange(SceneController.GameScenes.Profile);
            //    break;
        }
    }
    #endregion

    public bool CheckIsQuestCompleted()
    {
        if (unitControlller.allCharacterUnit.Find(x => x.tag.Contains("Enemy")) == null)
        {
            return true;
        }
        else
            return false;
    }

    public void SetPlayerAttackTarget(Unit target)
    {
        if (target == null)
            return;

        if (GamePlayManager.IsMultiPlayer)
        {
            if (unitControlller.currentUnit.name.Contains(PhotonNetwork.player.UserId))
            {
                string[] data = new string[] { target.name, unitControlller.currentUnit.selectedSpell.SpellName };
                //uiManager.ActionBar.selectedSpellSlot.EnableCD(3);
                PhotonNetwork.RaiseEvent(102, data, true, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient });
            }
        }
        else
        {
            unitControlller.currentUnit.attackTarget = target;
            IsSpellApplyStatus(unitControlller.currentUnit.attackTarget, unitControlller.currentUnit.selectedSpell);
            if (unitControlller.currentUnit.attackTarget != null)
                RaiseGameplayEvent("", GameplayEvent.AttackState);
        }
    }

    public void ShowTargetBySpellRange(FG_Spell spell)
    {
        //需要同步allcharacterunit的master and clinet 的排序

        if (spell == null)
            return;

        if (PhotonNetwork.isMasterClient || IsMultiPlayer)
        {
            map.ShowSpellRange();
            foreach (var go in unitControlller.allCharacterUnit)
            {
                if (go.selectedMask == null)
                    continue;

                if (spell.RangeX.Contains(go.tileX - unitControlller.allCharacterUnit[unitControlller.currentId].tileX) && spell.RangeY.Contains(go.tileY - unitControlller.allCharacterUnit[unitControlller.currentId].tileY))
                {
                    go.selectedMask.SetActive(true);
                }
            }
        }
        else //(!PhotonNetwork.isMasterClient)
        {
            map.ShowSpellRange();
            foreach (var go in unitControlller.allCharacterUnit)
            {
                if (go.selectedMask == null)
                    continue;

                if (spell.RangeX.Contains(go.tileX - map.selectedUnit.tileX) && spell.RangeY.Contains(go.tileY - map.selectedUnit.tileY))
                {
                    go.selectedMask.SetActive(true);
                }
            }
        }
    }

    public void DisableTargetMask()
    {
        map.DisableSpellRnage();
        foreach (var go in unitControlller.allCharacterUnit)
        {
            if (go.selectedMask == null)
                continue;
            go.selectedMask.SetActive(false);
        }
    }

    public static IEnumerator Wait(System.Action action,float sec)
    {
        yield return new WaitForSeconds(sec);
        action();
    }

    #region On Event Call
    int ready_count = 0;
    List<int> readied_id = new List<int>();
    void OnEvent(byte eventcode, object content, int senderid)
    {
        if(eventcode == 100)
        {
            if (readied_id.Contains(senderid))
                return;

            if (!readied_id.Contains(senderid))
            {
                readied_id.Add(senderid);
                ready_count++;
            }

            Debug.Log("eventcode == 100 __ " + ready_count);

            if (ready_count == PhotonNetwork.playerList.Length)
            {
                //UIManager.instance.RefreshConfirmPanel(true);
                RaiseGameplayEvent("", GameplayEvent.OnAllPlayerReady);
            }
        }
        else if (eventcode == 101)
        {
            if (PhotonNetwork.isMasterClient)
            {
                Vector2 v2 = (Vector2)content;
                map.GeneratePathTo((int)v2.x, (int)v2.y);
                RefreshRoomProperties(GameplayEvent.MoveState);
            }
        }

        else if(eventcode == 102)
        {
            string[] s = (string[])content;

            if (PhotonNetwork.isMasterClient)
            {
                Unit target = unitControlller.allCharacterDictionary[s[0]];//unitControlller.allCharacterUnit.Find(x => x.name.Contains(s[0]));

                if (target == null)
                    return;
                unitControlller.currentUnit.attackTarget = target;
                unitControlller.currentUnit.RandomSpell(s[1]);
                IsSpellApplyStatus(unitControlller.currentUnit.attackTarget, unitControlller.currentUnit.selectedSpell);
                RefreshRoomProperties( GameplayEvent.AttackState);
            }
        }

        else if(eventcode == 103)
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (myEvent == GameplayEvent.PreMoveState)
                {
                    RefreshRoomProperties( GameplayEvent.PreAttackState);
                }
                else if (myEvent ==  GameplayEvent.PreAttackState)
                {
                    RefreshRoomProperties( GameplayEvent.TurnEnd);
                }
            }
        }
    }

    #endregion

    void IsSpellApplyStatus(Unit unit,FG_Spell spell)
    {
        if (spell.ApplyStatus == null)
            return;

        foreach(var go in unitControlller.UintInSpellRange())
        {
            float rng = Random.Range(0f, 1f);
            bool success = (spell.ApplyStatus.ChanceToApply >= rng) ? true : false;
            FG_SpellStatus activeStatus = go.savedCharacter.PlayerVitals.ActiveStati.Find(x => x.StatusName.Contains(spell.ApplyStatus.StatusName));
            if (success && activeStatus == null)
            {
                FG_SpellStatus status = new FG_SpellStatus()
                {
                    ChanceToApply = spell.ApplyStatus.ChanceToApply,
                    FX = spell.ApplyStatus.FX,
                    Icon = spell.ApplyStatus.Icon,
                    ModifyAmount = spell.ApplyStatus.ModifyAmount,
                    StatModifierCode = spell.ApplyStatus.StatModifierCode,
                    StatusDescription = spell.ApplyStatus.StatusDescription,
                    StatusName = spell.ApplyStatus.StatusName,
                    Target = spell.ApplyStatus.Target,
                    Turns = spell.ApplyStatus.Turns
                };

                go.savedCharacter.PlayerVitals.ActiveStati.Add(status);
                unitControlller.spellController.TriggerImmediately(go, go.savedCharacter.PlayerVitals.ActiveStati.Count - 1);

                if (status.Target.Contains("Player"))
                {
                    go.EnableBuff(true);
                }
                else if (status.Target.Contains("Enemy"))
                {
                    go.EnableBuff(false);
                }

                Debug.Log("rng : " + rng + " | ChanceToApply : " + spell.ApplyStatus.ChanceToApply);

            }
            else if (success && activeStatus != null)
            {
                Debug.Log("unit is under effecting");
            }
            else
            {
                Debug.Log("Spell status did not trigged !");
            }
        }
       
    }

    public void GetLevelData()
    {
        if (IsMultiPlayer)
        {
            ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
            hs = PhotonNetwork.room.CustomProperties;
            if (hs.ContainsKey("Level"))
            {
                PF_GamePlay.ActiveQuest = new FG_LevelData();
                PF_GamePlay.ActiveQuest = PF_GameData.Levels[(string)hs["Level"]];

            }
            else
            {
                Debug.LogError(" level is not include ");
            }
        }
        else
        {
            //if (PF_GamePlay.ActiveQuest != null)
            //else
            //{
            //    Debug.LogError(" PF_gameplay do not get the data ");
            //}
        }
    }

    public void GetAllPlayerUnitData()
    {
        if (IsMultiPlayer)
        {
            ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
            hs = PhotonNetwork.room.CustomProperties;
            if (hs.ContainsKey("Players"))
            {
                PF_GamePlay.AllSavedCharacterUnitData = new Dictionary<string, FG_SavedCharacter>();
                string[] playerNames = (string[])hs["Players"];
                for (int i = 0; i < playerNames.Length; i++)
                {
                    PhotonPlayer player = PhotonNetwork.playerList.ToList().Find(x => x.UserId.Contains(playerNames[i]));
                    FG_SavedCharacter saved = PlayFab.Json.PlayFabSimpleJson.DeserializeObject<FG_SavedCharacter>((string)player.CustomProperties["SavedData"]);
                    PF_GamePlay.AllSavedCharacterUnitData.Add(playerNames[i], saved);
                }
            }
            else
            {
                Debug.Log("room prperties do not contain the key player");
            }
        }
        else
        {

        }
    }

    public void OnEndMovingClicked()
    {
        if (GamePlayManager.IsMultiPlayer)
        {
            PhotonNetwork.RaiseEvent(103, null, true, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient });
        }
        else
        {
            if (myEvent == GameplayEvent.PreMoveState)
            {
                RaiseGameplayEvent("", GameplayEvent.PreAttackState);
            }
            else if (myEvent == GameplayEvent.PreAttackState)
            {
                RaiseGameplayEvent("", GameplayEvent.TurnEnd);
            }
        }
    }

    public void OnMovingTileClicked(int tileX, int tileY)
    {
        //map.GeneratePathTo(tileX, tileY);
        if (GamePlayManager.IsMultiPlayer)
        {
            PhotonNetwork.RaiseEvent(101, new Vector2(tileX, tileY), true, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient });
        }
        else
        {
            map.GeneratePathTo(tileX, tileY);
            //GamePlayManager.RaiseGameplayEvent("",)
            RefreshRoomProperties(GamePlayManager.GameplayEvent.MoveState);
        }
        map.DisableTileMask();
    }

    public bool IsPhotonPlayersTurn(string str)
    {
        if (!GamePlayManager.IsMultiPlayer)
            return false;

        //if (str.Contains(PhotonNetwork.player.UserId))
        //    return true;

        if (PhotonNetwork.playerList.ToList().Exists( x => x.UserId == str))
            return true;
        else
            return false;
    }
}
