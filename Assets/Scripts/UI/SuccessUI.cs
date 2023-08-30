using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework.Core;
using UnityEngine.UI;

public class SuccessUI : UIBase
{
    //private Button repeatBtn;
    private Button returnBtn;
    protected override void OnAwake()
    {
        base.OnAwake();
        //repeatBtn = Get<Button>("RepeatBtn");
        returnBtn = Get<Button>("ReturnBtn");

        //repeatBtn.onClick.SetListener(Repeat);
        returnBtn.onClick.SetListener(Return);
    }
    private void Repeat()
    {
        GameMgr.CharacterMgr.PlayerDieRepeat();
        UIManager.Instance.Close(this);
    }
    private void Return()
    {
        UIManager.Instance.Close(this);
        ProcedureManager.Instance.ChangeTo<TitleProcedure>();
    }
}
