using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NameList;
using Chronos;
using QxFramework.Core;

public abstract class CharacterBase : MonoBehaviour, IDamageable, IAttackable
{//最基础的角色脚本，在这里初始化状态机和技能控制脚本
    protected bool Init { get; private set; }
    public static LayerMask groundLayer = Utils.GetMask(Layer.Ground);// { get => LayerMask.GetMask("Ground"); }
    
    [HideInInspector] public Timeline timeline;
    [HideInInspector] public LocalClock localClock;//角色行为时钟，仅影响角色动作，不影响buff持续
    [HideInInspector] public RigidbodyTimeline2D rb2D;
    [HideInInspector] public Collider2D col2D;
    [HideInInspector] public StateController stateController;
    [HideInInspector] public AnimatorTimeline animator;

    public EffectController effectCtr = new EffectController();
    public AttributeController atrCtr = new AttributeController();
    public CharacterAttribute cAtr => atrCtr.cAtr; // = new CharacterAttribute();
    private float normalTimeScale;
    public float NormalTimeScale
    {
        get => normalTimeScale;
        set
        {
            if (normalTimeScale == value) 
                return;
            localClock.localTimeScale = localClock.localTimeScale / normalTimeScale * value;
            normalTimeScale = value;
        }

    }
    public float NormalGravityScale { get; private set; }

    //public DamageAreaBase DamageArea { get; protected set; }
    #region 攻击相关
    public virtual float RangeLifeTimeRate => 1;
    public virtual bool AddRangeBulletCnt => false;
    public virtual Transform Target { get; set; }
    public int Attack => cAtr.Attack;
    public string Tag => tag;
    public Transform Transform => transform;
    public Timeline Timeline => timeline;
    public LocalClock LocalClock => localClock;
    #endregion
    #region 受伤相关
    private GameObject beHitEffect;//被命中特效
    public int JustDamage { get; private set; }
    public bool IsDead { get => cAtr.CurrentHealth <= 0; }
    public bool IsDeadState { get=> stateController.currentState.state == AllStates.Die;}
    #endregion
    #region 方向相关
    public bool IsFacingRight { get; private set; }
    public float AdaptFacing(float data) { return IsFacingRight ? data : -data; }
    public Vector2 AdaptFacing(Vector2 data) { return IsFacingRight ? data : new Vector2 { x = -data.x, y = data.y }; }//效率低于float AdaptFacing(float)，但为了代码美观、易读，还是推荐使用该函数
    #endregion
    #region 地面相关
    [SerializeField, Tooltip("地面检测线多长")]
    private float groundCheckDistance = 20f;
    [SerializeField, Tooltip("距离地面多远算将要落地")]//1
    private float overGroundDistance = 1f;
    [SerializeField, Tooltip("检测多远算是陡坡，为0则不检测斜面，建议0.2f")]
    private float steepSlopeCheckDistance = 0;
    public float DistanceToGround { get; private set; }
    public bool IsOnGround { get; private set; }
    public bool JustOnGround { get; private set; }//落地瞬间为true
    public bool JustExitGround { get; private set; }//脱离地面瞬间为true
    public bool IsOverGround { get; private set; }
    public bool IsOnSlope { get; private set; }
    public byte IsBesideSlope { get; private set; }
    public bool ForceOnGround { get; set; }
    /// <summary>
    /// 获取地面方向向量，仅角色在地面上时才能获取到
    /// </summary>
    public Vector2 DirectGround(bool isRight = false)
    {
        if (IsOnGround)
        {
            var nor = ground.normal;
            float tmp = nor.x;
            nor.x = nor.y;
            nor.y = -tmp;
            return nor;
        }
        else
        {
            return Vector2.right;
        }
    }
    //public Vector2 DirectGround(bool isRight)
    //{
    //    if (IsOnGround)
    //    {
    //        if (leftGround.distance == rightGround.distance)
    //        {//x一定为正数         
    //            return isRight ? new Vector2(rightGround.normal.y, -rightGround.normal.x) : new Vector2(leftGround.normal.y, -leftGround.normal.x);
    //        }
    //        else
    //        {
    //            return leftGround.distance > rightGround.distance ? new Vector2(rightGround.normal.y, -rightGround.normal.x) : new Vector2(leftGround.normal.y, -leftGround.normal.x);
    //        }
    //    }
    //    else
    //    {
    //        return Vector2.right;
    //    }
    //}
    public Vector2 GetSpeedWithGround(float speed)
    {
        var tmp = DirectGround();
        if (speed >= 0 == tmp.y >= 0) //上坡
            return new Vector2() { x = speed, y = 0 };
        tmp.x *= speed;
        tmp.y *= speed;
        return tmp;
        //var estimateVel = speed * tmp.x * tmp;
        //if (estimateVel.y > 0) //上坡
        //    estimateVel = new Vector2(estimateVel.y * estimateVel.y / estimateVel.x + estimateVel.x, 0);
    }
    private RaycastHit2D ground;// leftGround, rightGround, 
    private float realSlopeCheckDistance;
    #endregion


