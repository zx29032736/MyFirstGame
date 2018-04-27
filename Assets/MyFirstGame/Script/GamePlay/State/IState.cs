using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    public abstract void Enter();


    public virtual void Stay()
    {

    }

    public virtual void Leave()
    {

    }
	
}
