using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : UIBase
{
    private Slider healthBar;
    private CharacterBase cB;
    protected override void OnAwake()
    {
        base.OnAwake();
        healthBar = Get<Slider>("HealthBar");
    }
    protected override void OnDisplay(object args)
    {
        cB = (CharacterBase)args;
    }
    protected override void OnUpdate()
    {
        healthBar.value = cB.cAtr.CurrentHealth / cB.cAtr.MaxHealth;
    }
}
