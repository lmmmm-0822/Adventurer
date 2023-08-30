using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle_Ghost", menuName = "States/Ghost/Idle_Ghost")]
public class Idle_Ghost : State_Enemy
{

    public float maxSpeed;//移动速度
    private Attack_Ghost attack;

    private float lastPauseTime;//剩余攻击暂停时间
    public float pauseTime;//攻击暂停时间
    public float pauseTime2;//攻击暂停时间

    private float lastTime;//剩余等待时间
    private float ySpeed;

    private bool hasMetTarget;

    public override void Init()
    {
        base.Init();
        attack = controller.GetState<Attack_Ghost>(AllStates.Attack);
        enemy.rb2D.gravityScale = 0;
    }

    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        hasMetTarget = false;
        lastPauseTime = Random.Range(pauseTime,pauseTime2);
        ySpeed = 0.1f;

        if (lastState.state == AllStates.Run || lastState.state == AllStates.Attack || lastState.state == AllStates.BeHit)
        {
            hasMetTarget = true;
        }
        PlayAnimation("Idle");
    }

    public override void OnUpdate(float deltaTime)
    {
        if (enemy.Target != null && !attack.justAfterAttack)
        {
            controller.ChangeState(AllStates.Run);
            return;
        }
        if (attack.justAfterAttack && lastPauseTime >= 0)
        {
            lastPauseTime -= deltaTime;
        }
        else
            attack.justAfterAttack = false;
        if (controller.currentStateTime > 5)
            hasMetTarget = false;

    }
    public override void OnFixedUpdate(float deltaTime)
    {

        if (hasMetTarget)
        {
            if (enemy.DistanceToGround < 0.5f)
                enemy.rb2D.velocity = new Vector2(enemy.AdaptFacing(maxSpeed), 10);
            else
            {
                enemy.rb2D.velocity = new Vector2(enemy.AdaptFacing(maxSpeed), ySpeed);
                if (lastTime <= 0)
                {
                    ySpeed = Random.Range(-0.75f, 1.25f);
                    lastTime = Random.Range(1, 4);
                    enemy.ChangeFacing(enemy.IsFacingRight ? false : true);
                }
                else
                {
                    lastTime -= deltaTime;

                }

            }
        }

        else
        {
            if (enemy.DistanceToGround > 2)
                enemy.rb2D.velocity = new Vector2(enemy.rb2D.velocity.x, -1);
            else
                Stopping(0.1f);
        }
            

    }

    public override void OnExitState(StateBase nextState)
    {
        base.OnExitState(nextState);
    }

    public void Stopping(float reduceSpeed)
    {
        Vector2 velocity = enemy.rb2D.velocity;
        if (velocity.x == 0) return;
        if (reduceSpeed > velocity.x / velocity.normalized.x
         || velocity.normalized.x == 0)
            enemy.rb2D.velocity = Vector2.zero;
        else
            enemy.rb2D.velocity -= reduceSpeed * velocity.normalized;
    }

}