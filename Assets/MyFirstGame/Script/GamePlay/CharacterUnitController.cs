using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using DG.Tweening;

public class CharacterUnitController : MonoBehaviour {

    public GamePlayManager playManager;
    public TileMap map;
    public SpellActions spellController;
    public Unit unitPre;

    public List<Unit> allCharacterUnit;
    public List<Unit> allPlayerUnit;
    public List<Unit> AllEnemyUnit;
    public Dictionary<string, Unit> allCharacterDictionary = new Dictionary<string, Unit>();
    public Unit currentUnit;
    public int currentId = 0;

    private void OnEnable()
    {
        GamePlayManager.OnGameplayEvent += OnGameplayEventRecived;
    }
    private void OnDisable()
    {
        GamePlayManager.OnGameplayEvent -= OnGameplayEventRecived;
    }

    private void OnGameplayEventRecived(string message, GamePlayManager.GameplayEvent type)
    {

        if(type == GamePlayManager.GameplayEvent.IntroQuest)
        {
            Init();
        }

        if(type == GamePlayManager.GameplayEvent.PreMoveState)
        {
            PreMovement();
            //currentUnit.myFX.gold.enabled = true;
        }

        if(type == GamePlayManager.GameplayEvent.MoveState)
        {
            Movement();
        }

        if(type == GamePlayManager.GameplayEvent.PreAttackState)
        {
            PreAttack();
        }

        if(type == GamePlayManager.GameplayEvent.AttackState)
        {
            Attack();
        }
        
        if(type == GamePlayManager.GameplayEvent.TurnEnd)
        {
            //currentUnit.myFX.gold.enabled = false;

            TurnEnd();
        }
    }

    public void Init()
    {
        //Set up Enemy Data
        List<string> encounters = PF_GamePlay.ActiveQuest.Acts.Values.ToList()[0].Encounters.SpawnSpecificEncountersByID;
        List<List<int>> positions = PF_GamePlay.ActiveQuest.Acts.Values.ToList()[0].Encounters.EncounterSpawnPosition;
        for (int i = 0; i < encounters.Count; i++)
        {
            Unit unit = Instantiate(unitPre, Vector3.zero, Quaternion.identity);
            unit.tag = "Enemy";
            unit.name = "Enemy_"  + encounters[i] + "_" + i;
            unit.transform.position = new Vector2(positions[i][0], positions[i][1]);

            FG_ClassDetail detail = PF_GameData.Classes[encounters[i]];
            FG_SavedCharacter saved = new FG_SavedCharacter()
            {
                baseClass = detail,
                characterData = new FG_CharacterData()
                {
                    Spell1 = detail.Spell1,
                    Spell2 = detail.Spell2,
                    Spell3 = detail.Spell3,
                    Spell4 = detail.Spell4,
                    CharacterLevel = PF_GamePlay.ActiveQuest.Acts.Values.ToList()[0].Encounters.EncountersLevel[i]
                }
            };
            allCharacterUnit.Add(unit);
            AllEnemyUnit.Add(unit);
            allCharacterDictionary.Add(unit.name, unit);
            unit.Init(saved, this);

            map.OnUnitMoving(unit.tileX, unit.tileY, unit.tileX, unit.tileY);
        }


        ///Set Up Player Data
        List<List<int>> playerPositions = PF_GamePlay.ActiveQuest.Acts.Values.ToList()[0].PlayerSpawnPosition;
        for (int i = 0; i < PF_GamePlay.AllSavedCharacterUnitData.Count; i++)
        {
            Unit unit = Instantiate(unitPre);//, new Vector2(4 + i, 4 + i), Quaternion.identity);
            unit.transform.position = new Vector2(playerPositions[i][0], playerPositions[i][1]);
            unit.tag = "Player";
            unit.name = PF_GamePlay.AllSavedCharacterUnitData.Keys.ToList()[i];

            FG_SavedCharacter saved = PF_GamePlay.AllSavedCharacterUnitData[unit.name];
            allCharacterUnit.Add(unit);
            allPlayerUnit.Add(unit);
            allCharacterDictionary.Add(unit.name, unit);
            unit.gameObject.SetActive(true);
            unit.Init(saved, this);

            map.OnUnitMoving(unit.tileX, unit.tileY, unit.tileX, unit.tileY);

        }

        allCharacterUnit.Sort();
    }

