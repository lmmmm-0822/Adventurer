using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework.Core;
using System;

public class SkillController : SkillControllerBase
{
    private Character character;

    /// <summary>
    /// 三招之内是否使用过该技能
    /// </summary>
    public bool HaveUsedThisSkill { get; private set; }

    private class UseSkillTimes
    {//特性中TimesLimited需要
        public UseSkillTimes(int num)
        {
            skillNum = num;
            usedTimes = 0;
        }
        public int skillNum;
        public int usedTimes;
    }
    private List<int> usedAirSkill = new List<int>(8);//用于记录空中释放过的技能
    //private List<CharacterInput> group;//搓招用

    public void Init(Character character)
    {
        Init(character,"SkillAttribute");
        this.character = character;
        AddSkill(11);
        AddSkill(21);
    }
    protected override void HandleUseSkill()
    {
        if (character.JustOnGround)
        {
            usedAirSkill.Clear();//RefreshUsedSkill();
        }

        if (PlayerInput.Instance.GetKeyDown(CharacterInput.attack1))
        {
            switch (currentState.state)
            {
                case AllStates.Defend:
                case AllStates.Idle:
                case AllStates.Run:
                case AllStates.Jump:
                case AllStates.Fall:
                    TryUseSkill(11);
                    return;
            }
        }
        if (PlayerInput.Instance.GetKeyDown(CharacterInput.attack2))
        {//近战攻击
            switch (currentState.state)
            {
                case AllStates.Defend:
                case AllStates.Idle:
                case AllStates.Run:
                case AllStates.Jump:
                case AllStates.Fall:
                    TryUseSkill(21);
                    return;
            }
        }
    }
    /// <summary>
    /// 检查技能是否匹配输入
    /// </summary>
    /// <param name="skillNum"></param>
    /// <returns></returns>
    private bool CheckInput(int skillNum)
    {
        switch (skillNum)
        {
            case 11:
            case 21:
                return true;
        }
        Debug.LogWarning("没有设置技能" + skillNum.ToString() + "的输入");
        return false;
    }
    /// <summary>
    /// 检查其他条件
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    private bool CheckCondition(Skill skill)
    {
        if (skill == null)//未获取该技能
            return false;

        //if (skill.focusConsume > character.cAtr.CurrentFocus)//专注值不够ToDo
        //    return false;

        switch (skill.GetDisplaceType())
        {//在地面不能释放空中招式、空中不能释放地面招式
            case Skill.Type.air:
                if (character.IsOnGround)
                    return false;
                if (usedAirSkill.Contains(skill.num)) //在空中使用过的技能不可以再被释放
                    return false;
                break;
            case Skill.Type.airToGround:
                if (character.IsOnGround)
                    return false;
                break;
            case Skill.Type.ground:
            case Skill.Type.groundToAir:
                if (!character.IsOnGround)
                    return false;
                break;
        }
        return true;
    }
    /// <summary>
    /// 尝试使用技能，会检查输入和条件
    /// </summary>
    /// <param name="num"></param>
    /// <param name="checkInput"></param>
    /// <returns></returns>
    private bool TryUseSkill(int num, bool checkInput = true)
    {
        //检查输入和条件
        if (checkInput && !CheckInput(num))
            return false;
        var skill = GetOwnSkill(num);
        if (!CheckCondition(skill))
            return false;

        //使用技能
        stateController.ChangeState(AllStates.Attack, args: skill);//stateChange
        return true;
    }
    /// <summary>
    /// 真的用出了技能，没有被打断什么的，由Attack调用
    /// </summary>
    /// <param name="skill"></param>
    public void RealUseSkill(Skill skill)
    {//使用技能产生影响
        if (skill.IsType(Skill.Type.air) && !usedAirSkill.Contains(skill.num))
            usedAirSkill.Add(skill.num);//将空中使用的技能记录下来
    }
    

    public override void AddSkill(int num)
    {
        int temp1 = num / 10 - 1;
        int temp2 = num % 10 - 1;
        ownSkills[temp1] |= (byte)(1 << temp2);
    }
}