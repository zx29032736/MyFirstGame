using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : IState
{
    public override void Enter()
    {
        //Debug.Log("Start to mvoe!");
        ////GamePlayManager.instances.MapInit();
    }

    public override void Stay()
    {
        //GamePlayManager.instances.AutoMove();
    }

    public override void Leave()
    {
        //GamePlayManager.instances.LeaveMoveState();
    }
}
