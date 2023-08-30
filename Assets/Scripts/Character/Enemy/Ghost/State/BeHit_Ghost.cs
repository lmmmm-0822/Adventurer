using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BeHit_Ghost", menuName = "States/Ghost/BeHit_Ghost")]
public class BeHit_Ghost : BeHitBase_Enemy
{
    //private Idle_Ghost idle;
    public float waitTime;//让作用域更大的变量符合本意更好 等待时间
    private float lastWaitTime;//剩余等待时间
    //private float lastStopTime;
    public float maxSpeed;
    //public int beHitTimes;

    //public override void Init()
    //{
    //    idle = controller.GetState<Idle_Ghost>(AllStates.Idle);
    //}
  
    public override void OnEnterState(StateBase lastState,float value,object args)
    {
        base.OnEnterState(lastState, value, args);
        //enemy.rb2D.gravityScale = 1;
        //PlayAnimation("BeHit");
        lastWaitTime = waitTime;
        //enemy.ChangeFacing(enemy.FacingDirection==1 ? false: true) ;
        //beHitTimes++;
    }
    
    public override void OnUpdate(float deltaTime)//瞬间修改速度不用放到FixedUpdate中
    {
        if (enemy.DistanceToGround < 0.3f && lastWaitTime > 0)
        {
            enemy.rb2D.velocity = new Vector2(enemy.rb2D.velocity.x, 0.1f);
            lastWaitTime -= deltaTime;
        }
        else
        {
            if (lastWaitTime > 0)
            {
                enemy.rb2D.velocity = new Vector2(enemy.AdaptFacing(0.1f), 0.1f);//这里强制修改了速度，那就没必要修改重力大小了
                lastWaitTime -= deltaTime;
            }
            else
            {
                if (enemy.cAtr.CurrentHealth == 8)
                    controller.ChangeState(AllStates.Attack, 1);
                else
                    controller.ChangeState(AllStates.Idle);
            }
        }
    }
    protected override void SetGravityScale()
    {
        //enemy.rb2D.gravityScale = 1;
    }
    protected override void SetAnimation()
    {
        PlayAnimation("BeHit");
    }
}
