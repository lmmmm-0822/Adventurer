using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using QxFramework.Core;
using UnityEngine;
using Chronos;

[System.Serializable]
public class GameTimeManager : LogicModuleBase, IGameTimeManager
{

    private class TimeActionItem
    {
        public Func<GameDateTime, bool> Action;
        public int IntervalTime;
        public string key;
    }

    private GameTimeData data;
    private readonly List<TimeActionItem> _repeatsActions = new List<TimeActionItem>();

    public GlobalClock TimeClock => timeClock;
    private GlobalClock timeClock;
    public bool IsStop => !_playing;
    private float _time = 0f;//用于累计和判断需要更新的时间
    private bool _playing = false;
    private readonly List<Func<bool>> _pauseConditions = new List<Func<bool>>();
    /// <summary>
    /// 停止步进时间
    /// </summary>
    private bool _stopStep;

    private int targetMinutes;
    private int haveSteppedMinutes;
    private Action<int> stepMinutesAction;//float为0表示开始，为-1代表结束，为-99代表被打断

    private int currentOrder;//将change分摊到几帧执行
    private int updateCount;

    public override void Init()
    {
        base.Init();
        timeClock = Timekeeper.instance.Clock("Time");
        if (!InitData(out data))
        {
            data.Now.Days = 0;
            data.Now.Hours = 15;
            data.Now.Minutes = 40;
        }
        DoStart();

    }
    public override void OnDestroy()
    {
        MessageManager.Instance.RemoveAbout(this);
    }

    /// <summary>
    /// 经过time秒，步进minutes分钟
    /// </summary>
    /// <param name="minutes"></param>
    /// <param name="time"></param>
    public void StepMinute(int minutes, float time, Action<int> action = null)
    {
        _playing = true;
        timeClock.localTimeScale = minutes / Math.Max(0.2f, time);
        haveSteppedMinutes = 0;//表示开始
        targetMinutes = minutes;
        stepMinutesAction = action;
    }
    /// <summary>
    ///步进分钟数
    /// </summary>
    /// <param name="stepTime">步进分钟</param>
    /// <returns>返回是否成功，未被打断</returns>
    private bool StepMinute(float stepTime)//, bool inside = false)
    {
        _time += stepTime;
        currentOrder++;

        while (_time >= 1f )//&& !_stopStep)
        {
            //if (currentOrder != 0 && updateCount != changes.Count)
            //    foreach (var change in changes)
            //        if (change.UpdateOrder >= currentOrder)
            //            change.Set(GetNow());
            currentOrder = 0;
            updateCount = 0;
            _time -= 1;
            //游戏时间增加
            data.Now += 1;

            foreach (var item in _repeatsActions)
            {
                if (data.Now.TotalMinutes % item.IntervalTime == 0)
                {
                    _stopStep = (!item.Action(data.Now)) || _stopStep;
                }

                //是否被打断
                if (_stopStep)
                {
                    Debug.Log("[GameTimer] 打断于" + _time);
                    //break;
                }
            }

            if (targetMinutes != 0)
            {
                if (haveSteppedMinutes == 0)
                    stepMinutesAction?.Invoke(haveSteppedMinutes);//开始时直接执行一次
                if (_stopStep)
                {
                    timeClock.localTimeScale = 1;
                    targetMinutes = 0;
                    haveSteppedMinutes = -99;//被打断设为-99
                }
                else
                {
                    haveSteppedMinutes++;
                    if (haveSteppedMinutes >= targetMinutes)
                    {
                        timeClock.localTimeScale = 1;
                        targetMinutes = 0;
                        haveSteppedMinutes = -1;//结束设为-1
                    }
                }
                stepMinutesAction?.Invoke(haveSteppedMinutes);
                if (haveSteppedMinutes == 0)
                    stepMinutesAction = null;
            }

        }

        //复位标识
        _stopStep = false;

        //if (!inside)
        //{//如果是外部跳过时间
        //    MessageManager.Instance.Get<TimeMsg>().DispatchMessage(TimeMsg.Change, this);
        //}

        return true;//!_stopStep;
    }


    public void StopStep()
    {
        _stopStep = true;
        //_playing = false;
    }

