using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Die", menuName = "States/Player/Die")]
//受伤后在Character中判断currentHealth转入
public class Die : State
{
    private float uiTime;
    //public override void Init()
    //{
    //    controller.RegisterOnUpdateAction((t) =>
    //    {
    //        if (character.IsDead && controller.currentState.state != AllStates.Die)
    //            controller.ChangeState(AllStates.Die);
    //    });
    //}
    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        PlayAnimation("Die");
        if (character.IsOnGround)
            character.rb2D.gravityScale = 0;
        character.rb2D.velocity = Vector2.zero;
        TimeEventManager.Instance.ChangeTimeScale(Chronos.Timekeeper.instance.Clock("World"), 0.4f, 1f);
        uiTime = 0.7f;
    }
    public override void OnUpdate(float deltaTime)
    {
        if (character.JustOnGround)
            character.rb2D.gravityScale = 0;
        uiTime -= deltaTime;
        if (uiTime <= 0)
        {
            UIManager.Instance.Open(NameList.UI.DieUI);
            return;
        }
    }
    public override void OnExitState(StateBase nextState)
    {
        character.rb2D.gravityScale = character.NormalGravityScale;
    }
}

