using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 存储所有游戏相关可存档数据
/// 因为集中数据所以方便做Debug和调试功能
/// 并且也方便做数据更新时通知
/// </summary>
[Serializable]
public class GameDataContainer
{
    /// <summary>
    /// key和类型名字字典
    /// </summary>
    [SerializeField]
    private readonly Dictionary<string, Dictionary<Type, GameDataBase>> _objDics
        = new Dictionary<string, Dictionary<Type, GameDataBase>>();

    /// <summary>
    /// 初始化游戏数据，应该在加载过json文件后调用
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="data">引用的数据</param>
    /// <param name="key">名称</param>
    /// <returns>是否已经成功赋值</returns>
    public bool InitData<T>(out T data, string key) where T : GameDataBase, new()
    {
        var saved = Get<T>(key);
        if (saved != null)
        {//从json文件加载之后，存在相应的数据
            data = saved;
            return true;
        }
        else
        {
            data = new T();//初始化数据
            Add<T>(data, key);//添加到字典里
            return false;
        }
    }
    /// <summary>
    /// 取得游戏数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Get<T>(string key) where T : GameDataBase, new()
    {
        try
        {
            return (T)_objDics[key][typeof(T)];
        }
        catch
        {
            return null;
        }
        //return (T)GetOrInitSaved<T>(key);
    }
    /// <summary>
    /// 判断以某个字符串为key的存档是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contain(string key)
    {
        return _objDics.ContainsKey(key);
    }
    /// <summary>
    /// 添加单个存档的单个数据，不会自动写入到json文件中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="key"></param>
    public void Add<T>(T data, string key) where T : GameDataBase
    {
        if (!_objDics.TryGetValue(key, out var dic))
        {
            dic = new Dictionary<Type, GameDataBase>();
            _objDics[key] = dic;
        }
        dic[data.GetType()] = data;
    }
    /// <summary>
    /// 删除存档，不会记录到json文件中，需要手动调用ToSaveJson
    /// </summary>
    /// <param name="key"></param>
    public void Delete(string key)
    {
        _objDics.Remove(key);
    }
    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        _objDics.Clear();
    }

    [Serializable]
    public class GameDataPair
    {
        [SerializeField]
        public string Key;

        [SerializeField]
        public List<GameDataBase> Value;

        public GameDataPair(string key, List<GameDataBase> value)
        {
            Key = key;
            Value = value;
        }
    }

    public string ToSaveJson()
    {
        List<GameDataPair> list = new List<GameDataPair>();
        foreach (var pair in _objDics)
        {
            var kyPair = new GameDataPair(pair.Key, new List<GameDataBase>());

            list.Add(kyPair);
            foreach (var savedGame in pair.Value)
            {
                {
                    kyPair.Value.Add(savedGame.Value);
                }
            }
        }

        var json = JsonUtil.Serialize(list);
        return json;
    }
    public void FromSaveJson(string json)
    {
        var list  = JsonUtil.Deserialize<List<GameDataPair>>(json);
        _objDics.Clear();
        foreach (var item in list)
        {
            Dictionary<Type, GameDataBase> dic;
            if (!_objDics.TryGetValue(item.Key, out dic))
            {
                dic = new Dictionary<Type, GameDataBase>();
                _objDics.Add(item.Key,dic);
            }
            foreach (var data in item.Value)
            {
                dic[data.GetType()] = data;
                Debug.Log(data.GetType());
            }
        }
    }

    public Dictionary<string, Dictionary<Type, GameDataBase>> GetAll()
    {
        return _objDics;
    }
}

[XLua.LuaCallCSharp]
[Serializable]
public class GameDataBase
{
}

//internal class SerializedGameData
//{
//    public string Key;
//    public string TypeName;
//    public string SerializedData;
//}