    /// <summary>
    /// 仅在实例化时调用一次
    /// </summary>
    protected virtual void Initialize()
    {
        foreach(var t in GetComponentsInChildren<TimelineEffector>(true))
        {//Timeline初始化
            t.Init();
        }
        //获取组件
        localClock = GetComponent<LocalClock>();
        timeline = GetComponent<Timeline>();
        rb2D = timeline.rigidbody2D;
        col2D = GetComponent<Collider2D>();
        stateController = GetComponent<StateController>();
        animator = transform.GetChild(0).GetComponent<TimelineChild>().animator;
    }
    protected virtual void InitData()
    {
        atrCtr.Init(this);
        effectCtr.Init(this);
        stateController.Init(this);
        NormalGravityScale = rb2D.gravityScale;//不会修改的值，直接在初始化的时候赋值就好
        realSlopeCheckDistance = col2D.bounds.extents.x + steepSlopeCheckDistance;
        IsFacingRight = transform.localScale.x == 1;
    }
    /// <summary>
    /// 角色生成时调用
    /// </summary>
    /// <param name="data">不为null时使用data生成数据</param>
    protected virtual void RebuildData(CharacterData.EntityData data)
    {
        if (data != null)
        {
            transform.position = data.position;
            effectCtr.RebuildData(data.effects);//先还原好buff，以便属性控制器刷新属性
        }
        atrCtr.RebuildData(data);
        stateController.StateInit();
    }
    /// <summary>
    /// 角色每次加载到场景中时调用
    /// </summary>
    protected virtual void RefreshData()
    {//之后需要刷新effect，因为离开场景后敌人不会继续刷新effect  Todo
        localClock.localTimeScale = 1;
        normalTimeScale = 1;
        effectCtr.RefreshData();
    }
    protected virtual void OnUpdate(float deltaTime) 
    {
        CheckGround();//放在FixedUpdate里会有问题
    }
    protected virtual void OnFixedUpdate(float deltaTime) 
    {
        stateController.OnFixedUpdate(deltaTime);
    }
    /// <summary>
    /// 切换场景时会调用，但die为false；死亡时会调用，此时die为true
    /// </summary>
    /// <param name="die"></param>
    protected virtual void OnRecycle(bool die)
    {
        MessageManager.Instance.RemoveAbout(this);
    }
    public void ChangeFacing(bool faceToRight)
    {
        if (IsFacingRight == faceToRight)
            return;
        IsFacingRight = faceToRight;
        transform.localScale = new Vector3 { x = faceToRight ? 1 : -1, y = 1, z = 1 };
        //appearance.flipX = !faceToRight;
    }
    /// <summary>
    /// 默认面向positionX的位置
    /// </summary>
    /// <param name="positionX"></param>
    /// <param name="faceTo">面向还是背向</param>
    public void ChangeFacing(float positionX, bool faceTo = true)
    {
        ChangeFacing(positionX > transform.position.x == faceTo);
    }
    public virtual void StopAnimation()
    {
        animator.speed = 0;
    }
    public virtual void RestartAnimation()
    {
        animator.speed = 1;
    }

