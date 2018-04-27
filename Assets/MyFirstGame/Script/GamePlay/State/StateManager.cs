using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager {

    private static StateManager instance;
    public List<IState> stateList = new List<IState>();
    private int currentID;

    private StateManager()
    {

    }

    public static StateManager GetInstance()
    {
        if (instance == null)
        {
            instance = new StateManager();
        }
        return instance;
    }

    public void AddState(IState state)
    {
        stateList.Add(state);
    }

    public void CreateRoundStateList()
    {
        AddState(new GameStartState());
        AddState(new PreMoveState());
        AddState(new MoveState());
        AddState(new PreAttackState());
        AddState(new AttackState());
        AddState(new TurnEndState());
        AddState(new GameWinState());
    }

    public void ChangeState(int i)
    {
        stateList[currentID].Leave();
        currentID = i;
        stateList[currentID].Enter();
    }

    public void UpdateState()
    {
        stateList[currentID].Stay();
    }
}
