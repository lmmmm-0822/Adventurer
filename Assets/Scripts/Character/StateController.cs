using UnityEngine;
using System.Collections.Generic;
using System;
using QxFramework.Core;
using System.Linq;

public enum AllStates
{
    IgnoreStatesEnum = 0,
    Idle = 1,
    Run = 2,
    Jump = 3,
    Fall = 4,
    Slide = 5,
    Attack = 6,
    Accumulate = 7,
    Dodge = 8,
    BeHit = 9,
    Die = 10,
    Defend = 11,
    BeParried = 13,

    Interactive = 20,
    FightTransition = 21,
    //End,
}
public class StateController : MonoBehaviour//由CharacterBase初始化
{//遍历状态列表，查找所需状态
    [SerializeField, Tooltip("开启之后，运行中修改状态实例的值也可以影响角色相应的值，但是同时存在多个使用同一套状态的角色，表现会不正常")]
    private bool debug;
    [SerializeField]
    private List<StateBase> states = new List<StateBase>();//在Inspector面板里手动添加
    //public float[,] statesTime;
    private List<Func<AllStates, bool>> changeStateCheck;
    public StateBase currentState;
    public StateBase defaultState;
    public float currentStateTime;//当前状态持续时间


    private CharacterBase characterBase;
    private Action<AllStates, bool> enterExitAction = null;
    private Action<float> onUpdateAction = null;
    private Action<float> onFixedUpdateAction = null;

    public void RegisterEnterExitAction(Action<AllStates, bool> action)
    {
        if (enterExitAction == null)
            enterExitAction = action;
        else
            enterExitAction += action;
    }
    public void RegisterOnUpdateAction(Action<float> onUpdate)
    {
        if (onUpdate == null) return;
        if (onUpdateAction == null)
        {
            onUpdateAction = onUpdate;
        }
        else
        {
            onUpdateAction += onUpdate;
        }
    }
    public void RegisterOnFixedUpdateAction(Action<float> onFixedUpdate)
    {
        if (onFixedUpdate == null) return;
        if (onFixedUpdateAction == null)
        {
            onFixedUpdateAction = onFixedUpdate;
        }
        else
        {
            onFixedUpdateAction += onFixedUpdate;
        }
    }
    public void RegisterChangeStateCheck(AllStates register, Func<AllStates, bool> func)
    {
        for (int i = 0,cnt = states.Count; i < cnt; i++)
        {
            if (states[i].state == register)
            {
                changeStateCheck[i] = func;
                return;
            }
        }
        Debug.LogError("changeStateCheck 未包含" + register);
    }
    public void Init(CharacterBase characterBase)
    {
        if (!debug)
            CloneStates();
        int cnt = states.Count;
        changeStateCheck = new List<Func<AllStates, bool>>(cnt);
        this.characterBase = characterBase;
        for (int i = 0; i < cnt; i++)
        {
            changeStateCheck.Add((st) => true);
            states[i].SetController(this);
            states[i].SetCharacter(characterBase);
            states[i].Init();
        }
    }
    private void CloneStates()
    {
        //states = (from data in states select ScriptableObject.Instantiate(data)).ToList();
        int cnt = states.Count;
        List<StateBase> newStates = new List<StateBase>(cnt);
        for (int i = 0; i < cnt; i++)
        {
            StateBase newState = ScriptableObject.Instantiate(states[i]);
            newStates.Add(newState);
            if (states[i] == defaultState)
                defaultState = newState;
        }
        states = newStates;
    }
    public void StateInit()
    {
        currentStateTime = 0;
        currentState = defaultState;
        enterExitAction?.Invoke(currentState.state, true);
        currentState.OnEnterState(defaultState);
    }
    public virtual void OnFixedUpdate(float deltaTime)
    {
        onFixedUpdateAction?.Invoke(deltaTime);
        currentState.OnFixedUpdate(deltaTime);
    }
    public virtual void OnUpdate(float deltaTime)
    {
        currentStateTime += deltaTime;
        onUpdateAction?.Invoke(deltaTime);
        currentState.OnUpdate(deltaTime);
    }
    public bool ChangeState(AllStates state, float value = default, object args = null)
    {
        int id = 0, cnt = states.Count;
        for (; id < cnt; id++)
        {
            if (states[id].state == state)
                break;
        }
        if (id == cnt)
        {
            Debug.LogError("状态列表中不包含" + state + "状态，试图转到不存在的状态");
            StateBase lastError = currentState;
            currentState.OnExitState(defaultState);
            currentState = defaultState;
            currentState.OnEnterState(lastError);
            return true;
        }
        if (!changeStateCheck[id](currentState.state))
            return false;
        currentStateTime = 0;
        StateBase last = currentState;
        currentState.OnExitState(states[id]);

        enterExitAction?.Invoke(currentState.state, false);
        currentState = states[id];//这步是切换状态
        enterExitAction?.Invoke(state,true);

        ConsoleProDebug.LogToFilter(characterBase.name.PadRight(12) + "\t 由 " + last.state.ToString().PadRight(10) + "\t 转到 " + state.ToString(), "State");
        
        currentState.OnEnterState(last, value, args);

        return true;
    }
    public T GetState<T>(AllStates state) where T : StateBase
    {
        for (int id = 0,cnt = states.Count; id < cnt; id++)
        {
            if (states[id].state == state)
                return (T)states[id];
        }
        Debug.LogError("试图获取不存在的状态" + state);
        return null;
    }

