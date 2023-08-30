using UnityEngine;
using System;
using System.Diagnostics;
using System.Reflection;
using QxFramework.Core;
using System.Collections.Generic;

public abstract class State : StateBase
{
    protected Character character;

    public override void SetCharacter(CharacterBase characterBase)
    {
        character = (Character)characterBase;
    }

    public override void PlayAnimation(string name,float normalizeTime = 0)
    {
        //character.erAnimator.Test(name);
        character.animator.component.Play(name, -1, normalizeTime);
        // layer:
        //     The layer index. If layer is -1, it plays the first state with the given state
        //     name or hash.
        character.animator.component.Update(0);
    }
}
