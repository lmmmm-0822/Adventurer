using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Run", menuName = "States/Enemy1/Run")]
public class Run_Enemy1 : State_Enemy
{
    public float attackDistance;
    public float speed;

    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        PlayAnimation("Run");
    }
    public override void OnUpdate(float deltaTime)
    {
        if (enemy.Target == null)
        {
            controller.ChangeState(AllStates.Idle);
            return;
        }
        if(Mathf.Abs(enemy.Target.position.x - enemy.transform.position.x) < attackDistance)
        {
            controller.ChangeState(AllStates.Attack, 0);
        }
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        enemy.rb2D.velocity = new Vector2(enemy.IsFacingRight ? speed : -speed, enemy.rb2D.velocity.y);
    }
}
