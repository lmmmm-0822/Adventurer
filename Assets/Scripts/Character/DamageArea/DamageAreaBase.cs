using Chronos;
using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageAreaBase : MonoBehaviour
{
    protected Collider2D col;
    public IAttackable cB;
    protected Skill skill;
    protected Timeline timeline;

    /// <summary>
    /// 首次命中敌人的那一帧为true
    /// </summary>
    public bool FirstHit { get; private set; }
    /// <summary>
    /// 命中敌人后为true
    /// </summary>
    public bool HaveHit { get; private set; }
    /// <summary>
    /// 每次命中敌人的那一帧为true;
    /// </summary>
    public bool JustHit { get; private set; }
    /// <summary>
    /// 首次击杀敌人的那一帧为true
    /// </summary>
    public bool FirstKill { get; private set; }
    /// <summary>
    /// 击杀敌人后为true
    /// </summary>
    public bool HaveKilled { get; private set; }
    /// <summary>
    /// 每次击杀敌人的那一帧为true;
    /// </summary>
    public bool JustKill { get; private set; }
    /// <summary>
    /// 刚刚造成的伤害，命中多个敌人的话取最大的
    /// </summary>
    public int JustCauseDamage { get; private set; }

    private Dictionary<IDamageable, Transform> hitCache //记录一帧内命中的优先级最高的可受伤区域（目前只是弱点高于其他）
        = new Dictionary<IDamageable, Transform>(4);
    //private List<(Transform damageableArea, IDamageable cB)> hitCache = new List<(Transform, IDamageable)>(4);

    [HideInInspector]
    public List<Transform> hit = new List<Transform>(4);
    //protected int lastHitCount;
    public int strength;//攻击的强度，现在用于近战攻击击毁远程子弹

    private int damage;
    private List<Transform> beGreatDodge = new List<Transform>(2);//这个伤害区域被哪个角色极限闪避掉了

    protected virtual void Awake()
    {
        col = GetComponent<Collider2D>();
        timeline = GetComponent<Timeline>();
        if (timeline != null)
            timeline.Init();
    }
    protected virtual void OnEnable()
    {
        hit.Clear();
        beGreatDodge.Clear();
        hitCache.Clear();
        //lastHitCount = 0;
        FirstHit = false;
        JustHit = false;
        HaveHit = false;
        FirstKill = false;
        JustKill = false;
        HaveKilled = false;
    }
    public static DamageAreaBase Attack(Skill skill, IAttackable cB)
    {
        DamageAreaBase tmp = ResourceManager.Instance.Instantiate("Prefabs/" + (skill.IsRange ? skill.prefabPath : "Character/MeleeDamageArea"), cB.Transform).GetComponent<DamageAreaBase>();
        tmp.skill = skill;
        tmp.cB = cB;
        tmp.Attack();
        if (skill.IsRange && cB.AddRangeBulletCnt)
        {
            tmp = ResourceManager.Instance.Instantiate("Prefabs/" + skill.prefabPath, cB.Transform).GetComponent<DamageAreaBase>();
            tmp.skill = skill;
            tmp.cB = cB;
            ((RangeDamageArea)tmp).SetDirection(new Vector2(8, 1));
            tmp.Attack();
            tmp = ResourceManager.Instance.Instantiate("Prefabs/" + skill.prefabPath, cB.Transform).GetComponent<DamageAreaBase>();
            tmp.skill = skill;
            tmp.cB = cB;
            ((RangeDamageArea)tmp).SetDirection(new Vector2(8, -1));
            tmp.Attack();
        }
        return tmp;
    }
    public static void TempRangeAttack(Skill skill, IAttackable cB, Vector2 dir)
    {
        if (!skill.IsRange)
            return;
        DamageAreaBase tmp = ResourceManager.Instance.Instantiate("Prefabs/" + skill.prefabPath, cB.Transform).GetComponent<DamageAreaBase>();
        tmp.skill = skill;
        tmp.cB = cB;
        ((RangeDamageArea)tmp).SetDirection(dir, true);
        tmp.Attack();
    }
    public static void TempRangeAttack(Skill skill, IAttackable cB,int cnt)
    {
        if (!skill.IsRange)
            return;
        DamageAreaBase tmp;
        float initAngle = Random.Range(0, 2 * Mathf.PI);
        for (int i = 0; i < cnt; ++i)
        {
            tmp = ResourceManager.Instance.Instantiate("Prefabs/" + skill.prefabPath, cB.Transform).GetComponent<DamageAreaBase>();
            tmp.skill = skill;
            tmp.cB = cB;
            Vector2 dir = new Vector2(Mathf.Cos(2 * Mathf.PI / cnt * i + initAngle), Mathf.Sin(2 * Mathf.PI / cnt * i + initAngle));
            ((RangeDamageArea)tmp).SetDirection(dir,true);
            tmp.Attack();
        }
    }
    protected virtual void Attack()
    {
        if (timeline == null)
            timeline = cB.Timeline;
        transform.localPosition = skill.offset;

        damage = cB.Attack;
    }
    protected virtual void Update()
    {
        FirstHit = false;
        JustHit = false;
        FirstKill = false;
        JustKill = false;
        JustCauseDamage = 0;

        foreach (var pair in hitCache)
        {
            var tmp = pair.Key.Damage(new DamageArgs(cB,this,skill,damage,pair.Value));// pair.Key.Damage(damage, poiseDamage, pair.Value, skill, cB);//Todo
            if (tmp != null)//命中了敌人
            {
                if (hit.Count == 0)
                {
                    FirstHit = true;
                    HaveHit = true;
                }
                JustHit = true;

                if (pair.Key.IsDead)
                {
                    if (!HaveKilled)
                    {
                        HaveKilled = true;
                        FirstKill = true;
                    }
                    JustKill = true;
                }

                JustCauseDamage = System.Math.Max(JustCauseDamage, pair.Key.JustDamage);
                hit.Add(tmp);
            }
        }
        hitCache.Clear();

    }

    public bool CanGreatDodgeCheck(Transform character)
    {
        if (hit.Contains(character))
            return false;
        if (!beGreatDodge.Contains(character))
        {
            beGreatDodge.Add(character);
            return true;
        }
        else
            return false;
    }
    public virtual void ReplayAttack()
    {
        beGreatDodge.Clear();
        hit.Clear();
        TimeEventManager.Instance.RegisterTimeAction(1, () => { col.enabled = true; }, () => { col.enabled = false; });
    }
    public virtual void EndAttack()
    {
        //if (lastHitCount != hit.Count)
        //    TimeEventManager.Instance.RegisterTimeAction(1, () => { ObjectPool.Recycle(gameObject); });
        //else
        col.enabled = false;
        ObjectPool.Recycle(gameObject);
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == (int)NameList.Layer.DamageArea)
            return;//之后可能处理弹刀之类的 Todo

        if (collision.CompareTag(cB.Tag))
            return;//排除自身及友方Todo

        var tran = collision.transform;
        IDamageable damageable = tran.GetComponent<IDamageable>();
        while (damageable == null)
        {
            tran = tran.parent;
            if (tran == null) break;
            damageable = tran.GetComponent<IDamageable>();
        }
        if (damageable == null)
            return;//排除不能受伤的物体

        if (hit.Contains(tran))
            return;//排除已经命中过的目标

        if (beGreatDodge.Contains(tran))
            return;//排除对该伤害区域极限闪避过的对象

        if (!hitCache.ContainsKey(damageable))
        {
            hitCache.Add(damageable, collision.transform);
        }
    }

    public struct DamageArgs
    {
        public readonly IAttackable perpetrator;
        public readonly DamageAreaBase damageArea;
        public readonly Skill skill;
        public readonly int damage;
        public readonly Transform beHitArea;
        public DamageArgs(IAttackable perpetrator, DamageAreaBase damageArea, Skill skill, int damage,Transform beHitArea)
        {
            this.perpetrator = perpetrator;
            this.damageArea = damageArea;
            this.skill = skill;
            this.damage = damage;
            this.beHitArea = beHitArea;
        }
    }
}