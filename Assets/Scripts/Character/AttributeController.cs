using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttributeController
{
    private CharacterBase cB;
    private bool isCharacter;
    public CharacterIntrinsicAttribute realCAtr;//根据cBAtr计算出的属性(character)，初始属性(enemy)
    public CharacterIntrinsicAttribute otherCAtr;// = new CharacterAttribute();//装备、道具等物品额外附带的属性
    public CharacterAttribute cAtr;//最终属性

    public void Init(CharacterBase cB)
    {
        this.cB = cB;
        realCAtr = new CharacterIntrinsicAttribute();
        otherCAtr = new CharacterIntrinsicAttribute();
        cAtr = new CharacterAttribute();
        isCharacter = cB is Character;
    }
    public void RebuildData(CharacterData.EntityData data)
    {
        if (isCharacter)
        {
            if (data != null)
            {
                var cdata = (CharacterData.CharacterEntityData)data;
                realCAtr.Set(cdata.realAtr);
                RefreshOtherCAtr();
                cAtr.CurrentHealth = cdata.runtimeData.health;
                cAtr.SetFocus(0, cdata.runtimeData.focus);
                cAtr.CurrentMagic = cdata.runtimeData.magic;
            }
            else
            {//没有data说明是新存档（或者是新的可操作角色？）
                realCAtr.Reset();
                realCAtr.baseHealth = 1;
                realCAtr.baseMagic = 1;
                realCAtr.baseFocus = 100;
                realCAtr.baseAttack = 1;
                RefreshOtherCAtr();
                cAtr.CurrentHealth = cAtr.MaxHealth;
                cAtr.SetFocus(0, cAtr.MaxFocus);
                cAtr.CurrentMagic = cAtr.MaxMagic;
            }
        }
        else
        {
            if (data != null)
            {
                var cdata = (CharacterData.EnemyEntityData)data;
                realCAtr.Set(cdata.realAtr);
                RefreshOtherCAtr();
                cAtr.CurrentHealth = cdata.runtimeData.health;
                cAtr.SetFocus(0, cdata.runtimeData.focus);
                cAtr.CurrentMagic = cdata.runtimeData.magic;
            }
            else
            {
                realCAtr.Reset();
                var tableAgent = Data.Instance.TableAgent;
                var enemyTypeStr = ((Enemy)cB).enemyType.ToString();
                realCAtr.baseHealth = tableAgent.GetInt("Enemy", enemyTypeStr, "Health");
                realCAtr.baseMagic = tableAgent.GetInt("Enemy", enemyTypeStr, "Magic");
                realCAtr.baseFocus = tableAgent.GetInt("Enemy", enemyTypeStr, "Focus");
                realCAtr.baseAttack = tableAgent.GetInt("Enemy", enemyTypeStr, "Attack");
                RefreshOtherCAtr();
                cAtr.CurrentHealth = cAtr.MaxHealth;
                cAtr.SetFocus(0, cAtr.MaxFocus);
                cAtr.CurrentMagic = cAtr.MaxMagic;
            }
        }
    }
    #region 属性管理
    /// <summary>
    /// 刷新装备、道具带来的属性
    /// </summary>
    public void RefreshOtherCAtr()
    {
        cB.effectCtr.RefreshCAtrEffect();
        RefreshCAtr();
    }
    /// <summary>
    /// 刷新最终属性
    /// </summary>
    public void RefreshCAtr()
    {
        float tmp;
        tmp = cAtr.CurrentHealth / cAtr.MaxHealth;//记录百分比
        cAtr.BaseHealth         = realCAtr.baseHealth       + otherCAtr.baseHealth;
        cAtr.CurrentHealth      = tmp * cAtr.MaxHealth;
        tmp = cAtr.CurrentMagic / cAtr.MaxMagic;//记录百分比
        cAtr.BaseMagic          = realCAtr.baseMagic        + otherCAtr.baseMagic;
        cAtr.CurrentMagic       = tmp * cAtr.MaxMagic;
        
        cAtr.BaseFocus          = realCAtr.baseFocus        + otherCAtr.baseFocus;
        cAtr.BaseAttack         = realCAtr.baseAttack       + otherCAtr.baseAttack;

        if(isCharacter)
            MessageManager.Instance.Get<Character.CharacterMsg>().DispatchMessage(Character.CharacterMsg.RefreshAttribute, this);
    }
    #endregion

}

