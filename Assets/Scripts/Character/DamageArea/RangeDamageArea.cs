using Chronos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class RangeDamageArea : DamageAreaBase
{
    [SerializeField, Tooltip("最大存在时间")]
    private float lifeTime;
    [SerializeField, Tooltip("能否穿透敌人（是否命中敌人后不消失）")]
    private bool penetrate;
    [SerializeField, Tooltip("能否穿透地面")]
    private bool penetrateGround;
    [SerializeField]
    private Vector2 direction = new Vector2(1, 0);
    [SerializeField]
    private float speed;

    private RigidbodyTimeline2D rb2D;
    private TrailRenderer trail;
    private float lastTime;
    private Vector2 realDir;

    protected override void Awake()
    {
        base.Awake();
        rb2D = timeline.rigidbody2D;
        trail = GetComponent<TrailRenderer>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        lastTime = lifeTime;
        realDir = direction;
    }

    protected override void Update()
    {
        lastTime -= timeline.deltaTime;
        if (lastTime <= 0)
        {
            EndAttack();
            return;
        }
        base.Update();

        if(!penetrate&&JustHit)
            EndAttack();
    }
    public void SetDirection(Vector2 newDir, bool face = false)
    {
        realDir = newDir;
        if (face && !cB.IsFacingRight)
            realDir.x = -realDir.x;
    }
    protected override void Attack()
    {
        base.Attack();
        lastTime *= cB.RangeLifeTimeRate;
        col.enabled = true;
        transform.parent = null;
        transform.position = (Vector2)cB.Transform.position + new Vector2(cB.IsFacingRight ? skill.offset.x : -skill.offset.x, skill.offset.y);

        var t = realDir.normalized;
        rb2D.velocity = speed * new Vector2(cB.IsFacingRight ? t.x : -t.x, t.y);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!penetrateGround && collision.gameObject.layer == (int)NameList.Layer.Ground)
        {
            EndAttack();
            return;
        }
        if (collision.gameObject.layer == (int)NameList.Layer.DamageArea)
        {
            var otherArea = collision.GetComponent<DamageAreaBase>();
            if (otherArea.cB.Tag == cB.Tag)
                return;
            if (otherArea.strength >= strength)
            {
                EndAttack();
                return;
            }
        }

        base.OnTriggerEnter2D(collision);
    }

    public override void EndAttack()
    {
        col.enabled = false;
        StartCoroutine(End());
    }
    IEnumerator End()
    {
        rb2D.velocity = Vector2.zero;
        col.enabled = false;
        yield return new WaitForSeconds(0.1f);
        if (trail)
        {
            trail.Clear();
        }
        ObjectPool.Recycle(gameObject);
    }
}
