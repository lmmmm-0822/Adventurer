using QxFramework.Core;
using QxFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.Reflection;
using System.IO;

namespace QxFramework.Core
{
    public class Data : MonoSingleton<Data>
    {
        //public float TimeSize = 1;
        public string currentSaveKey;

        #region 游戏数据存取接口

        private readonly GameDataContainer _gameDataContainer = new GameDataContainer();

        /// <summary>
        /// 取得游戏数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetData<T>(string key) where T : GameDataBase, new()
        {
            return _gameDataContainer.Get<T>(key);
        }
        public void AddData<T>(T data, string key)where T : GameDataBase
        {
            _gameDataContainer.Add<T>(data, key);
        }
        public bool DeleteSave(string FileName,string key)
        {
            try
            {
                _gameDataContainer.Delete(key);
                SaveToFile(FileName);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
        /// <summary>
        /// 初始化游戏数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool InitData<T>(out T data, string key = null) where T : GameDataBase, new()
        {
            if (key == null)
                key = currentSaveKey;
            return _gameDataContainer.InitData<T>(out data, key);
        }
        public void ClearData()
        {
            _gameDataContainer.Clear();
        }

        ///// <summary>
        ///// 设置被修改
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="action"></param>
        ///// <param name="key"></param>
        //public void SetModify<T>(T t, object modifier, string key = "Default")
        //    where T : GameDataBase, new()
        //{
        //    _gameDataContainer.SetModify<T>(t, modifier, key);
        //}

        ///// <summary>
        ///// 注册更新监听
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="action"></param>
        ///// <param name="key"></param>
        //public void RegisterUpdateListener<T>(Action<GameDataBase> action, string key = "Default")
        //    where T : GameDataBase, new()
        //{
        //    _gameDataContainer.RegisterUpdateListener<T>(action, key);
        //}

        ///// <summary>
        ///// 消除有关某个对象的所有监听
        ///// </summary>
        ///// <typeparam name="T">类型</typeparam>
        ///// <param name="obj">添加过监听的对象</param>
        //public void RemoveAbout(object obj)
        //{
        //    _gameDataContainer.RemoveAbout(obj);
        //}

        /// <summary>
        /// 获取所有注册的数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<Type, GameDataBase>> GetAllData()
        {
            return _gameDataContainer.GetAll();
        }

        public string GetSavePath()
        {
            var path =
#if UNITY_EDITOR
               Application.dataPath.Replace("Assets", "Cache/Save");
#else
               Application.dataPath + "/Save";
#endif
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public void SaveToFile(string FileName)
        {
            if (FileName == "")
            {
                return;
            }
            MessageManager.Instance.Get<DataMsg>().DispatchMessage(DataMsg.WillSave, this);
            var json = _gameDataContainer.ToSaveJson();
            File.WriteAllText(GetSavePath() + "/" + FileName, json);
        }
        public void TryCreateNewSave()
        {//应在LoadFromFile之后再调用
            if (_gameDataContainer.Contain(currentSaveKey))
                return;
            try
            {//如果还没有以currentSaveKey为键的存档，则生成初始存档
                var json = ResourceManager.Instance.Load<TextAsset>("Text/InitSave").ToString();
                var list = JsonUtil.Deserialize<List<GameDataContainer.GameDataPair>>(json);//获取初始存档数据
                foreach (var data in list[0].Value)//因为初始存档只有一个key（Init）所以直接获取第一个数据
                    AddData(data, currentSaveKey);
            }
            catch
            {
                Debug.LogError("读取初始存档出错");
                return;
            }
        }
        public bool LoadFromFile(string FileName)
        {
            try
            {
                var json = File.ReadAllText(GetSavePath() + "/" + FileName);
                _gameDataContainer.FromSaveJson(json);
                return true;
                //LoadLevel(FileName);
            }
            catch
            {
                Debug.LogError($"读取文件出错：{FileName}");
                return false;
            }
        }
        public void DeleteFile(string FileName)
        {
            File.Delete(GetSavePath() + "/" + FileName);
        }

        /// <summary>
        /// 加载关卡的逻辑放在这里
        /// </summary>
        /// <returns></returns>
        public void LoadLevel(string FileName)
        {
            //ProcedureManager.Instance.ChangeTo(Launcher.Instance.StartProcedure);
        }

        public enum DataMsg
        {
            WillSave,
        }


#endregion 游戏数据存取接口

#region 变量的声明和初始化
#endregion 变量的声明和初始化

#region 读表类函数,基层轮子

        private readonly TableAgent _tableAgent = new TableAgent();

        public TableAgent TableAgent
        {
            get { return _tableAgent; }
        }

        public void SetTableAgent()
        {
            var list = ResourceManager.Instance.LoadAll<TextAsset>("Text/Table/");
            var t = list.Length;
            for (int i = 0; i < t; i++)
            {
                _tableAgent.Add(list[i].text);
            }
        }

#endregion 读表类函数,基层轮子

#region 胜利失败的监听

#endregion 胜利失败的监听
    }
}