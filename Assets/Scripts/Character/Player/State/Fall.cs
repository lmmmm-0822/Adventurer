using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fall", menuName = "States/Player/Fall")]
//由Idle、Run从平台掉落转入，由Jump经判断速度转入
public class Fall : State
{
    //[Tooltip("建议4倍重力（9.81）")]
    //public float gravity = 39.24f;
    [SerializeField, Tooltip("土狼时间")]
    private float graceTime = 0.1f;
    //[SerializeField, Tooltip("落地后直接跳跃的预输入时间")]
    //private float preinputTime = 0.1f;//转入Idle后进行处理
    [Header("水平移动，数值一般与Jump相同")]
    [SerializeField]
    private float horizontalSpeed = 7;
    [SerializeField]
    private float horizontalAddSpeed = 0.1f;
    //[SerializeField]
    //private float horizontalSpeedEfficiency = 3;
    [SerializeField]
    private float horizontalInitialSpeed = 1;
    [SerializeField]
    private float reduceSpeed = 0.1f;
    //private float stopEfficiency = 2;

    private float lastGraceTime;
    private Jump jump;
    private Dodge dodge;
    private Run run;
    private Idle idle;
    public override void Init()
    {
        base.Init();
        jump = controller.GetState<Jump>(AllStates.Jump);
        dodge = controller.GetState<Dodge>(AllStates.Dodge);
        run = controller.GetState<Run>(AllStates.Run);
        idle = controller.GetState<Idle>(AllStates.Idle);
    }
    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        if (character.JustExitGround)
            PlayAnimation("Jumping");
        if (character.rb2D.velocity.y > 0)
            character.rb2D.velocity = new Vector2(character.rb2D.velocity.x, 0f);
        if (character.JustExitGround)
        {//从平台掉落，上一个状态是Run或者Idle
            //jump.ChangeLastJumpTimes(-1);  //不是跳跃结束后自然掉落需要减lastJumpTimes
            lastGraceTime = graceTime;  //不是跳跃结束后掉落，获得土狼时间
        }
    }
    //public void Falling()
    //{
    //    character.rb2D.velocity += new Vector2(0, -gravity * Time.fixedDeltaTime);
    //}

    public override void OnUpdate(float deltaTime)
    {
        character.FireInAir();
        #region 处理玩家状态
        run.RefreshFacing();
        if (character.IsOnGround)
        {
            if (PlayerInput.Instance.PreInputs(CharacterInput.jump) <= 0.2f)
            {
                controller.ChangeState(AllStates.Jump);
                return;
            }
            controller.ChangeState(AllStates.Idle);
            return;
        }
        if (!character.IsOverGround)
        {//低空状态不能操作，让角色尽快落地，以刷新跳跃次数，防止角色低空闪避后，不能快速落地进行其他操作
            if (PlayerInput.Instance.GetKeyDown(CharacterInput.dodge))
            {
                controller.ChangeState(AllStates.Dodge);
                return;
            }
            if (jump.CheckCanJump() || lastGraceTime > 0)
            {
                if (PlayerInput.Instance.GetKeyDown(CharacterInput.jump))
                {
                    if (lastGraceTime > 0)
                    {
                        lastGraceTime = 0;
                        jump.ChangeLastJumpTimes(1);
                    }
                    controller.ChangeState(AllStates.Jump);//二段跳和从平台掉落后跳跃
                    return;
                }
            }
        }
        #endregion

        lastGraceTime -= lastGraceTime > 0 ? deltaTime : 0;
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        if (character.rb2D.velocity.y < -12)//* character.timeline.timeScale)//ToDo
            character.rb2D.velocity += new Vector2(0, 40 * deltaTime/*每物理帧加的速度*/);
        //Falling();
        if (PlayerInput.Instance.GetKey(CharacterInput.moveRight)
         || PlayerInput.Instance.GetKey(CharacterInput.moveLeft))
        {
            jump.Move(horizontalSpeed * character.moveSpeedRate, horizontalAddSpeed * character.timeline.timeScale, initialSpeed: horizontalInitialSpeed);//jump.RunningAir(deltaTime, horizontalSpeed, horizontalSpeedEfficiency, horizontalInitialSpeed);
        }
        else
        {
            idle.Stopping(reduceSpeed * character.timeline.timeScale);
        }
    }
    public override void OnExitState(StateBase nextState)
    {
        if (character.IsFacingRight != character.rb2D.velocity.x > 0)
        {
            character.rb2D.velocity /= 3;
        }
    }
}

