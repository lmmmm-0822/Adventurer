using QxFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : UIBase
{
    [ChildValueBind("StartBtn", nameof(Button.onClick))]
    Action OnStartButton;

    //[ChildValueBind("LoadBtn", nameof(Button.onClick))]
    //Action OnLoadButton;

    [ChildValueBind("ExitBtn", nameof(Button.onClick))]
    Action OnExitButton;

    protected override void OnDisplay(object args)
    {
        base.OnDisplay(args);
        CollectObject();
        OnStartButton = StartGame;
        //OnLoadButton = Load;
        OnExitButton = Exit;
        CommitValue();

    }
    public void StartGame()
    {
        //UIManager.Instance.Open("DialogWindowUI", args: new DialogWindowUI.DialogWindowUIArg("提示", "是否开始新游戏", null,
        //     "确定", () => { UIManager.Instance.Open("CreateSaveUI"); }));
        ProcedureManager.Instance.ChangeTo<GameProcedure>();
    }
    //public void Load()
    //{
    //    UIManager.Instance.Open("SaveUI");
    //}
    public void Exit()
    {
        UIManager.Instance.Open("DialogWindowUI", args: new DialogWindowUI.DialogWindowUIArg("提示", "是否退出游戏", null,
             "确定", () =>
             {
                 Application.Quit();
             }));
    }
}
