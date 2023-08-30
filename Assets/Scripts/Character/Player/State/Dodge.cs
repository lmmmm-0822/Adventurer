using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dodge", menuName = "States/Player/Dodge")]
public class Dodge : State
{
    [Header("次数")]
    [SerializeField, Tooltip("连续闪避的次数")]
    private int canDodgeTimes = 2;
    [SerializeField, Tooltip("空中闪避的次数")]
    private int canDodgeTimesInAir = 1;

    [Header("速度")]
    [SerializeField, Tooltip("闪避起始速度")]
    private float startSpeed = 5;
    [SerializeField, Tooltip("闪避的速度")]
    private float speed = 8;
    [SerializeField, Tooltip("闪避结束时的速度")]
    private float endSpeed = 0;

    [Header("时间")]
    [SerializeField, Tooltip("闪避加速的时间")]
    private float beforeTime = 0.08f;
    [SerializeField, Tooltip("匀速闪避的时间")]
    private float dodgingTime = 0.14f;
    [SerializeField, Tooltip("闪避减速的时间")]
    private float afterTime = 0.2f;
    [SerializeField, Tooltip("剩余连续闪避次数耗尽时，能够再次闪避的时间")]
    private float intervalTime = 0.3f;

    [Header("极限闪避")]
    [SerializeField, Tooltip("时间减缓的程度")]
    private float timeScale = 0.1f;
    [SerializeField, Tooltip("时间减缓的持续时间")]
    private float keepTime = 0.3f;

    [Header("专注消耗")]
    [SerializeField, Tooltip("闪避消耗多少专注值")]
    private float focusConsume = 10f;

    /// <summary>
    /// 是否免疫伤害
    /// </summary>
    public bool Immuse { get; private set; }
    /// <summary>
    /// 是否处于闪避后摇
    /// </summary>
    public bool AfterDodge { get; private set; }
    /// <summary>
    /// 是否是极限闪避
    /// </summary>
    public bool GreatDodge { get; private set; }
    private Run run;
    private Jump jump;

    private int direction;
    private Vector2 realStartSpeed;
    private Vector2 realSpeed;
    private Vector2 realEndSpeed;

    private int lastDodgeTimes;
    private int lastDodgeTimesInAir;
    private float lastTime;
    private float dodgeCD;

    private Vector2 startPos;
    private bool canGreatDodge;
    private RaycastHit2D[] greatDodgeCheck = new RaycastHit2D[4];
    private enum DodgeState
    {
        startDodge,
        dodging,
        afterDodge,
        end,
    }
    private DodgeState currentDodgeState;
    private void ChangeDodgeState(DodgeState dodgeState)
    {
        if (dodgeState == DodgeState.startDodge)
        {
            AfterDodge = false;
            //canGreatDodge = true;
            character.IgnoreEnemyCollision(true);
            lastTime = beforeTime;
            character.rb2D.velocity = realStartSpeed;
        }
        else if (dodgeState == DodgeState.dodging)
        {
            canGreatDodge = false;
            lastTime = dodgingTime;

            character.rb2D.velocity = realSpeed;
        }
        else if (dodgeState == DodgeState.afterDodge)
        {
            Immuse = false;
            AfterDodge = true;
            character.IgnoreEnemyCollision(false);
            lastTime = afterTime;

            character.rb2D.velocity = realEndSpeed;
        }
        else//if(dodgeState == DodgeState.end)
        {
            if (!character.IsOnGround)
                controller.ChangeState(AllStates.Fall);
            else
                controller.ChangeState(AllStates.Idle);
            return;
        }
        currentDodgeState = dodgeState;
    }
    private void InitDodgeSpeed(int direction)
    {
        var tmp = character.DirectGround(direction > 0);
        realStartSpeed = direction * startSpeed * tmp.x * tmp;
        realSpeed = direction * speed * tmp.x * tmp;
        realEndSpeed = direction * endSpeed * tmp.x * tmp;
    }
    private void HandleDodgeSpeed()
    {
        if (currentDodgeState == DodgeState.startDodge)
        {
            character.rb2D.velocity = Vector2.Lerp(realSpeed, realStartSpeed, lastTime / beforeTime);
        }
        //else if (currentDodgeState == DodgeState.afterDodge)
        //{
        //    character.rb2D.velocity = Vector2.Lerp(realEndSpeed, realSpeed, lastTime / direction!=0?afterTime:upAfterTime);
        //}
    }
    public override void Init()
    {
        run = controller.GetState<Run>(AllStates.Run);
        jump = controller.GetState<Jump>(AllStates.Jump);
        controller.RegisterOnUpdateAction((deltaTime) =>
        {
            dodgeCD -= deltaTime;

            if (character.JustOnGround)
                lastDodgeTimesInAir = canDodgeTimesInAir;
        });
        controller.RegisterChangeStateCheck(AllStates.Dodge, (lastState) =>
             {
                 if (!character.IsOnGround)
                 {
                     if (lastDodgeTimesInAir <= 0)
                         return false;
                 }

                 if (dodgeCD <= 0)
                 {
                     lastDodgeTimes = canDodgeTimes;
                 }
                 if (lastDodgeTimes <= 0)
                     return false;
                 dodgeCD = intervalTime + beforeTime + dodgingTime + afterTime;
                 return true;
             });
    }
    public override void OnEnterState(StateBase lastState,float value , object args)
    {
        character.rb2D.gravityScale = 0;
        character.cAtr.SetFocus(-1, focusConsume);

        run.RefreshFacing();

        lastDodgeTimes--;
        if (!character.IsOnGround)
            lastDodgeTimesInAir--;

        GreatDodge = false;
        //设置闪避方向
        character.ForceOnGround = character.IsOnGround;
        switch (PlayerInput.Instance.GetMoveKey)
        {
            case 0:
                PlayAnimation("DodgeBack");
                direction = character.IsFacingRight ? -1 : 1;
                break;
            case -1:
                PlayAnimation("DodgeFront");
                direction = -1;
                break;
            case 1:
                PlayAnimation("DodgeFront");
                direction = 1;
                break;
        }
        
        InitDodgeSpeed(direction);
        //记录开始闪避的位置
        startPos = character.transform.position;

        ChangeDodgeState(DodgeState.startDodge);
    }
    ///// <summary>
    ///// 回满剩余闪避次数
    ///// </summary>
    //public void RefreshLastDodgeTimes()
    //{
    //    lastDodgeTimes = canDodgeTimes;
    //}
    public override void OnUpdate(float deltaTime)
    {
        if(currentDodgeState == DodgeState.startDodge)
        {
            if (controller.currentStateTime >= 0.03f)
            {
                Immuse = true;
                canGreatDodge = true;
            }
        }
        if (currentDodgeState == DodgeState.afterDodge)
        {
            if (lastTime < 0.1f && PlayerInput.Instance.PreInputs(CharacterInput.dodge) <= 0.1f)
            {
                controller.ChangeState(AllStates.Dodge);
                return;
            }
        }
        if (canGreatDodge)
        {
            GreatDodge = GreatDodgeCheck(direction, startPos);
            canGreatDodge = !GreatDodge;
        }
    }

