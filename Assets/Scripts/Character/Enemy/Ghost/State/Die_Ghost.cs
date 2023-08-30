using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Die_Ghost", menuName = "States/Ghost/Die_Ghost")]
public class Die_Ghost : DieBase_Enemy
{
    private Idle_Ghost idle;

    public override void Init()
    {
        idle = controller.GetState<Idle_Ghost>(AllStates.Idle);
    }

    public override void OnEnterState(StateBase lastState, float value = 0, object args = null)
    {
        base.OnEnterState(lastState, value, args);
        enemy.rb2D.velocity = new Vector2(0, 0);
        enemy.rb2D.gravityScale = 0;
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        idle.Stopping(0.02f);
    }
    public override void OnUpdate(float deltaTime)
    {
        lastExistTime -= deltaTime;
        if (lastExistTime <= 0)
        {
            Drop();
            GameMgr.CharacterMgr.RemoveCharacter(enemy);
            UIManager.Instance.Close(NameList.UI.BossUI);
            Data.Instance.StartCoroutine(SuccessUI());
        }
    }
    IEnumerator SuccessUI()
    {
        yield return new WaitForSeconds(1);
        UIManager.Instance.Open("SuccessUI");
    }
    protected override void SetAnimation()
    {
        PlayAnimation("Die");
    }
}
