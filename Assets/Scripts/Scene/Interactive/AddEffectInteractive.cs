using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework.Core;

public class AddEffectInteractive : InteractiveTrigger
{
    AddEffectInteractive()
    {
        type = InteractiveType.AddEffect;
    }
    public int effectId;
    public bool onlyOne;
    private string key;
    public void SetEffect(int id)
    {
        effectId = id;
        key = null;
        GetComponentInParent<SpriteRenderer>().sprite = ResourceManager.Instance.Load<Sprite>("Textures/Effect/" + Effect.GetTemplate(id).IconPath);
        key = id switch
        {
            7 => "Shield",
            _ => null,
        };
        onlyOne = true;
    }
    public void AddEffect()
    {
        GameMgr.CharacterMgr.Character.effectCtr.AddEffect(effectId, key: key);
        if (onlyOne)
            ObjectPool.Recycle(transform.parent.gameObject);
    }
}
