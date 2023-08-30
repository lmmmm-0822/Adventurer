using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class GameSceneManager : LogicModuleBase, IGameSceneManager
{
    private SceneMgrData data;
    //private CannotMoveData _cannotMove;
    //private GameObject unLoadedSceneInteractiveTriggerPool;

    //private HashSet<string> sceneInThisGame = new HashSet<string>();//这次启动游戏后进入的场景
    //private bool isLoading = false;

    public override void Init()
    {
        base.Init();
        //unLoadedSceneInteractiveTriggerPool = ResourceManager.Instance.Instantiate("Prefabs/Scene/UnLoadedSceneInteractiveTriggerPool");
        InitData(out data);//使用了初始存档
        if (data.createdObjectIndex == 0)//因为使用了初始存档，所以这里用createdObjectIndex作为有没有存档的判定
            data.createdObjectIndex = 1;
        else//没有存档时LastScene为null，有存档时LastScene为CurrentScene
            LastScene = CurrentScene;
        SceneManager.sceneLoaded += SceneLoaded;

        MessageManager.Instance.Get<Data.DataMsg>().RegisterHandler(Data.DataMsg.WillSave, (sender, args) =>
        {
            foreach (var sceneSave in GameMgr.SceneMgr.CurrentSceneAnchorPoint.GetComponentsInChildren<ISceneSave>())
                sceneSave.UpdateData();
        });
    }

    public override void Awake()
    {
        if (SceneManager.sceneCount == 1)
        {
            SceneManager.LoadScene(CurrentScene,LoadSceneMode.Additive);//进入GameProcedure后创建GameMgr，初始化模块，然后执行各个模块的Awake函数，此时加载场景。
        }
        else
        {//仅为了Debug方便，之后删除该条件。
            data.current = SceneManager.GetSceneAt(1).name;
            SceneLoaded(default, 0);
        }
    }

    #region 场景管理
    public Transform CurrentSceneAnchorPoint { get; private set; }
    public string LastScene { get; private set; }
    public string CurrentScene => data.current;
    public void ChangeScene(string sceneName)
    {
        //isLoading = true;
        LastScene = CurrentScene;

        SceneWillUnload();
        SceneManager.UnloadSceneAsync(LastScene);

        data.current = sceneName;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    private void SceneWillUnload()
    {
        //顺便关掉UI，就不用消息系统了
        interactiveTrans.Clear();
        UIManager.Instance.Close(NameList.UI.InteractiveTipUI);

        foreach (var sceneSave in CurrentSceneAnchorPoint.GetComponentsInChildren<ISceneSave>())
            sceneSave.UpdateData();

        //RecycleInteractiveTrigger(CurrentScene);
        GetSceneData(CurrentScene).exitTime = GameMgr.TimeMgr.GetNow().TotalMinutes;//记录离开场景的时间
        MessageManager.Instance.Get<GameSceneMsg>().DispatchMessage(GameSceneMsg.WillUnload, this);
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //sceneInThisGame.Add(CurrentScene);
        //设置当前场景锚点
        CurrentSceneAnchorPoint = GameObject.FindGameObjectWithTag("SceneAnchorPoint").transform;

        //RestoreInteractiveTrigger(CurrentScene);

        var sceneData = GetSceneData(CurrentScene);
        //将存档数据写入场景固有物体（场景收集物、陷阱、特殊可拾取物体
        foreach (var sceneSave in CurrentSceneAnchorPoint.Find("InherentObject").GetComponentsInChildren<ISceneSave>())
        {
            //当数据中没有相应的data时，data为null；sceneSave因为接收到null所以会new一个data出来并返回，数据将该data存入
            if (!sceneData.inherentObjectDatas.TryGetValue(sceneSave.GameObjectPath, out var data))
                Debug.LogError($"{CurrentScene}场景没有{sceneSave.GameObjectPath}的存档数据，记得生成初始存档");
            sceneSave.InitData(data);
        }
        //根据存档数据生成相应的产生物体（掉落物、放置物
        foreach(var createdObjectData in sceneData.createdObjectDatas)
        {
            //确认父级
            int len = createdObjectData.Key.LastIndexOf('/') + 1;
            Transform parent = len == 0 ? CurrentSceneAnchorPoint : CurrentSceneAnchorPoint.Find(createdObjectData.Key.Substring(0, len));
            
            //生成实例并写入数据
            var go = ResourceManager.Instance.Instantiate("Prefabs/" + createdObjectData.Value.PrefabPath, parent);
            go.name = createdObjectData.Key.Substring(len);
            go.transform.position = createdObjectData.Value.Position;
            go.GetComponent<ISceneSave>().InitData(createdObjectData.Value);//因为data不会为null（为null会被清除），所以不需要写回
        }

        //发送场景加载结束消息，以便其他管理器执行切换场景逻辑
        MessageManager.Instance.Get<GameSceneMsg>().DispatchMessage(GameSceneMsg.NewSceneLoaded, this);

        //显示场景名称
        UIManager.Instance.Open(NameList.UI.TipUI, args: Data.Instance.TableAgent.GetString("Scene", CurrentScene, "Name")); 
    }
    public override void OnDestroy()
    {
        //ObjectPool.Recycle(unLoadedSceneInteractiveTriggerPool);
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.UnloadSceneAsync(CurrentScene);
    }
    public uint GetNewCreatedObjectIndex()
    {
        return ++data.createdObjectIndex;
    }
    public void CreateObject(string gameObjectPath, CreatedObjectData data, string scene = null)
    {
        if (data == null)
        {
            Debug.LogError($"用空数据注册了生成物体：目标场景{scene}，物体层级{gameObjectPath}");
            return;
        }
        if (scene == null) scene = CurrentScene;
        gameObjectPath += "_" + GameMgr.SceneMgr.GetNewCreatedObjectIndex().ToString();//确保字典key唯一
        var sceneData = GetSceneData(scene);
        if (scene == CurrentScene)
        {
            int len = gameObjectPath.LastIndexOf('/') + 1;
            Transform parent = len == 0 ? CurrentSceneAnchorPoint : CurrentSceneAnchorPoint.Find(gameObjectPath.Substring(0, len));

            var go = ResourceManager.Instance.Instantiate("Prefabs/" + data.PrefabPath, parent);
            go.name = gameObjectPath.Substring(len);
            go.transform.position = data.Position;
            go.GetComponent<ISceneSave>().InitData(data);//写入实体
        }
        sceneData.createdObjectDatas[gameObjectPath] = data;//写入存档
    }
    public void RemoveObject(string gameObjectPath, string scene = null)
    {
        if (scene == null) scene = CurrentScene;
        if(scene == CurrentScene)
        {
            var entity = GetEntity(scene, gameObjectPath);
            if (entity)
                ObjectPool.Recycle(entity);
        }
        var sceneData = GetSceneData(scene);
        sceneData.createdObjectDatas.Remove(gameObjectPath);
    }
    public void RegisterCreatedObjectData(string gameObjectPath, CreatedObjectData data, string scene)
    {
        if (scene == null) scene = CurrentScene;//默认场景为当前场景
        var sceneData = GetSceneData(scene);
        var entity = GetEntityInterface(scene, gameObjectPath);//非当前场景获取不到实体
        if (entity != null)
        {//当前场景 用数据初始化实体并存入数据
            entity.InitData(data);
            sceneData.createdObjectDatas[gameObjectPath] = data;
        }
        else
        {//其他场景 将数据存入
            if (data == null)
            {
                Debug.LogError($"不允许用空数据注册其他场景的生成物体：当前场景{CurrentScene}，目标场景{scene}，物体层级{gameObjectPath}");
                return;
            }
            sceneData.createdObjectDatas[gameObjectPath] = data;
        }
    }
    public void UnRegisterCreatedObjectData(string gameObjectPath, string scene)
    {
        if (scene == null) scene = CurrentScene;
        var sceneData = GetSceneData(scene);
        sceneData.createdObjectDatas.Remove(gameObjectPath);
    }
    #endregion

    #region 场景物体交互
    private List<Transform> interactiveTrans = new List<Transform>();//在交互范围内的
    private List<InteractiveTrigger> interactiveTriggers = new List<InteractiveTrigger>(3);//单个物体多个交互
    private Transform currentTran;
    private Transform CurrentTran
    {
        get => currentTran;
        set
        {
            currentTran = value;
            if (currentTran != null)
                currentTran.GetComponents(interactiveTriggers);
            HoldDownPress = 0;
            HoldDownProgress = 0;
        }
    }
    private int lastHoldDownPress;
    private int holdDownPress;
    public int HoldDownPress
    {
        get => holdDownPress;
        private set
        {
            if (value != holdDownPress)
            {
                holdDownPress = value;
                if (holdDownPress != 0)
                {
                    if (lastHoldDownPress != holdDownPress)
                        HoldDownProgress = 0;
                    lastHoldDownPress = holdDownPress;
                }
            }
        }
    }
    public float HoldDownProgress { get; private set; }
    public override void Update()
    {
        if (interactiveTrans.Count >= 2)
        {
            if (!IsCurrentTriggerClosest(out var temp))
            {
                CurrentTran = temp;  
                UIManager.Instance.Open(NameList.UI.InteractiveTipUI, args: temp);
            }
        }
        if (CurrentTran != null && GameMgr.CharacterMgr.Character.CanInteract)
        {
            int button = 0, i = 0;
            foreach (var inter in interactiveTriggers)
            {
                if (inter.holdDown == 0)
                {
                    if (PlayerInput.Instance.GetKeyDown(CharacterInput.interactive1 + i))
                    {
                        ExecuteTriggerAction(inter);
                        return;
                    }
                }
                else
                {
                    button |= 1 << i;
                }
                i++;
            }
            HoldDownPress = PlayerInput.Instance.GetInteractiveKey(button);
            if (HoldDownPress != 0)
            {
                HoldDownProgress += Time.deltaTime / interactiveTriggers[HoldDownPress - 1].holdDown;
                if (HoldDownProgress > 1.05f)
                {
                    HoldDownProgress = 0;
                    ExecuteTriggerAction(interactiveTriggers[HoldDownPress - 1]);
                }
                return;//跳过Progress减少
            }
        }
        if (HoldDownProgress > 0)
            HoldDownProgress -= 2 * Time.deltaTime;
    }
    private bool IsCurrentTriggerClosest(out Transform closest)
    {
        //float minDistanceSqr = 500,tempDistanceSqr;
        float minDistance = 20, tempDistance;
        closest = null;
        foreach (var target in interactiveTrans)
        {
            //tempDistanceSqr = ((Vector2)target.transform.position - (Vector2)GameMgr.CharacterMgr.Character.transform.position).sqrMagnitude;
            //if (tempDistanceSqr < minDistanceSqr)
            tempDistance = Mathf.Abs(target.position.x - GameMgr.CharacterMgr.Character.transform.position.x);
            if (tempDistance < minDistance)
            {
                closest = target;
                //minDistanceSqr = tempDistanceSqr;
                minDistance = tempDistance;
            }
        }
        return closest == CurrentTran;
    }
    public void ExecuteTriggerAction(InteractiveTrigger inter)
    {//todo
        switch (inter.type)
        {
            case InteractiveTrigger.InteractiveType.ChangeScene:
                ChangeScene(((ChangeSceneInteractive)inter).changeSceneName);
                break;
            case InteractiveTrigger.InteractiveType.AddEffect:
                ((AddEffectInteractive)inter).AddEffect();
                break;
            case InteractiveTrigger.InteractiveType.OnlyTip:
                UIManager.Instance.Open(NameList.UI.TipUI, args:((OnlyTipInteractive)inter).tip);
                break;
            case InteractiveTrigger.InteractiveType.Shop:
                ((ShopInteractive)inter).GetItem();
                break;
            default:
                Debug.LogError("没有设置 " + inter.type.ToString() + " 的交互逻辑");
                break;
        }

        inter.Trigger();//todo 之后再交互状态判断、调用
    }
    public void RegistInteractiveTrigger(Transform interactiveTrigger)
    {
        if (interactiveTrans.Contains(interactiveTrigger))
            return;

        interactiveTrans.Add(interactiveTrigger);

        if (interactiveTrans.Count == 1 || !IsCurrentTriggerClosest(out interactiveTrigger))
        {
            CurrentTran = interactiveTrigger;
            UIManager.Instance.Open(NameList.UI.InteractiveTipUI, args: interactiveTrigger);
        }
    }
    public void UnRegistInteractiveTrigger(Transform interactiveTrigger)
    {
        interactiveTrans.Remove(interactiveTrigger);
        switch (interactiveTrans.Count)
        {
            case 0:
                CurrentTran = null;
                UIManager.Instance.Close(NameList.UI.InteractiveTipUI);
                break;
            case 1:
                if (interactiveTrans[0] != CurrentTran)
                {
                    CurrentTran = interactiveTrans[0];
                    UIManager.Instance.Open(NameList.UI.InteractiveTipUI, args: CurrentTran);
                }
                break;
            default:
                if (!IsCurrentTriggerClosest(out var temp))
                {
                    CurrentTran = temp;
                    UIManager.Instance.Open(NameList.UI.InteractiveTipUI, args: temp);
                }
                break;
        }
    }

    ///// <summary>
    ///// 卸载旧场景时使用，将即将卸载掉的场景中的所有InteractiveTrigger加入到unLoadedSceneInteractiveTriggerPool中
    ///// </summary>
    ///// <param name="lastScene">即将卸载的场景的场景名</param>
    //private void RecycleInteractiveTrigger(string lastScene)
    //{
    //    Transform currentSceneInteractiveTrigger = GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("SceneTriggers").transform.Find(lastScene);
    //    // 如果没有找到，就说明当前场景中没有可交互物体(陷阱)
    //    if (currentSceneInteractiveTrigger == null)
    //    {
    //        ConsoleProDebug.LogToFilter(lastScene+"中没有可交互物体(陷阱)", "Other");
    //        return;
    //    }

    //    // 如果找到了，就回收该陷阱，并将其父亲设置为unLoadedSceneInteractiveTriggerPool
    //    currentSceneInteractiveTrigger.SetParent(unLoadedSceneInteractiveTriggerPool.transform);
    //    currentSceneInteractiveTrigger.gameObject.SetActive(false);
    //}

    ///// <summary>
    ///// 加载新场景时使用，将unLoadedSceneInteractiveTriggerPool中原来场景中的InteractiveTrigger放入到场景中
    ///// </summary>
    ///// <param name="newScene">新加载的场景的场景名</param>
    //private void RestoreInteractiveTrigger(string newScene)
    //{
    //    Transform currentSceneInteractiveTrigger = unLoadedSceneInteractiveTriggerPool.transform.Find(newScene);
    //    // 如果该场景没有可陷阱等可交互物品，就直接结束
    //    if (currentSceneInteractiveTrigger == null)
    //    {
    //        ConsoleProDebug.LogToFilter(newScene + "中没有可交互物体(陷阱)", "Other");
    //        return;
    //    }

    //    // 如果有可交互物品，就设置
    //    Transform targetParent = GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("SceneTriggers");
    //    currentSceneInteractiveTrigger.SetParent(targetParent);
    //    currentSceneInteractiveTrigger.gameObject.SetActive(true);
    //}
    #endregion

    public int ExitTime(string name, bool onlyExitTime = false)
    {//通常获取当前场景的离开时间返回现在的时间，如果onlyExitTime则返回上次离开当前场景的时间
        if (!onlyExitTime && name == CurrentScene)
            return GameMgr.TimeMgr.GetNow().TotalMinutes;
        return GetSceneData(name).exitTime;//默认值为-1，即没来过这个场景会返回-1
    }
    public bool CheckTimesLimit(ITimesLimit limit)
    {
        if (GetSceneData(CurrentScene).timesLimit.TryGetValue(limit.Name,out var value))
        {
            if (value >= limit.Threshold)
                return false;
        }
        return true;
    }
    public void TimesLimitTrigger(ITimesLimit limit)
    {
        if (GetSceneData(CurrentScene).timesLimit.ContainsKey(limit.Name))
            GetSceneData(CurrentScene).timesLimit[limit.Name]++;
        else
            GetSceneData(CurrentScene).timesLimit.Add(limit.Name, 1);

        if (GetSceneData(CurrentScene).timesLimit[limit.Name] >= limit.Threshold)
            limit.Target.SetActive(false);
    }

    public SceneMgrData.SceneData GetSceneData(string scene)
    {
        if (!data.allSceneDatas.TryGetValue(scene, out var sceneData))
        {//因为生成了初始存档，所以这里应该不会执行
            Debug.LogError($"获取了没有初始存档的场景数据：{scene}");
            sceneData = new SceneMgrData.SceneData(true);
            data.allSceneDatas[scene] = sceneData;
        }
        return sceneData;
    }
    public Transform GetEntity(string scene, string path) => scene == CurrentScene ? CurrentSceneAnchorPoint.Find(path) : null;
    public ISceneSave GetEntityInterface(string scene, string path) => GetEntity(scene, path)?.GetComponent<ISceneSave>();
   
}
public enum SceneObjectType
{
    None,
    Item,
    CollectItem,
    Trap,
    Building,
}
public enum GameSceneMsg
{
    WillUnload,
    NewSceneLoaded,
}
public class SceneMgrData : GameDataBase
{
    public SceneMgrData() { }
    public SceneMgrData(bool init)
    {
        current = "Level1";
        allSceneDatas = new Dictionary<string, SceneData>();
    }
    public string current;
    public uint createdObjectIndex;
    public Dictionary<string, SceneData> allSceneDatas;
    public class SceneData
    {
        public int exitTime;
        public Dictionary<string, SceneSaveData> inherentObjectDatas;//固有场景物体数据
        public Dictionary<string, CreatedObjectData> createdObjectDatas;//运行中产生的物体

        public Dictionary<string, int> timesLimit;

        public SceneData() { }//存档系统自动调用
        public SceneData(bool init)//实际初始化调用
        {
            exitTime = -1;
            inherentObjectDatas = new Dictionary<string, SceneSaveData>();
            createdObjectDatas = new Dictionary<string, CreatedObjectData>();

            timesLimit = new Dictionary<string, int>();
        }
    }
}
