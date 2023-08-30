using Chronos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//不设置StatesPoiseCoefficient，成功防御时直接修改poise
[CreateAssetMenu(fileName = "Defend", menuName = "States/Player/Defend")]
public class Defend : State
{
    [SerializeField,Tooltip("前摇时间")]
    private float beforeDefendTime;
    [SerializeField, Tooltip("后摇时间")]
    private float afterDefendTime;
    [SerializeField,Tooltip("防御时的移动速度")]
    private float moveSpeed;
    [SerializeField,Tooltip("精防时间")]
    private float perfectTime;

    public bool Defending { get => currentDefendState == DefendState.defending; }
    public bool IsPerfect { get => currentDefendState == DefendState.defending && currentStateTime <= perfectTime; }
    public bool JustPerfectDefend
    {
        get => havePerfectDefendedTime < 0.5f;
        set
        {
            if (value)
                havePerfectDefendedTime = 0;
        }
    }

    private Idle idle;
    private Run run;
    private bool moving;
    private bool playingMove;
    private enum DefendState
    {
        before,
        defending,
        after,
        end,
    }
    private DefendState currentDefendState;
    private float currentStateTime;
    private float havePerfectDefendedTime;
    private void ChangeDefendState(DefendState nextState)
    {
        currentStateTime = 0;
        if (nextState == DefendState.before)
        {
            run.RefreshFacing();
            if (playingMove)
                PlayAnimation("DefendMoveStart");
            else
                PlayAnimation("DefendStart");

            if (character.rb2D.velocity.sqrMagnitude > moveSpeed * moveSpeed)
            {
                character.rb2D.velocity = moveSpeed * character.rb2D.velocity.normalized;
            }
        }
        else if (nextState == DefendState.defending)
        {

        }
        else if (nextState == DefendState.after)
        {
            PlayAnimation("DefendEnd");
        }
        else
        {
            controller.ChangeState(AllStates.Idle);
            return;
        }
        currentDefendState = nextState;
    }
    public void BeHitWhileDefending(IAttackable perpetrator,Skill skill, ref int damage)
    {
        character.rb2D.velocity = character.GetSpeedWithGround(2 * (perpetrator.IsFacingRight ? skill.setSpeed.x : -skill.setSpeed.x));
        damage = 0;
        if (IsPerfect)
        {
            JustPerfectDefend = true;            
            perpetrator.BePerfectDefended();
            TimeEventManager.Instance.ChangeTimeScale(Timekeeper.instance.Clock("Enemy"), 0, 0.5f, 5);//Todo!
        }
        else
        {
            character.cAtr.SetFocus(-1, 10f);
            perpetrator.BeDefended();
        }
    }
    public override void Init()
    {
        idle = controller.GetState<Idle>(AllStates.Idle);
        run = controller.GetState<Run>(AllStates.Run);
    }
    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        havePerfectDefendedTime = 1;
        character.rb2D.gravityScale = 0;
        playingMove = lastState.state == AllStates.Run;
        ChangeDefendState(DefendState.before);
    }
    public override void OnUpdate(float deltaTime)
    {
        if (PlayerInput.Instance.GetKeyDown(CharacterInput.dodge))
        {
            controller.ChangeState(AllStates.Dodge);
            return;
        }
        if (!character.IsOnGround)
        {
            controller.ChangeState(AllStates.Fall);
            return;
        }

        havePerfectDefendedTime += deltaTime;

        currentStateTime += deltaTime;
        switch (currentDefendState)
        {
            case DefendState.after:
                if (currentStateTime > afterDefendTime)
                    ChangeDefendState(DefendState.end);
                if (PlayerInput.Instance.GetKeyDown(CharacterInput.defend))
                    ChangeDefendState(DefendState.before);
                break;

            case DefendState.before:
                if (currentStateTime > beforeDefendTime)
                    ChangeDefendState(DefendState.defending); goto default;
            default:
                if (!PlayerInput.Instance.GetKey(CharacterInput.defend))
                    ChangeDefendState(DefendState.after);
                break;
        }

        if (currentDefendState == DefendState.defending)
        {
            if (moving && !playingMove)
            {
                PlayAnimation("DefendMove");
                playingMove = true;
            }
            else if (!moving && playingMove)
            {
                PlayAnimation("Defend");
                playingMove = false;
            }
        }
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        switch (PlayerInput.Instance.GetMoveKey)
        {
            case 0: moving = false; idle.Stopping(1f); break;
            case 1: moving = true; run.Move(moveSpeed, 0.5f, !character.IsFacingRight, moveSpeed); break;
            case -1: moving = true; run.Move(moveSpeed, 0.5f, character.IsFacingRight, moveSpeed); break;
        }
    }
    public override void OnExitState(StateBase nextState)
    {
        character.rb2D.gravityScale = character.NormalGravityScale;
    }
}
