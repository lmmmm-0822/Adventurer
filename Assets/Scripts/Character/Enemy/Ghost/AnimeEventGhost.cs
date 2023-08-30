using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimeEventGhost : AnimeEventEnemy
{
    private void CanDamage(int tof)
    {
        ((Ghost)enemy).CanDamage(tof == 1);
    } 
}
