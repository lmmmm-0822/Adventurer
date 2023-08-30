using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyTipInteractive : InteractiveTrigger
{
    OnlyTipInteractive()
    {
        type = InteractiveType.OnlyTip;
    }
    public string tip;
}
