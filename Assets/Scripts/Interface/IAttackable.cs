using Chronos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public float RangeLifeTimeRate { get; }
    public bool AddRangeBulletCnt { get; }
    public int Attack { get; }
    public string Tag { get; }
    public Transform Transform { get; }
    public Timeline Timeline { get; }
    public LocalClock LocalClock { get; }
    public Transform Target { get; }
    public bool IsFacingRight { get; }
    public void BePerfectDefended();
    public void BeDefended();
}