    public override void FixedUpdate()
    {
        _pauseConditions.RemoveAll((f) => !f());
        //判断一遍所有条件，去掉未能达成的
        if (_pauseConditions.Count > 0)
        {
            return;
        }

        //如果单纯的被停了
        if (_playing)
        {
            StepMinute(timeClock.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 注册时间循环函数
    /// </summary>
    /// <param name="action"></param>
    public void RegisterTimeRepeat(Func<GameDateTime, bool> action, GameDateTime interval,string key = null)
    {
        ConsoleProDebug.LogToFilter($"注册时间刷新函数{interval.ToDurationString()} " + (key == null ? null : $"，key为{key}"), "Other");
        _repeatsActions.Add(new TimeActionItem()
        {
            Action = action,
            IntervalTime = interval.TotalMinutes,
            key = key,
        });
    }
    /// <summary>
    /// 添加时间临时暂停条件，主要是为了防止所有弹窗都写一遍
    /// </summary>
    /// <param name="condition">条件函数</param>
    public void AddTempPauseCondition(Func<bool> condition)
    {
        _pauseConditions.Add(condition);
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        _playing = false; 
        MessageManager.Instance.Get<TimeMsg>().DispatchMessage(TimeMsg.Pause, this);
    }

    /// <summary>
    /// 开始
    /// </summary>
    public void DoStart()
    {
        _playing = true;
        MessageManager.Instance.Get<TimeMsg>().DispatchMessage(TimeMsg.Start, this);
    }

    /// <summary>
    /// 获取当前时间
    /// </summary>
    /// <returns>返回当前时间</returns>
    public GameDateTime GetNow()
    {
        return data.Now;
    }

    public enum TimeMsg
    {
        Start,
        Pause,
        //Change,
    }
    //public override void OnDestroy()
    //{
    //    for (int i = 0, cnt = _afterTimeActions.Count; i < cnt; i++)
    //    {
    //        if (_afterTimeActions[i].AfterTime > 0)
    //        {
    //            _afterTimeActions[i].Action();
    //            _afterTimeActions[i].AfterTime = -1;
    //        }
    //    }
    //}
}

[Serializable]
public class GameTimeData : GameDataBase
{
    public GameDateTime Now;
}

/// <summary>
/// 封装好的游戏时刻类
/// </summary>
[Serializable]
[XLua.LuaCallCSharp]
public struct GameDateTime
{
    [SerializeField]
    public int Days;

    [SerializeField]
    public int Hours;

    [SerializeField]
    public int Minutes;

    public const int MinutesPerHour = 60;

    public const int HoursPerDay = 24;

    /// <summary>
    /// 获取总时间
    /// </summary>
    public int TotalMinutes => (Days * HoursPerDay * MinutesPerHour) + Hours * MinutesPerHour + Minutes;
    public int TodayMinutes => Hours * MinutesPerHour + Minutes;

    public GameDateTime(int days, int hours, int minutes)
    {
        //这么从秒数重新计算是为了防止传入会溢出的值
        var totalMinutes = (days * HoursPerDay * MinutesPerHour) + hours * MinutesPerHour + minutes;
        Minutes = totalMinutes % MinutesPerHour;
        Hours = (totalMinutes - Minutes) / MinutesPerHour % HoursPerDay;
        Days = (totalMinutes - Minutes - Hours * MinutesPerHour) / MinutesPerHour / HoursPerDay;
    }

    public override string ToString()
    {
        return $"{Days}:{Hours}:{Minutes}";
    }

    /// <summary>
    /// 获取时长字符串，花费几天几小时几分钟
    /// </summary>
    /// <returns></returns>
    public string ToDurationString()
    {
        return (Days > 0 ? $"{Days}天" : "")
              + (Hours > 0 ? $"{Hours}时" : "")
                  + (Minutes > 0 ? $"{Minutes}分" : "");
    }

    /// <summary>
    /// 转化为时刻字符串  第几天几时几分
    /// </summary>
    /// <returns></returns>
    public string ToMomentString()
    {
        return $"{Days}天{Hours:00}:{Minutes:00}";
    }

    /// <summary>
    /// 使用分钟创建时间
    /// </summary>
    /// <param name="totalMinutes"></param>
    /// <returns></returns>
    public static GameDateTime ByMinutes(int totalMinutes)
    {
        var mins = totalMinutes % MinutesPerHour;
        var hours = (totalMinutes - mins) / MinutesPerHour % HoursPerDay;
        var days = (totalMinutes - mins - hours * MinutesPerHour) / MinutesPerHour / HoursPerDay;
        return new GameDateTime(days, hours, mins);
    }

    public static GameDateTime ByHours(float totalHours)
    {
        var hours = (int)totalHours % HoursPerDay;
        var days = ((int)totalHours - hours) / HoursPerDay;
        var minutes = (int)(totalHours % 1 * MinutesPerHour);
        return new GameDateTime(days, hours, minutes);
    }

    /// <summary>
    /// 当前是白天
    /// </summary>
    /// <returns></returns>
    public bool IsDayTime()
    {
        return Hours > 5 && Hours < 18;
    }

    #region 运算符

    public static GameDateTime operator +(GameDateTime t1, GameDateTime t2)
    {
        return ByMinutes(t1.TotalMinutes + t2.TotalMinutes);
    }

    public static GameDateTime operator -(GameDateTime t1, GameDateTime t2)
    {
        return ByMinutes(t1.TotalMinutes - t2.TotalMinutes);
    }

    public static GameDateTime operator +(GameDateTime t1, int t2)
    {
        return ByMinutes(t1.TotalMinutes + t2);
    }

    public static GameDateTime operator -(GameDateTime t1, int t2)
    {
        return ByMinutes(t1.TotalMinutes - t2);
    }

    public static bool operator ==(GameDateTime t1, GameDateTime t2)
    {
        return t1.Minutes == t2.Minutes && t1.Hours == t2.Hours && t1.Days == t2.Days;
    }

    public static bool operator !=(GameDateTime t1, GameDateTime t2)
    {
        return !(t1 == t2);
    }

    public static bool operator >=(GameDateTime t1, GameDateTime t2)
    {
        return t1.TotalMinutes >= t2.TotalMinutes;
    }

    public static bool operator <=(GameDateTime t1, GameDateTime t2)
    {
        return t1.TotalMinutes <= t2.TotalMinutes;
    }

    public static bool operator >(GameDateTime t1, GameDateTime t2)
    {
        return t1.TotalMinutes > t2.TotalMinutes;
    }

    public static bool operator <(GameDateTime t1, GameDateTime t2)
    {
        return t1.TotalMinutes < t2.TotalMinutes;
    }

    public bool Equals(GameDateTime other)
    {
        return Days == other.Days && Hours == other.Hours && Minutes == other.Minutes;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is GameDateTime && Equals((GameDateTime)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Days;
            hashCode = (hashCode * 397) ^ Hours;
            hashCode = (hashCode * 397) ^ Minutes;
            return hashCode;
        }
    }

    #endregion 运算符

}
