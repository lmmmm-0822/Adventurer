using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Run_Ghost", menuName = "States/Ghost/Run_Ghost")]
public class Run_Ghost : State_Enemy
{
    public float attackDistance;
    public float maxSpeed;


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

        enemy.ChangeFacing(enemy.Target.position.x);

        if ((enemy.Target.position - enemy.transform.position).sqrMagnitude < attackDistance * attackDistance&& enemy.transform.position.y-enemy.Target.position.y<1.5f /*&& enemy.transform.position.y - enemy.Target.position.y>0.5f*/)
        {
            if ((enemy.Target.position - enemy.transform.position).sqrMagnitude > 1f)
            {
                if (Mathf.Abs((enemy.Target.position.x - enemy.transform.position.x)
                    / (enemy.Target.position.y - enemy.transform.position.y)) > 0.25)
                {
                    int skillId = 0;
                    if (((Ghost)enemy).level == 2)
                        skillId = Random.Range(0, 1f) > 0.6f ? 1 : 0;
                    controller.ChangeState(AllStates.Attack, skillId);
                }
                return;
            }
        }
      
    }
    public override void OnFixedUpdate(float deltaTime)
    {
        //if (System.Math.Abs(enemy.Target.position.y+1 - enemy.transform.position.y)<0.1f)
        //    enemy.rb2D.velocity = new Vector2(enemy.FacingDirection == 1 ? maxSpeed : -maxSpeed, 0);
        //else
        if (enemy.Target.position.y > enemy.transform.position.y )//加1防止了它抽搐
        {
            enemy.rb2D.velocity = new Vector2(enemy.AdaptFacing(maxSpeed), maxSpeed);
        }
        if (enemy.Target.position.y < enemy.transform.position.y - 1)
        {
            enemy.rb2D.velocity = new Vector2(enemy.AdaptFacing(maxSpeed),  -maxSpeed);
        }
        
    }
}
