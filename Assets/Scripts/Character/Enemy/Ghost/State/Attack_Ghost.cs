using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack_Ghost", menuName = "States/Ghost/Attack_Ghost")]
public class Attack_Ghost : AttackBase_Enemy
{
    public bool justAfterAttack;

    private Idle_Ghost idle;

    public override void Init()
    {
        base.Init();
        idle = controller.GetState<Idle_Ghost>(AllStates.Idle);
        allSkills = new Skill[2];
        allSkills[0] = new Skill
        {
            num = 0,
            offset = new Vector2(0.3f, -0.2f),
            type = 0b_100,
            stopTime = 0.05f,
            setSpeed = new Vector2(1, 0),
            prefabPath = "Character/EnemyBullet3",
        };
        allSkills[1] = new Skill
        {
            num = 1,
            type = 0b_100,
            stopTime = 0.05f,
            setSpeed = new Vector2(1, 0),
            prefabPath = "Character/EnemyBullet3",
        };
    }

    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        enemy.ChangeFacing(enemy.Target.position.x > enemy.transform.position.x);
        //PlayAnimation("Attack");
        base.OnEnterState(lastState, value, args);
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        idle.Stopping(0.1f);
    }

    public override void OnExitState(StateBase nextState)
    {
        if (currentAttackState == AttackState.attacking)
            RecycleDamageArea();
        justAfterAttack = true;
    }

    public override void CreateDamageArea()
    {
        if (Skill.num == 1)
            DamageAreaBase.TempRangeAttack(Skill, enemy, Random.Range(7, 11));
        else if (Skill.num == 0)
            DamageAreaBase.TempRangeAttack(Skill, enemy, enemy.Target.position - enemy.transform.position);
    }
}
