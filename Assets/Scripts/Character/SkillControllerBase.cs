using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using QxFramework.Core;

public abstract class SkillControllerBase//抽象类：不能生成对象（实例化）只能用作基类，用于类型隐藏和充当全局变量。
{
    protected StateController stateController;
    
    protected List<List<Skill>> allSkills = new List<List<Skill>>();//使用了类似交错数组存储所有技能
    protected byte[] ownSkills;//按位表示角色是否有该技能，1有0没有

    protected StateBase currentState;
    /// <summary>
    /// 仅由子类调用，需要提供技能表的名字
    /// </summary>
    protected void Init(CharacterBase cB,string skillTableName)
    {
        if (skillTableName == null)
        {
            Debug.LogError("SkillControllerBase中的Init函数没有被重写，需要重写并且设置技能表名字");
            return;
        }
        stateController = cB.GetComponent<StateController>();

        #region 读表以初始化技能
        TableAgent _tableAgent = Data.Instance.TableAgent;
        List<string> numbers = _tableAgent.CollectKey1(skillTableName);
        List<Skill> tempSkills = new List<Skill>();
        for (int i = 1,cnt = numbers.Count; i < cnt; i++)//从1开始，不读取第一个num为0的样例技能
        {
            Skill skill = new Skill
            {
                num = int.Parse(numbers[i]),
                level = 1,
                offset = _tableAgent.GetVector2(skillTableName, numbers[i], "PositionOffset"),
                radius = _tableAgent.TryGetFloat(skillTableName, numbers[i], "Radius", 0),
                size = _tableAgent.GetVector2(skillTableName, numbers[i], "Size"),
                stopTime = _tableAgent.GetFloat(skillTableName, numbers[i], "StopTime"),
                type = _tableAgent.GetIntByByte(skillTableName, numbers[i], "Type"),
                prefabPath = _tableAgent.GetString(skillTableName, numbers[i], "PrefabPath"),

                setSpeed = _tableAgent.GetVector2(skillTableName, numbers[i], "AddSpeed"),
            };

            //if (i == 0)
            //{//将编号为0的技能存为allSkills[0][0]
            //    allSkills.Add(new List<Skill>(1) { skill });
            //}
            //else
            //{
            if (i != 1 && skill.num % 10 == 1)
            {//如果技能尾数为1，则将tempSkills加入allSkills，并重新分配tempSkills的内存，再存入新的skill
                allSkills.Add(tempSkills);
                tempSkills = new List<Skill>();
            }
            tempSkills.Add(skill);
            //}
        }
        allSkills.Add(tempSkills);//存入最后一组tempSkills
        #endregion

        #region 初始化角色拥有的技能
        ownSkills = new byte[10];//支持11~18、21~28……101~108 
        #endregion
    }
    public virtual void AddSkill(int num)
    {//将技能编号对应的ownSkills位设置为1，表示获得了该技能
        int temp1 = num / 10 - 1;
        int temp2 = num % 10 - 1;
        ownSkills[temp1] |= (byte)(1 << temp2);
    }
    public virtual void OnUpdate(float deltaTime)
    {
        currentState = stateController.currentState;
        HandleUseSkill();
        //CheckInput();
        //CheckUseSkill();
    }
    protected virtual void HandleUseSkill() { }
    //protected virtual void CheckInput() { }
    //protected virtual void CheckUseSkill() { }

    public Skill GetSkill(int num)
    {
        try { return allSkills[num / 10 - 1][num % 10 - 1]; }
        catch
        {
            Debug.LogError("不存在编号为" + num + "的技能");
            return null;
        }
    }
    public virtual Skill GetOwnSkill(int num)
    {//判断是否有编号为num的技能
        int temp1 = num / 10 - 1;
        int temp2 = num % 10 - 1;
        try
        {
            if ((ownSkills[temp1] & (1 << temp2)) > 0)
                return allSkills[temp1][temp2];
            else return null;
        }
        catch
        {
            Debug.LogError("技能编号超出范围");//现在因为有了输入判断，所以应该不会触发 //但实际上只是个位越界的话，不会报错
            return null;
        }
    }
}

public class Skill
{
    public enum Type
    {
        ground = 0b_000,//_00
        air = 0b_011,//_11
        groundToAir = 0b_001,//_01
        airToGround = 0b_010,//_10
        range = 0b_100,//1 _ _;
        //melee = 0,//0 _ _
    }
    public bool IsType(Type type)
    {
        return type switch
        {
            Type.ground => this.type % 4 == 0,
            Type.air => this.type % 4 == 3,
            Type.groundToAir => this.type % 4 == 1,
            Type.airToGround => this.type % 4 == 2,
            Type.range => (this.type & 4) != 0,
            _ => false,
        };
    }
    /// <summary>
    /// 返回是地面招式、空中招式....
    /// </summary>
    /// <returns></returns>
    public Type GetDisplaceType()
    {
        return (Type)(type % 4);
    }

    /// <summary>
    /// 伤害区域是否是圆形的
    /// </summary>
    public bool IsCircle { get => radius != 0; }
    public bool IsRange { get => (type & 4) != 0; }

    //技能自身属性
    public int num;
    public int level;
    public Vector2 offset;
    public float radius;
    public Vector2 size;
    public float stopTime;//卡肉时间
    public int type { private get; set; }
    public string prefabPath;//如果是远程攻击，需要获取预设的路径
    //对敌人属性
    public Vector2 setSpeed;

    //之后完全用动画来控制，暂时为了方便先这么用着
    public Vector2 beforeAttackMoveSpeed;
    public Vector2 attackingMoveSpeed;
    public Vector2 afterAttackMoveSpeed;
}

