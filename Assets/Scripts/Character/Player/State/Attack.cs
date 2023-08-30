using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*攻击的过程：
 *释放技能：
 * 检测到了玩家输入，skillController修改下一次攻击释放的技能
 * 该技能到了释放的时机（如果在Attack状态，不在直接释放），skillController释放下一次技能，让角色转到Attack状态
 * 攻击的阶段、伤害区域的产生由动画控制，在attacking的阶段动画事件调用CreateDamageArea函数，产生伤害区域
 *处理攻击方伤害：
 * CreateDamageArea函数被调用后，DamageAreaBase根据角色属性和技能伤害倍率得出，在敌人防御力为0的情况下，会造成多少伤害
 * 若有敌人进入伤害区域，则调用其Damage函数（IDamageable），DamageArgs参数传入
 *处理受击：
 * 若敌人!canBeDamage，则退出Damage函数，不做其他处理
 * 否则处理卡肉、屏幕震动、击退、伤害
*/


[CreateAssetMenu(fileName = "Attack", menuName = "States/Player/Attack")]
public class Attack : State,IDamageAreaControl
{//攻击的阶段由动画事件控制
    [SerializeField, Tooltip("进入后摇直接跳跃的预输入时间")] float preinputJumpTime = 0.1f;
    [SerializeField, Tooltip("闪现的速度")] float flashSpeed = 50;

    public Skill Skill { get; private set; }
    /// <summary>
    /// 是否免疫伤害
    /// </summary>
    public bool Immune { get; private set; }
    public Transform Target { get; private set; }//在NeedTarget技能attaccking时刷新（带有LockTarget特性的技能一般在NeedTarget技能之后
    public DamageAreaBase DamageArea { get; private set; }
    private bool canDodge;
    //private Vector2 startPos;
    private bool endAnimationEventFlag;

    private AttackEffectCharacter attackEffect;
    private Run run;
    private Jump jump;
    //private Dodge dodge;

    private float animeTime;
    private enum AttackState
    {
        beforeAttack,
        attacking,
        afterAttack,
        end,
    }
    private AttackState currentAttackState;

    private Collider2D[] targets = new Collider2D[15];//NeedTarget
    private Vector2 targetPos;//Flash
    public override void Init()
    {
        base.Init();
        //attackEffect = character.transform.Find("SpecialEffects/AttackEffect").GetComponent<AttackEffectCharacter>();
        //attackEffect.Init();
        run = controller.GetState<Run>(AllStates.Run);
        jump = controller.GetState<Jump>(AllStates.Jump);
        //dodge = controller.GetState<Dodge>(AllStates.Dodge);
    }
    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        character.CanMove = false;
        character.rb2D.gravityScale = 0;
        character.rb2D.velocity = Vector2.zero;

        canDodge = false;
        endAnimationEventFlag = false;

        animeTime = 0;
        run.RefreshFacing();//必要，否则一套技能连起来放，玩家不能转换方向

        Skill = (Skill)args;
        if (Skill.IsType(Skill.Type.ground))
            character.ForceOnGround = true;
        Target = null;

