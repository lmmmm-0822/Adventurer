using Chronos;
using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CharacterManager : LogicModuleBase, ICharacterManager
{
    public Character Character { get; private set; }
    private CharacterData data;
    private GameObject unloadedSceneCharactersPool;
    private List<CharacterBase> characters = new List<CharacterBase>(16);
    private int charactersCnt;
    public override void Init()
    {
        InitData(out data);

        unloadedSceneCharactersPool = new GameObject("UnloadedSceneCharactersPool");
        if ((Character = Object.FindObjectOfType<Character>()) == null)//Todo等场景加载完之后再加载玩家
            Character = ResourceManager.Instance.Instantiate("Prefabs/Character/Player").GetComponent<Character>();

        MessageManager.Instance.Get<Data.DataMsg>().RegisterHandler(Data.DataMsg.WillSave, (sender, args) =>
        {//即将存档时
            data.characterData = new CharacterData.CharacterEntityData(Character);
            
            //记录Enemy实体数据
            int now = GameMgr.TimeMgr.GetNow().TotalMinutes;
            foreach(var key in data.enemyEntityDatas.Keys.ToList())
            {
                if (now - GameMgr.SceneMgr.ExitTime(key) > 7200)//超过5天没有进入这个场景
                    data.enemyEntityDatas.Remove(key);//移除该场景的实体数据
            }
            List<Enemy> enemys = new List<Enemy>();
            foreach (Transform sceneCreators in unloadedSceneCharactersPool.transform)
                RecordSceneEnemyData(sceneCreators.name, sceneCreators);
            RecordSceneEnemyData(GameMgr.SceneMgr.CurrentScene, GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("EnemyCreators"));
            
            void RecordSceneEnemyData(string scene,Transform sceneCreators)
            {
                if (now - GameMgr.SceneMgr.ExitTime(scene) > 7200)
                {
                    return;
                }
                List<List<CharacterData.EnemyEntityData>> sceneEnemy = new List<List<CharacterData.EnemyEntityData>>();
                foreach (Transform creator in sceneCreators)
                {//检查单个creator
                    List<CharacterData.EnemyEntityData> enemyDatas = new List<CharacterData.EnemyEntityData>();
                    creator.GetComponentsInChildren(enemys);//获取单个creator的所有enemy
                    foreach (var enemy in enemys)
                        enemyDatas.Add(new CharacterData.EnemyEntityData(enemy));
                    sceneEnemy.Add(enemyDatas);
                }
                data.enemyEntityDatas[scene] = sceneEnemy;
            }
        });
        MessageManager.Instance.Get<GameSceneMsg>().RegisterHandler(GameSceneMsg.WillUnload, (sender, args) => { RecycleCreators(); });
        MessageManager.Instance.Get<GameSceneMsg>().RegisterHandler(GameSceneMsg.NewSceneLoaded, (sender, args) =>
        {
            LoadCreators();
            ChangeCharacterPosition();
            //CollectCharacters();
        });
    }
    public override void Awake()
    {
        //AddCharacter(Character, true, data.characterData);
        UIManager.Instance.Open(NameList.UI.MainUI);
    }
    public override void Update()
    {
        for (int i = 0; i < charactersCnt; i++)
        {
            characters[i].OnUpdate();
        }
    }
    public override void FixedUpdate()
    {
        for (int i = 0; i < charactersCnt; i++)
        {
            characters[i].OnFixedUpdate();
        }
    }
    public override void OnDestroy()
    {
        MessageManager.Instance.RemoveAbout(this);
        if (Character != null)
            RemoveCharacter(Character);
        Object.Destroy(unloadedSceneCharactersPool);
    }
    #region 场景切换
    public void PlayerDieRepeat()
    {//之后改成读档
        for (int i = 0; i < charactersCnt; i++)
        {
            characters[i].Enable(true, null);
        }
        GameMgr.SceneMgr.ChangeScene(GameMgr.SceneMgr.CurrentScene);
    }
    private void ChangeCharacterPosition()
    {
        Transform t = null;
        if (GameMgr.SceneMgr.LastScene != null)//为null时是初始存档
        {
            t = GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("ComeFromPoint/" + GameMgr.SceneMgr.LastScene);
            if (t == null)
                Debug.LogWarning("从不相连的场景“" + GameMgr.SceneMgr.LastScene + "”传送到了此场景");
        }
        if (t == null)
        {
            t = GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("ComeFromPoint/Default");
            if (t == null)
            {
                Debug.LogWarning("未设置默认传送点");
                Character.transform.position = new Vector3 { x = 0, y = 10, z = 0 };//方便观察
                return;
            }
        }
        Character.transform.position = t.position;
    }
    //private void CollectCharacters()
    //{
    //    characters.Clear();
    //    characters.AddRange(GameObject.FindObjectsOfType<CharacterBase>());
    //    for (int i = 0, cnt = characters.Count; i < cnt; i++)
    //    {
    //        if (!characters[i].Init)
    //        {
    //            characters[i].OnAwake();
    //            characters[i].OnStart();//不同于Unity的调用顺序，这里Start先于Enable
    //            characters[i].Enable();
    //        }
    //    }
    //}
    private void LoadCreators()
    {//加载完场景后执行
        if(!Character.gameObject.activeSelf)
        Character = ResourceManager.Instance.Instantiate("Prefabs/Character/Player").GetComponent<Character>();
        AddCharacter(Character, true, data.characterData);
        List<List<CharacterData.EnemyEntityData>> enemyDatas = null;
        var enemyCreators = unloadedSceneCharactersPool.transform.Find(GameMgr.SceneMgr.CurrentScene);
        //if (enemyCreators != null)
        //{//若不是第一次进入该场景，则将之前的Creator覆盖到此场景
        //    if (enemyCreators.childCount == 0)
        //        return;//没有creator，什么都不做
        //
        //    Object.Destroy(GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("EnemyCreators").gameObject);
        //    enemyCreators.SetParent(GameMgr.SceneMgr.CurrentSceneAnchorPoint);
        //    enemyCreators.name = "EnemyCreators";
        //    enemyCreators.gameObject.SetActive(true);
        //}
        //else
        {//如果是这次启动游戏后第一次进入该场景，则对Creator初始化
            enemyCreators = GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("EnemyCreators");
            if (enemyCreators.childCount == 0)
                return;//没有creator，什么都不做
            data.enemyEntityDatas.TryGetValue(GameMgr.SceneMgr.CurrentScene, out enemyDatas);//获取存档中的实体数据
        }

        var createDatas = data.allCreatorDatas[GameMgr.SceneMgr.CurrentScene];
        var nextCreates = data.allEnemys[GameMgr.SceneMgr.CurrentScene];
        for (int i = 0, cnt = enemyCreators.childCount; i < cnt; ++i)
        {
            enemyCreators.GetChild(i).GetComponent<CreateEnemy>().Enable(createDatas[i], nextCreates[i], enemyDatas?[i]);
        }
    }
    private void RecycleCreators()
    {//卸载场景前执行，收集当前场景的Creator

        for(int i = charactersCnt-1; i >0; --i)
        {
            RemoveCharacter(characters[i]);
        }
        ////Creator本身设置
        //var enemyCreators = GameMgr.SceneMgr.CurrentSceneAnchorPoint.Find("EnemyCreators");
        //enemyCreators.SetParent(unloadedSceneCharactersPool.transform);
        //enemyCreators.name = GameMgr.SceneMgr.CurrentScene;
        //enemyCreators.gameObject.SetActive(false);

        ////记录Create数据
        //int cnt = enemyCreators.childCount;
        //if (cnt == 0)
        //{
        //    //data.enemyEntityDatas.Remove(GameMgr.SceneMgr.CurrentScene);
        //    return;
        //}
        //List<CharacterData.CreateEnemyData> createDatas;
        //try { createDatas = data.allCreatorDatas[GameMgr.SceneMgr.CurrentScene]; }//用于记录createData
        //catch
        //{//onlyUnity
        //    Debug.LogError($"初始存档中不存在{GameMgr.SceneMgr.CurrentScene}的enemy数据，记得重新生成一次初始存档");
        //    createDatas = new List<CharacterData.CreateEnemyData>();
        //    int tmp = 0;
        //    foreach (Transform t in enemyCreators)
        //        t.GetComponent<CreateEnemy>().Disable(ref tmp);
        //}
        ////List<Enemy> enemys = new List<Enemy>();
        ////List<List<CharacterData.EnemyEntityData>> sceneEnemy = new List<List<CharacterData.EnemyEntityData>>();
        //for (int i = 0; i < cnt; ++i)
        //{
        //    //Creator数据
        //    var creator = enemyCreators.GetChild(i);
        //    creator.GetComponent<CreateEnemy>().Disable(ref createDatas[i].disableTime);

        //    ////Enemy实体数据
        //    //List<CharacterData.EnemyEntityData> enemyDatas = new List<CharacterData.EnemyEntityData>();
        //    //creator.GetComponentsInChildren(enemys);//获取单个creator的所有enemy
        //    //foreach (var enemy in enemys)
        //    //    enemyDatas.Add(new CharacterData.EnemyEntityData(enemy));
        //    //sceneEnemy.Add(enemyDatas);
        //}
        ////data.enemyEntityDatas[GameMgr.SceneMgr.CurrentScene] = sceneEnemy;
    }
    #endregion

    public void PreCreateEnemy(string scene, int creatorIndex, EnemyType type, int count)
    {
        List<EnemyType> creator = null;
        try {creator = data.allEnemys[scene][creatorIndex]; }
        catch { Debug.LogError($"{scene}场景不存在或没有{creatorIndex}号生成点"); }
        for (int i = 0; i < count; ++i)
            creator.Add(type);
    }
    public bool InstantiateEnemy(EnemyType type, Vector3 localPosition, out Enemy enemy, Transform parent = null, CharacterData.EnemyEntityData entityData = null)
    {
        try
        {
            enemy = ResourceManager.Instance.Instantiate("Prefabs/Character/" + type.ToString(), parent).GetComponent<Enemy>();
            enemy.transform.localPosition = localPosition;
        }
        catch
        {
            enemy = null;
            Debug.LogError("没有在Resources/Prefabs/Character文件夹中找到名称为" + type.ToString() + "的敌人");
            return false;
        }

        AddCharacter(enemy, true, entityData);
        return true;
    }
    public void AddCharacter(CharacterBase c, bool create, CharacterData.EntityData entityData = null)
    {
        if (characters.Count > charactersCnt)//characters里有空位
            characters[charactersCnt] = c;//将c填入空位
        else
            characters.Add(c);
        ++charactersCnt;//计数增1
        c.Enable(create, entityData);
    }
    public void RemoveCharacter(CharacterBase c, bool destroy = true)
    {
        if (destroy && c is Enemy enemy)
        {//摧毁的时候将enemy从数据中移除（因为添加的时候是EnemyCreator直接在模拟生成的时候就添加了，所以不用在AddCharacter里添加）
            data.allEnemys[GameMgr.SceneMgr.CurrentScene][enemy.transform.parent.GetSiblingIndex()].Remove(enemy.enemyType);
        }

        for (int i = 0; i < charactersCnt; ++i)
        {
            if (characters[i] == c)
            {
                characters[i].Disable(destroy);
                if (destroy)
                {
                    ObjectPool.Recycle(characters[i]);
                }
                characters[i] = characters[--charactersCnt];//将最后一个需要更新的角色移动过去
                return;
            }
        }

        c.Disable(destroy);
        Debug.LogWarning("未控制的角色请求回收" + c.name);
        if (destroy)
        {
            ObjectPool.Recycle(c);
        }
    }
    public List<List<EnemyType>> GetSceneEnemyInfo(string scene)
    {
        if (scene == GameMgr.SceneMgr.CurrentScene)
            return data.allEnemys[scene];//当前处于需要获取的场景里

        var createDatas = data.allCreatorDatas[scene];
        var nextCreates = data.allEnemys[scene];
        var cnt = createDatas.Count;
        if (cnt == 0)
            return nextCreates;

        for (int i = 0; i < cnt; ++i)
        {//模拟生成
            CreateEnemy.SimulateCreate(createDatas[i], nextCreates[i]);
        }
        return nextCreates;//返回存档里的enemy
    }

}
public class CharacterData : GameDataBase
{//存入了初始存档，运行时不会没有初始化
    public CharacterData() { }
    public CharacterData(bool init)
    {
        allEnemys = new Dictionary<string, List<List<EnemyType>>>();
        allCreatorDatas = new Dictionary<string, List<CreateEnemyData>>();
        enemyEntityDatas = new Dictionary<string, List<List<EnemyEntityData>>>();//存档前会刷新
    }
    public Dictionary<string, List<List<EnemyType>>> allEnemys;
    public Dictionary<string, List<CreateEnemyData>> allCreatorDatas;
    public Dictionary<string, List<List<EnemyEntityData>>> enemyEntityDatas;
    public CharacterEntityData characterData;
    public class CreateEnemyData
    {
        public CreateEnemyData() { }
        public CreateEnemyData(CreateEnemy creator)
        {
            enemies = creator.enemies;
            randomTime = creator.random;
            minTime = creator.minTime;
            maxTime = creator.maxTime;
            certainTime = creator.certainTime;
            maxCount = creator.maxCount;
            initCount = creator.initCount;
        }

