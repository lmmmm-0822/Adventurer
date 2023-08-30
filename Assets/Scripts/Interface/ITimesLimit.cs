using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimesLimit
{
    public string Name { get; }
    public int Threshold { get; }
    public GameObject Target { get; }
}
