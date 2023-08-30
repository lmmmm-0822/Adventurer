using UnityEngine;

public interface ISceneSave
{
    SceneSaveData GetInitSaveData();
    string GameObjectPath { get; }//场景内需要唯一，用于标识数据对应的实体
    /// <summary>
    /// 进入场景时会执行
    /// </summary>
    void InitData(SceneSaveData data);
    /// <summary>
    /// 即将保存时以及离开场景时调用
    /// </summary>
    void UpdateData();
}
public class SceneSaveData
{
    public SceneObjectType ObjectType;
    public Vector3 Position;
}
public class CreatedObjectData : SceneSaveData
{
    public string PrefabPath;
}