using Chronos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BeHit", menuName = "States/Player/BeHit")]
//被打之后在Character中判断韧性后转入
public class BeHit : State
{

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
    private float reduceSpeed;
    private Idle idle;
    /// <summary>
    /// 动画事件调用
    /// </summary>
    public void EndBeHit()
    {
        //currentPoiseDamageLevel = 0;//如果不是自然退出BeHit状态，则受击等级叠加
        //TimeEventManager.Instance.RegisterTimeAction(3f, () =>
        //     {
        //         if (controller.currentState.state != AllStates.BeHit)
        //             character.cAtr.BeHitPoiseCoefficient = 1;
        //     });
        controller.ChangeState(AllStates.Idle);
        return;
    }
    public override void Init()
    {
        idle = controller.GetState<Idle>(AllStates.Idle);

    }
    public override void OnEnterState(StateBase lastState, float hitRecoverType, object args = null)
    {
        //if (lastState.state == AllStates.FightTransition)//character.Stance == Character.AllStance.Iaido)
        //{
        //    //if (character.Stance == Character.AllStance.Iaido)
        //    if (((FightTransition)lastState).IsIaidoAnimation)
        //    {
        //        character.Stance = Character.AllStance.Normal;//如果居合时被打出硬直，则转换成常态
        //        Timekeeper.instance.Clock("World").localTimeScale = 1;
        //    }
        //}


        if (lastState.state == AllStates.BeHit && RecoverType != HitRecover.Weak && hitRecoverType == -1)
            return;//在大硬直的情况下不会进入小硬直
        //硬直类型
        RecoverType = (HitRecover)(int)hitRecoverType;

        //强制落地
        if (character.IsOnGround && RecoverType != HitRecover.Weak)
        {
            character.ForceOnGround = true;
            character.rb2D.gravityScale = 0;
        }

        //currentPoiseDamageLevel += (int)justPoisesDamageLevel;
        
        //朝向
        if (character.rb2D.velocity.x != 0)
        {
            character.ChangeFacing(character.rb2D.velocity.x < 0);//根据受击施加的速度后退
        }
        else
        {
            character.ChangeFacing(((Transform)args).position.x);
        }

        //ConsoleProDebug.LogToFilter(character.name + "韧性受伤等级" + currentPoiseDamageLevel.ToString(), "Fight");
       
        //韧性系数
        if (RecoverType == HitRecover.Weak)//currentPoiseDamageLevel >= 3)
        {
            //PlayAnimation("BeHit3");
            //character.rb2D.velocity = new Vector2((character.rb2D.velocity.x == 0 ? character.IsFacingRight : character.rb2D.velocity.x < 0) ? -4 : 4, character.rb2D.velocity.y);
        }
        else
        {
            //PlayAnimation("BeHit" + currentPoiseDamageLevel.ToString());
        }
        //character.cAtr.BeHitPoiseCoefficient = 1 + currentPoiseDamageLevel * 0.5f;

        //动画
        PlayAnimation("BeHit");

        //速度
        reduceSpeed =(int)RecoverType switch
        {
            -1 => 0.1f,
            0 => 0.2f,
            1 => 0,
            2 => 0.1f,
            3 => 0.02f,
        };
        if (RecoverType == HitRecover.Repel)
        {
            if (character.rb2D.velocity.x < 4 && character.rb2D.velocity.x > 0)
                character.rb2D.velocity = new Vector2(4, character.rb2D.velocity.y);
            else if (character.rb2D.velocity.x > -4 && character.rb2D.velocity.x < 0)
                character.rb2D.velocity = new Vector2(-4, character.rb2D.velocity.y);
        }
    }
    public override void OnUpdate(float deltaTime)
    {
        if(character.JustOnGround)
        {
            character.rb2D.gravityScale = 0;
        }
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        idle.Stopping(reduceSpeed * character.timeline.timeScale);
    }
    public override void OnExitState(StateBase nextState)
    {
        character.ForceOnGround = false;
        if (character.rb2D.gravityScale == 0)
            character.rb2D.gravityScale = character.NormalGravityScale;
        //character.cAtr.StatesPoiseCoefficient = 1;
    }
    public override void AnimationEnd(string name)
    {
        controller.ChangeState(AllStates.Idle);
        return;
    }
}