[System.Serializable]
public class CharacterAttribute
{//角色最终属性，包括自身固有属性和buff加值，以及当前状态（当前血量、魔力值等）
    public struct SaveData
    {
        public SaveData(CharacterAttribute cAtr)
        {
            health = cAtr.CurrentHealth;
            magic = cAtr.CurrentMagic;
            focus = cAtr.CurrentFocus;
        }
        public float health;
        public float magic;
        public float focus;
    }
    [SerializeField]
    private float baseHealth;
    [SerializeField]
    private float currentHealth;

    //魔力值
    private float baseMagic;
    private float currentMagic;

    //专注值
    protected float baseFocus;
    protected float currentFocus;
    
    //攻击
    [SerializeField]
    private int baseAttack;
        
    public float MaxHealth
    {
        get => baseHealth;
    }
    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value > MaxHealth ? MaxHealth : value;
        }
    }
    public float MaxMagic
    {
        get => baseMagic;
    }
    public float CurrentMagic
    {
        get => currentMagic;
        set
        {
            if (value < 0)
                Debug.LogWarning("当前魔力值被试图设置为负数：" + value);
            currentMagic = Mathf.Clamp(value, 0, MaxMagic);
        }
    }
    public float MaxFocus
    {
        get => baseFocus;
    }
    public float CurrentFocus
    {
        get
        {
            return currentFocus;
        }
        protected set
        {
            currentFocus = Mathf.Clamp(value, 0, MaxFocus);
        }
    }
    /// <summary>
    /// 会根据相应系数设置专注值
    /// </summary>
    /// <param name="type">小于0是在原有基础上减少，大于是增加，等于0是直接设置</param>
    /// <param name="num"></param>
    /// <returns></returns>
    public virtual void SetFocus(int type, float num)
    {
        if (type == 0)
            CurrentFocus = num;
        else
            CurrentFocus += type > 0 ? num : -num;
    }
    public int Attack
    {
        get => (int)(baseAttack);
    }

    #region 基础属性
    public float BaseHealth
    {
        get => baseHealth;
        set
        {
            if (value < 0)
                Debug.LogError("基础血量被试图设置为负数：" + value);
            else
                baseHealth = value;
        }
    }
    public float BaseMagic
    {
        get => baseMagic;
        set
        {
            if (value < 0)
                Debug.LogError("基础魔力值被试图设置为负数" + value);
            else
                baseMagic = value;
        }
    }
    public float BaseFocus
    {
        get => baseFocus;
        set
        {
            if (value < 0)
                Debug.LogError("基础专注值被试图设置为负数" + value);
            else
                baseFocus = value;
        }
    }
    public int BaseAttack
    {
        get => baseAttack;
        set
        {
            if (value < 0)
                Debug.LogError("基础攻击力被试图设置为负数" + value);
            else
                baseAttack = value;
        }
    }
    #endregion
}
[System.Serializable]
public class CharacterIntrinsicAttribute
{//角色本身属性，以及buff提供属性
    public void Set(CharacterIntrinsicAttribute other)
    {
        baseHealth = other.baseHealth;
        baseMagic = other.baseMagic;
        baseFocus = other.baseFocus;
        baseAttack = other.baseAttack;
    }
    public void Reset()
    {
        baseHealth = baseMagic = baseFocus = baseAttack = 0;
    }
    //生命值
    public float baseHealth;

    //魔力值
    public float baseMagic;

    //专注值
    public float baseFocus;

    //攻击
    public int baseAttack;
}