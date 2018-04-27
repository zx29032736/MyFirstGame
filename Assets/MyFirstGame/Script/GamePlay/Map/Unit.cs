using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using System.Linq;
using System;

public class Unit : MonoBehaviour, IComparable<Unit> {

    [HideInInspector]
    public TileMap map;
    [HideInInspector]
    public int tileX;
    [HideInInspector]
    public int tileY;
    [HideInInspector]
    public CharacterUnitController unitController;

    public List<Node> currentPath = null;

    public float remainingMovement = 3;

    public Unit attackTarget = null;
    public int attackRange = 2;


    public FG_SavedCharacter savedCharacter;
    public Dictionary<string, FG_Spell> mySpells = new Dictionary<string, FG_Spell>();
    public FG_Spell selectedSpell;

    public GameObject selectedMask;
    public GameObject isPlayerMark;
    public GameObject isEnemyMark;
    public GameObject isBuffMark;
    public GameObject isDeBuffMark;

    //public bool isPlayer = false;

    public void Init(FG_SavedCharacter saved, CharacterUnitController controller)
    {
        unitController = controller;
        map = controller.map;
        tileX = (int)transform.position.x;
        tileY = (int)transform.position.y;

        savedCharacter = saved;
        savedCharacter.SetMaxVitals();
        savedCharacter.RefillVitals();
        GetComponent<SpriteRenderer>().sprite = GameController.Instance.iconManager.GetIconById(savedCharacter.baseClass.Icon);

        //add spells
        AddSpell(saved.characterData.Spell1);
        AddSpell(saved.characterData.Spell2);
        AddSpell(saved.characterData.Spell3);
        AddSpell(saved.characterData.Spell4);
        
        //
        if(gameObject.tag == "Player")
        {
            isPlayerMark.SetActive(true);
            isEnemyMark.SetActive(false);
        }
        else
        {
            isPlayerMark.SetActive(false);
            isEnemyMark.SetActive(true);
        }

        gameObject.SetActive(true);
        InitState();
    }

    public void InitState()
    {
        remainingMovement = savedCharacter.PlayerVitals.Speed;
        currentPath = null;
        selectedSpell = null;
        attackTarget = null;
    }

    public void SearchTarget()
    {
        //if (attackTarget == null)
        //{
        attackTarget = FindNearestTarget();
        //Debug.Log(attackTarget.name + " __________________ ::");
        //}

        if (attackTarget == null)
            return;

        int rngX = (int)attackTarget.transform.position.x;//Random.Range(0, 9);
        int rngY = (int)attackTarget.transform.position.y;//Random.Range(0, 9);

        map.GeneratePathTo(rngX, rngY);
    }

    public bool CanAttackTarget()
    {
        if (attackTarget == null)
            return false;

        float distance = Distance(attackTarget.transform.position, this.transform.position);

        if (distance <= attackRange)
            return true;
        else
        {
            attackTarget = FindNearestTarget();
            distance = Distance(attackTarget.transform.position, this.transform.position);

            if (distance <= attackRange)
                return true;
            else
            {
                attackTarget = null;
                return false;
            }
        }
    }

    //IEnumerator Movement()
    //{
    //    if (remainingMovement > 0)
    //    {
    //        if (currentPath == null)
    //        {
    //            Debug.Log(gameObject.name + " currentPath = null!!");
    //            GamePlayManager.instances.SwitchState(GamePlayManager.States.PreMoveState);
    //            isMoving = false;
    //            yield break;
    //        }

    //        // Get cost from current tile to next tile
    //        remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

    //        if (remainingMovement < 0)
    //        {
    //            this.moveState = MoveState.End;
    //            yield break;
    //        }
    //        // Move us to the next tile in the sequence
    //        map.OnUnitMoving(tileX, tileY, currentPath[1].x, currentPath[1].y);
    //        tileX = currentPath[1].x;
    //        tileY = currentPath[1].y;

    //        //transform.position = map.TileCoordToWorldCoord( tileX, tileY );	// Update our unity world position
    //        transform.DOMove(map.TileCoordToWorldCoord(tileX, tileY), 0.25f);
    //        yield return new WaitForSeconds(0.3f);

    //        // Remove the old "current" tile
    //        currentPath.RemoveAt(0);

    //        currentPath = null;
    //        isMoving = false;

    //        if (remainingMovement <= 0)
    //        {
    //            this.moveState = MoveState.End;
    //        }
    //        else
    //        {
    //            GamePlayManager.instances.SwitchState(GamePlayManager.States.PreMoveState);
    //        }

    //    }
    //    else
    //    {
    //        this.moveState = MoveState.End;
    //    }
        
    //}

    //public void AttackMovement()
    //{

    //   StartCoroutine(Attack());

    //}

    //IEnumerator Attack()
    //{
      
    //}

    //public void AttackSimulate()
    //{
    //    StartCoroutine(Attack());
    //}

