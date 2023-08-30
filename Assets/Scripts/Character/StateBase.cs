using UnityEngine;
using System;
using System.Diagnostics;
using System.Reflection;
using QxFramework.Core;
using System.Collections.Generic;

public abstract class StateBase : ScriptableObject
{
    public AllStates state;
    protected StateController controller;
    protected StateBase()
    {
        try
        {
            state = (AllStates)Enum.Parse(typeof(AllStates), this.GetType().Name.Split('_')[0]);
        }
        catch
        {
            UnityEngine.Debug.LogWarning("无法从这个类“" + this.GetType().Name + "”的名字，对应到AllStates枚举，需要修改该类的命名，或者在AllState的枚举中添加该类");
        }
    }
    public void SetController(StateController stateController)
    {
        controller = stateController;
    }
    public virtual void SetCharacter(CharacterBase characterBase)
    {

    }

    public virtual void Init()
    {

    }

    public virtual void OnUpdate(float deltaTime)
    {

    }
    public virtual void OnLateUpdate(float deltaTime)
    {

    }

    public virtual void OnFixedUpdate(float deltaTime)
    {

    }

    public virtual void OnEnterState(StateBase lastState, float value = default, object args = null)
    {

    }

    public virtual void OnExitState(StateBase nextState)
    {

    }

    public virtual void PlayAnimation(string name,float normalizeTime)
    {

    }
    /// <summary>
    /// 需要设置动画事件来调用
    /// </summary>
    /// <param name="name"></param>
    public virtual void AnimationEnd(string name)
    {

    }
}

