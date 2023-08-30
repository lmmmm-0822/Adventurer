using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameProcedure : ProcedureBase
{
    GameObject gameMgr;
    protected override void OnEnter(object args)
    {
        //Data.Instance.LoadFromFile("从主存档中读取文件");
        Data.Instance.currentSaveKey = args == null ? "Debug" : (string)args;
        Data.Instance.TryCreateNewSave();
        gameMgr = new GameObject("GameMgr", typeof(GameMgr));
        GameMgr.Instance.InitModules();
        base.OnEnter(args);
        AddSubmodule(new GameControlModule());
        AudioControl.Instance.BGMPlay("2",0.75f);
        //UIManager.Instance.Open(NameList.UI.MainUI);
    }

    protected override void OnLeave()
    {
        base.OnLeave();
        GameObject.DestroyImmediate(gameMgr);
        //Data.Instance.SaveToFile("FileName.json");
        Data.Instance.ClearData();
        UIManager.Instance.CloseAll();
        AudioControl.Instance.BGMStop();
    }
}
