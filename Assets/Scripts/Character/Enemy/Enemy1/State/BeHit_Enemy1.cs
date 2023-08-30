using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BeHit", menuName = "States/Enemy1/BeHit")]
public class BeHit_Enemy1 : BeHitBase_Enemy
{
    //private Idle_Slime idle;

    public float maxSpeed;

    public override void OnExitState(StateBase nextState)
    {
        base.OnExitState(nextState);
        enemy.Stopping(0.5f);
    }
}
