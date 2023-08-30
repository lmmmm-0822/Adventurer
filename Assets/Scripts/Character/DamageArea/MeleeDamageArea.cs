using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 只处理近战攻击
/// </summary>
public class MeleeDamageArea : DamageAreaBase
{
    private CircleCollider2D cirCol2D;
    private BoxCollider2D boxCol2D;

    protected override void Awake()
    {
        base.Awake();
        cirCol2D = GetComponent<CircleCollider2D>();
        boxCol2D = GetComponent<BoxCollider2D>();
    }
    protected override void Attack()
    {
        base.Attack();
        if (skill.IsCircle)
        {
            col = cirCol2D;
            cirCol2D.radius = skill.radius;
            cirCol2D.enabled = true;
            boxCol2D.enabled = false;
        }
        else
        {
            col = boxCol2D;
            boxCol2D.size = skill.size;
            boxCol2D.enabled = true;
            cirCol2D.enabled = false;
        }
    }
}