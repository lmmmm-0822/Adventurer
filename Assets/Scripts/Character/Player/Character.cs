using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NameList;
using Chronos;
using QxFramework.Core;
using System;

public class Character : CharacterBase
{
    public SkillController skillCtr = new SkillController();

    public override Transform Target
    {//一般只有Enemy会用到，但RangeAttackArea也会用到
        get
        {
            if (stateController.currentState.state == AllStates.Attack)
                return ((Attack)stateController.currentState).Target;
            return null;
        }
    }

    #region 角色状态
    //public bool Invincible
    //{
    //    get => _invincible == 0;
    //    set
    //    {
    //        _invincible += value ? -1 : 1;//设置为false则canAttack加1，否则减1
    //        if (_invincible < 0) Debug.LogError("哪里没有设置无敌就取消了无敌");
    //    }
    //}
    public bool CanInteract
    {
        get => _canInteract == 0;
        set
        {
            _canInteract += value ? -1 : 1;//设置为false则canInteract加1，否则减1
            if (_canInteract < 0) Debug.LogError("哪里没有设置不可交互就取消了不可交互");
            //if (_canInteract == 0)
            //    MessageManager.Instance.Get<AbilityMsg>().DispatchMessage(AbilityMsg.CanInteractive, this);
            //else if (!value && _canInteract == 1)
            //    MessageManager.Instance.Get<AbilityMsg>().DispatchMessage(AbilityMsg.CannotInteractive, this);
        }
    }
    public bool CanAttack
    {
        get => _canAttack == 0;
        set
        {
            _canAttack += value ? -1 : 1;//设置为false则canAttack加1，否则减1
            if (_canAttack < 0) Debug.LogError("哪里没有设置不可攻击就取消了不可攻击");
            //if (_canAttack == 0)
            //    MessageManager.Instance.Get<AbilityMsg>().DispatchMessage(AbilityMsg.CanAttack, this);
            //else if (!value && _canAttack == 1)
            //    MessageManager.Instance.Get<AbilityMsg>().DispatchMessage(AbilityMsg.CannotAttack, this);
        }
    }
    public bool CanMove
    {
        get => _canMove == 0;
        set
        {
            _canMove += value ? -1 : 1;//设置为false则canAttack加1，否则减1
            if (_canMove < 0)
            {
                TimeEventManager.Instance.RegisterUpdateAction(() =>
                {
                    if (_canMove < 0)
                        Debug.LogError("哪里没有设置不可移动就取消了不可移动");
                    TimeEventManager.Instance.UnRegistTimeAction("多次取消不可移动".GetHashCode());
                }, 0.5f, "多次取消不可移动".GetHashCode());
            }
            //if (_canMove == 0)
            //    MessageManager.Instance.Get<AbilityMsg>().DispatchMessage(AbilityMsg.CanMove, this);
            //else if (!value && _canMove == 1)
            //    MessageManager.Instance.Get<AbilityMsg>().DispatchMessage(AbilityMsg.CannotMove, this);
        }
    }

    private int _canInteract = 1;//初始不可交互，进入Idle状态后可交互
    private int _canAttack;
    private int _canMove;
    #endregion

    public int CurrentPropPos { get; private set; }//使用物品相关
    public int TempProp { get; private set; }//使用物品相关，表示哪个位置有物品，即使数量为0，也算有物品，通过UI修改道具栏，才会刷新
    public override float RangeLifeTimeRate => bulletRangeRate;
    public override bool AddRangeBulletCnt => addBulletCnt;

    public float bulletRangeRate;
    public float moveSpeedRate;
    public bool addJumpHeight;
    public float bulletIntervalRate;
    public bool addBulletCnt;
    public bool haveShield;
    public bool fireInAir;

    public int gold;



