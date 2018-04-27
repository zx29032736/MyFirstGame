using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndState : IState {

    public override void Enter()
    {
        //GamePlayManager.instances.TurnEndEnter();
    }

    public override void Stay()
    {

    }

    public override void Leave()
    {
        //GamePlayManager.instances.TurnEndLeave();
    }
}