    #region 处理攻击被阻挡
    public virtual void BePerfectDefended()
    {

    }
    public virtual void BeDefended()
    {

    }
    #endregion

    #region 处理被攻击
    //protected float[] degree;
    //public virtual Transform Damage(int damage, int poiseDamage,Transform beHitArea, Skill skill, CharacterBase perpetrator)
    public virtual Transform Damage(DamageAreaBase.DamageArgs damageArgs)
    {
        Skill skill = damageArgs.skill;
        int damage = damageArgs.damage;
        IAttackable perpetrator = damageArgs.perpetrator;
        DamageAreaBase damageArea = damageArgs.damageArea;


        if (IsDeadState)    
            return null;                                         
                                                                    
        int realDamage = GetCurrentDamage(damage);

        bool backToPerpetrator = perpetrator.Transform.position.x - transform.position.x > 0 != IsFacingRight;
        if (!HandleDamageByState(damage, backToPerpetrator, skill, perpetrator, ref realDamage))
            return null;

        Hurt(realDamage);
        JustDamage = realDamage;

        //处理受击特效ToDO
        HandleBeHitEffect();
        //击退
        KnockBack(skill, perpetrator, damageArea);
        //受击状态
        if (CheckBeHit())
            ChangeToBeHitState(skill, perpetrator,damageArea);

        return transform;
    }
    public void TempDamage(Transform t)
    {
        Hurt(1);
        JustDamage = 1;

        //处理受击特效ToDO
        HandleBeHitEffect();
        //击退
        rb2D.velocity = new Vector2 { x = t.position.x-transform.position.x>0?-3:3, y = rb2D.velocity.y };
        //受击状态
        if (CheckBeHit())
            stateController.ChangeState(AllStates.BeHit, -1, t);//stateChange
    }
    protected virtual void Hurt(int realDamage)
    {
        cAtr.CurrentHealth -= realDamage;
    }
    protected virtual void KnockBack(Skill skill, IAttackable perpetrator, DamageAreaBase damageArea)
    {
        float setSpeedY = rb2D.velocity.y;
        var symbol = (skill.IsRange ? damageArea.transform.position.x < transform.position.x : perpetrator.IsFacingRight) ? 1 : -1;
        rb2D.velocity = new Vector2 { x = symbol * skill.setSpeed.x, y = setSpeedY };
    }
    protected virtual int GetCurrentDamage(int damage)
    {
        return damage;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>是否应该进入受击状态</returns>
    protected virtual bool CheckBeHit()
    {
        if (cAtr.CurrentHealth <= 0)
        {
            stateController.ChangeState(AllStates.Die);//stateChange
            return false;
        }
        if (stateController.currentState.state == AllStates.BeHit || JustDamage == 0)
            return false;
        return true;
    }
    protected virtual bool HandleDamageByState(int damage, bool backToPerpetrator, Skill skill,IAttackable perpetrator, ref int realDamage)
    {
        return true;
    }
    protected virtual void HandleBeHitEffect()
    {
        if (beHitEffect != null)
            ObjectPool.Recycle(beHitEffect);
        beHitEffect = ResourceManager.Instance.Instantiate("Prefabs/Effect/BeHitEffect", transform);
        beHitEffect.transform.localPosition = Vector3.zero;
        TimeEventManager.Instance.RegisterTimeAction(0.2f, () =>
        {
            if (beHitEffect != null)
            {
                ObjectPool.Recycle(beHitEffect);
                beHitEffect = null;
            }
        });
    }
    protected virtual void ChangeToBeHitState(Skill skill, IAttackable perpetrator,DamageAreaBase damageArea)
    {
        stateController.ChangeState(AllStates.BeHit, -1, perpetrator.Transform);//stateChange
    }
    #endregion

    private void CheckGround()
    {
        var rayStart = col2D.DownPoint();
        ground = Utils.Raycast(rayStart, Vector2.down, groundCheckDistance, groundLayer, Color.red);
        //没有检测到地面，或超出距离：空中
        if (!ground || ground.distance > overGroundDistance)         
        {
            DistanceToGround = ground ? ground.distance : groundCheckDistance;
            JustOnGround = false;
            JustExitGround = IsOnGround;
            IsOnGround = false;
            IsOverGround = false;
            IsOnSlope = false;
            return;
        }

        //检测到地面：低空、地面
        #region 判断是否在地面上
        if (ground.distance < 0.08f)
        {//在地面
            if (ForceOnGround && ground.distance > 0.07f)
            {
                Vector2 target = transform.position;
                target.y -= 0.06f;
                rb2D.component.MovePosition(target);
            }
            JustOnGround = !IsOnGround;
            JustExitGround = false;
            IsOnGround = true;
            IsOverGround = false;
        }
        else
        {//在低空
            if (ForceOnGround && ground.distance < 0.6f)
            {//但是需要强制落地
                ConsoleProDebug.LogToFilter("在" + stateController.currentState.state.ToString() + "状态下强制落地", "Other");
                Vector2 target = transform.position;
                target.y -= ground.distance - 0.03f;
                rb2D.component.MovePosition(target);
                rb2D.velocity = GetSpeedWithGround(rb2D.velocity.magnitude);

                JustOnGround = !IsOnGround;
                JustExitGround = false;
                IsOnGround = true;
                IsOverGround = false;
            }
            else
            {
                JustOnGround = false;
                JustExitGround = IsOnGround;
                IsOnGround = false;
                IsOverGround = true;
            }
        }
        #endregion
        DistanceToGround = ground.distance;

        #region 判断斜面
        if (steepSlopeCheckDistance == 0)
            return;//检测距离为0时不检测斜面
        rayStart.y += 0.01f; 
        RaycastHit2D left = Utils.Raycast(rayStart, Vector2.left, realSlopeCheckDistance, groundLayer, Color.green);
        RaycastHit2D right = Utils.Raycast(rayStart, Vector2.right, realSlopeCheckDistance, groundLayer, Color.green);
        //检测到的斜坡大于45度才会被认为是陡坡，检测到是陡坡Run速度会减为0
        bool leftCheck = left && left.normal.y < 0.707f;
        bool rightCheck = right && right.normal.y < 0.707f;
        if (!leftCheck && !rightCheck)
        {//左和右都没有检测到：缓坡、平地
            IsBesideSlope = 0;
            IsOnSlope = false;
        }
        else
        {
            if (leftCheck && rightCheck)//两边都有斜坡 11
                IsBesideSlope = 3;
            else if (leftCheck)//只有左边有斜坡 10
                IsBesideSlope = 2;
            else//只有右边有斜坡 01
                IsBesideSlope = 1;
            IsOnSlope = IsOnGround && ground.normal.y < 0.707f;
        }
        #endregion
    }

    #region 没必要修改的代码
    public void Enable(bool birth, CharacterData.EntityData data)
    {
        if (!Init)
        {//OnAwake
            Init = true;
            Initialize();
            InitData();
        }
        if (birth)
            RebuildData(data);
        RefreshData();
    }
    public void OnUpdate()
    {
        OnUpdate(timeline.deltaTime);
    }
    public void OnFixedUpdate()
    {
        OnFixedUpdate(timeline.fixedDeltaTime);
    }
    public void Disable(bool die)
    {
        OnRecycle(die);
    }
    #endregion
}
