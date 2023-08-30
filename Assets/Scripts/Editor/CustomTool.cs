using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Collections.Generic;
using QxFramework.Core;
using System.Collections;
using UnityEngine.SceneManagement;

public class CustomTool : OdinMenuEditorWindow
{
    public GameObject objectToDrag;

    [MenuItem("Tools/自定义工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<CustomTool>("自定义工具");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 200);
    }
    public class SceneTool
    {
        private static List<string> allScenes;
        private static List<string> AllScenes
        {
            get
            {
                if (allScenes == null)
                {
                    allScenes = new List<string>();
                    foreach (var scene in EditorBuildSettings.scenes)
                        allScenes.Add(Utils.GetFileNameWithoutExtention(scene.path));
                }
                return allScenes;
            }
        }
        //private List<string> AllScenes => CustomTool.AllScenes;
        //private IEnumerable AllItemsPair => CustomTool.AllItemsPair;
        [LabelText("目标场景"), ValueDropdown("AllScenes")] public string sceneName = "Village";
        [LabelText("目标位置")] public Vector2 pos = new Vector2(0, 2);

        [FoldoutGroup("生成buff"), LabelText("buffId")]
        public int buffId = 1;
        [FoldoutGroup("生成buff"), Button("确认生成")]
        private void CreateBuff()
        {
            var go = ResourceManager.Instance.Instantiate("Prefabs/Scene/Effect");
            go.transform.position = pos;
            go.GetComponentInChildren<AddEffectInteractive>().SetEffect(buffId);
        }
    }
    public class ItemTool
    {
    }
    public class TimeTool
    {
    }
    public class EnemyTool
    {
        [FoldoutGroup("获取场景敌人"), LabelText("目标场景")]
        public string scene = "Village_Outside";
        [FoldoutGroup("获取场景敌人"), LabelText("获取结果"), ShowInInspector, ReadOnly]
        public List<List<EnemyType>> resEnemy = new List<List<EnemyType>>();
        [FoldoutGroup("获取场景敌人"), Button("获取敌人")]
        private void GetEnemy()
        {
            resEnemy = GameMgr.CharacterMgr.GetSceneEnemyInfo(scene);
        }

        [FoldoutGroup("生成敌人"), LabelText("目标场景")]
        public string scene1 = "Village_Outside";
        [FoldoutGroup("生成敌人"), LabelText("生成点索引")]
        public int creatorIndex = 1;
        [FoldoutGroup("生成敌人"), LabelText("怪物种类")]
        public EnemyType createType;
        [FoldoutGroup("生成敌人"), LabelText("生成数量")]
        public int createCnt = 1;
        [FoldoutGroup("生成敌人"), Button("生成敌人")]
        private void CreateEnemy()
        {
            GameMgr.CharacterMgr.PreCreateEnemy(scene1, creatorIndex, createType, createCnt);
        }
    }
    public class SaveTool
    {
        [LabelText("文件名")]
        public string fileName = "SaveByCustomTool";
        [Button("存入")]
        private void Save()
        {
            Data.Instance.SaveToFile(fileName);
        }
        [Button("读取")]
        private void Load()
        {
            ProcedureManager.Instance.ChangeTo<TitleProcedure>();
            Data.Instance.LoadFromFile(fileName);
            TimeEventManager.Instance.RegisterTimeAction(1f, () =>
            {
                ProcedureManager.Instance.ChangeTo<GameProcedure>();
            });
        }
    }
    public class SortingOrderChange
    {
        public GameObject target;
        public int changeNum;
        [Button("修改", ButtonSizes.Medium)]
        private void DoIt()
        {
            if (target == null)
            {
                Debug.LogWarning("需要target");
                return;
            }
            foreach (var render in target.GetComponentsInChildren<SpriteRenderer>())
                render.sortingOrder += changeNum;
            Debug.Log($"Order修改已完成，{target.name}及其子物体的SpriteRenderer的SortingOrder发生改变{changeNum}");
        }
    }

    SceneTool sceneTool = new SceneTool();
    ItemTool itemTool = new ItemTool();
    TimeTool timeTool = new TimeTool();
    EnemyTool enemyTool = new EnemyTool();
    SaveTool saveTool = new SaveTool();
    SortingOrderChange sortingOrder = new SortingOrderChange();
    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree;
        if (Application.isPlaying)
            tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                { "场景相关", sceneTool },
                { "物品相关", itemTool },
                { "时间相关", timeTool },
                { "敌人相关", enemyTool },
                { "存档相关", saveTool },
            };
        else
            tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                { "整体层级修改", sortingOrder },
            };

        tree.SortMenuItemsByName();
        return tree;
    }
}