using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveTipUI : UIBase
{
    public override int UILayer => 0;
    private Transform tips;
    private Transform target;
    private Transform _group;
    private List<InteractiveTrigger> interactiveTriggers = new List<InteractiveTrigger>(3);
    //private List<(InteractiveTrigger inter, Image image)> holdDowns = new List<(InteractiveTrigger, Image)>(3) { default, default, default };
    private int lastPress;
    private readonly Image[] holdDowns = new Image[3];

    protected override void OnAwake()
    {
        base.OnAwake();
        tips = Get<Transform>("Tips");
        for (int i = 0; i < 3; i++)
            holdDowns[i] = tips.GetChild(i).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
    }
    protected override void OnDisplay(object args)
    {
        OnReOpen(args);//图省事
        //SetTipActive(GameMgr.CharacterMgr.Character.CanInteract);
    }
    //protected override void OnRegisterHandler()
    //{
    //    RegisterMessage(Character.AbilityMsg.CanInteractive, (sender, args) => { SetTipActive(true); });
    //    RegisterMessage(Character.AbilityMsg.CannotInteractive, (sender, args) => { SetTipActive(false); });
    //}
    protected override void OnReOpen(object args)
    {
        target = (Transform)args;
        target.GetComponents(interactiveTriggers);
        lastPress = -1;
        int i = 0;
        foreach (var inter in interactiveTriggers)
        {
            RefreshDisplay(inter, i);
            i++;
        }
        for (; i < tips.childCount; i++)
        {
            tips.GetChild(i).gameObject.SetActive(false, true);
        }
        tips.position = Camera.main.WorldToScreenPoint(target.position) + Vector3.left;
    }
    protected override void OnUpdate()
    {
        if (!GameMgr.CharacterMgr.Character.CanInteract)
        {
            SetTipActive(false);
            return;
        }
        SetTipActive(true);
        if (GameMgr.SceneMgr.HoldDownPress == 0)
        {
            if (lastPress != -1)
                holdDowns[lastPress].fillAmount = GameMgr.SceneMgr.HoldDownProgress;
        }
        else
        {
            if (GameMgr.SceneMgr.HoldDownPress != lastPress + 1)
            {
                if (lastPress != -1)
                    holdDowns[lastPress].fillAmount = 0;
                lastPress = GameMgr.SceneMgr.HoldDownPress - 1;
            }
            holdDowns[lastPress].fillAmount = GameMgr.SceneMgr.HoldDownProgress;
        }
        tips.position = Camera.main.WorldToScreenPoint(target.position) + Vector3.left;
    }
    private void RefreshDisplay(InteractiveTrigger trigger, int index)
    {
        _group = tips.GetChild(index);
        _group.gameObject.SetActive(true, true);
        _group.GetChild(0).GetChild(0).gameObject.SetActive(trigger.holdDown != 0, true);
        holdDowns[index].fillAmount = 0;
        _group.GetChild(0).GetChild(1).GetComponent<Text>().text = PlayerInput.Instance.GetCharacterInputKey(CharacterInput.interactive1 + index).ToString();
        _group.GetChild(1).GetComponent<Text>().text = trigger.displayInfo.Trim() != "" ? trigger.displayInfo : trigger.type switch
        {
            InteractiveTrigger.InteractiveType.ChangeScene => "移动",
            InteractiveTrigger.InteractiveType.AddItem => "拾取",
            InteractiveTrigger.InteractiveType.Communicate => "对话",
            InteractiveTrigger.InteractiveType.Break => "休息",
            //InteractiveTrigger.InteractiveType.RecycleBuilding => "回收",
            InteractiveTrigger.InteractiveType.Shop => "商店",
            InteractiveTrigger.InteractiveType.CollectItem => "采集",
            InteractiveTrigger.InteractiveType.Trap => "挣脱",
            _ => "没有设置 " + trigger.type.ToString() + " 的默认交互说明"
        };
        //TimeEventManager.Instance.RegisterTimeAction(2, () => { tips.GetChild(index).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tips.GetChild(index).GetChild(0).GetComponent<RectTransform>().rect.width + tips.GetChild(index).GetChild(1).GetComponent<RectTransform>().rect.width + 20); });
    }
    private void SetTipActive(bool active)
    {
        tips.gameObject.SetActive(active, true);
    }
}
