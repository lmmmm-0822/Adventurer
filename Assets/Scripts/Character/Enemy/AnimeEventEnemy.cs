using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class AnimeEventEnemy : MonoBehaviour
{
    protected CharacterBase enemy;
    //private AnimatorTimeline animator;
    private AttackBase_Enemy attack;
    private BeHitBase_Enemy beHit;
    //private RangeDamageArea RangeDamageArea;
    //private RangeDamageArea ThrowAction;

    public virtual void Init(CharacterBase enemy)
    {
        this.enemy = enemy;
        attack = enemy.stateController.GetState<AttackBase_Enemy>(AllStates.Attack);
        beHit = enemy.stateController.GetState<BeHitBase_Enemy>(AllStates.BeHit);
        //animator = character.animator;
    }
    private void SetSpeed(float speed)
    {
        var tmp = enemy.DirectGround(enemy.IsFacingRight == speed > 0);
        enemy.rb2D.velocity = enemy.AdaptFacing(speed) * tmp.x * tmp;
    }
    private void SetSpeedX(float speed)
    {
        enemy.rb2D.velocity = new Vector2(enemy.AdaptFacing(speed), enemy.rb2D.velocity.y);
    }
    private void SetSpeedY(float speed)
    {
        enemy.rb2D.velocity = new Vector2(enemy.rb2D.velocity.x, speed);
    }
    private void BeforeAttackInterval()
    {//前摇间隔，相较于前摇与攻击之间停顿，在前摇中停顿会更好
        attack.BeforeAttackInterval();
        TimeEventManager.Instance.RegisterTimeAction(0.1f, enemy.RestartAnimation, enemy.StopAnimation);
    }
    private void Attacking()
    {
        attack.ChangeAttackState(1);

    }
    private void AfterAttack()
    {
        attack.ChangeAttackState(2);
    }
    private void EndAttack()
    {
        attack.ChangeAttackState(3);
    }
    //private void Attack()
    //{
    //    RangeDamageArea.Attack();
    //}
    private void StopAnimation(float keepTime = 0.3f)
    {//动画事件调用后，必须在代码里继续播放动画，即调用enemy.RestartAnimation()
        if (enemy.stateController.currentState.state == AllStates.Attack &&
            attack.Skill.IsType(Skill.Type.airToGround))
        {
            if (enemy.IsOnGround)
                return;
        }
        else if (enemy.stateController.currentState.state == AllStates.BeHit)
        {
            if (beHit.RecoverType == BeHitBase_Enemy.HitRecover.Ground)
                if (enemy.IsOnGround)
                    return;
            if (beHit.RecoverType == BeHitBase_Enemy.HitRecover.Air)
                if (enemy.IsOnGround)
                    return;
        }
        enemy.StopAnimation();
    }
    private void EndBeHit()
    {
        beHit.EndBeHit();
    }
    private void BeHitToDie()
    {//在BeHit动画中途根据当前生命值结束动画
        if(enemy.cAtr.CurrentHealth<=0)
        {
            beHit.EndBeHit();
        }
    }
}
