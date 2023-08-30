using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopInteractive : InteractiveTrigger
{
    ShopInteractive()
    {
        type = InteractiveType.Shop;
    }
    private int effectId;
    private int cost;

    public void SetItem(int id)
    {
        effectId = id;
        if (id == 8) cost = 100;
        else cost = 300;

        transform.parent.gameObject.SetActive(true, true);
        GetComponentInParent<SpriteRenderer>().sprite = ResourceManager.Instance.Load<Sprite>("Textures/Effect/" + Effect.GetTemplate(id).IconPath);
        transform.parent.Find("Txt").GetComponent<TextMeshPro>().text = cost.ToString();
    }
    public void GetItem()
    {
        if(GameMgr.CharacterMgr.Character.gold < cost)
        {
            UIManager.Instance.Open(NameList.UI.TipUI, args: "资金不足");
            return;
        }
        GameMgr.CharacterMgr.Character.gold -= cost;
        var go = ResourceManager.Instance.Instantiate("Prefabs/Scene/Effect");
        go.transform.position = transform.position;
        go.GetComponentInChildren<AddEffectInteractive>().SetEffect(effectId);
        transform.parent.gameObject.SetActive(false, true);
    }
}