    public void ForceMoveTo(int x, int y)
    {
        if (map.FindTile(x, y).isCharacter || !map.UnitCanEnterTile(x, y))
            return;
        transform.position = new Vector2(x, y);
        tileX = x;
        tileY = y;
    }

    public void TakeDamage(int damage)
    {
        this.savedCharacter.PlayerVitals.Health -= damage;
        this.transform.DOShakePosition(0.3f, 1, 100);

        if (this.savedCharacter.PlayerVitals.Health <= 0)
        {
            // GamePlayManager.instances.OnCharacterDied(this);
            unitController.OnCharacterDied(this);
            this.gameObject.SetActive(false);
            Debug.Log(this.name + " is died");

        }
    }

    public void GetHealth(int amount)
    {
        this.savedCharacter.PlayerVitals.Health += amount;

        if(this.savedCharacter.PlayerVitals.Health > this.savedCharacter.PlayerVitals.MaxHealth)
        {
            this.savedCharacter.PlayerVitals.Health = this.savedCharacter.PlayerVitals.MaxHealth;
        }
    }

    public void EnableBuff(bool isBuff)
    {
        if (isBuff)
        {
            if (!isBuffMark.activeInHierarchy)
                isBuffMark.SetActive(true);
        }
        else
        {
            if (!isDeBuffMark.activeInHierarchy)
                isDeBuffMark.SetActive(true);
        }
    }

    public void DisableBuff(bool isBuff)
    {
        if (isBuff)
        {
            if (isBuffMark.activeInHierarchy)
                isBuffMark.SetActive(false);
        }
        else
        {
            if (isDeBuffMark.activeInHierarchy)
                isDeBuffMark.SetActive(false);
        }
    }

    public void RandomSpell(string beSelected = null)
    {
        if(beSelected != null)
        {
            if (this.mySpells[beSelected] != null)
                this.selectedSpell = this.mySpells[beSelected];
            else
                Debug.LogError(" my spells do not contain the spell __ " + beSelected);
            return;
        }

        int rng = UnityEngine.Random.Range(0, this.mySpells.Count);
        this.selectedSpell = this.mySpells.Values.ToList()[rng];
        //Debug.Log("Is going to use " + sp.Icon + " and it caused " + sp.BaseDmg + " damage to enemy");
    }

    void AddSpell(string spellName)
    {
        if (PF_GameData.Spells.ContainsKey(spellName))
        {
            FG_SpellDetail spellData = PF_GameData.Spells[spellName];
            FG_Spell sp = new FG_Spell()
            {
                SpellName = spellName,
                Target = spellData.Target,
                ApplyStatus = spellData.ApplyStatus,
                Cooldown = spellData.Cooldown,
                Description = spellData.Description,
                Dmg = spellData.BaseDmg,
                Icon = spellData.Icon,
                FX = spellData.FX,
                RangeX = spellData.RangeX,
                RangeY = spellData.RangeY,
                LevelReq = spellData.LevelReq
            };

            //if (savedCharacter.characterData.CharacterLevel >= sp.LevelReq)
            //{
                this.mySpells.Add(spellName, sp);
                this.unitController.spellController.SetupSpellFX(sp);
            //}
            //else
                //Debug.Log(string.Format("character level : {0} is too low to use spell : {1} and required level : {2}.", savedCharacter.characterData.CharacterLevel, sp.SpellName, sp.LevelReq));
        }
        else
        {
            Debug.LogError(" Do not contain this spell data : __ " + spellName);
        }
    }

    Unit FindNearestTarget()
    {
        Unit nearest = attackTarget;
        float dis = 10;
        foreach (var go in unitController.allCharacterUnit)
        {
            if (go == this || go.tag.Contains(gameObject.tag))
                continue;

            float f = Distance(this.transform.position, go.transform.position);

            if (f <= dis)
            {
                dis = f;
                nearest = go;
            }
        }
        return nearest;
    }

    float Distance(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2).magnitude;
    }

    private void OnMouseDown()
    {
        //selected this to be the target
        if(unitController.playManager.myEvent ==  GamePlayManager.GameplayEvent.PreAttackState)
        {
            if (this.selectedMask.activeInHierarchy)
            {
                unitController.playManager.SetPlayerAttackTarget(this);
                unitController.playManager.DisableTargetMask();
            }
            else
            {
                Debug.Log("this target is too far..");
            }
        }
        else
        {
            Debug.Log(gameObject.name + " is Clicked");
            //GamePlayManager.RaiseGameplayEvent(gameObject.name, GamePlayManager.GameplayEvent.ShowStatUI);
        }
    }

    private void OnMouseDrag()
    {
        UIManager.RaiseUiEvent(gameObject.name, UIManager.UiEvent.EnableCharacterStat);

    }

    private void OnMouseUp()
    {
        UIManager.RaiseUiEvent(gameObject.name, UIManager.UiEvent.DisableCharacterStat);

    }

    public int CompareTo(Unit other)
    {
        return other.savedCharacter.PlayerVitals.Speed.CompareTo(savedCharacter.PlayerVitals.Speed);
    }
}
