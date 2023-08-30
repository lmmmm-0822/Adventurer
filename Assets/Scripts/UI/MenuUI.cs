using Chronos;
using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : UIBase
{
    protected override void OnAwake()
    {
        base.OnAwake();PlayerInput
        Get<Button>("ContinueBtn").onClick.SetListener(Continue);
        Get<Button>("ReturnBtn").onClick.SetListener(Return);
    }
    protected override void OnDisplay(object args)
    {
        GameMgr.Instance.Pause = true;
    }
    protected override void OnReOpen(object args)
    {
        UIManager.Instance.Close(this);
    }
    protected override void OnClose()
    {
        base.OnClose();
        GameMgr.Instance.Pause = false;
    }
    private void Continue()
    {
        UIManager.Instance.Close(this);
    }
    private void Return()
    {
        UIManager.Instance.Close(this);
        ProcedureManager.Instance.ChangeTo<TitleProcedure>();
    }
}
