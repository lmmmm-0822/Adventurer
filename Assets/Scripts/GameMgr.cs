using Chronos;
using QxFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏管理器，用于管理之前由MonoSingleton所有逻辑
/// </summary>
public class GameMgr : MonoSingleton<GameMgr>
{
    /// <summary>
    /// 所有模块列表
    /// </summary>
    private readonly List<LogicModuleBase> modules = new List<LogicModuleBase>(6);
    private readonly List<LogicModuleBase> sleepModules = new List<LogicModuleBase>(6);

    //方便Debug
    [SerializeField] private GameTimeManager timeManager;
    [SerializeField] private GameSceneManager sceneManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private EventManager eventManager;

    private bool pause;
    /// <summary>
    /// 仅对游戏内容管理器生效，不对UI、玩家输入、流程等生效
    /// </summary>
    public bool Pause
    {
        get => pause;
        set
        {
            if (pause != value)
            {
                pause = value;
                if (pause)
                {
                    Sleep();
                    Timekeeper.instance.Clock("Root").paused = true;
                }
                else
                {
                    Timekeeper.instance.Clock("Root").paused = false;
                    WeakUp();
                }
            }
        }
    }
    public static IGameTimeManager TimeMgr { get; private set; }
    public static IGameSceneManager SceneMgr { get; private set; }
    public static ICharacterManager CharacterMgr { get; private set; }
    public static IEventManager EventMgr { get; private set; }

    /// <summary>
    /// 初始化所有模块
    /// </summary>
    public void InitModules()
    {
        modules.Clear();

        timeManager = new GameTimeManager();
        sceneManager = new GameSceneManager();
        characterManager = new CharacterManager();

        TimeMgr = Add<IGameTimeManager>(timeManager);
        SceneMgr = Add<IGameSceneManager>(sceneManager);
        CharacterMgr = Add<ICharacterManager>(characterManager);

        foreach(var module in modules)
        {
            module.Awake();
        }
    }
    private T Add<T>(LogicModuleBase module)
    {
        modules.Add(module);
        module.Init();
        return (T)(object)module;
    }
    private void Sleep()
    {
        foreach(var module in modules)
        {
            sleepModules.Add(module);
        }
        modules.Clear();
    }
    private void WeakUp()
    {
        foreach (var module in sleepModules)
        {
            modules.Add(module);
        }
        sleepModules.Clear();
    }

    private void Update()
    {
        foreach( var module in modules)
        {
            module.Update();
        }
    }

    private void FixedUpdate()
    {
        foreach( var module in modules)
        {
            module.FixedUpdate();
        }
    }

    private void OnDestroy()
    {
        foreach (var module in modules)
        {
            module.OnDestroy();
        }
        modules.Clear();

        //释放内存
        TimeMgr = null;
        SceneMgr = null;
        CharacterMgr = null;

        timeManager = null;
        sceneManager = null;
        characterManager = null;
    }
}
