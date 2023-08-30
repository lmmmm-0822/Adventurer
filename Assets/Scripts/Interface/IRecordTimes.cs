using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRecordTimes
{
    string Key { get; }
    int Threshold { get; }
}
