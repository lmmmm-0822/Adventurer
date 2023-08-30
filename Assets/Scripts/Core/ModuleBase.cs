using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicModuleBase
{
    public LogicModuleBase()
    {
        Debug.Log("创建" + GetType().Name);
    }
    public virtual void Init()
    {
    }
    protected bool InitData<T>(out T data,string key = default)where T: GameDataBase,new()
    {
        if (key == default)
            key = Data.Instance.currentSaveKey;
        return Data.Instance.InitData<T>(out data, key);
    }
    protected void AddData<T>(T data,string key = default)where T:GameDataBase
    {
        if (key == default)
            key = Data.Instance.currentSaveKey;
        Data.Instance.AddData<T>(data, key);
    }

    public virtual void Awake()
    {
    }

    public virtual void Update()
    {
    }
    public virtual void LateUpdate()
    {
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void OnDestroy()
    {
    }
    ~LogicModuleBase()
    {
        Debug.Log(GetType().Name + "被GC");
    }
}