    protected override void Initialize()
    {
        base.Initialize();
    }
    protected override void InitData()
    {
        base.InitData();
        skillCtr.Init(this);
        transform.GetChild(0).GetComponent<AnimeEventCharacter>().Init(this);


        stateController.RegisterEnterExitAction((nextState, enter) =>//enter为false时代表exit
        {
            switch (nextState)
            {
                case AllStates.Idle:
                case AllStates.Run:
                    CanInteract = enter;
                    break;
            }
        });

        MessageManager.Instance.Get<CharacterMsg>().DispatchMessage(CharacterMsg.Instantiate, this);
    }
    protected override void RebuildData(CharacterData.EntityData data)
    {
        _canInteract = 1;
        _canAttack = 0;
        _canMove = 0;
        base.RebuildData(data);
    }
    protected override void RefreshData()
    {
        fireInAir = false;
        bulletRangeRate = 1;
        moveSpeedRate = 1;
        bulletIntervalRate = 1;
        addBulletCnt = false;
        addJumpHeight = false;
        haveShield = false;
        gold = 0;
        effectCtr.Init(this);
        base.RefreshData();

        MessageManager.Instance.Get<GameSceneMsg>().RegisterHandler(GameSceneMsg.NewSceneLoaded, (sender, args) => { if (Init) stateController.ChangeState(AllStates.Idle); });

        MessageManager.Instance.Get<CharacterMsg>().DispatchMessage(CharacterMsg.Appear, this);
    }
    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);//CheckGround

        switch (stateController.currentState.state)
        {
            case AllStates.Idle:
            case AllStates.Interactive:
                cAtr.SetFocus(1, deltaTime * 2f);
                break;
        }
        if (JustOnGround)
            fireInAir = false;

        stateController.OnUpdate(deltaTime);
        skillCtr.OnUpdate(deltaTime);
        effectCtr.OnUpdate();
    }


    #region 被攻击
    protected override void Hurt(int realDamage)
    {
        if (realDamage == 0) 
            return;
        if(haveShield)
        {
            effectCtr.RemoveEffect("Shield");
            return;
        }
        base.Hurt(realDamage);
        //MessageManager.Instance.Get<CharacterMsg>().DispatchMessage(CharacterMsg.Damaged, this);
    }
    protected override bool HandleDamageByState(int damage, bool backToPerpetrator, Skill skill, IAttackable perpetrator, ref int realDamage)
    {
        switch (stateController.currentState.state)
        {
            case AllStates.Attack:
                if (((Attack)stateController.currentState).Immune)//免疫伤害则不会受伤
                    return false;
                break;
            case AllStates.Dodge:
                if (((Dodge)stateController.currentState).Immuse)
                {
                    return false;
                }
                break;
            case AllStates.Defend://状态内部自己设置了防御中修改状态韧性系数，和伤害减免，所以不用在这里设置
                if (backToPerpetrator || !((Defend)stateController.currentState).Defending)
                {
                    break;
                }
                ((Defend)stateController.currentState).BeHitWhileDefending(perpetrator, skill, ref realDamage);
                break;
            default:
                break;
        }

        return true;
    }
    protected override void HandleBeHitEffect()
    {
        //ToDo
    }
    #endregion

    public void IgnoreEnemyCollision(bool check)
    {
        if (check) gameObject.layer = Utils.NameToLayer(Layer.PlayerIgnoreEnemy);
        else gameObject.layer = Utils.NameToLayer(Layer.Player);
    }
    public void FireInAir()
    {
        if (PlayerInput.Instance.GetKeyDown(CharacterInput.attack2) && !fireInAir)
        {
            fireInAir = true;
            DamageAreaBase.Attack(skillCtr.GetOwnSkill(21), this);
        }
    }
    public enum CharacterMsg
    {
        Instantiate,
        Appear,
        Damaged,
        FightStart,
        FightEnd,
        UseShortcutsProp,
        ChangeSelectProp,
        RefreshAttribute,
        ChangeEffect,
        ChangePersonality,
    }
    public enum AbilityMsg
    {
        CanInteractive,
        CannotInteractive,
        CanAttack,
        CannotAttack,
        CanMove,
        CannotMove,
    }
}
