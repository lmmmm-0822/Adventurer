using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//在空中被打成倒地硬直可以停止播放动画，在落地时会继续播放
//产生击飞硬直后可以停止播放动画，在落地时会继续播放
public abstract class BeHitBase_Enemy : State_Enemy
{//转入判断：不在Die状态 &&（削韧大于韧性 || 生命值小于0 || 浮空）
 //在强硬直受击状态下也能以弱硬直状态转入，但不会执行弱硬直相关逻辑

    //private int currentPoiseDamageLevel;
    public enum HitRecover
    {
        Weak = -1,//小硬直
        Ground = 0,//倒地
        Air = 1,//击飞
        Strong = 2,//原地大硬直
        Repel = 3,//后退大硬直
    }
    public HitRecover RecoverType { get; private set; }
    protected float reduceSpeed;
    protected int airState;//0代表上升阶段，1代表停滞阶段，2表示下降阶段
    protected bool isFinish;//动画是否播放结束，是否可以进入idle状态
    /// <summary>
    /// 动画事件调用
    /// </summary>
    public virtual void EndBeHit()
    {
        isFinish = true;
    }
    public override void OnEnterState(StateBase lastState, float hitRecoverType, object args = null)
    {
        //硬直类型
        RecoverType = (HitRecover)(int)hitRecoverType;

        if(RecoverType == HitRecover.Air)
            airState = 0;

        isFinish = false;

        //设置重力和强制落地
        SetGravityScale();

        //朝向
        SetFacing((Transform)args);

        //动画
        SetAnimation();

        //速度
        SetSpeed();
    }
    public override void OnUpdate(float deltaTime)
    {
        if (isFinish)
        {
            if (enemy.cAtr.CurrentHealth > 0)
                controller.ChangeState(AllStates.Idle);
            else
                controller.ChangeState(AllStates.Die);
        }
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        enemy.Stopping(reduceSpeed);
    }
    public override void OnExitState(StateBase nextState)
    {
        enemy.ForceOnGround = false;
        enemy.rb2D.gravityScale = enemy.NormalGravityScale;
    }
    /// <summary>
    /// 处理在强硬直状态下重复进入弱硬直
    /// </summary>
    protected virtual void WeakBeHitWhileStrongHitRecover()
    {
    }
    protected virtual void SetGravityScale()
    {
        if (enemy.IsOnGround && RecoverType != HitRecover.Air)
        {
            enemy.ForceOnGround = true;
            enemy.rb2D.gravityScale = 0;//防止下坡会滚得很远（如果是翻滚动画的话
        }
    }
    protected virtual float SetExitedTime()
    {
        return 1;
    }
    protected virtual void SetFacing(Transform args)
    {
        if (enemy.rb2D.velocity.x != 0)
        {
            enemy.ChangeFacing(enemy.rb2D.velocity.x < 0);//根据受击施加的速度后退
        }
        else
        {
            enemy.ChangeFacing(args.position.x);
        }
    }
    protected virtual void SetAnimation()
    {
        PlayAnimation("BeHit");
    }
    protected virtual void SetSpeed()
    {
        reduceSpeed = 0.1f;
    }
}