        ChangeAttackState(AttackState.beforeAttack);
        PlayAnimation("Skill" + Skill.num.ToString(), animeTime);
        if(Skill.num == 21)
        {
            character.animator.speed = 1 / character.bulletIntervalRate;
        }
    }
    public override void OnUpdate(float deltaTime)
    {
        if (DamageArea != null)
        {//专注值及ResumeHealth
            if (DamageArea.FirstHit)
            {
                //命中后不回复专注值//Todo
                //if (!Skill.IsRange && !character.skillCtr.HaveUsedThisSkill)//远程攻击不回复专注值
                //    character.cAtr.SetFocus(1, Skill.focusResume);

            }
            if (DamageArea.FirstKill)
            {
            }
            if (DamageArea.JustHit)
            {
            }
        }

        if (Skill.IsType(Skill.Type.air))
        {
            if (character.IsOnGround)
            {
                if (character.rb2D.gravityScale != 0 && currentAttackState != AttackState.attacking)
                {
                    controller.ChangeState(AllStates.Fall);
                    return;
                }
                else
                    character.transform.position += new Vector3(0, 0.09f, 0);//使空中攻击可以连起来
            }
        }
        if (character.JustOnGround && Skill.IsType(Skill.Type.airToGround))
        {
            character.rb2D.velocity = Vector2.zero;//因为没有摩擦，防止角色落地时滑动
            character.RestartAnimation();
            CameraControl.Instance.Shake(1, Skill.stopTime);
        }

        #region 处理玩家状态
        if (Skill.IsType(Skill.Type.ground) && character.JustExitGround)
        {//掉落
            character.rb2D.velocity = new Vector2(0, character.rb2D.velocity.y);
            controller.ChangeState(AllStates.Fall);
            return;
        }
        if (canDodge)
        {//闪避、跳跃
            if (PlayerInput.Instance.PreInputs(CharacterInput.dodge) <= 0.1f)
            {
                controller.ChangeState(AllStates.Dodge);
                return;
            }
            if (jump.CheckCanJump())
            {
                if (PlayerInput.Instance.PreInputs(CharacterInput.jump) <= preinputJumpTime)
                {
                    controller.ChangeState(AllStates.Jump);
                    return;
                }
            }
            if(character.IsOnGround && PlayerInput.Instance.GetKey(CharacterInput.defend))
            {
                controller.ChangeState(AllStates.Defend);
                return;
            }
        }
        if (character.CanMove)
        {//移动
            if (PlayerInput.Instance.GetKey(CharacterInput.moveLeft)
             || PlayerInput.Instance.GetKey(CharacterInput.moveRight))
            {
                controller.ChangeState(AllStates.Run);
                return;
            }
        }
        #endregion
    }
    public override void OnExitState(StateBase nextState)
    {
        character.CanMove = true;
        character.ForceOnGround = false;
        character.rb2D.gravityScale = character.NormalGravityScale;

        if (currentAttackState == AttackState.attacking)
        {
            if (DamageArea != null)
                RecycleDamageArea();

            character.RestartAnimation();
        }
        character.animator.speed = 1;
    }
    public override void AnimationEnd(string name)
    {
        if (endAnimationEventFlag)
            ChangeAttackState(AttackState.end);
    }

    private void ChangeAttackState(AttackState attackState)
    {
        if (attackState == AttackState.beforeAttack)
        {

        }
        else if (attackState == AttackState.attacking)
        {
            endAnimationEventFlag = true;

            CreateDamageArea();
            character.skillCtr.RealUseSkill(Skill);
        }
        else if (attackState == AttackState.afterAttack)
        {
            RecycleDamageArea();
        }
        else //endAttack
        {
            if (!character.IsOnGround)
                controller.ChangeState(AllStates.Fall);
            else
                controller.ChangeState(AllStates.Idle);
            return;
        }
        currentAttackState = attackState;
    }
    /// <summary>
    /// 仅获取正前方斜率-2到2之间的敌人，无目标则return null
    /// </summary>
    /// <returns></returns>
    private Transform GetTarget()
    {
        int cnt = Physics2D.OverlapCircleNonAlloc(character.transform.position, 5, targets, Utils.GetMask(NameList.Layer.Enemy, NameList.Layer.EnemyIgnoreOthers));
        int num = -1;
        float sqrMinDistance = 400;
        Vector2 vet;
        for (int i = 0; i < cnt; i++)
        {
            if (!targets[i].GetComponent<Enemy>().IsDeadState)
            {
                vet = targets[i].transform.position - character.transform.position;
                if ((character.IsFacingRight ? vet.x > 0 : vet.x < 0) && vet.y / vet.x <= 2 && vet.y / vet.x >= -2)
                {
                    if (vet.sqrMagnitude < sqrMinDistance)
                    {
                        sqrMinDistance = vet.sqrMagnitude;
                        num = i;
                    }
                }
            }
        }
        return num == -1 ? null : targets[num].transform;
    }

    #region 伤害区域
    public void CreateDamageArea()
    {
        DamageArea = DamageAreaBase.Attack(Skill, character);
        //attackEffect.PlayAttackEffect(Skill);
    }
    public void RecycleDamageArea()
    {
        if (!(DamageArea is MeleeDamageArea))
            return;
        DamageArea.EndAttack();
        DamageArea = null;
    }
    #endregion

    #region 动画事件专用
    ///// <summary>
    ///// 动画事件专用
    ///// </summary>
    //public bool errorAfterAttack;
    public void CanDodge(bool canDodge)
    {
        this.canDodge = canDodge;
    }
    public void ChangeAttackState(int attackStateNum)
    {//仅由动画事件调用
        ChangeAttackState((AttackState)attackStateNum);
    }
    #endregion
}

