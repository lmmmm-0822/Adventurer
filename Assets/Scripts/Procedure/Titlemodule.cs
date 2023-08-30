using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Titlemodule : Submodule {

    protected override void OnInit()
    {
        base.OnInit();
        InitGame();
    }
    private void InitGame()
    {
        //if (SceneManager.sceneCount > 1)
        //    SceneManager.UnloadSceneAsync(1);        
        //Data.Instance.SetTableAgent();
        //GameMgr.Instance.InitModules();
    }
}
