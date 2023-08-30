using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBase_Enemy : State_Enemy , IDamageAreaControl
{
    protected Skill[] allSkills;
    public Skill Skill { get; protected set; }
    protected AttackState currentAttackState;
    public DamageAreaBase DamageArea { get; private set; }
    protected enum AttackState
    {
        beforeAttack,
        attacking,
        afterAttack,
        end,
    }
    #region 动画调用函数
    public void BeforeAttackInterval()
    {
        enemy.rb2D.velocity = Vector2.zero;
    }
    public void ChangeAttackState(int attackStateNum)
    {//仅由动画事件调用
        ChangeAttackState((AttackState)attackStateNum);
    }
    #endregion
    protected virtual void ChangeAttackState(AttackState attackState)
    {
        if (attackState == AttackState.beforeAttack)
        {
            enemy.rb2D.velocity = enemy.AdaptFacing(Skill.beforeAttackMoveSpeed);
        }
        else if (attackState == AttackState.attacking)
        {
            CreateDamageArea();

            enemy.rb2D.velocity = enemy.AdaptFacing(Skill.attackingMoveSpeed);
        }
        else if (attackState == AttackState.afterAttack)
        {
            RecycleDamageArea();
            enemy.rb2D.velocity = enemy.AdaptFacing(Skill.afterAttackMoveSpeed);
        }
        else//end
        {
            AttackStateEnd();
            return;
        }
        currentAttackState = attackState;
    }
    protected virtual void AttackStateEnd()
    {
        controller.ChangeState(AllStates.Idle);
    }
    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        Skill = allSkills[(int)value];
        ChangeAttackState(AttackState.beforeAttack);
        PlayAnimation("Skill" + ((int)value).ToString());
    }
    public override void OnExitState(StateBase nextState)
    {
        if(currentAttackState==AttackState.attacking)
        {
            enemy.RestartAnimation();
            RecycleDamageArea();//防止正在攻击时被打断而伤害区域不消失
        }
    }
    public override void OnUpdate(float deltaTime)
    {
        //如果没有重写CharacterBase中的BeParried函数，则可能在攻击时被转换至BeParried状态
        if (Skill.IsType(Skill.Type.airToGround) && enemy.JustOnGround)
        {
            enemy.RestartAnimation();
            CameraControl.Instance.Shake(0.2f, Skill.stopTime/2);
        }
    }
    #region 伤害区域
    public virtual void CreateDamageArea()
    {
        DamageArea = DamageAreaBase.Attack(Skill, enemy);
        //attackEffect.PlayAttackEffect(skill, isWeak);
    }
    public void ResetDamageArea()
    {
        if (!(DamageArea is MeleeDamageArea))
        {
            Debug.LogError("没有MeleeDamageArea的情况下执行了ReplayAttack的函数");
            return;
        }
        DamageArea.ReplayAttack();
        //attackEffect.ReplayAttackEffect();
    }
    public void RecycleDamageArea()
    {
        if (!(DamageArea is MeleeDamageArea))
            return;
        DamageArea.EndAttack();
        DamageArea = null;
    }
    #endregion
}
