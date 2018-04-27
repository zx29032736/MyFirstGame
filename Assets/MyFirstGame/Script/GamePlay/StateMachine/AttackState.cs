using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState {

    public override void Enter()
    {
    }

    public override void Stay()
    {
        //GamePlayManager.instances.AutoAttack();
    }

    public override void Leave()
    {
        //GamePlayManager.instances.AttackLeave();
    }
}
