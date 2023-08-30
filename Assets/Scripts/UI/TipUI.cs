using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipUI : UIBase
{
    private float lastTime;
    protected override void OnDisplay(object args)
    {
        Get<Text>("Text").text = (string)args;
        lastTime = 2f;
    }
    protected override void OnReOpen(object args)
    {
        OnDisplay(args);
    }
    public void SetTime(float time)
    {
        lastTime = time;
    }
    protected override void OnUpdate()
    {
        if (lastTime > 0)
        {
            lastTime -= Time.deltaTime;
            if(lastTime<=0)
            {
                UIManager.Instance.Close(this);
            }
        }
    }
}
