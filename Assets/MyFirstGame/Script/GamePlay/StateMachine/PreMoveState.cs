using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreMoveState : IState
{
    public override void Enter()
    {
        //GamePlayManager.instances.PreMoveEnter();
    }
    public override void Stay()
    {
        //GamePlayManager.instances.PreMove();
    }
    public override void Leave()
    {

    }
}
