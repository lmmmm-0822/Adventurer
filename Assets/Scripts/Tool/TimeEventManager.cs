using QxFramework.Core;
using QxFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Chronos;

/// <summary>
/// 基于unity的时间，不受Timekeeper影响
/// </summary>
public class TimeEventManager : QxFramework.Utilities.Singleton<TimeEventManager>, ISystemModule
{
    public enum EventKey
    {
        ScreenShake = 0,
        GlobalTime = 1,
        CameraMoveAreaChange = 2,
        None,
    }
    private class TimeAction
    {
        public TimeAction(Action action, float afterTime)
        {
            this.action = action;
            this.afterTime = afterTime;
        }
        public TimeAction(int sign, Action action, float afterTime)
        {
            this.sign = sign;
            this.action = action;
            this.afterTime = afterTime;
        }
        public int sign;
        public Action action;
        public float afterTime;
    }
    private List<TimeAction> delayActions;
    private List<TimeAction> delayFlameActions;
    private List<TimeAction> updateActions;
    //private Action<float> onFixedUpdateAction = null;
    public override void Initialize()
    {
        delayActions = new List<TimeAction>((int)EventKey.None + 2);
        delayFlameActions = new List<TimeAction>();
        updateActions = new List<TimeAction>();
        for (int i = 0; i < (int)EventKey.None; i++)
        {
            delayActions.Add(new TimeAction(null, 0));
        }
    }
    public void ChangeTimeScale(Clock clock, float scale, float keepTime, int sign = 0)
    {
        if (keepTime == 0)
            return;
        //Debug.Break();
        if (scale == 0)
        {
            clock.paused = true;
            RegisterTimeAction(keepTime, () => { clock.paused = false; }, sign: sign);
        }
        else
        {
            clock.localTimeScale *= scale;
            RegisterTimeAction(keepTime, () => { clock.localTimeScale /= scale; }, sign: sign);
        }
    }

    /// <summary>
    /// 几帧之后执行delay
    /// </summary>
    /// <param name="afterFlame"></param>
    /// <param name="delay"></param>
    /// <param name="immediacy"></param>
    /// <param name="eventKey"></param>
    public void RegisterTimeAction(int afterFlame, Action delay, Action immediacy = null, int sign = 0)
    {
        if (immediacy != null)
            immediacy();
        for (int i = 0,cnt = delayFlameActions.Count; i < cnt; i++)
        {
            if (delayFlameActions[i].action == null)
            {
                delayFlameActions[i].sign = sign;
                delayFlameActions[i].action = delay;
                delayFlameActions[i].afterTime = afterFlame;
                return;
            }
        }
        delayFlameActions.Add(new TimeAction(sign, delay, afterFlame));
    }
    public void RegisterTimeAction(float afterTime, Action delay,Action immediacy =null, EventKey eventKey = EventKey.None,int sign=0)
    {
        if (eventKey == EventKey.None)
        {
            if (immediacy != null)
                immediacy();
            for (int i = (int)EventKey.None,cnt = delayActions.Count; i < cnt; i++)
            {
                if (delayActions[i].action == null)
                {
                    delayActions[i].sign = sign;
                    delayActions[i].action = delay;
                    delayActions[i].afterTime = afterTime;
                    return;
                }
            }
            delayActions.Add(new TimeAction(sign,delay, afterTime));
        }
        else
        {
            if (delayActions[(int)eventKey].afterTime <= afterTime)
            {
                if (delayActions[(int)eventKey].action != null)
                {
                    Debug.LogWarning(eventKey + "提前执行了委托");
                    delayActions[(int)eventKey].action();
                    if (immediacy == null)
                        Debug.LogWarning("但" + eventKey + "没有执行immediacy的函数，可能上一委托的函数晚于本次立刻执行的函数");
                }
                if (immediacy != null)
                    immediacy();
                delayActions[(int)eventKey].action = delay;
                delayActions[(int)eventKey].afterTime = afterTime;
            }
            else
            {
                Debug.LogWarning("注册时间事件失败，因为" + eventKey + "已经注册了时间更长的事件");
            }
        }
    }
    ////public void RegisterOnUpdateAction(Action<float> onUpdate)
    ////{
    ////    if (onUpdate == null) return;
    ////    if (onUpdateAction == null)
    ////    {
    ////        onUpdateAction = onUpdate;
    ////    }
    ////    else
    ////    {
    ////        onUpdateAction += onUpdate;
    ////    }
    ////}
    ////public void RegisterOnFixedUpdateAction(Action<float> onFixedUpdate)
    ////{
    ////    if (onFixedUpdate == null) return;
    ////    if (onFixedUpdateAction == null)
    ////    {
    ////        onFixedUpdateAction = onFixedUpdate;
    ////    }
    ////    else
    ////    {
    ////        onFixedUpdateAction += onFixedUpdate;
    ////    }
    ////}
    public void RegisterUpdateAction(Action action, float keepTime,int sign = 0)
    {
        for (int i = 0, cnt = updateActions.Count; i < cnt; i++)
        {
            if (updateActions[i].action == null)
            {
                updateActions[i].sign = sign;
                updateActions[i].action = action;
                updateActions[i].afterTime = keepTime;
                return;
            }
        }
        updateActions.Add(new TimeAction(sign, action, keepTime));
    }
    public void UnRegistTimeAction(int sign)
    {
        if (sign == 0)
        {
            Debug.LogError("取消注册的委托时，标志不能是0");
            return;
        }
        for(int i = 0, cnt = delayFlameActions.Count; i < cnt; i++)
        {
            if(delayFlameActions[i].sign == sign)
            {
                delayFlameActions[i].action = null;
                return;
            }
        }
        for (int i = (int)EventKey.None, cnt = delayActions.Count; i < cnt; i++)
        {
            if (delayActions[i].sign == sign)
            {
                delayActions[i].action = null;
                return;
            }
        }
        for (int i = 0, cnt = updateActions.Count; i < cnt; i++)
        {
            if (updateActions[i].sign == sign)
            {
                updateActions[i].action = null;
                return;
            }
        }
    }
    public void Update(float deltaTime)
    {
        for (int i = 0,cnt = delayActions.Count; i < cnt; i++)
        {
            if (delayActions[i].action == null) continue;

            delayActions[i].afterTime -= deltaTime;
            if (delayActions[i].afterTime <= 0)
            {
                delayActions[i].action();
                delayActions[i].action = null;
            }
        }
        for (int i = 0, cnt = delayFlameActions.Count; i < cnt; i++)
        {
            if (delayFlameActions[i].action == null) continue;

            delayFlameActions[i].afterTime -= 1;
            if (delayFlameActions[i].afterTime <= 0)
            {
                delayFlameActions[i].action();
                delayFlameActions[i].action = null;
            }
        }
        for (int i = 0, cnt = updateActions.Count; i < cnt; i++)
        {
            if (updateActions[i].action == null) continue;

            updateActions[i].action();
            updateActions[i].afterTime -= deltaTime;
            if (updateActions[i].afterTime <= 0)
                updateActions[i].action = null;
        }
    }
    public void FixedUpdate(float deltaTime)
    {

    }
    public void Dispose()
    {

    }

}
