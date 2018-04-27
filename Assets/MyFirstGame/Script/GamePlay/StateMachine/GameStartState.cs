using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartState : IState {

    public override void Enter()
    {
        //Debug.Log(" Game Start!!");
        //GamePlayManager.instances.StartCoroutine(GamePlayManager.Wait(() => {  }, 1));
        //UIManager.instance.OpenConfirmPanel();
        //GamePlayManager.instances.GameStartEnter();
    }

    public override void Stay()
    {

    }

    public override void Leave()
    {

    }
}
