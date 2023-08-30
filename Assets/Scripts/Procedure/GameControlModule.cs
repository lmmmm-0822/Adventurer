using QxFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Chronos;
public class GameControlModule : Submodule
{
    protected override void OnInit()
    {
        base.OnInit();
        InitGame();
    }
    protected override void OnUpdate()
    {
        if (PlayerInput.Instance.GetKeyDown(CharacterInput.exit))
        {
            UIManager.Instance.Open(NameList.UI.MenuUI);
        }
    }
    private void InitGame()
    {
        //if (SceneManager.sceneCount == 1)
        //    SceneManager.LoadScene(1, LoadSceneMode.Additive);
        //UIManager.Instance.Open("HintUI");
        //GameMgr.Get<IEventManager>().ForceEvent(100);
    }
}