    public bool GreatDodgeCheck(float direction, Vector2 startPos)
    {
        int num, i; 
        bool greatDodge = false;
        if (direction == 0)
        {
            for (float tmp = -0.2f; tmp < character.col2D.bounds.size.x + 0.3f; tmp += 0.15f)
            {
                num = Utils.RaycastNonAlloc(new Vector2(character.col2D.Left() + tmp, character.col2D.bounds.center.y), startPos - (Vector2)character.transform.position, greatDodgeCheck, (startPos - (Vector2)character.transform.position).magnitude + character.col2D.bounds.extents.y, Utils.GetMask(NameList.Layer.DamageArea), Color.blue);
                for(i = 0; i < num; i++)
                {
                    if (greatDodgeCheck[i].collider.transform.GetComponent<DamageAreaBase>().CanGreatDodgeCheck(character.transform))
                        break;
                }
                if (i != num)
                {
                    greatDodge = true;
                    break;
                }
            }
        }
        else
        {
            float rayStart = direction > 0 ? character.col2D.Right() : character.col2D.Left();
            for (float tmp = 0.2f; tmp < character.col2D.bounds.size.y - 0.1f; tmp += 0.4f)
            {
                num = Utils.RaycastNonAlloc(new Vector2(rayStart, character.col2D.Down() + tmp), startPos - (Vector2)character.transform.position,greatDodgeCheck, (startPos - (Vector2)character.transform.position).magnitude + character.col2D.bounds.size.y, Utils.GetMask(NameList.Layer.DamageArea), Color.blue);
                for (i = 0; i < num; i++)
                {
                    if (greatDodgeCheck[i].collider.transform.GetComponent<DamageAreaBase>().CanGreatDodgeCheck(character.transform))
                        break;
                }
                if (i != num)
                {
                    greatDodge = true;
                    break;
                }
            }
        }
        if (greatDodge)
        {
            character.cAtr.SetFocus(1, focusConsume);
            //character.cAtr.CurrentFocus += (int)(0.3f * character.cAtr.MaxFocus);
            TimeEventManager.Instance.ChangeTimeScale(Chronos.Timekeeper.instance.Clock("World"), timeScale, keepTime);
        }
        return greatDodge;
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        HandleDodgeSpeed();
        if (lastTime <= 0)
            ChangeDodgeState(currentDodgeState + 1);
        lastTime -= deltaTime;
    }
    public override void OnExitState(StateBase nextState)
    {
        character.ForceOnGround = false;
        character.rb2D.gravityScale = character.NormalGravityScale;
        //character.rb2D.velocity = realEndSpeed;

        if(currentDodgeState!=DodgeState.afterDodge)
        {
            character.IgnoreEnemyCollision(false);
        }

        //dodgeCD = intervalTime;
    }
}
