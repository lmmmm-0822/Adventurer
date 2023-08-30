using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "States/Enemy1/Attack")]
public class Attack_Enemy1 : AttackBase_Enemy
{
    public bool justAfterAttack;
    public string bulletName;

    public override void Init()
    {
        base.Init();

        allSkills = new Skill[1];
        allSkills[0] = new Skill
        {
            num = 0,//攻击技能
            offset = new Vector2(0.268f, 0.23f),
            setSpeed = new Vector2(1f, -0.5f),
            type = 0b_100,//range
            stopTime = 0.1f,
            prefabPath = "Character/" + bulletName,
        };
    }

    public override void OnEnterState(StateBase lastState, float value, object args)
    {
        enemy.ChangeFacing(enemy.Target.position.x);
        base.OnEnterState(lastState, value, args);
    }
    public override void OnExitState(StateBase nextState)
    {
        if (currentAttackState == AttackState.attacking)
            RecycleDamageArea();
        justAfterAttack = true;
    }
}
