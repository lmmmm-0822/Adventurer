using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "States/Enemy1/Idle")]
public class Idle_Enemy1 : State_Enemy
{
    private Attack_Enemy1 attack;
    private float lastPauseTime;//剩余攻击暂停时间
    private float pauseTime;//攻击暂停时间

    public override void Init()
    {
        base.Init();
        attack = controller.GetState<Attack_Enemy1>(AllStates.Attack);
    }
    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        PlayAnimation("Idle");
        pauseTime = Random.Range(1, 3);
        lastPauseTime = pauseTime;
    }
    public override void OnUpdate(float deltaTime)
    {
        if (enemy.Target != null && !attack.justAfterAttack&&enemy.IsOnGround)
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
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        enemy.Stopping(0.25f);
    }
}
