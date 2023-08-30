using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Jump", menuName = "States/Player/Jump")]
//按下跳跃键后可由Idle无判断转入，由Run、Fall经判断后转入
public class Jump : State
{//15 2 0.22 0.1
    [SerializeField]
    private float initalSpeed = 15f;
    [SerializeField]
    private int canJumpTimes = 2;
    [SerializeField, Tooltip("操作间隔时间")]
    private float intervalTime = 0.22f;//对二段跳、闪避起作用
    [SerializeField, Tooltip("二段跳之前的预输入时间")]
    private float preinputTime = 0.1f;
    [Header("水平移动")]//3 1 3 1 2
    //[SerializeField]
    //private float horizontalInitalAddSpeed = 4;
    //[SerializeField]
    //private float horizontalSpeed = 4;
    [SerializeField]
    private float horizontalAddSpeed = 0.1f;
    ////[SerializeField]
    ////private float horizontalSpeedEfficiency = 3;
    //[SerializeField]
    //private float horizontalInitialSpeed = 1;
    [SerializeField]
    private float reduceSpeed = 0.1f;
    //private float stopEfficiency = 2;

    private int lastJumpTimes;
    private float lastIntervalTime;
    private float realHorizontalSpeed;
    private bool doubleJump;
    //private bool preinputJump;
    private Run run;
    private Idle idle;
    private Fall fall;
    public override void Init()
    {
        base.Init();
        doubleJump = false;
        run = controller.GetState<Run>(AllStates.Run);
        idle = controller.GetState<Idle>(AllStates.Idle);
        fall = controller.GetState<Fall>(AllStates.Fall);
        controller.RegisterOnUpdateAction((t) =>
        {
            if (character.JustOnGround)
                lastJumpTimes = canJumpTimes + (doubleJump ? 1 : 0);
            if (character.JustExitGround)
                lastJumpTimes--;
        });
    }
    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        if (lastState.state != AllStates.Fall)
            realHorizontalSpeed = value;
        if (realHorizontalSpeed == 0)
            realHorizontalSpeed = 2;
        PlayAnimation("Jump");
        ToJump();
    }
    /// <summary>
    /// 剩余跳跃次数大于0，且距离上次跳跃时间大于所需间隔时间
    /// </summary>
    /// <returns></returns>
    public bool CheckCanJump()
    {
        return lastJumpTimes > 0 && lastIntervalTime <= 0;
    }
    public void SetDoubleJump(bool set)
    {
        doubleJump = set;
    }
    private bool CheckCanOperate() { return !character.IsOverGround && lastIntervalTime <= 0; }
    /// <summary>
    /// 默认回满次数，1代表加一次，-1代表减一次，0代表剩余次数置为0
    /// </summary>
    /// <param name="times"></param>
    public void ChangeLastJumpTimes(int times = 255)
    {
        if (times == 255)
            lastJumpTimes = canJumpTimes + (doubleJump ? 1 : 0);
        else if (times == 0)
            lastJumpTimes = 0;
        else
            lastJumpTimes += times;
    }
    public void ToJump()
    {
        if (!character.IsOnGround)
            lastJumpTimes--;
        //preinputJump = false;
        lastIntervalTime = intervalTime;
        //if (character.IsFacingRight && PlayerInput.Instance.GetKey(CharacterInput.moveRight))
        //    character.rb2D.velocity = new Vector2(character.rb2D.velocity.x + horizontalInitalAddSpeed, initalSpeed);
        //else if (!character.IsFacingRight && PlayerInput.Instance.GetKey(CharacterInput.moveLeft))
        //    character.rb2D.velocity = new Vector2(character.rb2D.velocity.x - horizontalInitalAddSpeed, initalSpeed);
        //else
        character.rb2D.velocity = new Vector2(character.rb2D.velocity.x, initalSpeed * (character.addJumpHeight ? 1.2f : 1));
    }
    //public void RefreshIntervalTime()
    //{
    //    lastIntervalTime -= lastIntervalTime > 0 ? Time.fixedDeltaTime : 0;
    //}
    public override void OnUpdate(float deltaTime)
    {
        character.FireInAir();
        #region 处理玩家状态
        run.RefreshFacing();
        if (CheckCanOperate())
        {
            if (PlayerInput.Instance.PreInputs(CharacterInput.dodge) <= preinputTime)
            {
                controller.ChangeState(AllStates.Dodge);
                return;
            }
        }
        if (CheckCanJump())
        {//二段跳的间隔时间大于角色跳起来超过isOverGround高度的时间，所以不用检测是不是!isOverGround
            if (PlayerInput.Instance.PreInputs(CharacterInput.jump) <= preinputTime)
                ToJump();
        }
        if (character.rb2D.velocity.y <= 0)
        {
            controller.ChangeState(AllStates.Fall);
            return;
        }
        #endregion
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        //fall.Falling();
        if (PlayerInput.Instance.GetKey(CharacterInput.moveRight)
         || PlayerInput.Instance.GetKey(CharacterInput.moveLeft))
        {
            Move(realHorizontalSpeed, horizontalAddSpeed * character.timeline.timeScale);//, initialSpeed: horizontalInitialSpeed);//RunningAir(deltaTime,horizontalSpeed, horizontalSpeedEfficiency, horizontalInitialSpeed);
        }
        else
        {
            idle.Stopping(reduceSpeed * character.timeline.timeScale);
        }

        lastIntervalTime -= lastIntervalTime > 0 ? deltaTime : 0;
    }
    public void Move(float maxSpeed, float addSpeed, bool back = false, float initialSpeed = default)
    {
        bool moveRight = character.IsFacingRight != back;//面向右不后退，面向左后退
        if (moveRight)
        {
            if (initialSpeed != default && character.rb2D.velocity.x < initialSpeed)
            {
                character.rb2D.velocity = new Vector2(initialSpeed, character.rb2D.velocity.y);
            }
            else
            {
                if (character.rb2D.velocity.x + addSpeed >= maxSpeed)
                    character.rb2D.velocity = new Vector2(maxSpeed, character.rb2D.velocity.y);
                else
                    character.rb2D.velocity += new Vector2(addSpeed, 0);
            }
        }
        else
        {
            if (initialSpeed != default && character.rb2D.velocity.x > -initialSpeed)
            {
                character.rb2D.velocity = new Vector2(-initialSpeed, character.rb2D.velocity.y);
            }
            else
            {
                if (character.rb2D.velocity.x - addSpeed <= -maxSpeed)
                    character.rb2D.velocity = new Vector2(-maxSpeed, character.rb2D.velocity.y);
                else
                    character.rb2D.velocity -= new Vector2(addSpeed, 0);
            }
        }
    }
}
