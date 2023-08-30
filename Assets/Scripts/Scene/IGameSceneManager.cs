using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameSceneManager
{
    #region 场景管理
    Transform CurrentSceneAnchorPoint { get; }
    string LastScene { get; }
    string CurrentScene { get; }
    void ChangeScene(string sceneName);
    uint GetNewCreatedObjectIndex();
    void CreateObject(string gameObjectPath, CreatedObjectData data, string scene = null);
    void RemoveObject(string gameObjectPath, string scene = null);
    void RegisterCreatedObjectData(string gameObjectPath, CreatedObjectData data, string scene = null);
    void UnRegisterCreatedObjectData(string gameObjectPath, string scene = null);
    //void SetSceneCannot(string sceneName,bool can);
    //bool CheckCanMove(string sceneName);
    #endregion
    #region 场景物体交互
    int HoldDownPress { get; }
    float HoldDownProgress { get; }
    void ExecuteTriggerAction(InteractiveTrigger inter);
    void RegistInteractiveTrigger(Transform interactiveTigger);
    void UnRegistInteractiveTrigger(Transform interactiveTigger);
    #endregion
    int ExitTime(string name, bool onlyExitTime = false);
    bool CheckTimesLimit(ITimesLimit limit);
    void TimesLimitTrigger(ITimesLimit limit);
    SceneMgrData.SceneData GetSceneData(string name);
    Transform GetEntity(string scene, string path);
    ISceneSave GetEntityInterface(string scene, string path);
}
