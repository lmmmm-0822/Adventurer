using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class AnimeEventCharacter : MonoBehaviour
{
    private Character character;
    //private AnimatorTimeline animator;
    private Attack attack;
    private BeHit beHit;

    public void Init(Character character)
    {
        this.character = character;
        attack = character.stateController.GetState<Attack>(AllStates.Attack);
        beHit = character.stateController.GetState<BeHit>(AllStates.BeHit);
        //animator = character.animator;
    }
    private void SetSpeed(float speed)
    {
        var tmp = character.DirectGround(character.IsFacingRight == speed > 0);
        character.rb2D.velocity = character.AdaptFacing(speed) * tmp.x * tmp;
    }
    private void SetSpeedY(float speed)
    {
        character.rb2D.velocity = new Vector2(character.rb2D.velocity.x, speed);
    }
    private void ChangeTimeSpeed(float speed)
    {
        Timekeeper.instance.Clock("World").localTimeScale = speed;//之后时间统一管理
    }
    private void StopWorldTime(float time)
    {
        TimeEventManager.Instance.ChangeTimeScale(Timekeeper.instance.Clock("World"), 0, time);
    }
    private void UnRegistTimeEvent(int sign)
    {
        TimeEventManager.Instance.UnRegistTimeAction(sign);
    }
    private void ChangeEnemyTimeSpeed(float speed)
    {
        if (speed != 0)
            Timekeeper.instance.Clock("Enemy").paused = false;
        Timekeeper.instance.Clock("Enemy").localTimeScale = speed;
    }

    #region 技能事件
    private void Attacking()
    {
        attack.ChangeAttackState(1);
    }
    private void AfterAttack()
    {
        attack.ChangeAttackState(2);
    }
    private void CanDodge(int canDodge)
    {
        attack.CanDodge(canDodge == 1);
    }
    private void EndAttack()
    {
        attack.ChangeAttackState(3);
    }
    private void StopAnimation(float keepTime = 0.3f)
    {
        switch (attack.Skill.GetDisplaceType())
        {
            case Skill.Type.airToGround:
                if (!character.IsOnGround)
                    character.StopAnimation();
                break;
            default:
                character.StopAnimation();
                break;
        }
    }
    #endregion

    #region 受伤事件
    private void EndBeHit()
    {
        beHit.EndBeHit();
    }
    #endregion
}
