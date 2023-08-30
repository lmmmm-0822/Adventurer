using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NameList;

public class TitleProcedure : ProcedureBase {

    protected override void OnEnter(object args)
    {
        AddSubmodule(new Titlemodule());
        base.OnEnter(args);

        UIManager.Instance.Open(UI.TitleUI);
        //Data.Instance.LoadFromFile("FileName.json");
    }
    protected override void OnLeave()
    {
        base.OnLeave();
        UIManager.Instance.CloseAll();
    }
}