        //用于数据记录
        public int lastTime;
        public int disableTime;

        //模拟生成
        public List<CreateEnemy.EnemyWeight> enemies;
        public bool randomTime;
        public int minTime;
        public int maxTime;
        public int certainTime;
        public int maxCount;
        public int initCount;
    }
    public class EntityData
    {
        public EntityData() { }
        public EntityData(CharacterBase cb)
        {
            position = cb.transform.position;
            effects = new List<Effect.SaveData>();
            foreach (var eff in cb.effectCtr.effects)
                effects.Add(new Effect.SaveData(eff));
        }
        public Vector2 position;
        public List<Effect.SaveData> effects;
    }
    public class EnemyEntityData : EntityData
    {
        public EnemyEntityData() { }
        public EnemyEntityData(Enemy enemy) : base(enemy)
        {
            realAtr = enemy.atrCtr.realCAtr;
            runtimeData = new CharacterAttribute.SaveData(enemy.cAtr);
            type = enemy.enemyType;
        }
        public CharacterIntrinsicAttribute realAtr;//之后可能会有随机属性，所以需要存一下
        public CharacterAttribute.SaveData runtimeData;//用于存储当前生命值等
        public EnemyType type;//仅用于确定实体类型
    }
    public class CharacterEntityData : EntityData
    {
        public CharacterEntityData() { }
        public CharacterEntityData(Character character) : base(character)
        {
            realAtr = character.atrCtr.realCAtr;
            runtimeData = new CharacterAttribute.SaveData(character.cAtr);
        }
        public CharacterIntrinsicAttribute realAtr;
        public CharacterAttribute.SaveData runtimeData;//用于存储当前生命值等
    }
}