using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
/// <summary>
/// 目的是在场景未加载时也可以知道场景里有什么
/// </summary>
public class CreateInitSave
{
    [MenuItem("Tools/生成初始存档")]
    static void CreateSave()
    {
        GameDataContainer container = new GameDataContainer();

        CharacterData characterData = new CharacterData(true);
        SceneMgrData sceneData = new SceneMgrData(true);
        container.Add(characterData, "Init");
        container.Add(sceneData, "Init");

        List<GameObject> tmp = new List<GameObject>();
        int scenesInHierarchyCnt = SceneManager.sceneCount;
        List<string> scenesInHierarchyPath = new List<string>(scenesInHierarchyCnt);
        List<bool> scenesIsLoaded = new List<bool>(scenesInHierarchyCnt);
        for (int i = 0; i < scenesInHierarchyCnt; ++i)
        {//记录当前Hierarchy面板中的所有场景的打开状态
            var tmpScene = SceneManager.GetSceneAt(i);
            scenesInHierarchyPath.Add(tmpScene.path);
            scenesIsLoaded.Add(tmpScene.isLoaded);
        }
        foreach (EditorBuildSettingsScene S in EditorBuildSettings.scenes)
        {//遍历build setting中的场景
            if (!S.enabled)//只检测在built setting中被勾选的场景
                continue;

            string name = S.path;//得到场景的名称
            Scene scene = EditorSceneManager.OpenScene(name);//打开这个场景
            scene.GetRootGameObjects(tmp);
            foreach (var go in tmp)
            {
                if (!go.CompareTag("SceneAnchorPoint"))
                    continue;
                #region 敌人生成
                var enemyCreators = go.transform.Find("EnemyCreators");
                var enemyTypess = new List<List<EnemyType>>();
                var createDatas = new List<CharacterData.CreateEnemyData>();
                foreach (Transform creator in enemyCreators)
                {
                    var create = creator.GetComponent<CreateEnemy>();
                    enemyTypess.Add(new List<EnemyType>());
                    createDatas.Add(new CharacterData.CreateEnemyData(create));
                }
                characterData.allEnemys.Add(scene.name, enemyTypess);
                characterData.allCreatorDatas.Add(scene.name, createDatas);
                #endregion
                #region 场景固有物体
                var inherentObject = go.transform.Find("InherentObject");
                var theSceneData = new SceneMgrData.SceneData(true);
                foreach(var sceneSave in inherentObject.GetComponentsInChildren<ISceneSave>())
                {
                    theSceneData.inherentObjectDatas.Add(sceneSave.GameObjectPath, sceneSave.GetInitSaveData());
                }
                sceneData.allSceneDatas.Add(scene.name, theSceneData);
                #endregion

                break;
            }
        }
        Scene mainScene = default;
        var firstScene = EditorSceneManager.OpenScene(scenesInHierarchyPath[0]);
        if (scenesInHierarchyPath[0].Contains("Main"))
            mainScene = firstScene;
        for (int i = 1; i < scenesInHierarchyCnt; ++i)
        {//将之前的场景状态复原
            var t = EditorSceneManager.OpenScene(scenesInHierarchyPath[i], scenesIsLoaded[i] ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
            if (mainScene == default && scenesInHierarchyPath[i].Contains("Main"))
                mainScene = t;
            if (i == 1)
            {
                if (!scenesIsLoaded[0])
                    EditorSceneManager.CloseScene(firstScene, false);
            }
        }
        if (mainScene != default)
            SceneManager.SetActiveScene(mainScene);

        var json = container.ToSaveJson();
        File.WriteAllText(Application.dataPath + "/Resources/Text/InitSave.json", json);
        Debug.Log("成功生成初始存档/Resources/Text/InitSave.json");

        AssetDatabase.Refresh();
    }
}
