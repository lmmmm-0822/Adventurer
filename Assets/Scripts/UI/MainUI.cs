using Chronos;
using QxFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : UIBase
{
    public override int UILayer => 1;
    private Image combineBuff;
    private Image rProgressBar;
    private Image bProgressBar;
    private Image gProgressBar;
    private GameObject shield;

    private void Awake()
    {
        CollectObject();
        combineBuff = Get<Image>("CombineBuff");
        rProgressBar = Get<Image>("BuffRProgressBar");
        bProgressBar = Get<Image>("BuffBProgressBar");
        gProgressBar = Get<Image>("BuffGProgressBar");
        shield = Child["Shield"];
    }
    protected override void OnUpdate()
    {
        var c = GameMgr.CharacterMgr.Character;
        if (c == null)
            return;
        var eff = c.effectCtr;
        if (eff.HaveEffect(4, out _))
            combineBuff.color = Color.magenta;
        else if (eff.HaveEffect(5, out _))
            combineBuff.color = Color.cyan;
        else if (eff.HaveEffect(6, out _))
            combineBuff.color = Color.yellow;
        else
            combineBuff.color = Color.black;
        rProgressBar.fillAmount = eff.HaveEffect(1, out var ri) ? (eff.effects[ri].LastTime / 120) : 0;
        bProgressBar.fillAmount = eff.HaveEffect(2, out var bi) ? (eff.effects[bi].LastTime / 120) : 0;
        gProgressBar.fillAmount = eff.HaveEffect(3, out var gi) ? (eff.effects[gi].LastTime / 120) : 0;
        Get<Image>("BuffR").color = eff.HaveEffect(1, out _) ? Color.red : new Color(0.5f, 0, 0, 1);
        Get<Image>("BuffB").color = eff.HaveEffect(2, out _) ? Color.blue : new Color(0, 0, 0.5f, 1);
        Get<Image>("BuffG").color = eff.HaveEffect(3, out _) ? Color.green : new Color(0, 0.5f, 0, 1);
        shield.SetActive(eff.HaveEffect(7, out _), true);
        Get<Text>("GoldTxt").text = GameMgr.CharacterMgr.Character.gold.ToString();
    }
}
