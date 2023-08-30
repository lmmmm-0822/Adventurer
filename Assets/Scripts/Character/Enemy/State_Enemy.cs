using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State_Enemy : StateBase
{
    protected Enemy enemy;
    public override void SetCharacter(CharacterBase characterBase)
    {
        enemy = (Enemy)characterBase;
    }
    public override void PlayAnimation(string name, float normalizeTime = default)
    {
        enemy.animator.component.Play(name, -1, normalizeTime);
        enemy.animator.component.Update(0);
    }
}
