using NameList;
using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : Enemy
{
    public override Transform Target
    {
        get => target; 
        set
        {
            if (target == null && value != null)
            {
                UIManager.Instance.Open(UI.BossUI, args: this);
                target = value;
                AudioControl.Instance.BGMPlay("1", 0.3F);
            }
            else if (value == null && target != null)
            {
                UIManager.Instance.Close(UI.BossUI);
                target = value;
                AudioControl.Instance.BGMPlay("2",0.75f);
            }
        }
    }
    private Transform target;
    private SpriteRenderer apperance;
    private GameObject damageableArea;
    private float lastCanDamageTime;
    private bool canDamage;
    public int level;
    protected override void Initialize()
    {
        base.Initialize();
        apperance = transform.Find("Appearance").GetComponent<SpriteRenderer>();
        damageableArea = transform.Find("DamageableArea").gameObject;
    }
    protected override void RefreshData()
    {
        base.RefreshData();
        level = 1;
        canDamage = true;
        lastCanDamageTime = 0;
    }
    protected override bool HandleDamageByState(int damage, bool backToPerpetrator, Skill skill, IAttackable perpetrator, ref int realDamage)
    {
        if (lastCanDamageTime > 0)
            return false;
        if (!canDamage)
            return false;
        if (cAtr.CurrentHealth <= 8)
            level = 2;
        if (level == 2 && cAtr.CurrentHealth > 0)//二阶段被打后一段时间不会受伤
        {
            lastCanDamageTime = 1;
            damageableArea.SetActive(false, true);
        }
        return true;
    }
    protected override bool CheckBeHit()
    {
        return base.CheckBeHit()//生命值允许
            && (!(stateController.currentState is Attack_Ghost attack && attack.Skill.num == 1));//并且不是大招
    }
    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if (lastCanDamageTime > 0)
        {
            lastCanDamageTime -= deltaTime;
            if (lastCanDamageTime <= 0)
                damageableArea.SetActive(true, true);
        }
        var oriColor = apperance.color;
        oriColor.a = canDamage && lastCanDamageTime <= 0 ? 1 : 0.5f;
        apperance.color = oriColor;
    }
    protected override void HandleCollisionLayer()
    {
        gameObject.layer = Utils.NameToLayer(Layer.EnemyIgnoreOthers);
    }

    public void CanDamage(bool can)
    {
        canDamage = can;
        damageableArea.SetActive(can, true);
    }
    protected override void OnRecycle(bool die)
    {
        base.OnRecycle(die);
        UIManager.Instance.Close(UI.BossUI);
    }
}
