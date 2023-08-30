using Chronos;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGameTimeManager {
    GlobalClock TimeClock { get; }
    bool IsStop { get; }
    GameDateTime GetNow();
    void DoStart();
    void Pause();
    void RegisterTimeRepeat(Func<GameDateTime, bool> action, GameDateTime interval, string key = null);
    void StepMinute(int minutes, float time, Action<int> action = null);
    void StopStep();
    //bool StepMinute(float stepTime);//,bool inside = false);
}