    #region 旧版本，自动从文件夹里读取状态，以索引查找状态列表，但扩展性差
    //protected virtual void Start()
    //{
    //    states = new List<State>((int)AllStates.End);
    //    //statesTime = new float[(int)AllStates.End, 2];
    //    changeStateCheck = new List<Func<AllStates, bool>>((int)AllStates.End);
    //    string[] stateNames = Enum.GetNames(typeof(AllStates));
    //    CharacterBase characterBase = GetComponent<CharacterBase>();
    //    CharacterBase.CharacterType characterType = characterBase.characterType;
    //    for (int i = 0; i < (int)AllStates.End; i++)
    //    {
    //        states.Add(ResourceManager.Instance.Load<State>("States/" + characterType.ToString() + "/" + stateNames[i]));
    //        changeStateCheck.Add((st) => true);
    //        //statesTime[i, 0] = -1;
    //        //statesTime[i, 1] = -1;
    //    }
    //    for (int i = 0; i < (int)AllStates.End; i++)
    //    {
    //        if (states[i] == null) continue;
    //        states[i].SetController(this);
    //        states[i].SetCharacter(characterBase, characterType);
    //        states[i].Init();
    //    }
    //    currentState = defaultState;
    //    currentState.OnEnterState(null);
    //    //statesTime[(int)defaultState.state, 0] = 0;
    //}
    //public bool ChangeState(AllStates state, object args = null)
    //{
    //    #region 之前的状态判断
    //    //switch (currentState.state)
    //    //{
    //    //    case AllStates.Dodge://如果结束的是闪避状态
    //    //        Dodge temp = (Dodge)currentState;
    //    //        //if (temp.lastDodgeTimes == 0)
    //    //        lastDodgeIntervalTime = temp.intervalTime;
    //    //        break;
    //    //}
    //    //switch (state)
    //    //{
    //    //    case AllStates.Dodge://如果要进入闪避状态
    //    //        Dodge temp = (Dodge)states[(int)AllStates.Dodge];
    //    //        if (lastDodgeIntervalTime <= 0)
    //    //            temp.RefreshLastDodgeTimes();
    //    //        else if (temp.lastDodgeTimes == 0)
    //    //            return false;
    //    //        break;
    //    //}
    //    #endregion
    //    if (!changeStateCheck[(int)state](currentState.state))
    //    {
    //        return false;
    //    }

    //    State last = currentState;
    //    currentState.OnExitState();
    //    //statesTime[(int)currentState.state, 1] = 0;//上个状态的结束时间设为0
    //    currentState = states[(int)state];
    //    currentState.OnEnterState(last, args);
    //    //statesTime[(int)currentState.state, 0] = 0;//下个状态的开始时间设为0
    //    return true;
    //}

    //public T GetState<T>(AllStates state) where T : State
    //{
    //    return (T)states[(int)state];
    //}
    #endregion
}
