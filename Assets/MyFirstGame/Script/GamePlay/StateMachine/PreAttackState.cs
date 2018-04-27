using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreAttackState : IState {

    public override void Enter()
    {
        //GamePlayManager.instances.PreAttackEnter();
    }
    public override void Stay()
    {
        //GamePlayManager.instances.PreAttack();
    }
    public override void Leave()
    {
        //GamePlayManager.instances.PreAttackLeave();
    }

}