    void StartTurn()
    {
        //if (!allCharacterUnit[currentId].isPlayer)
        //{
        //}
    }

    void PreMovement()
    {

        if (PhotonNetwork.isMasterClient || !GamePlayManager.IsMultiPlayer)
        {

            map.selectedUnit = allCharacterUnit[currentId];
            currentUnit = allCharacterUnit[currentId];

            if (currentUnit.tag.Contains("Player"))
            {

                if (playManager.IsPhotonPlayersTurn(currentUnit.name))
                {
                    playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.PreMoveState);

                    if (PhotonNetwork.player.UserId.Contains(currentUnit.name))
                        GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.MyPlayerPreMove);
                }

                if (!PhotonNetwork.isMasterClient)
                {
                    playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.MyPlayerPreMove);
                }

                return;
            }
            else
            {
                currentUnit.SearchTarget();
                playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.PreMoveState);
            }
        }
        else if (!PhotonNetwork.isMasterClient && GamePlayManager.IsMultiPlayer)
        {

            Unit cha = allCharacterDictionary[playManager.conditions.CurrentChaName];//allCharacterUnit.Find(x => x.name == playManager.conditions.CurrentChaName);

            foreach (var go in allCharacterUnit)
            {
                if (playManager.conditions.AllCharacterPos.ContainsKey(go.name))
                    go.transform.position = new Vector2(playManager.conditions.AllCharacterPos[go.name].TileX, playManager.conditions.AllCharacterPos[go.name].TileY);
            }

            map.selectedUnit = cha;
            //cha.InitState();
            currentUnit = cha;

            if (!string.IsNullOrEmpty(playManager.conditions.TargetChaName))
            {
                Unit target = allCharacterDictionary[playManager.conditions.TargetChaName];//allCharacterUnit.Find(x => x.name == playManager.conditions.TargetChaName);
                currentUnit.attackTarget = target;
            }

            if (currentUnit.name.Contains(PhotonNetwork.player.UserId))
            {
                currentUnit.remainingMovement = playManager.conditions.RemainingMovement;
                //UIManager.instance.ActionBar.Init(map.selectedUnit);
                //map.ShowTilesCanBereached();
                GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.MyPlayerPreMove);
            }
        }

    }

    void Movement()
    {
        //map.DisableTileMask();
        if (PhotonNetwork.isMasterClient || !GamePlayManager.IsMultiPlayer)
        {
            if (string.IsNullOrEmpty(playManager.conditions.TargetChaName) && !currentUnit.tag.Contains("Player"))
            {
                StartCoroutine(GamePlayManager.Wait(() =>
                {
                    GamePlayManager.RaiseGameplayEvent(playManager.conditions.CurrentChaName, GamePlayManager.GameplayEvent.PreAttackState);
                        //playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.PreAttackState);
                    }, 0.3f));
                return;
            }
            currentUnit.remainingMovement = playManager.conditions.RemainingMovement;
            if (currentUnit.remainingMovement - map.CostToEnterTile(currentUnit.tileX, currentUnit.tileY, playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY) >= 0 ? true : false)
            {
                currentUnit.remainingMovement -= map.CostToEnterTile(currentUnit.tileX, currentUnit.tileY, playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY);
                map.OnUnitMoving(currentUnit.tileX, currentUnit.tileY, playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY);
                currentUnit.transform.DOMove(map.TileCoordToWorldCoord(playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY), 0.25f);
                currentUnit.tileX = playManager.conditions.MoveToTileX;
                currentUnit.tileY = playManager.conditions.MoveToTileY;
                StartCoroutine(GamePlayManager.Wait(() =>
                {
                    GamePlayManager.RaiseGameplayEvent(playManager.conditions.CurrentChaName, GamePlayManager.GameplayEvent.PreMoveState);
                    //playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.PreMoveState);/////////////////
                }, 0.3f));

                //RefreshRoomProperties(GameplayEvent.PreMoveState);

            }
            else
            {
                StartCoroutine(GamePlayManager.Wait(() => { GamePlayManager.RaiseGameplayEvent(playManager.conditions.CurrentChaName, GamePlayManager.GameplayEvent.PreAttackState); }, 0.3f));
            }
        }

        else if (!PhotonNetwork.isMasterClient && GamePlayManager.IsMultiPlayer)
        {
            currentUnit.remainingMovement = playManager.conditions.RemainingMovement;
            if (currentUnit.remainingMovement - map.CostToEnterTile(currentUnit.tileX, currentUnit.tileY, playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY) >= 0 ? true : false)
            {
                map.OnUnitMoving(currentUnit.tileX, currentUnit.tileY, playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY);
                currentUnit.transform.DOMove(map.TileCoordToWorldCoord(playManager.conditions.MoveToTileX, playManager.conditions.MoveToTileY), 0.25f);
                currentUnit.tileX = playManager.conditions.MoveToTileX;
                currentUnit.tileY = playManager.conditions.MoveToTileY;
            }
            //map.DisableTileMask();
        }
    }

    void PreAttack()
    {
        if (PhotonNetwork.isMasterClient || !GamePlayManager.IsMultiPlayer)
        {

            if (currentUnit.tag.Contains("Player"))
            {

                //waiting for input
                if (playManager.IsPhotonPlayersTurn(currentUnit.name))
                {
                    playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.PreAttackState);
                    GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.MyPlayerPreAttack);
                }

                if (!PhotonNetwork.isMasterClient)
                {
                    playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.MyPlayerPreAttack);
                }
                return;
            }

            //choose skill and find the cloest target to attack.
            bool canAtt = currentUnit.CanAttackTarget();

            if (canAtt)
            {
                //map.selectedUnit.attackTarget.GetComponent<SpriteRenderer>().color = Color.blue;
                currentUnit.RandomSpell();
            }

            playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.PreAttackState);
        }

        else if (!PhotonNetwork.isMasterClient && GamePlayManager.IsMultiPlayer)
        {
            if (!string.IsNullOrEmpty(playManager.conditions.TargetChaName))
            {
                if (currentUnit.name != playManager.conditions.TargetChaName)
                {
                    Unit unit = allCharacterDictionary[playManager.conditions.TargetChaName];//allCharacterUnit.Find(x => x.name == playManager.conditions.TargetChaName);
                    currentUnit.attackTarget = unit;
                }
                //map.selectedUnit.attackTarget.GetComponent<SpriteRenderer>().color = Color.blue;
                currentUnit.RandomSpell(playManager.conditions.SelectedSpell);
                Debug.LogError(playManager.conditions.CurrentChaName + " | " + playManager.conditions.SelectedSpell);
            }

            if (PhotonNetwork.player.UserId == playManager.conditions.CurrentChaName)
            {
                //UIManager.instance.ActionBar.EnableSkills();
                //return;
                GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.MyPlayerPreAttack);

            }

            if (!string.IsNullOrEmpty(playManager.conditions.TargetChaName))
            {
                //map.selectedUnit.attackTarget.GetComponent<SpriteRenderer>().color = Color.blue;
                //map.selectedUnit.RandomSpell(conditions.SelectedSpell);
                //Debug.LogError(conditions.CurrentChaName + " | " + conditions.SelectedSpell);
            }

        }

    }

    void Attack()
    {
        if (PhotonNetwork.isMasterClient || !GamePlayManager.IsMultiPlayer)
        {

            if (currentUnit.attackTarget == null)
            {
                GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.TurnEnd);
                return;
            }
            //player attack enemy
            if (currentUnit.tag.Contains("Player"))
            {
                spellController.ShowFX(currentUnit, currentUnit.selectedSpell.FX, currentUnit.selectedSpell);

                int damage = spellController.DmgCalculator(currentUnit.savedCharacter, currentUnit.attackTarget.savedCharacter, currentUnit.selectedSpell);
                //currentUnit.attackTarget.TakeDamage(damage);
                foreach(var go in UintInSpellRange())
                {
                    if (currentUnit.selectedSpell.Target.Contains("Enemy"))
                        go.TakeDamage(damage);
                    else if (currentUnit.selectedSpell.Target.Contains("Player"))
                        go.GetHealth(damage);
                }

                if (currentUnit.selectedSpell.Cooldown > 0)
                {
                    playManager.uiManager.ActionBar.selectedSpellSlot.EnableCD(currentUnit.selectedSpell.Cooldown);
                }
            }
            //enemy attack player
            else
            {
                int damage = spellController.DmgCalculator(currentUnit.savedCharacter, currentUnit.attackTarget.savedCharacter, currentUnit.selectedSpell);
                currentUnit.attackTarget.TakeDamage(1);
                spellController.ShowFX(currentUnit.attackTarget.gameObject, currentUnit.selectedSpell);
            }

            StartCoroutine(GamePlayManager.Wait(() =>
            {
                GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.TurnEnd);
            }, .75f));

        }
        else if (!PhotonNetwork.isMasterClient && GamePlayManager.IsMultiPlayer)
        {
            foreach (var go in allCharacterUnit)
            {
                if (playManager.conditions.AllSavedChaharacterData.ContainsKey(go.name))
                {
                    go.savedCharacter = playManager.conditions.AllSavedChaharacterData[go.name];
                }
            }

            if (!string.IsNullOrEmpty(playManager.conditions.TargetChaName))
            {
                currentUnit.attackTarget = allCharacterDictionary[playManager.conditions.TargetChaName];
                currentUnit.RandomSpell(playManager.conditions.SelectedSpell);
            }

            if (currentUnit.attackTarget == null)
            {
                return;
            }

            //player attack enemy
            if (currentUnit.tag.Contains("Player"))
            {
                spellController.ShowFX(currentUnit, currentUnit.selectedSpell.FX);

                int damage = spellController.DmgCalculator(currentUnit.savedCharacter, currentUnit.attackTarget.savedCharacter, currentUnit.selectedSpell);
                currentUnit.attackTarget.TakeDamage(damage);
                if (currentUnit.selectedSpell.Cooldown > 0)
                {
                    playManager.uiManager.ActionBar.selectedSpellSlot.EnableCD(currentUnit.selectedSpell.Cooldown);
                }
            }
            //enemy attack player
            else
            {
                int damage = spellController.DmgCalculator(currentUnit.savedCharacter, currentUnit.attackTarget.savedCharacter, currentUnit.selectedSpell);
                currentUnit.attackTarget.TakeDamage(damage / 10);
                spellController.ShowFX(currentUnit, currentUnit.selectedSpell.FX);
            }
        }
    }

    void TurnEnd()
    {
        
        if (PhotonNetwork.isMasterClient || !GamePlayManager.IsMultiPlayer)
        {

            currentId++;
            if (currentId > allCharacterUnit.Count - 1)
            {
                currentId = 0;
                playManager.currentTurn++;
                allCharacterUnit.Sort();
            }


            //if unit has status and do something
            spellController.TriggerStatus(allCharacterUnit[currentId]);

            //check is quest compeleted ?
            if (playManager.CheckIsQuestCompleted())
            {
                Debug.Log(" quest is completed");
                //GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.OutroAct);
                playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.OutroAct);
                return;
            }

            currentUnit.InitState();

            //StartCoroutine(GamePlayManager.Wait(() => {  }, 1));
            playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.TurnEnd);

        }
        else
        {

        }


    }

    public void OnCharacterDied(Unit unit)
    {
        allCharacterUnit.Remove(unit);
        
        // diedUnit --> currentUnit --> unit2
        if(allCharacterUnit.IndexOf(unit) > allCharacterUnit.IndexOf(currentUnit))
        {
            currentId--;
        }

    }

    public List<Unit> UintInSpellRange()
    {
        List<Unit> inRange = new List<Unit>();

        foreach(var go in allCharacterUnit)
        {
            if (currentUnit.selectedSpell.RangeX.Contains(go.tileX - currentUnit.tileX) && currentUnit.selectedSpell.RangeY.Contains(go.tileY - currentUnit.tileY))
            {
                inRange.Add(go);
            }
        }

        return inRange;
    }
}
