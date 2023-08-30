using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NameList;
using QxFramework.Core;

public enum EnemyType
{
    None = 0,
    Enemy1 = 1,
    Enemy2 = 2,
    Enemy3 = 3,
}
public class Enemy : CharacterBase
{
    private float searchAngle;
    public float searchAngleTan;//一个正数
    public bool searchDown;

    public static LayerMask targetLayer = Utils.GetMask(Layer.Player, Layer.PlayerIgnoreEnemy);
    public EnemyType enemyType;
    public float detectDistance = 6;
    public float loseTargetDistance = 9;
    [HideInInspector]
    public List<(int itemId, float probability)> drop;
    //public Transform Target;
    //public static LayerMask TargetLayer { get => LayerMask.GetMask("Player","PlayerIgnoreEnemy"); }
    //protected override void OnAwake()
    //{
    //    characterType = (CharacterType)System.Enum.Parse(typeof(CharacterType), this.name.Split(new char[] { '(' }, 2)[0]);
    //    base.OnAwake();
    //}
    protected override void InitData()
    {
        base.InitData();
        transform.GetChild(0).GetComponent<AnimeEventEnemy>().Init(this);
        string[] temp = Data.Instance.TableAgent.GetStrings("Enemy", enemyType.ToString(), "Drop");
        if (temp[0] != "")
        {
            drop = new List<(int, float)>(temp.Length);
            string[] feature;
            for (int j = 0, len = temp.Length; j < len; j++)
            {
                try
                {
                    feature = temp[j].Split(':');
                    drop.Add((int.Parse(feature[0]), float.Parse(feature[1])));
                }
                catch
                {
                    Debug.LogError("从Enemy读取掉落出错");
                }
            }
        }
    }
    protected override void RebuildData(CharacterData.EntityData data)
    {
        base.RebuildData(data);
    }
    protected override void RefreshData()
    {
        gameObject.layer = Utils.NameToLayer(Layer.Enemy);
        Target = null;
        base.RefreshData();
    }
    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        HandleCollisionLayer();
        stateController.OnUpdate(deltaTime);
        effectCtr.OnUpdate();
        HandleTarget();
    }
    protected virtual void HandleCollisionLayer()
    {
        //使怪物在空中不会相互碰撞（EnemyIgnoreOthers不会与自身碰撞）
        //使怪物不会落到其他角色头上（EnemyIgnoreOthers不会与Enemy、Player碰撞）
        if (stateController.currentState.state != AllStates.Die)
        {
            if (JustExitGround)
                gameObject.layer = Utils.NameToLayer(Layer.EnemyIgnoreOthers);
            else if (JustOnGround)
                gameObject.layer = Utils.NameToLayer(Layer.Enemy);
        }
    }
    protected virtual void HandleTarget()
    {
        Vector2 searchRight = new Vector2(1, searchAngle);
        Vector2 searchLeft = new Vector2(-1, searchAngle);
        if (searchAngle > searchAngleTan && !searchDown)
            searchAngle = 0;
        else
        {
            if (searchAngle > 0 && searchDown)
                searchAngle = -searchAngleTan;
            else
                searchAngle += 0.02f;
        }

       
        if (Target == null)
        {
            RaycastHit2D horizontal;
            if (IsFacingRight)
                horizontal = Utils.Raycast(col2D.bounds.min + new Vector3(-0.5f, col2D.bounds.extents.y, 0), searchRight, detectDistance, Enemy.targetLayer, Color.white);
            else
                horizontal = Utils.Raycast(col2D.bounds.max - new Vector3(-0.5f, col2D.bounds.extents.y, 0), searchLeft, detectDistance, Enemy.targetLayer, Color.white);
            if (horizontal)
                Target = horizontal.transform;
        }
        else
        {
            if (((Vector2)Target.position - (Vector2)transform.position).sqrMagnitude > loseTargetDistance * loseTargetDistance)
            {
                Target = null;
            }
        }
    }

    //public override Transform Damage(int damage, int poiseDamage, Transform beHitArea, Skill skill, CharacterBase perpetrator = null)
    public override Transform Damage(DamageAreaBase.DamageArgs args)
    {
        Target = args.perpetrator.Transform;
        return base.Damage(args);
    }

    public virtual void Stopping(float reduceSpeed)
    {
        reduceSpeed *= timeline.timeScale;
        Vector2 velocity = rb2D.velocity;
        if (Mathf.Approximately(velocity.x, 0))
            return;//发生碰撞时unity会施加一个很小的反向速度，如果把这个速度强制改为0，就会卡在碰撞到的collider里
        if (IsOnGround)
        {
            if (reduceSpeed > velocity.x / velocity.normalized.x
             || velocity.normalized.x == 0)
            {
                rb2D.velocity = Vector2.zero;
                //transform.position -= new Vector3(AdaptFacing(0.02f), 0, 0);//因为怪物受重力影响，所以在斜坡上会一直调用这条语句//紧贴着发生碰撞会推不动另一个collider，需要有一个缓冲距离
            }
            else
            {
                rb2D.velocity -= reduceSpeed * velocity.normalized;
            }
        }
    }

    public enum EnemyMsg
    {
        Die,
    }
}
