using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "States/Player/Idle")]
//默认状态，由Fall落地、Run松开移动键转入
public class Idle : State
{//0.5 7 0.1 0.1
    [SerializeField, Tooltip("每个fixedUpdate减速多少")]
    public float reduceSpeed = 0.5f;
    //[SerializeField, Tooltip("类似加速度，建议7")]
    //private float stopEfficiency = 7;
    [SerializeField, Tooltip("进入Idle后立刻跳跃的预输入时间")]
    private float preInputJumpTime = 0.1f;
    [SerializeField, Tooltip("进入Idle后立刻闪避的预输入时间")]
    private float preInputDodgeTime = 0.1f;
    //[SerializeField, Tooltip("进入Idle后立刻攻击的预输入时间")]
    //private float preInputAttackTime = 0.1f;
    //private Jump jump;
    //private Dodge dodge;
    //private bool getFightButtonDown;//在Idle状态下按下拔刀键，才可以切换FightTransition状态

    public void Stopping(float reduceSpeed)
    {
        Vector2 velocity = character.rb2D.velocity;
        if (character.IsOnGround)
        {
            if (Mathf.Approximately(velocity.x, 0))
            {
                //if (!Mathf.Approximately(velocity.y, 0))
                //    character.rb2D.velocity = new Vector2(velocity.x, 0);
                return;//发生碰撞时unity会施加一个很小的反向速度，如果把这个速度强制改为0，就会卡在碰撞到的collider里
            }
            float normalizedX = velocity.normalized.x;
            if (normalizedX == 0 || reduceSpeed > velocity.x / normalizedX)
            {
                character.rb2D.velocity = Vector2.zero;
                //character.transform.position -= new Vector3(character.AdaptFacing(0.02f), 0, 0);//紧贴着发生碰撞会推不动另一个collider，需要有一个缓冲距离
            }
            else
                character.rb2D.velocity -= reduceSpeed * velocity.normalized;
        }
        else
        {
            if (velocity.x < 0)
            {
                if (reduceSpeed < -velocity.x)
                    character.rb2D.velocity += new Vector2(reduceSpeed, 0);
                else character.rb2D.velocity -= new Vector2(velocity.x, 0);
            }
            else
            {
                if (reduceSpeed < velocity.x)
                    character.rb2D.velocity -= new Vector2(reduceSpeed, 0);
                else character.rb2D.velocity -= new Vector2(velocity.x, 0);
            }
        }
    }
    public override void Init()
    {
        base.Init();
        //jump = controller.GetState<Jump>(AllStates.Jump);
        //dodge = controller.GetState<Dodge>(AllStates.Dodge);
    }
    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        //getFightButtonDown = false;
        character.ForceOnGround = true;
        character.rb2D.gravityScale = 0;
        PlayAnimation("Idle");
        //jump.ChangeLastJumpTimes();
        HandleCharacterState();//在OnEnterState中改变状态后，会先执行该状态的OnExitState函数，然后执行剩余的OnEnterState代码
    }
    private void HandleCharacterState()
    {
        #region 处理玩家状态
        //以下操作，优先Dodge，Jump，后Fall，Slide，最后为Run
        if (PlayerInput.Instance.PreInputs(CharacterInput.dodge) <= preInputDodgeTime)
        {
            controller.ChangeState(AllStates.Dodge);
            return;
        }
        //既处理预输入跳跃，也处理正常跳跃
        if (PlayerInput.Instance.PreInputs(CharacterInput.jump) <= preInputJumpTime)
        {//因为进入Idle就会刷新lastJumpTimes，所以不用CheckCanJump
            controller.ChangeState(AllStates.Jump);
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
        //if (getFightButtonDown &&
        //    PlayerInput.Instance.GetKey(CharacterInput.drawSword) &&
        //    PlayerInput.Instance.PreInputs(CharacterInput.drawSword) > 0.3f)
        //{//按住拔刀键0.3秒
        //    controller.ChangeState(AllStates.FightTransition, value: 1);
        //    return;
        //}
        //if (getFightButtonDown &&
        //    PlayerInput.Instance.GetKeyUp(CharacterInput.drawSword))
        //{
        //    controller.ChangeState(AllStates.FightTransition);
        //    return;
        //}

        if (PlayerInput.Instance.GetKey(CharacterInput.moveRight)
         || PlayerInput.Instance.GetKey(CharacterInput.moveLeft))
        {
            controller.ChangeState(AllStates.Run);
            return;
        }
        #endregion
    }
    public override void OnUpdate(float deltaTime)
    {
        //if (PlayerInput.Instance.GetKeyDown(CharacterInput.drawSword))
        //    getFightButtonDown = true;
        HandleCharacterState();
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        Stopping(reduceSpeed * character.timeline.timeScale);
    }

    public override void OnExitState(StateBase nextState)
    {
        character.ForceOnGround = false;
        character.rb2D.gravityScale = character.NormalGravityScale;
    }
}
