using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneInteractive : InteractiveTrigger
{
    ChangeSceneInteractive()
    {
        type = InteractiveType.ChangeScene;
    }

    public string changeSceneName;
}
