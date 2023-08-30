using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Run", menuName = "States/Player/Run")]
//Run一般只能在地面触发，由Idle转入
public class Run : State
{//7 10 0.8 3
    [SerializeField, Tooltip("最大速度，建议4.5")]
    private float maxSpeed = 4.5f;
    [SerializeField, Tooltip("每个fixedUpdate加多少速度")]
    private float addSpeed = 0.8f;
    [SerializeField, Tooltip("初速度，建议3")]
    private float initialSpeed = 3;

    private Idle idle;
    private float realMaxSpeed;
    private float realInitSpeed;
    public override void Init()
    {
        base.Init();
        idle = controller.GetState<Idle>(AllStates.Idle);
    }
    public void RefreshFacing()
    {
        switch (PlayerInput.Instance.GetMoveKey)
        {
            case 1: character.ChangeFacing(true); break;
            case -1: character.ChangeFacing(false); break;
        }
    }
    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        character.ForceOnGround = true;
        character.rb2D.gravityScale = 0;
        PlayAnimation("Run");
        realMaxSpeed = maxSpeed;
        realInitSpeed = initialSpeed;
        HandleCharacterState();
    }
    public override void OnExitState(StateBase nextState)
    {
        character.ForceOnGround = false;
        character.rb2D.gravityScale = character.NormalGravityScale;
        character.animator.speed = 1;
    }

    private void HandleCharacterState()
    {
        #region 处理玩家状态
        RefreshFacing();
        //以下条件，优先Dodge，Jump，后Fall，最后Idle
        //if (type == RunType.walk && PlayerInput.Instance.GetKeyDown(CharacterInput.dodge)
        // || type == RunType.run && PlayerInput.Instance.GetKeyUp(CharacterInput.dodge))
        if (PlayerInput.Instance.GetKeyDown(CharacterInput.dodge))
        {
            controller.ChangeState(AllStates.Dodge);
            return;
        }
        if (PlayerInput.Instance.GetKeyDown(CharacterInput.jump))
        {
            controller.ChangeState(AllStates.Jump, character.GetSpeedWithGround(realMaxSpeed).x * character.moveSpeedRate);
            return;
        }
        if (!character.IsOnGround)
        {
            controller.ChangeState(AllStates.Fall);
            return;
        }
        if (PlayerInput.Instance.GetKey(CharacterInput.defend))
        {
            controller.ChangeState(AllStates.Defend);
            return;
        }
        if (!PlayerInput.Instance.GetKey(CharacterInput.moveRight)
          && !PlayerInput.Instance.GetKey(CharacterInput.moveLeft))
        {
            controller.ChangeState(AllStates.Idle);
            return;
        }
        #endregion

    }
    public override void OnUpdate(float deltaTime)
    {
        character.animator.speed = character.moveSpeedRate;
        HandleCharacterState();
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        Move(realMaxSpeed * character.moveSpeedRate, addSpeed * character.timeline.timeScale * character.moveSpeedRate, false, realInitSpeed * character.moveSpeedRate);//Running(deltaTime,maxSpeed, runEfficiency, initialSpeed, turnSpeed);
    }
    public void Move(float maxSpeed, float addSpeed, bool back = false, float initialSpeed = default)
    {
        bool moveRight = character.IsFacingRight != back;//面向右不后退，面向左后退
        var velocity = character.rb2D.velocity;
        //空中移动
        if (!character.IsOnGround)
        {
            Debug.LogError("感觉不会在空中调用这个函数，看看什么时候出现");
            if (moveRight)
            {
                if (initialSpeed != default && velocity.x < initialSpeed)
                {
                    character.rb2D.velocity = new Vector2(initialSpeed, velocity.y);
                }
                else
                {
                    if (velocity.x + addSpeed >= maxSpeed)
                        character.rb2D.velocity = new Vector2(maxSpeed, velocity.y);
                    else
                        character.rb2D.velocity += new Vector2(addSpeed, 0);
                }
            }
            else
            {
                if (initialSpeed != default && velocity.x > -initialSpeed)
                {
                    character.rb2D.velocity = new Vector2(-initialSpeed, velocity.y);
                }
                else
                {
                    if (velocity.x - addSpeed <= -maxSpeed)
                        character.rb2D.velocity = new Vector2(-maxSpeed, velocity.y);
                    else
                        character.rb2D.velocity -= new Vector2(addSpeed, 0);
                }
            }
            return;
        }
        //地面移动
        #region 在陡坡旁
        if (character.IsBesideSlope == 2 && !moveRight
         || character.IsBesideSlope == 1 && moveRight
         || character.IsBesideSlope == 3)
        {
            var y = velocity.y;
            if (y > 0) y = 0;
            character.rb2D.velocity = new Vector2(0, y);
            return;
        }
        #endregion
        #region 减速转向
        if (!back
         && (!moveRight && velocity.x > 0.2f
          || moveRight && velocity.x < -0.2f))
        {
            idle.Stopping(addSpeed * 2);
            return;
        }
        #endregion

        float estimate = velocity.magnitude;
        if (initialSpeed != default && estimate <= initialSpeed - addSpeed)
        {//有最小速度，且当前速度小于最小速度
            estimate = initialSpeed;
        }
        else if (estimate >= maxSpeed - addSpeed)
        {//当前速度大于或接近最大速度
            if (estimate >= maxSpeed + 0.5f)
            {//大于
                estimate -= 0.5f;
            }
            else
            {
                estimate = maxSpeed;
            }
        }
        else
        {
            estimate += addSpeed;
        }
        if (!moveRight) estimate = -estimate;
        character.rb2D.velocity = character.GetSpeedWithGround(estimate);
    }
}
